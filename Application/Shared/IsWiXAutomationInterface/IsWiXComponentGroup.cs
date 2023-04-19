using FireworksFramework.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace IsWiXAutomationInterface
{
    public class IsWiXComponentGroup
    {
        XNamespace _ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        string _fileName;
        private const string SourceDirVar = "$(var.SourceDir)";
        public IsWiXComponentGroup()
        {
            _fileName = Path.GetFileNameWithoutExtension(_documentManager.DocumentPath).ToLower();
            Load();
        }

        public void Load()
        {
            EstablishFragment();
        }

        private void EstablishFragment()
        {
            _ns = _documentManager.Document.GetWiXNameSpace();
            bool foundComponentGroup = _documentManager.Document.Descendants(_ns + "ComponentGroup").Where(e => e.GetOptionalAttribute("Id") == _fileName).Any();
            if (!foundComponentGroup)
            {
                XElement element = new XElement(_ns + "Fragment", new XAttribute("Id", _fileName), new XElement(_ns + "ComponentGroup", new XAttribute("Id", _fileName)));
                _documentManager.Document.GetSecondOrderRoot().AddAfterSelf(element);
            }

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

            XElement componentGroupElement = _documentManager.Document.Descendants(_ns + "ComponentGroup").Where(e => e.GetOptionalAttribute("Id") == _fileName).First();
            foreach (var componentElement in componentGroupElement.Elements(_ns + "Component"))
            {
                dirs.Add(Path.Combine(componentElement.GetOptionalAttribute("Directory"), componentElement.GetOptionalAttribute("Subdirectory")));
            }
            dirs.Sort();
            return dirs;
        }

        //public void SortXML()
        //{
        //    XElement rootElement = _documentManager.Document.GetSecondOrderRoot();

        //    var fragments = rootElement.Elements(ns + "Fragment")
        //                    .OrderBy(s => (string)s.Attribute("Id").Value).ToArray();
        //    _documentManager.Document.Descendants(ns + "Fragment").Remove();
        //    var element = _documentManager.Document.GetElementToAddAfterSelf("Fragment");

        //    foreach (var fragment in fragments.Reverse())
        //    {
        //        if (element == null)
        //        {
        //            rootElement.AddFirst(fragment);
        //        }
        //        else
        //        {
        //            element.AddAfterSelf(fragment);
        //        }
        //    }
        //}

        public List<XElement> GetFiles(string path)
        {
            List<XElement> fileElements = new List<XElement>();
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
                fileElements.AddRange(componentElement.Descendants(_ns + "File").ToList());
            }
            return fileElements;
        }
    }
}
