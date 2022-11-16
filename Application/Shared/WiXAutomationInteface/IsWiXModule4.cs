using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXModule4
    {
        XNamespace ns;
        XElement _moduleElement;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public IsWiXModule4()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            _moduleElement = _documentManager.Document.Descendants(ns + "Module").First();
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

        public String Guid
        {
            get
            {

                string guid;
                guid = _moduleElement.Attribute("Guid").Value;
                if (guid == "00000000-0000-0000-0000-000000000000")
                {
                    guid = System.Guid.NewGuid().ToString();
                }
                return guid;
            }
            set
            {
                _moduleElement.SetAttributeValue("Guid", value);
            }
        }

        public String Id
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

        public Int32? InstallerVersion
        {
            get
            {
                Int32 temp;
                Int32? installerVersion;
                string value = _moduleElement.GetOptionalAttribute("InstallerVersion");

                if(Int32.TryParse(value, out temp))
                {
                    installerVersion = temp;
                }
                else
                {
                    installerVersion = null;
                }
                return installerVersion;
            }
            set
            {
                    _moduleElement.SetAttributeValue("InstallerVersion", value);
            }
        }


        public Int32 Language
        {
            get
            {
                return int.Parse(_moduleElement.GetOptionalAttribute("Language"));
            }
            set
            {
                _moduleElement.Attribute("Language").Value = Convert.ToString(value);
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
