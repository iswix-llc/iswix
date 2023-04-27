using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
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

    public class IsWiXComponentGroup
    {
        XNamespace _ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        XElement _componentGroupElement;
        string _fileName;
        private const string SourceDirVar = "$(var.SourceDir)";
        public IsWiXComponentGroup()
        {
            _ns = _documentManager.Document.GetWiXNameSpace();
            _fileName = Path.GetFileNameWithoutExtension(_documentManager.DocumentPath);
           _componentGroupElement = GetComponentGroup();
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
            XElement element = GetComponentGroup();

            var test = from d in element.Descendants(_ns + "Component")
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
            XElement directoryComponentElement;
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            EstablishFragment();
            XElement element = GetComponentGroup();

            if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                directoryComponentElement = element.Descendants(_ns + "Component").Where(
                c => c.Attribute("Directory").Value == directoryMeta.Directory).FirstOrDefault();
            }
            else
            {
                directoryComponentElement = element.Descendants(_ns + "Component").Where(
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
                element.Add(directoryComponentElement);
            }
            return directoryComponentElement;
        }


        private XElement GetFragment()
        {
            XElement element = null;
            bool foundFragment = _documentManager.Document.Descendants(_ns + "Fragment").Where(e => e.GetOptionalAttribute("Id").ToLower() == _fileName.ToLower()).Any();
            if (foundFragment)
            {
                element = _documentManager.Document.Descendants(_ns + "Fragment").Where(e => e.GetOptionalAttribute("Id") == _fileName).First();
            }
            return element;
        }


        private XElement GetComponentGroup()
        {
            XElement element = null;
            bool foundComponentGroup = _documentManager.Document.Descendants(_ns + "ComponentGroup").Where(e => e.GetOptionalAttribute("Id").ToLower() == _fileName.ToLower()).Any();
            if (foundComponentGroup)
            {
                element = _documentManager.Document.Descendants(_ns + "ComponentGroup").Where(e => e.GetOptionalAttribute("Id") == _fileName).First();
                if(element.Attribute("Id").Value != _fileName)
                {
                    element.Attribute("Id").Value = _fileName;
                }
            }
            return element; 
        }

        private void EstablishFragment()
        {
            XElement fragment = GetFragment();
            if(fragment==null)
            {
                fragment = new XElement(_ns + "Fragment", new XAttribute("Id", _fileName));
                _documentManager.Document.GetSecondOrderRoot().AddAfterSelf(fragment);
            }
            XElement element = GetComponentGroup();
            if (element == null)
            {
                fragment.Add(new XElement(_ns + "ComponentGroup", new XAttribute("Id", _fileName)));
            }
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
                        if(fields[1].Replace("\"", "").Trim() == "OneToMany")
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

            if (documentManager.Document.GetWiXVersion() == WiXVersion.v4 &&
                (
                  documentManager.Document.GetDocumentType() == IsWiXDocumentType.Fragment ||
                  documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                )
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public List<string> GetDirectories()
        {
            List<string> dirs = new List<string>();

            XElement componentGroup = GetComponentGroup();
            if (componentGroup!=null)
            {
                foreach (var componentElement in componentGroup.Elements(_ns + "Component"))
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

            XElement componentGroupElement = GetComponentGroup();
            List<XElement> componentElements;
            if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                componentElements = componentGroupElement.Elements(_ns + "Component").Where(e => e.GetOptionalAttribute("Directory") == directoryMeta.Directory && String.IsNullOrEmpty(e.GetOptionalAttribute("Subdirectory"))).ToList();
            }
            else
            {
                componentElements = componentGroupElement.Elements(_ns + "Component").Where(e => e.GetOptionalAttribute("Directory") == directoryMeta.Directory && e.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory).ToList();
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
                if(FileExists(file))
                {
                    if(string.IsNullOrEmpty(directoryMeta.Subdirectory))
                    {
                        var fileElements = from f in GetComponentGroup().Descendants(_ns + "File")
                                           where f.Attribute("Source").Value == file.Source &&
                                                f.Parent.Attribute("Directory").Value == directoryMeta.Directory
                                           select f;
                        var listElements = fileElements.ToList();
                        foreach (var fileElement in listElements)
                        {
                            fileElement.Remove();
                        }
                    }
                    else
                    {
                        var fileElements = from f in GetComponentGroup().Descendants(_ns + "File")
                                           where f.Attribute("Source").Value == file.Source &&
                                                f.Parent.Attribute("Directory").Value == directoryMeta.Directory &&
                                                f.Parent.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory
                                           select f;
                        List<XElement> test = fileElements.ToList();
                        foreach (var fileElement in test)
                        {
                            fileElement.Remove();
                        }

                    }
                }
            }
            var elements = _componentGroupElement.Descendants(_ns + "Component").Where(c => !c.Elements().Any()).ToList();
            foreach (var element in elements)
            {
                if (string.IsNullOrEmpty(element.GetOptionalAttribute("Id")))
                {
                    element.Remove();
                }
                else
                {
                    element.Add(new XElement(_ns + "CreateFolder"));
                }
            }
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
                        if(string.IsNullOrEmpty(directoryMeta.Subdirectory))
                        { 
                            GetComponentGroup().Add(new XElement(_ns + "Component", new XAttribute("Directory", directoryMeta.Directory),
                            new XElement(_ns + "File", new XAttribute("Source", file.Source), new XAttribute("KeyPath", "yes"))));
                        }
                        else
                        
                            GetComponentGroup().Add(new XElement(_ns + "Component", 
                                new XAttribute("Directory", directoryMeta.Directory),
                                new XAttribute("Subdirectory", directoryMeta.Subdirectory),
                            new XElement(_ns + "File", new XAttribute("Source", file.Source), new XAttribute("KeyPath", "yes"))));
                        }
                    else
                    {
                        XElement directoryComponentElement = GetOrCreateDirectoryComponent(file.Destination);
                        directoryComponentElement.Elements(_ns + "CreateFolder").Remove();
                        directoryComponentElement.Add(new XElement(_ns + "File", new XAttribute("Source", file.Source)));

                    }
                }
            }
        }

        private bool FileExists(FileMeta file)
        {
            DirectoryMeta directoryMeta = SplitDirectory(file.Destination);
            var files = from f in GetComponentGroup().Descendants(_ns + "File")
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

        public void DeleteDirectory(string directoryPath)
        {
            XElement componentGroupElement = GetComponentGroup();
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            List<XElement> elements = new List<XElement>();

            if(string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                elements.AddRange(componentGroupElement.Descendants(_ns + "Component").Where(c => c.Attribute("Directory").Value == directoryMeta.Directory).ToList());
            }
            else
            {
                elements.AddRange(componentGroupElement.Descendants(_ns + "Component").Where(
                    c => c.Attribute("Directory").Value == directoryMeta.Directory &&
                    (c.GetOptionalAttribute("Subdirectory") == directoryMeta.Subdirectory || c.GetOptionalAttribute("Subdirectory").StartsWith(directoryMeta.Subdirectory + @"\"))));
            }
            foreach (var element in elements)
            {
                element.Remove();
            }
        }

        public bool RenameDirectory(string directoryPath, string newName)
        {
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            string directory = directoryMeta.Directory;
            string oldSubDirectory = directoryMeta.Subdirectory;

            string[] parts = directoryMeta.Subdirectory.Split('\\');
            parts[parts.Length-1] = newName;

            string newSubDirectory = string.Join('\\', parts);

            if(!DirectoryExists(Path.Combine(directory, newSubDirectory)))
            {
                List<XElement> components = GetComponentGroup().Descendants(_ns + "Component").Where(
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

            return GetComponentGroup().Descendants(_ns + "Component").Where(
                c => c.Attribute("Directory").Value == directoryMeta.Directory &&
                (c.GetOptionalAttribute("Subdirectory").Equals(directoryMeta.Subdirectory) || c.GetOptionalAttribute("Subdirectory").StartsWith(directoryMeta.Subdirectory + @"\"))).Any();
        }
    }
}
