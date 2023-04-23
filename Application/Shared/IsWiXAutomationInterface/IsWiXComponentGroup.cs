using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
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

        public void CreateRootDirectory(string directoryName)
        {
            EstablishFragment();
            XElement element = GetComponentGroup();
            element.Add(
                new XElement(_ns + "Component",
                    new XAttribute("Id", $"{_fileName}{directoryName}"),
                    new XAttribute("Directory", directoryName),
                    new XAttribute("Guid", Guid.NewGuid().ToString()),
                    new XAttribute("KeyPath", "true"),
                    new XElement(_ns+"CreateFolder"))
                    );
            SortXML();
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
                element = new XElement(_ns + "Fragment", new XAttribute("Id", _fileName), new XElement(_ns + "ComponentGroup", new XAttribute("Id", _fileName)));
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
            return sourceDirValue;
        }

        public string GetComponentRules()
        {
            string componentRules = string.Empty;
            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("ComponentRules"))
                    {
                        componentRules = fields[1].Replace("\"", "").Trim();
                    }
                }
            }
            return componentRules;
        }

        public void SetComponentRules(string componentRules)
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
                        xpi.Data = string.Format("ComponentRules={0}", componentRules);
                    }
                }
            }

            if (!componentRulesExists)
            {
                _documentManager.Document.Descendants(_ns + "Wix").First().AddFirst(
                    new XProcessingInstruction("define", string.Format("ComponentRules={0}", componentRules)));
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
            List<Tuple<string, bool>> files = new List<Tuple<string, bool>>();
            string[] parts = path.Split(new char[] { '\\' });
            string directory = parts[0];
            string subDirectory = string.Empty;
            if(parts.Length > 1)
            {
                subDirectory = path.Substring(directory.Length + 1);
            }

            XElement componentGroupElement = _documentManager.Document.Descendants(_ns + "ComponentGroup").Where(e => e.GetOptionalAttribute("Id") == _fileName).First();
            var componentElements = componentGroupElement.Elements(_ns + "Component").Where(e => e.GetOptionalAttribute("Directory") == directory && e.GetOptionalAttribute("Subdirectory") == subDirectory).ToList();

            foreach (var componentElement in componentElements)
            {
                foreach (var file in componentElement.Descendants(_ns + "File").ToList())
                {
                    files.Add(new Tuple<string, bool>(file.Attribute("Source").Value, file.GetOptionalYesNoAttribute("KeyPath", false)));
                }
            }
            return files;
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
