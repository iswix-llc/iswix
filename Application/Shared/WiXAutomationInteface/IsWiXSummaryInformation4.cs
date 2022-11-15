using FireworksFramework.Managers;
using System;
using System.Xml.Linq;

namespace IsWiXAutomationInterface
{
    public class IsWiXSummaryInformation4
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        XElement _summaryInformationElement;

        public IsWiXSummaryInformation4()
        {
            ns = _documentManager.Document.GetWiXNameSpace();
            _summaryInformationElement = _documentManager.Document.GetProductOrModuleElement().Element(ns + "SummaryInformation");
        }

        public string Codepage
        {
            get
            {
                string codepage;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    codepage = _summaryInformationElement.GetOptionalAttribute("Codepage");
                }
                else
                {
                    codepage = _summaryInformationElement.Attribute("Codepage").Value;
                }
                return codepage;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _summaryInformationElement.SetAttributeValue("Codepage", value);
            }
        }

        public string Description
        {
            get
            {
                string description;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    description = _summaryInformationElement.GetOptionalAttribute("Description");
                }
                else
                {
                    description = _summaryInformationElement.Attribute("Description").Value;
                }
                return description;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _summaryInformationElement.SetAttributeValue("Description", value);
            }
        }

        public string Keywords
        {
            get
            {
                string keywords;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    keywords = _summaryInformationElement.GetOptionalAttribute("Keywords");
                }
                else
                {
                    keywords = _summaryInformationElement.Attribute("Keywords").Value;
                }
                return keywords;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _summaryInformationElement.SetAttributeValue("Codepage", value);
            }
        }

        public string Manufacturer
        {
            get
            {
                string manufacturer;
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
                {
                    manufacturer = _summaryInformationElement.GetOptionalAttribute("Manufacturer");
                }
                else
                {
                    manufacturer = _summaryInformationElement.Attribute("Manufacturer").Value;
                }
                return manufacturer;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                _summaryInformationElement.SetAttributeValue("Manufacturer", value);
            }
        }


    }

}