using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using IsWiXAutomationInterface;
using DocumentManagement.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXShortCuts : List<IsWiXShortCut>
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public IsWiXShortCuts()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            Load();
        }

        public void Load()
        {
            Clear();

            try
            {
                foreach (var shortcutElement in _documentManager.Document.GetProductModuleOrFragmentElement().Descendants(ns + "Shortcut"))
                {
                    IsWiXShortCut isWiXShortcut = new IsWiXShortCut(_documentManager.Document, shortcutElement);
                    Add(isWiXShortcut);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing XML. Please check your Shortcut elements.\r\n" + ex.Message);
            }

        }

        public Dictionary<string, string> GetShortCutCandidates()
        {
            Dictionary<string, string> candidates = new Dictionary<string, string>();

            var files = from f in _documentManager.Document.Descendants(ns + "File")
                        select f;
            foreach (var file in files)
            {
                candidates.Add(file.Attribute("Id").Value, file.GetDestinationFilePath());
            }
            return candidates;
        }

        public IsWiXShortCut Create(string name, string fileId, string directory)
        {
            XElement shortcutElement = new XElement(ns + "Shortcut");
            string scID = "sc" + Guid.NewGuid().ToString().ToUpper().Replace("-", string.Empty);
            shortcutElement.SetAttributeValue("Id", "sc" + IsWiXHelpers.GetMd5Hash(directory + name));
            shortcutElement.SetAttributeValue("Name", name);
            shortcutElement.SetAttributeValue("Directory", directory);

            var elements = from a in _documentManager.Document.Descendants(ns + "File")
                           where a.Attribute("Id").Value == fileId
                           select a;

            XElement fileElement = elements.First();

            fileElement.Add(shortcutElement);

            IsWiXShortCut iswixShortCut = new IsWiXShortCut(_documentManager.Document, shortcutElement);
            this.Add(iswixShortCut);
            return iswixShortCut;
        
        }


    }

    public class IsWiXShortCut
    {
        XNamespace ns;
        XDocument _document;
        XElement _shortCutElement;

        public IsWiXShortCut(XDocument document, XElement shortCutElement)
        {
            ns = _document.GetWiXNameSpace();
            _document = document;
            _shortCutElement = shortCutElement;
        }

        public void Delete()
        {
            RetrieveShortcutElement();
            _shortCutElement.Remove();
        }

        private void RetrieveShortcutElement()
        {
            ns = _document.GetWiXNameSpace();
            var foo = from a in _document.Descendants(ns + "Shortcut")
                      where a.Attribute("Id").Value == this.Id
                      select a;
            _shortCutElement =  foo.First();
        }

        public string Id
        {
            get
            {
                return _shortCutElement.Attribute("Id").Value;
            }
        }

        public string Description
        {
            get
            {
                return _shortCutElement.GetOptionalAttribute("Description");
            }
            set
            {
                RetrieveShortcutElement();
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _shortCutElement.SetAttributeValue("Description", value);
            }
        }

        public string Name
        {
            get
            {
                return _shortCutElement.Attribute("Name").Value;
            }
            set
            {
                RetrieveShortcutElement();
                _shortCutElement.SetAttributeValue("Name", value);
                _shortCutElement.SetAttributeValue("Id", "sc" + IsWiXHelpers.GetMd5Hash(this.Directory + this.Name));
            }
        }

        public string Directory
        {
            get
            {
                return _shortCutElement.GetOptionalAttribute("Directory");
            }
            set
            {
                RetrieveShortcutElement();
                _shortCutElement.SetAttributeValue("Directory", value);
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return _shortCutElement.GetOptionalAttribute("WorkingDirectory");
            }
            set
            {
                RetrieveShortcutElement();
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _shortCutElement.SetAttributeValue("WorkingDirectory", value);
            }
        }

        public string Arguments
        {
            get
            {
                return _shortCutElement.GetOptionalAttribute("Arguments");
            }
            set
            {
                RetrieveShortcutElement();
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _shortCutElement.SetAttributeValue("Arguments", value);
            }
        }

        public Show? Show
        {
            get
            {
                Show? show = null;
                string showValue = _shortCutElement.GetOptionalAttribute("Show");
                if (showValue != string.Empty)
                {
                    show = (Show)Enum.Parse(typeof(Show), showValue, true);
                }
                return show;
            }
            set
            {
                RetrieveShortcutElement();
                _shortCutElement.SetAttributeValue("Show", value);
            }
        }

        public string DestinationFilePath
        {
            get
            {
                return _shortCutElement.Parent.GetDestinationFilePath();
            }
        }

    }

    public enum Show
    {
        normal,
        minimized,
        maximized
    }
}
