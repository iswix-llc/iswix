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
                return _summaryInformationElement.GetOptionalAttribute("Codepage");
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
                    return _summaryInformationElement.GetOptionalAttribute("Description");
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
                    return _summaryInformationElement.GetOptionalAttribute("Keywords");
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
                    return _summaryInformationElement.GetOptionalAttribute("Manufacturer");
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