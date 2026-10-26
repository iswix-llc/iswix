using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using FireworksFramework.Managers;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace FilesAndFoldersDesigner
{
    public enum ComponentRules { OneToOne, OneToMany };

    public class FileMeta
    {
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
    }

    // "Destination Computer\[INSTALLFOLDER]\bin\x64" is Directory INSTALLFOLDER, Subdirectory bin\x64.
    public class DirectoryMeta
    {
        public DirectoryMeta(string filePath)
        {
            string directory = filePath.Replace(@"Destination Computer\", "");
            string[] parts = directory.Split(new char[] { '\\' });
            Directory = parts[0].Replace("[", "").Replace("]", "");
            Subdirectory = string.Empty;
            for (int i = 1; i < parts.Length; i++)
            {
                Subdirectory = Path.Combine(Subdirectory, parts[i]);
            }
        }
        public string Directory { get; set; }
        public string Subdirectory { get; set; }
    }

    /// <summary>
    /// The fragment's component group (the one named after the file), read and written through the typed model.
    /// Each folder is the components with that Directory and Subdirectory. Components and files are kept in IsWiX's
    /// canonical order.
    /// </summary>
    public class ComponentGroupDocument
    {
        const string SourceDirVar = "$(var.SourceDir)";
        const string SourceDirDefine = "SourceDir";
        const string ComponentRulesDefine = "ComponentRules";

        readonly DocumentManager _documentManager;
        XDocument _document;
        Wxs.Wix _wix;
        Wxs.ComponentGroup? _componentGroup;
        string _fileName;

        public ComponentGroupDocument(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            _document = _documentManager.Document;
            _wix = Wxs.WixDocument.FromXml(_document).Root;
            _fileName = Path.GetFileNameWithoutExtension(_documentManager.DocumentPath);
            _componentGroup = _wix.Fragments.SelectMany(f => f.ComponentGroups)
                .FirstOrDefault(g => string.Equals(g.Id, _fileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Gets whether the document is one this designer edits: a WiX fragment.</summary>
        public static bool IsValidDocument(XDocument document)
        {
            var root = document?.Root;
            return root?.Name == Wxs.Wix.ElementName && root.Elements().FirstOrDefault()?.Name == Wxs.Fragment.ElementName;
        }

        IEnumerable<Wxs.Component> Components => _componentGroup?.Components ?? Enumerable.Empty<Wxs.Component>();

        static string Subdirectory(Wxs.Component component) => component.Subdirectory ?? string.Empty;

        static bool IsKeyPath(WixValue<bool>? keyPath) => keyPath is { IsLiteral: true } literal && literal.Value;

        public string GetNextSubDirectoryName(string directoryPath)
        {
            var newFolderName = "New Folder";
            DirectoryMeta directoryMeta = new DirectoryMeta(directoryPath);

            int count = Components.Count(c => c.Directory == directoryMeta.Directory &&
                Subdirectory(c).StartsWith(Path.Combine(directoryMeta.Subdirectory, newFolderName)));
            if (count > 0)
            {
                newFolderName += String.Format(" ({0})", count);
            }
            return newFolderName;
        }

        // A folder's own component: it has no files of its own (a CreateFolder) or holds the folder's non-PE files.
        public Wxs.Component GetOrCreateDirectoryComponent(string directoryPath, bool peFile = true)
        {
            EnsureComponentGroup();

            Wxs.Component? directoryComponent;
            DirectoryMeta directoryMeta = new DirectoryMeta(directoryPath);

            if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                directoryComponent = peFile
                    ? Components.FirstOrDefault(c => c.Directory == directoryMeta.Directory)
                    : Components.FirstOrDefault(c => c.Directory == directoryMeta.Directory && IsKeyPath(c.KeyPath));
            }
            else
            {
                directoryComponent = Components.FirstOrDefault(c => c.Directory == directoryMeta.Directory &&
                    Subdirectory(c) == directoryMeta.Subdirectory && IsKeyPath(c.KeyPath));
            }

            if (directoryComponent == null)
            {
                directoryComponent = new Wxs.Component
                {
                    Id = DirectoryComponentId(directoryMeta.Directory, directoryMeta.Subdirectory),
                    Directory = directoryMeta.Directory,
                    Subdirectory = string.IsNullOrEmpty(directoryMeta.Subdirectory) ? null : directoryMeta.Subdirectory,
                    Guid = Guid.NewGuid().ToString(),
                    KeyPath = true,
                };
                directoryComponent.Add(new Wxs.CreateFolder());
                // Added through the model, so it lands at its canonical position.
                _componentGroup.Add(directoryComponent);
            }
            return directoryComponent;
        }

        // A file's fragment and component group are created when the first file or folder is added.
        [MemberNotNull(nameof(_componentGroup))]
        void EnsureComponentGroup()
        {
            if (_componentGroup != null)
            {
                return;
            }

            var fragment = _wix.Fragments.FirstOrDefault() ?? _wix.Add(new Wxs.Fragment());
            _componentGroup = fragment.Add(new Wxs.ComponentGroup { Id = _fileName });
        }

        public string GetRootPath()
        {
            string sourceDir = GetDefine(SourceDirDefine) ?? ".";
            return new DirectoryInfo(Path.Combine(new FileInfo(_documentManager.DocumentPath).DirectoryName ?? string.Empty, sourceDir)).FullName;
        }

        public ComponentRules GetComponentRules() =>
            GetDefine(ComponentRulesDefine) == nameof(ComponentRules.OneToMany) ? ComponentRules.OneToMany : ComponentRules.OneToOne;

        public void SetComponentRules(ComponentRules componentRules) => SetDefine(ComponentRulesDefine, componentRules.ToString());

        public List<string> GetDirectories()
        {
            List<string> dirs = Components.Select(c => Path.Combine(c.Directory ?? string.Empty, Subdirectory(c))).ToList();
            dirs.Sort();
            return dirs;
        }

        // The folder's files: their Source and whether each is its component's key path.
        public List<Tuple<string, bool>> GetFiles(string path)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(path);

            return Components
                .Where(c => c.Directory == directoryMeta.Directory && Subdirectory(c) == directoryMeta.Subdirectory)
                .SelectMany(c => c.Xml.Descendants(Wxs.File.ElementName))
                .Select(f => (Wxs.File)WixElement.Wrap(f))
                .Select(f => new Tuple<string, bool>(f.Source ?? string.Empty, IsKeyPath(f.KeyPath)))
                .ToList();
        }

        /// <summary>Gets the component group's element, or null while the document has none.</summary>
        public XElement? ComponentGroupElement => _componentGroup?.Xml;

        /// <summary>
        /// Gets the first component of the folder, or if it has none of its own (it is only on the way to a folder that
        /// has), the first component below it. Null if there is none.
        /// </summary>
        public XElement? FindFolderElement(string path)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(path);
            var inDirectory = Components.Where(c => c.Directory == directoryMeta.Directory).ToList();
            var component = inDirectory.FirstOrDefault(c => Subdirectory(c) == directoryMeta.Subdirectory) ??
                inDirectory.FirstOrDefault(c => directoryMeta.Subdirectory.Length == 0 || Subdirectory(c).StartsWith(directoryMeta.Subdirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal));
            return component?.Xml;
        }

        /// <summary>Gets the File element in the folder whose Source is <paramref name="source"/>, or null if there is none.</summary>
        public XElement? FindFileElement(string path, string source)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(path);
            return Components
                .Where(c => c.Directory == directoryMeta.Directory && Subdirectory(c) == directoryMeta.Subdirectory)
                .SelectMany(c => c.Xml.Descendants(Wxs.File.ElementName))
                .FirstOrDefault(f => (string?)f.Attribute("Source") == source);
        }

        /// <summary>Gets whether <paramref name="component"/> is one of the component group's.</summary>
        public static bool IsInComponentGroup(XElement component, string documentPath) =>
            component.Parent is { } parent && parent.Name == Wxs.ComponentGroup.ElementName &&
            string.Equals((string?)parent.Attribute("Id"), Path.GetFileNameWithoutExtension(documentPath), StringComparison.OrdinalIgnoreCase);

        public void DeleteFiles(List<FileMeta> files)
        {
            foreach (var file in files)
            {
                foreach (var fileElement in FindFiles(file).ToList())
                {
                    var component = (Wxs.Component)fileElement.Parent!;
                    fileElement.RemoveFromParent();
                    if (!component.Xml.HasElements)
                    {
                        component.RemoveFromParent();
                    }
                }
                UnpruneDirectory(file.Destination);
            }
        }

        public void AddFiles(List<FileMeta> files)
        {
            string rootDir = GetRootPath();
            if (files.Count > 0)
            {
                // Sources are written relative to $(var.SourceDir), so it must be defined.
                EnsureDefine(SourceDirDefine, ".");
            }

            // The files already in the group, looked up by folder and source. Looking each new file up with FindFiles
            // scans every component in the group, once per file, so a large drop was slow.
            var existing = ExistingFileKeys();
            // Only a folder with an empty component can be pruned, so only those are checked, rather than every
            // component in the group for every file.
            var emptyFolders = EmptyFolderKeys();
            foreach (var file in files)
            {
                DirectoryMeta directoryMeta = new DirectoryMeta(file.Destination);
                file.Destination = Path.Combine(directoryMeta.Directory, directoryMeta.Subdirectory);
                file.Source = file.Source.Replace(rootDir, SourceDirVar);
                if (file.Source == ".")
                {
                    GetOrCreateDirectoryComponent(file.Destination);
                    emptyFolders.Add(FolderKey(file));
                }
                else if (!existing.Contains(FileKey(file)))
                {
                    EnsureComponentGroup();
                    if (IsProgramExecutable(file.Source))
                    {
                        // Its own component, built before it is added so it lands at its canonical position.
                        var component = new Wxs.Component
                        {
                            Directory = directoryMeta.Directory,
                            Subdirectory = string.IsNullOrEmpty(directoryMeta.Subdirectory) ? null : directoryMeta.Subdirectory,
                        };
                        component.Add(new Wxs.File { Source = file.Source, KeyPath = true });
                        _componentGroup.Add(component);
                    }
                    else
                    {
                        var directoryComponent = GetOrCreateDirectoryComponent(file.Destination, false);
                        foreach (var createFolder in directoryComponent.Children.OfType<Wxs.CreateFolder>().ToList())
                        {
                            createFolder.RemoveFromParent();
                        }
                        directoryComponent.Add(new Wxs.File { Source = file.Source });
                    }
                    existing.Add(FileKey(file));
                }
                if (emptyFolders.Contains(FolderKey(file)))
                {
                    PruneDirectory(file.Destination);
                }
            }
        }

        // The folders that have an empty component (a CreateFolder alone), as FolderKey. Adding a file can only make
        // a new one with a "." entry, which AddFiles adds to the set itself.
        HashSet<string> EmptyFolderKeys()
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var component in Components)
            {
                if (component.Directory is string directory &&
                    component.Xml.Descendants(Wxs.CreateFolder.ElementName).Any() &&
                    component.Xml.Elements().Count() == 1)
                {
                    keys.Add(FolderKey(directory, Subdirectory(component)));
                }
            }
            return keys;
        }

        // A file's folder, as FolderKey, which PruneDirectory works on.
        static string FolderKey(FileMeta file)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(file.Destination);
            return FolderKey(directoryMeta.Directory, directoryMeta.Subdirectory);
        }

        static string FolderKey(string directory, string subdirectory) => directory + "\n" + subdirectory;

        // Every file in the group, as FileKey, for AddFiles to check against.
        HashSet<string> ExistingFileKeys()
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var component in Components)
            {
                if (component.Directory is not string directory)
                {
                    continue;
                }
                foreach (var file in component.Files)
                {
                    if (file.Source is string source)
                    {
                        keys.Add(FileKey(directory, Subdirectory(component), source));
                    }
                }
            }
            return keys;
        }

        // A file's folder and source, which are what make it the same file as one already in the group.
        static string FileKey(FileMeta file)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(file.Destination);
            return FileKey(directoryMeta.Directory, directoryMeta.Subdirectory, file.Source);
        }

        static string FileKey(string directory, string subdirectory, string source) => directory + "\n" + subdirectory + "\n" + source;

        IEnumerable<Wxs.File> FindFiles(FileMeta file)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(file.Destination);
            return Components
                .Where(c => c.Directory == directoryMeta.Directory && Subdirectory(c) == directoryMeta.Subdirectory)
                .SelectMany(c => c.Files)
                .Where(f => f.Source == file.Source);
        }

        private bool IsProgramExecutable(string fileName)
        {
            string name = fileName.ToLower();
            return
                name.EndsWith(".dll") ||
                name.EndsWith(".exe") ||
                name.EndsWith(".ocx") ||
                name.EndsWith(".chm") ||
                name.EndsWith(".hlp") ||
                GetComponentRules() == ComponentRules.OneToOne;
        }

        static string DirectoryComponentId(string directory, string subdirectory) => "owd" + GetSHA256Hash(Path.Combine(directory, subdirectory ?? string.Empty));

        static string GetSHA256Hash(string input)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] data = sha256.ComputeHash(Encoding.Default.GetBytes(input.ToUpper()));
            return string.Concat(data.Select(b => b.ToString("X2")));
        }

        public void DeleteDirectory(string directoryPath, string parentPath)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(directoryPath);
            var components = string.IsNullOrEmpty(directoryMeta.Subdirectory)
                ? Components.Where(c => c.Directory == directoryMeta.Directory)
                : Components.Where(c => c.Directory == directoryMeta.Directory && IsInFolder(c, directoryMeta.Subdirectory));

            foreach (var component in components.ToList())
            {
                component.RemoveFromParent();
            }
            UnpruneDirectory(parentPath);
        }

        static bool IsInFolder(Wxs.Component component, string subdirectory) =>
            Subdirectory(component) == subdirectory || Subdirectory(component).StartsWith(subdirectory + @"\");

        // Moves the folder's components (and its subfolders') to the new name. A folder component's Id follows its
        // path unless it was given another Id by hand.
        public bool RenameDirectory(string directoryPath, string newName)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(directoryPath);
            string directory = directoryMeta.Directory;
            string oldSubDirectory = directoryMeta.Subdirectory;

            string[] parts = directoryMeta.Subdirectory.Split('\\');
            parts[parts.Length - 1] = newName;

            string newSubDirectory = string.Join(@"\", parts);

            if (DirectoryExists(directory, newSubDirectory))
            {
                return false;
            }

            foreach (var component in Components.Where(c => c.Directory == directory && IsInFolder(c, oldSubDirectory)).ToList())
            {
                string oldValue = Subdirectory(component);
                string newValue = newSubDirectory + oldValue.Substring(oldSubDirectory.Length);
                if (IsGeneratedId(component.Id, directory, oldValue))
                {
                    component.Id = DirectoryComponentId(directory, newValue);
                }
                component.Subdirectory = newValue;
            }
            return true;
        }

        // Folder components are "owd" + the hash of their path; renames in earlier IsWiX versions wrote "owc".
        static bool IsGeneratedId(string? id, string directory, string subdirectory)
        {
            string hash = GetSHA256Hash(Path.Combine(directory, subdirectory));
            return id == "owd" + hash || id == "owc" + hash;
        }

        private bool DirectoryExists(string directory, string subdirectory) =>
            Components.Any(c => c.Directory == directory && IsInFolder(c, subdirectory));

        // A folder's empty (CreateFolder only) component is no longer needed once the folder has something else.
        public void PruneDirectory(string directoryPath)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(directoryPath);
            int componentCount = Components.Count(c => c.Directory == directoryMeta.Directory && Subdirectory(c).StartsWith(directoryMeta.Subdirectory));

            if (componentCount > 1)
            {
                var component = Components.FirstOrDefault(c =>
                    c.Directory == directoryMeta.Directory &&
                    Subdirectory(c) == directoryMeta.Subdirectory &&
                    c.Xml.Descendants(Wxs.CreateFolder.ElementName).Any() &&
                    c.Xml.Elements().Count() == 1);
                component?.RemoveFromParent();
            }
        }

        // A folder left with nothing gets its empty component back, so the folder stays.
        private void UnpruneDirectory(string directoryPath)
        {
            DirectoryMeta directoryMeta = new DirectoryMeta(directoryPath);
            int componentCount = Components.Count(c => c.Directory == directoryMeta.Directory && Subdirectory(c).StartsWith(directoryMeta.Subdirectory));
            if (componentCount < 1 && directoryPath != "Destination Computer")
            {
                GetOrCreateDirectoryComponent(directoryPath);
            }
        }

        // Puts components, and the files in each, back in canonical order after edits changed what they sort by.
        public void SortComponents()
        {
            var ordering = WixOrdering.Get(_document);
            if (ordering == null || _componentGroup == null)
            {
                return;
            }

            foreach (var component in Components.ToList())
            {
                ordering.Canonicalize(component.Xml);
            }
            ordering.Canonicalize(_componentGroup.Xml);
        }

        // The SourceDir and ComponentRules <?define?>s IsWiX keeps at the top of the file. Changing one rewrites
        // only its value; a new one goes after the existing defines, on its own line.
        WixDefines Defines => new WixDefines(_document);

        string? GetDefine(string name) => Defines[name];

        void SetDefine(string name, string value) => Defines[name] = value;

        void EnsureDefine(string name, string value)
        {
            if (Defines.Find(name) == null)
            {
                Defines[name] = value;
            }
        }
    }
}
