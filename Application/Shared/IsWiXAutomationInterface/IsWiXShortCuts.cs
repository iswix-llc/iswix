using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using IsWiXAutomationInterface;
using FireworksFramework.Managers;
using System.IO;

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
                foreach (var shortcutElement in _documentManager.Document.Descendants(ns + "Shortcut"))
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

        public List<ShortCutCandidate> GetShortCutCandidates()
        {
            List<ShortCutCandidate> candidates = new List<ShortCutCandidate>();

            var files = from f in _documentManager.Document.Descendants(ns + "File")
                        where !f.Attribute("Source").Value.EndsWith(".dll")
                        select f;
            foreach (var file in files)
            {
                ShortCutCandidate scc = new ShortCutCandidate();
                scc.FileKey = file.GetOptionalAttribute("Id");
                scc.DestinationFilePath = file.GetDestinationFilePath();
                scc.FileName = Path.GetFileNameWithoutExtension(file.Attribute("Source").Value.Split('\\').Last());
                scc.FileElement = file;
                candidates.Add(scc);
            }
            return candidates;
        }

        public IsWiXShortCut Create(string name, string directory, string subDirectory, XElement fileElement)
        {
            XElement shortcutElement = new XElement(ns + "Shortcut");
            string scID = "sc" + Guid.NewGuid().ToString().ToUpper().Replace("-", string.Empty);
            shortcutElement.SetAttributeValue("Id", "sc" + IsWiXHelpers.GetMd5Hash(directory + name));
            shortcutElement.SetAttributeValue("Name", name);
            shortcutElement.SetAttributeValue("Directory", directory);
            if (!string.IsNullOrEmpty(subDirectory))
            {
                shortcutElement.SetAttributeValue("Subdirectory", subDirectory);
            }

            if(_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Fragment)
            {
                shortcutElement.SetAttributeValue("Advertise", "Yes");
            }
            fileElement.Add(shortcutElement);

            IsWiXShortCut iswixShortCut = new IsWiXShortCut(_documentManager.Document, shortcutElement);
            this.Add(iswixShortCut);
            return iswixShortCut;
        
        }


    }

    public class ShortCutCandidate
    {
        public string FileKey { get; set; }
        public string FileName { get; set; }
        public string DestinationFilePath { get; set; }
        public XElement FileElement { get; set; }

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
            _shortCutElement = foo.FirstOrDefault();
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

        public string Subdirectory
        {
            get
            {
                return _shortCutElement.GetOptionalAttribute("Subdirectory");
            }
            set
            {
                RetrieveShortcutElement();
                _shortCutElement.SetAttributeValue("Subdirectory", value);
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
