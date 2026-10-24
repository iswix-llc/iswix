using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Wxs = IsWiX.WiXModel;

namespace FireworksFramework.Authoring
{
    /// <summary>Helpers the designers share for the files in a WiX document.</summary>
    public static class WixFiles
    {
        /// <summary>
        /// The Id hash IsWiX generates Ids from: the uppercase hex MD5 of the uppercased text, so the same name always
        /// gives the same Id.
        /// </summary>
        public static string IdHash(string input)
        {
            using MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes(input.ToUpper()));
            return string.Concat(data.Select(b => b.ToString("X2")));
        }

        /// <summary>Where a <c>File</c> element is installed, as <c>[Directory]Subdirectory\file</c>.</summary>
        public static string DestinationFilePath(XElement file)
        {
            string fileName = ((string?)file.Attribute("Source") ?? (string?)file.Attribute("Name") ?? string.Empty).Split('\\').Last();
            if (!string.IsNullOrEmpty((string?)file.Attribute("Id")))
            {
                // Files with an Id are placed by their Directory ancestors.
                string filePath = fileName;
                foreach (var ancestor in file.Ancestors(Wxs.Directory.ElementName))
                {
                    filePath = ParentDirectoryName(ancestor) + filePath;
                }

                int firstSlashLocation = filePath.IndexOf('\\');
                return firstSlashLocation < 0 ? filePath : ("[" + filePath.Insert(firstSlashLocation, "]")).Replace("]\\", "]");
            }

            // Otherwise by their component's Directory and Subdirectory (the directory may come from the group).
            var component = file.Parent;
            string directory = (string?)component?.Attribute("Directory") ??
                (string?)component?.Parent?.Attribute("Directory") ?? string.Empty;
            return "[" + directory + "]" + Path.Combine((string?)component?.Attribute("Subdirectory") ?? string.Empty, fileName);
        }

        static string ParentDirectoryName(XElement element)
        {
            string? id = (string?)element.Attribute("Id");
            string? name = (string?)element.Attribute("Name");

            if (!string.IsNullOrEmpty(name) && !name.Equals("SourceDir", StringComparison.InvariantCultureIgnoreCase))
            {
                return name + @"\";
            }

            if (!string.IsNullOrEmpty(id) && !id.Equals("TARGETDIR", StringComparison.InvariantCultureIgnoreCase))
            {
                return id + @"\";
            }

            return string.Empty;
        }
    }
}
