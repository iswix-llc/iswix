using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{

    public class IsWiXComponentGroup
    {
        XNamespace _ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        XElement _fragment;
        XElement _componentGroup;
        string _fileName;
        private const string SourceDirVar = "$(var.SourceDir)";
        public IsWiXComponentGroup()
        {
            _ns = _documentManager.Document.GetWiXNameSpace();
            _fileName = Path.GetFileNameWithoutExtension(_documentManager.DocumentPath);

            bool foundFragment = _documentManager.Document.Descendants(_ns + "Fragment").Where(e => e.GetOptionalAttribute("Id").ToLower() == _fileName.ToLower()).Any();
            if (foundFragment)
            {
                _fragment = _documentManager.Document.Descendants(_ns + "Fragment").Where(e => e.GetOptionalAttribute("Id") == _fileName).First();
            }

            bool foundComponentGroup = _documentManager.Document.Descendants(_ns + "ComponentGroup").Where(e => e.GetOptionalAttribute("Id").ToLower() == _fileName.ToLower()).Any();
            if (foundComponentGroup)
            {
                _componentGroup = _documentManager.Document.Descendants(_ns + "ComponentGroup").Where(e => e.GetOptionalAttribute("Id") == _fileName).First();
            }
            EstablishDefines();
        }

        private DirectoryMeta SplitDirectory(string directory)
        {
            directory = directory.Replace(@"Destination Computer\", "");

            DirectoryMeta directoryMeta = new DirectoryMeta();

            string[] parts = directory.Split(new char[] { '\\' });
            directoryMeta.Directory = parts[0].Replace("[", "").Replace("]", "");
            directoryMeta.Subdirectory = string.Empty;
            for (int i = 1; i < parts.Length; i++)
            {
                directoryMeta.Subdirectory = Path.Combine(directoryMeta.Subdirectory, parts[i]);
            }
            return directoryMeta;
        }

        public string GetNextSubDirectoryName(string directoryPath)
        {
            var newFolderName = "New Folder";
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);

            List<string> dirs = new List<string>();

            var test = from d in _componentGroup.Descendants(_ns + "Component")
                       where d.Attribute("Directory").Value == directoryMeta.Directory &&
                       d.GetOptionalAttribute("Subdirectory").StartsWith(Path.Combine(directoryMeta.Subdirectory, newFolderName))
                       select d;
            if (test.Count() > 0)
            {
                newFolderName += String.Format(" ({0})", test.Count());
            }
            return newFolderName;
        }
        public XElement GetOrCreateDirectoryComponent(string directoryPath)
        {
            if(_fragment == null)
            {
                _fragment = new XElement(_ns + "Fragment", new XAttribute("Id", _fileName));
                _documentManager.Document.Root.Add(_fragment); ;
            }
            if(_componentGroup == null)
            {
                _componentGroup = new XElement(_ns + "ComponentGroup", new XAttribute("Id", _fileName));
                _fragment.Add(_componentGroup);
            }

            XElement directoryComponentElement;
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);

            if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                directoryComponentElement = _componentGroup.Descendants(_ns + "Component").Where(
                c => c.Attribute("Directory").Value == directoryMeta.Directory).FirstOrDefault();
            }
            else
            {
                directoryComponentElement = _componentGroup.Descendants(_ns + "Component").Where(
                c => c.Attribute("Directory").Value == directoryMeta.Directory &&
                (c.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory)).FirstOrDefault();
            }

            if (directoryComponentElement == null)
            {
                string id = "owd" + GetSHA256Hash(Path.Combine(directoryMeta.Directory, directoryMeta.Subdirectory));
                if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
                {
                    directoryComponentElement =
                        new XElement(_ns + "Component",
                            new XAttribute("Id", id),
                            new XAttribute("Directory", directoryMeta.Directory),
                            new XAttribute("Guid", Guid.NewGuid().ToString()),
                            new XAttribute("KeyPath", "true"),
                            new XElement(_ns + "CreateFolder"));
                }
                else
                {
                    directoryComponentElement =
                        new XElement(_ns + "Component",
                            new XAttribute("Id", id),
                            new XAttribute("Directory", directoryMeta.Directory),
                            new XAttribute("Subdirectory", directoryMeta.Subdirectory),
                            new XAttribute("Guid", Guid.NewGuid().ToString()),
                            new XAttribute("KeyPath", "true"),
                            new XElement(_ns + "CreateFolder"));
                }
                _componentGroup.Add(directoryComponentElement);
            }
            return directoryComponentElement;
        }

        private void EstablishDefines()
        {
            bool sourceDirXpiExists = false;
            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("SourceDir"))
                    {
                        sourceDirXpiExists = true;
                        break;
                    }
                }
            }
            if (!sourceDirXpiExists)
            {
                _documentManager.Document.Descendants(_ns + "Wix").First().AddFirst(new XProcessingInstruction("define", "SourceDir=\".\""));
            }

            bool componentRulesExist = false;
            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("ComponentRules"))
                    {
                        componentRulesExist = true;
                        break;
                    }
                }
            }
            if (!componentRulesExist)
            {
                _documentManager.Document.Descendants(_ns + "Wix").First().AddFirst(new XProcessingInstruction("define", "ComponentRules=\"OneToOne\""));
            }
        }

        public string GetRootPath()
        {
            string sourceDirValue = string.Empty;
            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("SourceDir"))
                    {
                        sourceDirValue = Path.Combine(new FileInfo(_documentManager.DocumentPath).Directory.FullName, fields[1].Replace("\"", "").Trim());
                        break;
                    }
                }
            }
            return new DirectoryInfo(sourceDirValue).FullName;
        }

        public ComponentRules GetComponentRules()
        {
            ComponentRules componentRules = ComponentRules.OneToOne;
            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));
                    if (fields[0].Trim().Equals("ComponentRules"))
                    {
                        if (fields[1].Replace("\"", "").Trim() == "OneToMany")
                        {
                            componentRules = ComponentRules.OneToMany;
                        }
                    }
                }
            }
            return componentRules;
        }
        public void SetComponentRules(ComponentRules componentRules)
        {
            bool componentRulesExists = false;

            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("ComponentRules"))
                    {
                        componentRulesExists = true;
                        xpi.Data = $"ComponentRules=\"{componentRules}\"";
                    }
                }
            }

            if (!componentRulesExists)
            {
                _documentManager.Document.Descendants(_ns + "Wix").First().AddFirst(
                    new XProcessingInstruction("define", $"ComponentRules=\"{componentRules}\""));
            }
        }
        public static bool IsValidDocument()
        {
            DocumentManager documentManager = DocumentManager.DocumentManagerInstance;
            return (documentManager.Document.GetWiXVersion() == WiXVersion.v4 && documentManager.Document.GetDocumentType() == IsWiXDocumentType.Fragment);
        }
        public List<string> GetDirectories()
        {
            List<string> dirs = new List<string>();

            if (_componentGroup != null)
            {
                foreach (var componentElement in _componentGroup.Elements(_ns + "Component"))
                {
                    dirs.Add(Path.Combine(componentElement.GetOptionalAttribute("Directory"), componentElement.GetOptionalAttribute("Subdirectory")));
                }
                dirs.Sort();
            }
            return dirs;
        }
        public List<Tuple<string, bool>> GetFiles(string path)
        {
            DirectoryMeta directoryMeta = SplitDirectory(path);
            directoryMeta.Directory = directoryMeta.Directory.Replace("[", "").Replace("]", "");

            List<Tuple<string, bool>> files = new List<Tuple<string, bool>>();

            List<XElement> componentElements;
            if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                componentElements = _componentGroup.Elements(_ns + "Component").Where(e => e.GetOptionalAttribute("Directory") == directoryMeta.Directory && String.IsNullOrEmpty(e.GetOptionalAttribute("Subdirectory"))).ToList();
            }
            else
            {
                componentElements = _componentGroup.Elements(_ns + "Component").Where(e => e.GetOptionalAttribute("Directory") == directoryMeta.Directory && e.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory).ToList();
            }

            foreach (var componentElement in componentElements)
            {
                foreach (var file in componentElement.Descendants(_ns + "File").ToList())
                {
                    files.Add(new Tuple<string, bool>(file.Attribute("Source").Value, file.GetOptionalYesNoAttribute("KeyPath", false)));
                }
            }
            return files;
        }
        public void DeleteFiles(List<FileMeta> files)
        {
            foreach (var file in files)
            {
                DirectoryMeta directoryMeta = SplitDirectory(file.Destination);
                if (FileExists(file))
                {
                    if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
                    {
                        var fileElements = from f in _componentGroup.Descendants(_ns + "File")
                                           where f.Attribute("Source").Value == file.Source &&
                                                f.Parent.Attribute("Directory").Value == directoryMeta.Directory
                                           select f;
                        var listElements = fileElements.ToList();
                        foreach (var fileElement in listElements)
                        {
                            XElement componentElement = fileElement.Parent;
                            fileElement.Remove();
                            if (!componentElement.HasElements)
                            {
                                componentElement.Remove();
                            }
                        }
                    }
                    else
                    {
                        var fileElements = from f in _componentGroup.Descendants(_ns + "File")
                                           where f.Attribute("Source").Value == file.Source &&
                                                f.Parent.Attribute("Directory").Value == directoryMeta.Directory &&
                                                f.Parent.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory
                                           select f;
                        List<XElement> test = fileElements.ToList();
                        foreach (var fileElement in test)
                        {
                            XElement componentElement = fileElement.Parent;
                            fileElement.Remove();
                            if(!componentElement.HasElements)
                            {
                                componentElement.Remove();
                            }
                        }

                    }
                }
                UnpruneDirectory(file.Destination);
            }
            //var elements = _componentGroup.Descendants(_ns + "Component").Where(c => !c.Elements().Any()).ToList();
            //foreach (var element in elements)
            //{
            //    if (string.IsNullOrEmpty(element.GetOptionalAttribute("Id")))
            //    {
            //        element.Remove();
            //    }
            //    else
            //    {
            //        element.Add(new XElement(_ns + "CreateFolder"));
            //    }
            //}
        }
        public void AddFiles(List<FileMeta> files)
        {
            string rootDir = GetRootPath();

            foreach (var file in files)
            {
                DirectoryMeta directoryMeta = SplitDirectory(file.Destination);
                file.Destination = Path.Combine(directoryMeta.Directory, directoryMeta.Subdirectory);
                file.Source = file.Source.Replace(rootDir, "$(var.SourceDir)");
                if (!FileExists(file))
                {
                    if (IsProgramExecutable(file.Source))
                    {
                        if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
                        {
                            _componentGroup.Add(new XElement(_ns + "Component", new XAttribute("Directory", directoryMeta.Directory),
                            new XElement(_ns + "File", new XAttribute("Source", file.Source), new XAttribute("KeyPath", "yes"))));
                        }
                        else
                        {
                            _componentGroup.Add(new XElement(_ns + "Component",
                                new XAttribute("Directory", directoryMeta.Directory),
                                new XAttribute("Subdirectory", directoryMeta.Subdirectory),
                            new XElement(_ns + "File", new XAttribute("Source", file.Source), new XAttribute("KeyPath", "yes"))));
                        }
                    }
                    else
                    {
                        XElement directoryComponentElement = GetOrCreateDirectoryComponent(file.Destination);
                        directoryComponentElement.Elements(_ns + "CreateFolder").Remove();
                        directoryComponentElement.Add(new XElement(_ns + "File", new XAttribute("Source", file.Source)));

                    }
                }
                PruneDirectory(file.Destination);
            }
        }
        private bool FileExists(FileMeta file)
        {
            DirectoryMeta directoryMeta = SplitDirectory(file.Destination);
            var files = from f in _componentGroup.Descendants(_ns + "File")
                        where f.Attribute("Source").Value == file.Source &&
                                f.Parent.Attribute("Directory").Value == directoryMeta.Directory &&
                                f.Parent.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory
                        select f;

            return files.Any();
        }
        private bool IsProgramExecutable(string fileName)
        {

            return
                fileName.ToLower().EndsWith(".dll") ||
                fileName.ToLower().EndsWith(".exe") ||
                fileName.ToLower().EndsWith(".ocx") ||
                fileName.ToLower().EndsWith(".chm") ||
                fileName.ToLower().EndsWith(".hlp") ||
               GetComponentRules() == ComponentRules.OneToOne;

        }
        static string GetSHA256Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            SHA256 sha256Hasher = SHA256.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = sha256Hasher.ComputeHash(Encoding.Default.GetBytes(input.ToUpper()));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpper();
        }
        public void DeleteDirectory(string directoryPath, string parentPath)
        {
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            List<XElement> elements = new List<XElement>();

            if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                elements.AddRange(_componentGroup.Descendants(_ns + "Component").Where(c => c.Attribute("Directory").Value == directoryMeta.Directory).ToList());
            }
            else
            {
                elements.AddRange(_componentGroup.Descendants(_ns + "Component").Where(
                    c => c.Attribute("Directory").Value == directoryMeta.Directory &&
                    (c.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory || c.GetOptionalAttribute("Subdirectory").StartsWith(directoryMeta.Subdirectory + @"\"))));
            }
            foreach (var element in elements)
            {
                element.Remove();
            }
            UnpruneDirectory(parentPath);
        }
        public bool RenameDirectory(string directoryPath, string newName)
        {
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            string directory = directoryMeta.Directory;
            string oldSubDirectory = directoryMeta.Subdirectory;

            string[] parts = directoryMeta.Subdirectory.Split('\\');
            parts[parts.Length - 1] = newName;

            string newSubDirectory = string.Join('\\', parts);

            if (!DirectoryExists(Path.Combine(directory, newSubDirectory)))
            {
                List<XElement> components = _componentGroup.Descendants(_ns + "Component").Where(
                    c => c.Attribute("Directory").Value == directoryMeta.Directory &&
                    (c.GetOptionalAttribute("Subdirectory").Equals(directoryMeta.Subdirectory) || c.GetOptionalAttribute("Subdirectory").StartsWith(directoryMeta.Subdirectory + @"\"))).ToList();

                foreach (var component in components)
                {
                    string newValue = newSubDirectory + component.Attribute("Subdirectory").Value.Substring(oldSubDirectory.Length);
                    string id = "owc" + GetSHA256Hash(Path.Combine(directoryMeta.Directory, newValue));
                    component.Attribute("Subdirectory").Value = newValue;
                    component.Attribute("Id").Value = id;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        private bool DirectoryExists(string directoryPath)
        {
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);

            return _componentGroup.Descendants(_ns + "Component").Where(
                c => c.Attribute("Directory").Value == directoryMeta.Directory &&
                (c.GetOptionalAttribute("Subdirectory").Equals(directoryMeta.Subdirectory) || c.GetOptionalAttribute("Subdirectory").StartsWith(directoryMeta.Subdirectory + @"\"))).Any();
        }
        public void PruneDirectory(string directoryPath)
        {
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            bool needed = true;
            int componentCount = _componentGroup.Descendants(_ns + "Component").Where(
                c => c.Attribute("Directory").Value == directoryMeta.Directory && c.GetOptionalAttribute("Subdirectory").StartsWith(directoryMeta.Subdirectory)
            ).Count();

            if (componentCount > 1)
            {
                var component = _componentGroup.Descendants(_ns + "Component").Where(
                                 c => c.Attribute("Directory").Value == directoryMeta.Directory &&
                                 c.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory &&
                                 c.Descendants(_ns + "CreateFolder").Count() > 0 &&
                                 c.Elements().Count() == 1
                                 ).FirstOrDefault();
                if (component != null)
                {
                    component.Remove();
                }
            }
        }
        private void UnpruneDirectory(string directoryPath)
        {
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            bool needed = true;
            int componentCount = _componentGroup.Descendants(_ns + "Component").Where(
                c => c.Attribute("Directory").Value == directoryMeta.Directory && c.GetOptionalAttribute("Subdirectory").StartsWith(directoryMeta.Subdirectory)
            ).Count();
            if (componentCount < 1 && directoryPath != "Destination Computer")
            {
                GetOrCreateDirectoryComponent(directoryPath);
            }
        }
    }
    public enum ComponentRules { OneToOne, OneToMany };
    public class FileMeta
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }
    public class DirectoryMeta
    {
        public string Directory { get; set; }
        public string Subdirectory { get; set; }
    }
}
