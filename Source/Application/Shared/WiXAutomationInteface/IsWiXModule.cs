using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXModule
    {
        XNamespace ns;
        XElement _moduleElement;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public IsWiXModule()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            _moduleElement = _documentManager.Document.Descendants(ns + "Module").First();
        }

        public string Id
        {
            get
            {
                return _moduleElement.Attribute("Id").Value;
            }
            set
            {
                _moduleElement.Attribute("Id").Value = value;
            }
        }

        public string Codepage 
        {
            get
            {
                return _moduleElement.GetOptionalAttribute("Codepage");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _moduleElement.SetAttributeValue("Codepage", value);
            }
        }
        
        public String Language 
        {
            get
            {
               return _moduleElement.Attribute("Language").Value;
            }
            set
            {
                _moduleElement.Attribute("Language").Value = value;
            }
        }
        
        public string Version 
        {
            get
            {
                return _moduleElement.Attribute("Version").Value;
            }
            set
            {
                _moduleElement.Attribute("Version").Value = value;
            }
        }

    }
}
