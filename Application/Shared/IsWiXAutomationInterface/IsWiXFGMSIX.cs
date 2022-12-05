using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXFGMSIXs : List<IsWiXFGMSIX>
    {
        XNamespace ns;
        XDocument _document;

        public IsWiXFGMSIXs(XDocument Document)
        {
            _document = Document;
            ns = "http://www.firegiant.com/schemas/v3/wxs/fgmsix.xsd";
            Load();
        }

        public void Load()
        {
            Clear();

            try
            {

                foreach (var element in _document.GetSecondOrderRoot().Elements(ns + "Msix"))
                {
                    IsWiXFGMSIX isWiXFGMSIX = new IsWiXFGMSIX(_document, element);

                    Add(isWiXFGMSIX);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing XML. Please check your Property elements.\r\n" + ex.Message);
            }
        }
        public static bool IsFGWiXInstalled()
        {
            bool installed = false;

            try
            {
                RegistryKey regBaseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey subKey = regBaseKey.OpenSubKey(@"SOFTWARE\FireGiant\WixExpansionPack");
                if (subKey != null)
                {
                    installed = true;
                }
            }
            catch (Exception)
            {
                installed = false;
            }

            return installed;
        }

        public IsWiXFGMSIX Create(string id, string publisher, TargetType targetType)
        {

            XElement msixElement = new XElement(ns + "Msix");
            msixElement.SetAttributeValue("Id", id);
            msixElement.SetAttributeValue("Publisher", publisher);
            msixElement.SetAttributeValue("Target", targetType);
            var baseProperty = _document.GetElementToAddAfterSelf("Property");

            if (msixElement != null)
            {
                baseProperty.AddAfterSelf(msixElement);
            }
            else
            {
                _document.GetSecondOrderRoot().Add(msixElement);
            }

            IsWiXFGMSIX isWiXFGMSIX = new IsWiXFGMSIX(_document, msixElement);
            this.Add(isWiXFGMSIX);
            return isWiXFGMSIX;
        }

        public static string SuggestNextMSIXName(XDocument document)
        {
            XNamespace ns = "http://www.firegiant.com/schemas/v3/wxs/fgmsix.xsd";
            int i = 1;
            string suggestedMSIXName;
            bool found = false;

            var msixNames = from f in document.Descendants(ns + "Msix")
                            select f.Attribute("Id").Value;

            suggestedMSIXName = string.Format("NewMsix{0}", i);
            while (!found)
            {
                if (msixNames.Contains(suggestedMSIXName))
                {
                    i++;
                    suggestedMSIXName = string.Format("NewMsix{0}", i);
                }
                else
                {
                    found = true;
                }
            }

            return suggestedMSIXName;
        }

        public void SortXML()
        {
            var properties = _document.GetSecondOrderRoot().Elements(ns + "Msix")
                            .OrderBy(s => (string)s.Attribute("Id").Value).ToArray();
            _document.Descendants(ns + "Msix").Remove();
            var element = _document.GetElementToAddAfterSelf("Property");
            foreach (var property in properties.Reverse())
            {
                element.AddAfterSelf(property);
            }
        }
    }

    public class IsWiXFGMSIX
    {
        XNamespace ns = "http://www.firegiant.com/schemas/v3/wxs/fgmsix.xsd";
        XElement _msixElement;
        XDocument _document;

        public IsWiXFGMSIX(XDocument document, XElement msixElement)
        {
            _document = document;
            _msixElement = msixElement;
        }


        public string Id
        {
            get
            {
                return _msixElement.Attribute("Id").Value;
            }
            set
            {
                var foo = from a in _document.Descendants(ns + "Msix")
                          where a.Attribute("Id").Value == value
                          select a;

                if (foo.Count() > 0)
                {
                    throw new Exception("Duplicate Msix Name");
                }
                _msixElement.Attribute("Id").Value = value;
            }
        }
        public string Publisher
        {
            get
            {
                return _msixElement.Attribute("Publisher").Value;
            }
            set
            {
                _msixElement.Attribute("Publisher").Value = value;
            }
        }
        public TargetType Target
        {
            get
            {
                string target = _msixElement.Attribute("Target").Value;
                return (TargetType)Enum.Parse(typeof(TargetType), target, true);
            }
            set
            {
                _msixElement.SetAttributeValue("Target", value);
            }
        }
        public string Capabilities
        {
            get
            {
                return _msixElement.GetOptionalAttribute("Capabilities");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("Capabilities", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("Capabilities", null);
                }
            }
        }
        public string MainPackage
        {
            get
            {
                return _msixElement.GetOptionalAttribute("MainPackage");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("MainPackage", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("MainPackage", null);
                }
            }
        }

        public string Description
        {
            get
            {
                return _msixElement.GetOptionalAttribute("Description");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("Description", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("Description", null);
                }
            }
        }
        public string DisplayName
        {
            get
            {
                return _msixElement.GetOptionalAttribute("DisplayName");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("DisplayName", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("DisplayName", null);
                }
            }
        }
        public string LogoFile
        {
            get
            {
                return _msixElement.GetOptionalAttribute("LogoFile");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("LogoFile", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("LogoFile", null);
                }
            }
        }
        public string MinimumSupportedOS
        {
            get
            {
                return _msixElement.GetOptionalAttribute("MinimumSupportedOS");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("MinimumSupportedOS", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("MinimumSupportedOS", null);
                }
            }
        }
        public string MaximumTestedOS
        {
            get
            {
                return _msixElement.GetOptionalAttribute("MaximumTestedOS");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("MaximumTestedOS", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("MaximumTestedOS", null);
                }
            }
        }
        public string Manufacturer
        {
            get
            {
                return _msixElement.GetOptionalAttribute("Manufacturer");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("Manufacturer", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("Manufacturer", null);
                }
            }
        }
        public string Version
        {
            get
            {
                return _msixElement.GetOptionalAttribute("Version");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _msixElement.SetAttributeValue("Version", value);
                }
                else
                {
                    _msixElement.SetAttributeValue("Version", null);
                }
            }
        }

        public void Delete()
        {
            var foo = from a in _document.Descendants(ns + "Msix")
                      where a.Attribute("Id").Value == this.Id
                      select a;
            foo.First().Remove();
        }

    }


}
