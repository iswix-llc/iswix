using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
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
            SortXML();
        }

        private DirectoryMeta SplitDirectory(string directory)
        {
            DirectoryMeta directoryMeta= new DirectoryMeta();

            string[] parts = directory.Split(new char[] { '\\' });
            directoryMeta.Directory = parts[0];
            string subDirectory = string.Empty;
            if (parts.Length > 1)
            {
                directoryMeta.Subdirectory = directory.Substring(directory.Length + 1);
            }
            return directoryMeta;
        }
        public XElement GetOrCreateDirectoryComponent(string directoryPath)
        {
            XElement directoryComponentElement;
            DirectoryMeta directoryMeta = SplitDirectory(directoryPath);
            EstablishFragment();
            XElement element = GetComponentGroup();

            if (directoryMeta.Subdirectory == null)
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
                if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
                {
                    directoryComponentElement =
                        new XElement(_ns + "Component",
                            new XAttribute("Id", $"_{_fileName}_{directoryPath}"),
                            new XAttribute("Directory", directoryMeta.Directory),
                            new XAttribute("Guid", Guid.NewGuid().ToString()),
                            new XAttribute("KeyPath", "true"),
                            new XElement(_ns + "CreateFolder"));
                }
                else
                {
                    directoryComponentElement =
                        new XElement(_ns + "Component",
                            new XAttribute("Id", $"_{_fileName}_{directoryPath}"),
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
            XElement element = GetComponentGroup();
            if (element == null)
            {
                element = new XElement(_ns + "Fragment", new XAttribute("Id", _fileName), 
                            new XElement(_ns + "ComponentGroup", new XAttribute("Id", _fileName)));
                _documentManager.Document.GetSecondOrderRoot().AddAfterSelf(element);
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

            List<Tuple<string, bool>> files = new List<Tuple<string, bool>>();

            XElement componentGroupElement = GetComponentGroup();
            List<XElement> componentElements;
            if (string.IsNullOrEmpty(directoryMeta.Subdirectory))
            {
                componentElements = componentGroupElement.Elements(_ns + "Component").Where(e => e.GetOptionalAttribute("Directory") == directoryMeta.Directory).ToList();
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
                                                f.Parent.Attribute("Directory").Value == file.Destination
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
                                                f.Parent.Attribute("Directory").Value == file.Destination &&
                                                f.Parent.GetOptionalAttribute("Subdiectory") == directoryMeta.Subdirectory
                                           select f;
                        foreach (var fileElement in fileElements)
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
                file.Source = file.Source.Replace(rootDir, "$(var.SourceDir)");
                if (!FileExists(file))
                {
                    if (IsProgramExecutable(file.Source))
                    {
                        GetComponentGroup().Add(new XElement(_ns + "Component", new XAttribute("Directory", file.Destination),
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
            var files = from f in GetComponentGroup().Descendants(_ns + "File")
                        where f.Attribute("Source").Value == file.Source &&
                             f.Parent.Attribute("Directory").Value == file.Destination
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


        public void SortXML()
        {
            if (_componentGroupElement != null)
            {
                var components = _componentGroupElement.Elements(_ns + "Component")
                                .OrderBy(s => Path.Combine((string)s.Attribute("Directory").Value, s.GetOptionalAttribute("Subdirectory"))).ToList();
                _documentManager.Document.Descendants(_ns + "Component").Remove();
                _componentGroupElement.Add(components);
            }
        }
    }
}
