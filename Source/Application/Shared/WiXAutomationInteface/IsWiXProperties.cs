using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace IsWiXAutomationInterface
{
    public class IsWiXProperties : List<IsWiXProperty>
    {
        XNamespace ns;
        XDocument _document;

        public IsWiXProperties(XDocument Document)
        {
            _document = Document;
            ns = _document.GetWiXNameSpace();
            Load();
        }

        public void Load()
        {
            Clear();

            try
            {

                foreach (var element in _document.GetProductModuleOrFragmentElement().Elements(ns + "Property"))
                {
                    IsWiXProperty iswixProperty = new IsWiXProperty(_document, element);

                    Add(iswixProperty);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing XML. Please check your Property elements.\r\n" + ex.Message);
            }
        }
        public IsWiXProperty Create(string id)
        {

            XElement propertyElement = new XElement(ns + "Property");
            propertyElement.SetAttributeValue("Id", id);
            var baseProperty = _document.GetElementToAddAfterSelf("Property");

            if (baseProperty != null)
            {
                baseProperty.AddAfterSelf(propertyElement);
            }
            else
            {
                _document.GetProductModuleOrFragmentElement().Add(propertyElement);
            }

            IsWiXProperty iswixProperty = new IsWiXProperty(_document, propertyElement);
            this.Add(iswixProperty);
            return iswixProperty;
        }
        public void SortXML()
        {
            var properties = _document.GetProductModuleOrFragmentElement().Elements(ns + "Property")
                            .OrderBy(s => (string)s.Attribute("Id").Value).ToArray();
            _document.Descendants(ns + "Property").Remove();
            var element =  _document.GetElementToAddAfterSelf("Property");
            foreach (var property in properties.Reverse())
            {
                element.AddAfterSelf(property);
            }
        }
    }

    public class IsWiXProperty
    {
        XNamespace ns;
        XElement _propertyElement;
        XDocument _document;

        public IsWiXProperty(XDocument document, XElement propertyElement)
        {
            ns = document.GetWiXNameSpace();
            _document = document;
            _propertyElement = propertyElement;
        }


        public string Id
        {
            get
            {
                return _propertyElement.Attribute("Id").Value;
            }
            set
            {
                _propertyElement.Attribute("Id").Value = value;
            }
        }
        public string Value
        {
            get
            {
                return _propertyElement.GetOptionalAttribute("Value");
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _propertyElement.SetAttributeValue("Value", value);
                }
                else
                {
                    _propertyElement.SetAttributeValue("Value", null);
                }
            }

        }
        public bool SuppressModularization
        {
            get
            {
                return _propertyElement.GetOptionalYesNoAttribute("SuppressModularization", false);
            }
            set
            {
                if(value)
                {
                    _propertyElement.SetAttributeValue("SuppressModularization", "yes");
                }
                else
                {
                    _propertyElement.SetAttributeValue("SuppressModularization", null);
                }
            }
        }
        public bool Secure
        {
            get
            {
                return _propertyElement.GetOptionalYesNoAttribute("Secure", false);
            }
            set
            {
                if (value)
                {
                    _propertyElement.SetAttributeValue("Secure", "yes");
                }
                else
                {
                    _propertyElement.SetAttributeValue("Secure", null);
                }
            }
        }
        public bool Hidden
        {
            get
            {
                return _propertyElement.GetOptionalYesNoAttribute("Hidden", false);
            }
            set
            {
                if (value)
                {
                    _propertyElement.SetAttributeValue("Hidden", "yes");
                }
                else
                {
                    _propertyElement.SetAttributeValue("Hidden", null);
                }
            }
        }
        public bool Admin
        {
            get
            {
                return _propertyElement.GetOptionalYesNoAttribute("Admin", false);
            }
            set
            {
                if (value)
                {
                    _propertyElement.SetAttributeValue("Admin", "yes");
                }
                else
                {
                    _propertyElement.SetAttributeValue("Admin", null);
                }
            }
        }
        public void Delete()
        {
            var foo = from a in _document.Descendants(ns + "Property")
                      where a.Attribute("Id").Value == this.Id
                      select a;
            foo.First().Remove();
        }
    }
}
