using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXPackage4
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        XElement _packageElement;

        public IsWiXPackage4()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            _packageElement = _documentManager.Document.GetProductOrModuleElement().Element(ns + "Package");
        }

        public string Codepage
        {
            get
            {
                string codepage;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    codepage = _packageElement.GetOptionalAttribute("Codepage");
                }
                else
                {
                    codepage = _packageElement.Attribute("Codepage").Value;
                }
                return codepage;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Codepage", value);
            }
        }
        public YesNo? Compressed
        {
            get
            {
                YesNo? yesno = null;
                string compressed = _packageElement.GetOptionalAttribute("Compressed");
                if (compressed != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), compressed, true);
                }
                return yesno;
            }
            set
            {
                if (value != null && _documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
                {
                    throw new Exception("The commpressed attribute can not be set for a merge module.");
                }
                _packageElement.SetAttributeValue("Compressed", value);
            }
        }

        public Int32 InstallerVersion
        {
            get
            {
                return Convert.ToInt32(_packageElement.GetOptionalAttribute("InstallerVersion"));
            }
            set
            {
                _packageElement.Attribute("InstallerVersion").Value = Convert.ToString(value);
            }
        }

        public Int32 Language
        { 
            get
            {
                return Convert.ToInt32(_packageElement.GetOptionalAttribute("Language"));
            }
            set
            {
                _packageElement.Attribute("Language").Value = Convert.ToString(value);
            }
        }


        public string Manufacturer
        {
            get
            {
                string manufacturer;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    manufacturer = _packageElement.GetOptionalAttribute("Manufacturer");
                }
                else
                {
                    manufacturer = _packageElement.Attribute("Manufacturer").Value;
                }
                return manufacturer;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Manufacturer", value);
            }
        }
        public string Name
        {
            get
            {
                string name;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    name = _packageElement.GetOptionalAttribute("Name");
                }
                else
                {
                    name = _packageElement.Attribute("Name").Value;
                }
                return name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("Name", value);
            }
        }

        public string ProductCode
        {
            get
            {
                string productCode;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    productCode = _packageElement.GetOptionalAttribute("ProductCode");
                }
                else
                {
                    productCode = _packageElement.Attribute("ProductCode").Value;
                }
                return productCode;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _packageElement.SetAttributeValue("ProductCode", value);
            }
        }

        public InstallScope? Scope
        {
            get
            {
                InstallScope? scope = null;
                string installScope = _packageElement.GetOptionalAttribute("Scope");
                if (installScope != string.Empty)
                {
                    scope = (InstallScope)Enum.Parse(typeof(InstallScope), installScope, true);
                }
                return scope;
            }
            set
            {
                _packageElement.SetAttributeValue("Scope", value);
            }
        }
        public YesNo? ShortNames
        {
            get
            {
                YesNo? yesno = null;
                string shortNames = _packageElement.GetOptionalAttribute("ShortNames");
                if (shortNames != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), shortNames, true);
                }
                return yesno;
            }
            set
            {
                _packageElement.SetAttributeValue("ShortNames", value);
            }
        }

        public string UpgradeCode
        {
            get
            {
                string upgradecode = _packageElement.Attribute("UpgradeCode").Value;

                if (upgradecode.Equals("00000000-0000-0000-0000-000000000000"))
                {
                    upgradecode = Guid.NewGuid().ToString();
                    UpgradeCode = upgradecode;
                }
                return upgradecode;
            }
            set
            {
                _packageElement.Attribute("UpgradeCode").Value = value;
            }
        }

        public string Version
        {
            get
            {
                return _packageElement.Attribute("Version").Value;
            }
            set
            {
                _packageElement.Attribute("Version").Value = value;
            }
        }


    }
}