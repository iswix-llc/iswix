using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXProduct
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        XElement _productElement;

        public IsWiXProduct()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            _productElement = _documentManager.Document.Descendants(ns + "Product").First();
        }

        public string Id
        {
            get
            {
                string id = _productElement.Attribute("Id").Value;

                if (id.Equals("00000000-0000-0000-0000-000000000000"))
                {

                    id = Guid.NewGuid().ToString();
                    Id = id;
                }
                return id;
            }
            set
            {
                _productElement.Attribute("Id").Value = value;
            }
        }

        public string Codepage
        {
            get
            {
                return _productElement.GetOptionalAttribute("Codepage");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _productElement.SetAttributeValue("Codepage", value);
            }
        }

        public Int32 Language
        {
            get
            {
                return Convert.ToInt32(_productElement.Attribute("Language").Value);
            }
            set
            {
                _productElement.Attribute("Language").Value = value.ToString();
            }
        }

        public string Manufacturer
        {
            get
            {
                return _productElement.Attribute("Manufacturer").Value;
            }
            set
            {
                _productElement.Attribute("Manufacturer").Value = value;
            }
        }
        
        public string Name
        {
            get
            {
                return _productElement.Attribute("Name").Value;
            }
            set
            {
                _productElement.Attribute("Name").Value = value;
            }
        }
        
        public string UpgradeCode
        {
            get
            {
                string upgradecode = _productElement.Attribute("UpgradeCode").Value;

                if (upgradecode.Equals("00000000-0000-0000-0000-000000000000"))
                {
                    upgradecode = Guid.NewGuid().ToString();
                    UpgradeCode = upgradecode;
                }
                return upgradecode;
            }
            set
            {
                _productElement.Attribute("UpgradeCode").Value = value;
            }
        }
        
        public string Version
        {
            get
            {
                return _productElement.Attribute("Version").Value;
            }
            set
            {
                _productElement.Attribute("Version").Value = value;
            }
        }

    }
}
