using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace IsWiXAutomationInterface
{
    public class WiXNamespaces : Dictionary< string, string >
    {
        XDocument _document;
        XNamespace _ns;

        public WiXNamespaces(XDocument Document)
        {
            _document = Document;
            _ns = _document.GetWiXNameSpace();

            Load();
        }

        public void Load()
        {
            foreach (var item in this.PossibleNamespaces)
            {
                if (_document.NameSpaces().Keys.Contains(item.Key))
                {
                    base.Add(item.Key, item.Value);
                }
            }
        }

        public new void Add(string key, string uri)
        {
            base.Add(key, uri);
            _document.Root.Add(new XAttribute(XNamespace.Xmlns + key, uri));
        }

        public new void Remove(string key)
        {
            base.Remove(key);
            _document.Root.Attribute(XNamespace.Xmlns + key).Remove();
        }

        public Dictionary<string, string> PossibleNamespaces
        {
            get
            {

                var _extensions = new Dictionary<string, string>();

                try
                {
                    string currentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

                    string schemasDir;

                    if(_ns == "http://schemas.microsoft.com/wix/2006/wi")
                    {
                        schemasDir = Path.Combine(currentDirectory, "Schemas");
                    }
                    else 
                    {
                        schemasDir = Path.Combine(currentDirectory, @"Schemas\v4");
                    }
 
                    foreach (var file in new DirectoryInfo(schemasDir).GetFiles("*.xsd"))
                    {
                        string prefix = Path.GetFileNameWithoutExtension(file.Name).ToLower();
                        if (!prefix.Equals("wix"))
                        {
                            XDocument doc = XDocument.Load(file.FullName);
                            string targetNameSpace = doc.Root.Attribute("targetNamespace").Value;
                            _extensions.Add(prefix, targetNameSpace);
                        }
                    }
                }
                catch (Exception)
                {
                }

                return _extensions;
            }
        }
    }
}
