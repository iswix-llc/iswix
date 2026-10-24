using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Authoring;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace ShortCutsDesigner
{
    // The shortcuts in a fragment, read and written through the typed model. The designer's folders are not
    // elements: they are each shortcut's Directory and Subdirectory.
    internal static class ShortcutDocument
    {
        public static List<Wxs.Shortcut> Load(XDocument document) =>
            document.Descendants(Wxs.Shortcut.ElementName).Select(e => (Wxs.Shortcut)WixElement.Wrap(e)).ToList();

        /// <summary>Gets whether the document is one this designer edits: a WiX fragment.</summary>
        public static bool IsValidDocument(XDocument document)
        {
            var root = document?.Root;
            return root?.Name == Wxs.Wix.ElementName && root.Elements().FirstOrDefault()?.Name == Wxs.Fragment.ElementName;
        }

        // Absent reads as empty, as the folder comparisons expect.
        public static string Subdirectory(Wxs.Shortcut shortcut) => shortcut.Subdirectory ?? string.Empty;

        public static List<ShortCutCandidate> GetShortCutCandidates(XDocument document)
        {
            List<ShortCutCandidate> candidates = new List<ShortCutCandidate>();
            foreach (var file in document.Descendants(Wxs.File.ElementName))
            {
                string source = (string)file.Attribute("Source") ?? string.Empty;
                if (source.EndsWith(".dll") || source.EndsWith(".pdb"))
                {
                    continue;
                }

                ShortCutCandidate scc = new ShortCutCandidate();
                scc.FileKey = (string)file.Attribute("Id") ?? string.Empty;
                scc.DestinationFilePath = WixFiles.DestinationFilePath(file);
                scc.FileName = Path.GetFileNameWithoutExtension(source.Split('\\').Last());
                scc.FileElement = file;
                candidates.Add(scc);
            }
            return candidates;
        }

        public static Wxs.Shortcut Create(string name, string directory, string subdirectory, XElement fileElement)
        {
            var file = (Wxs.File)WixElement.Wrap(fileElement);
            return file.Add(new Wxs.Shortcut
            {
                Id = GeneratedId(directory, subdirectory, name),
                Name = name,
                Directory = directory,
                Subdirectory = string.IsNullOrEmpty(subdirectory) ? null : subdirectory,
                // The designer only edits fragments, whose shortcuts are advertised.
                Advertise = true,
            });
        }

        public static void Delete(Wxs.Shortcut shortcut) => shortcut.RemoveFromParent();

        // The Id follows the shortcut's folder and name, unless it was given another Id by hand.
        public static void SetName(Wxs.Shortcut shortcut, string name) =>
            Move(shortcut, shortcut.Directory, Subdirectory(shortcut), name);

        public static void SetSubdirectory(Wxs.Shortcut shortcut, string subdirectory) =>
            Move(shortcut, shortcut.Directory, subdirectory, shortcut.Name);

        public static void SetDirectory(Wxs.Shortcut shortcut, string directory) =>
            Move(shortcut, directory, Subdirectory(shortcut), shortcut.Name);

        static void Move(Wxs.Shortcut shortcut, string directory, string subdirectory, string name)
        {
            bool generatedId = shortcut.Id == GeneratedId(shortcut.Directory, Subdirectory(shortcut), shortcut.Name) ||
                // Earlier IsWiX versions renamed with this form.
                shortcut.Id == "sc" + WixFiles.IdHash(shortcut.Directory + shortcut.Name);

            if (generatedId)
            {
                shortcut.Id = GeneratedId(directory, subdirectory, name);
            }

            if (shortcut.Name != name)
            {
                shortcut.Name = name;
            }

            if (shortcut.Directory != directory)
            {
                shortcut.Directory = directory;
            }

            if (Subdirectory(shortcut) != (subdirectory ?? string.Empty))
            {
                shortcut.Subdirectory = string.IsNullOrEmpty(subdirectory) ? null : subdirectory;
            }
        }

        static string GeneratedId(string directory, string subdirectory, string name) =>
            "sc" + WixFiles.IdHash(Path.Combine(directory ?? string.Empty, subdirectory ?? string.Empty, name ?? string.Empty));

        public static string DestinationFilePath(Wxs.Shortcut shortcut) =>
            shortcut.Xml.Parent?.Name == Wxs.File.ElementName ? WixFiles.DestinationFilePath(shortcut.Xml.Parent) : string.Empty;
    }

    public class ShortCutCandidate
    {
        public string FileKey { get; set; }
        public string FileName { get; set; }
        public string DestinationFilePath { get; set; }
        public XElement FileElement { get; set; }
    }
}
