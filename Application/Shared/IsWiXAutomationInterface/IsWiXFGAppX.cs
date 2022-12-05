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
    public class IsWiXFGAppXs : List<IsWiXFGAppX>
    {
        XNamespace ns;
        XDocument _document;

        public IsWiXFGAppXs(XDocument Document)
        {
            _document = Document;
            ns = "http://www.firegiant.com/schemas/v3/wxs/fgappx.xsd";
            Load();
        }

        public void Load()
        {
            Clear();

            try
            {

                foreach (var element in _document.GetSecondOrderRoot().Elements(ns + "Appx"))
                {
                    IsWiXFGAppX isWiXFGAppX = new IsWiXFGAppX(_document, element);

                    Add(isWiXFGAppX);
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

        public IsWiXFGAppX Create(string id, string publisher, TargetType targetType)
        {

            XElement appxElement = new XElement(ns + "Appx");
            appxElement.SetAttributeValue("Id", id);
            appxElement.SetAttributeValue("Publisher", publisher);
            appxElement.SetAttributeValue("Target", targetType);
            var baseProperty = _document.GetElementToAddAfterSelf("Property");

            if (appxElement != null)
            {
                baseProperty.AddAfterSelf(appxElement);
            }
            else
            {
                _document.GetSecondOrderRoot().Add(appxElement);
            }

            IsWiXFGAppX isWiXFGAppX = new IsWiXFGAppX(_document, appxElement);
            this.Add(isWiXFGAppX);
            return isWiXFGAppX;
        }

        public static string SuggestNextAppXName(XDocument document)
        {
            XNamespace ns = "http://www.firegiant.com/schemas/v3/wxs/fgappx.xsd";
            int i = 1;
            string suggestedAppxName;
            bool found = false;

            var appxNames = from f in document.Descendants(ns + "Appx")
                            select f.Attribute("Id").Value;

            suggestedAppxName = string.Format("NewAppX{0}", i);
            while (!found)
            {
                if (appxNames.Contains(suggestedAppxName))
                {
                    i++;
                    suggestedAppxName = string.Format("NewAppX{0}", i);
                }
                else
                {
                    found = true;
                }
            }

            return suggestedAppxName;
        }

        public void SortXML()
        {
            var properties = _document.GetSecondOrderRoot().Elements(ns + "Appx")
                            .OrderBy(s => (string)s.Attribute("Id").Value).ToArray();
            _document.Descendants(ns + "Appx").Remove();
            var element = _document.GetElementToAddAfterSelf("Property");
            foreach (var property in properties.Reverse())
            {
                element.AddAfterSelf(property);
            }
        }
    }

    public class IsWiXFGAppX
    {
        XNamespace ns = "http://www.firegiant.com/schemas/v3/wxs/fgappx.xsd";
        XElement _appxElement;
        XDocument _document;

        public IsWiXFGAppX(XDocument document, XElement appxElement)
        {
            _document = document;
            _appxElement = appxElement;
        }


        public string Id
        {
            get
            {
                return _appxElement.Attribute("Id").Value;
            }
            set
            {
                var foo = from a in _document.Descendants(ns + "Appx")
                          where a.Attribute("Id").Value == value
                          select a;

                if (foo.Count() > 0)
                {
                    throw new Exception("Duplicate AppX Name");
                }
                _appxElement.Attribute("Id").Value = value;
            }
        }
        public string Publisher
        {
            get
            {
                return _appxElement.Attribute("Publisher").Value;
            }
            set
            {
                _appxElement.Attribute("Publisher").Value = value;
            }
        }
        public TargetType Target
        {
            get
            {
                string target = _appxElement.Attribute("Target").Value;
                return (TargetType)Enum.Parse(typeof(TargetType), target, true);
            }
            set
            {
                _appxElement.SetAttributeValue("Target", value);
            }
        }
        public string Description
        {
            get
            {
                return _appxElement.GetOptionalAttribute("Description");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _appxElement.SetAttributeValue("Description", value);
                }
                else
                {
                    _appxElement.SetAttributeValue("Description", null);
                }
            }
        }
        public string DisplayName
        {
            get
            {
                return _appxElement.GetOptionalAttribute("DisplayName");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _appxElement.SetAttributeValue("DisplayName", value);
                }
                else
                {
                    _appxElement.SetAttributeValue("DisplayName", null);
                }
            }
        }
        public string LogoFile
        {
            get
            {
                return _appxElement.GetOptionalAttribute("LogoFile");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _appxElement.SetAttributeValue("LogoFile", value);
                }
                else
                {
                    _appxElement.SetAttributeValue("LogoFile", null);
                }
            }
        }
        public string MainPackage
        {
            get
            {
                return _appxElement.GetOptionalAttribute("MainPackage");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _appxElement.SetAttributeValue("MainPackage", value);
                }
                else
                {
                    _appxElement.SetAttributeValue("MainPackage", null);
                }
            }
        }
        public string Manufacturer
        {
            get
            {
                return _appxElement.GetOptionalAttribute("Manufacturer");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _appxElement.SetAttributeValue("Manufacturer", value);
                }
                else
                {
                    _appxElement.SetAttributeValue("Manufacturer", null);
                }
            }
        }
        public string Version
        {
            get
            {
                return _appxElement.GetOptionalAttribute("Version");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _appxElement.SetAttributeValue("Version", value);
                }
                else
                {
                    _appxElement.SetAttributeValue("Version", null);
                }
            }
        }

        public void Delete()
        {
            var foo = from a in _document.Descendants(ns + "Appx")
                      where a.Attribute("Id").Value == this.Id
                      select a;
            foo.First().Remove();
        }

    }


    public enum TargetType
    {
        desktop,
        server,
    }

}
