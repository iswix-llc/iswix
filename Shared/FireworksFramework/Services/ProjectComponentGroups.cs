using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FireworksFramework.Services
{
    /// <summary>A component group, and the file that defines it.</summary>
    public sealed class ComponentGroupInfo
    {
        public ComponentGroupInfo(string id, string? filePath)
        {
            Id = id;
            FilePath = filePath;
        }

        /// <summary>Gets the component group's Id.</summary>
        public string Id { get; }

        /// <summary>Gets the .wxs file that defines it, or null for one that no file in the project does.</summary>
        public string? FilePath { get; }
    }

    /// <summary>
    /// Finds the component groups a feature can reference. They can be defined in the file being edited or in any other
    /// .wxs file of its project, which is taken to be what is under the folder of the nearest .wixproj above the file
    /// (the WiX SDK compiles every .wxs under it); a file with no project above it stands alone, with the files in its
    /// own folder.
    /// </summary>
    public static class ProjectComponentGroups
    {
        // Folders that hold build output, caches or other people's files, not the project's sources.
        static readonly HashSet<string> SkippedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", "node_modules", "packages",
        };

        // What was read from a file, for as long as the file is as it was.
        static readonly ConcurrentDictionary<string, CachedFile> Cache = new ConcurrentDictionary<string, CachedFile>(StringComparer.OrdinalIgnoreCase);

        sealed class CachedFile
        {
            public DateTime LastWriteUtc;
            public long Length;
            public List<string> Ids = new List<string>();
        }

        /// <summary>
        /// Gets the folder of the project the file belongs to: the nearest one, going up, that holds a .wixproj; the
        /// file's own folder if none does.
        /// </summary>
        public static string ProjectRoot(string documentPath)
        {
            var own = Path.GetDirectoryName(Path.GetFullPath(documentPath)) ?? string.Empty;
            for (var folder = own; !string.IsNullOrEmpty(folder); folder = Path.GetDirectoryName(folder))
            {
                try
                {
                    if (Directory.EnumerateFiles(folder, "*.wixproj").Any())
                    {
                        return folder;
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // A folder that can't be read has no project in it as far as can be told: keep going up.
                }
            }

            return own;
        }

        /// <summary>Gets the .wxs files of the project, in a stable order.</summary>
        public static List<string> ProjectFiles(string documentPath)
        {
            var files = new List<string>();
            var root = ProjectRoot(documentPath);
            // A file with no project above it stands with the files in its folder, not everything below it.
            var hasProject = Directory.Exists(root) && SafeEnumerateFiles(root, "*.wixproj").Any();
            Collect(root, hasProject, files);
            files.Sort(StringComparer.OrdinalIgnoreCase);
            return files;
        }

        static void Collect(string folder, bool recurse, List<string> files)
        {
            files.AddRange(SafeEnumerateFiles(folder, "*.wxs"));
            if (!recurse)
            {
                return;
            }

            IEnumerable<string> subfolders;
            try
            {
                subfolders = Directory.EnumerateDirectories(folder).ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return;
            }

            foreach (var subfolder in subfolders)
            {
                var name = Path.GetFileName(subfolder);
                if (name.StartsWith(".", StringComparison.Ordinal) || SkippedFolders.Contains(name))
                {
                    continue;
                }

                try
                {
                    // Not through a link that could lead back up.
                    if ((File.GetAttributes(subfolder) & FileAttributes.ReparsePoint) != 0)
                    {
                        continue;
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    continue;
                }

                Collect(subfolder, true, files);
            }
        }

        static IEnumerable<string> SafeEnumerateFiles(string folder, string pattern)
        {
            try
            {
                return Directory.EnumerateFiles(folder, pattern).ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>Gets the Ids of the component groups an open document defines, as it is in memory.</summary>
        public static List<string> Read(XDocument document) =>
            document.Descendants()
                .Where(e => e.Name.LocalName == "ComponentGroup")
                .Select(e => (string?)e.Attribute("Id"))
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => id!)
                .ToList();

        /// <summary>
        /// Gets the Ids of the component groups a file on disk defines; none if it can't be read or isn't well-formed
        /// XML, as a file being edited often isn't. The result is kept until the file changes.
        /// </summary>
        public static List<string> Scan(string filePath)
        {
            FileInfo info;
            try
            {
                info = new FileInfo(filePath);
                if (!info.Exists)
                {
                    return new List<string>();
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                return new List<string>();
            }

            if (Cache.TryGetValue(filePath, out var cached) && cached.LastWriteUtc == info.LastWriteTimeUtc && cached.Length == info.Length)
            {
                return new List<string>(cached.Ids);
            }

            var ids = new List<string>();
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null, IgnoreComments = true };
                using (var reader = XmlReader.Create(filePath, settings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "ComponentGroup")
                        {
                            var id = reader.GetAttribute("Id");
                            if (!string.IsNullOrEmpty(id))
                            {
                                ids.Add(id!);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is XmlException || ex is IOException || ex is UnauthorizedAccessException)
            {
                // Whatever was read before the error stands: a file half-edited still has its other groups.
            }

            Cache[filePath] = new CachedFile { LastWriteUtc = info.LastWriteTimeUtc, Length = info.Length, Ids = ids };
            return new List<string>(ids);
        }

        /// <summary>
        /// Gets the component groups the project's other files define, from disk. This reads files, so a caller that
        /// shouldn't wait for it runs it away from the user interface.
        /// </summary>
        /// <param name="documentPath">The file being edited, which is left out: its groups come from memory.</param>
        public static List<ComponentGroupInfo> FindInOtherFiles(string documentPath)
        {
            var own = Path.GetFullPath(documentPath);
            var groups = new List<ComponentGroupInfo>();
            foreach (var file in ProjectFiles(documentPath))
            {
                if (string.Equals(Path.GetFullPath(file), own, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                groups.AddRange(Scan(file).Select(id => new ComponentGroupInfo(id, file)));
            }

            return groups;
        }

        /// <summary>Gets the component groups the open document defines, from memory.</summary>
        public static List<ComponentGroupInfo> FindInDocument(XDocument document, string documentPath) =>
            Read(document).Select(id => new ComponentGroupInfo(id, documentPath)).ToList();

        /// <summary>
        /// Gets the file that defines the component group, the file being edited first (from <paramref name="document"/>,
        /// as it is in memory), or null if no file of the project does.
        /// </summary>
        public static string? FindDefinition(string documentPath, XDocument document, string id)
        {
            if (Read(document).Contains(id))
            {
                return documentPath;
            }

            return FindInOtherFiles(documentPath).FirstOrDefault(g => g.Id == id)?.FilePath;
        }
    }
}
