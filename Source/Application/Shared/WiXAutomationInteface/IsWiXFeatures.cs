using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXFeatures : List<IsWiXFeature>
    {
        static XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public IsWiXFeatures()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            Load(_documentManager.Document.GetProductOrFragmentElement());
        }

        public IsWiXFeatures(string parentId)
        {
            var elements = from a in _documentManager.Document.Descendants(ns + "Feature")
                           where a.Attribute("Id").Value == parentId
                           select a;
            Load(elements.First());

        }

        public static string SuggestNextFeatureName()
        {
            DocumentManager documentManager = DocumentManager.DocumentManagerInstance;

            ns = documentManager.Document.GetWiXNameSpace();

            int i = 1;
            string suggestedFeatureName;
            bool found = false;

            var featureNames = from f in documentManager.Document.Descendants(ns + "Feature")
                                  select f.Attribute("Id").Value;

            suggestedFeatureName = string.Format("NewFeature{0}", i);
            while(!found)
            {
                if (featureNames.Contains(suggestedFeatureName))
                {
                    i++;
                    suggestedFeatureName = string.Format("NewFeature{0}", i);
                }
                else
                {
                    found = true;
                }
            }

            return suggestedFeatureName;
        }
        public void Load(XElement startingElement)
        {
            Clear();

            try
            {

                foreach (var element in startingElement.Elements(ns + "Feature"))
                {
                    Add(new IsWiXFeature(element));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing XML. Please check your Feature elements.\r\n" + ex.Message);
            }
        }

        public IsWiXFeature Create(string id)
        {

            XElement featureElement = new XElement(ns+"Feature");
            featureElement.SetAttributeValue("Id", id);
            var baseFeature = _documentManager.Document.GetElementToAddAfterSelf("Feature");
            
            if(baseFeature!=null)
            {
                baseFeature.AddAfterSelf(featureElement);
            }
            else
            {
                _documentManager.Document.GetProductModuleOrFragmentElement().Add(featureElement);
            }
            
            IsWiXFeature iswixFeature = new IsWiXFeature(featureElement);
            this.Add(iswixFeature);
            return iswixFeature;
        }

        public IsWiXFeature Create(string insertAfter, string id)
        {

            XElement featureElement = new XElement(ns + "Feature");
            featureElement.SetAttributeValue("Id", id);
            XElement insertAfterElement = _documentManager.Document.Descendants(ns+"Feature").First(vw => (string)vw.Attribute("Id") == insertAfter);
            insertAfterElement.AddAfterSelf(featureElement);
            IsWiXFeature iswixFeature = new IsWiXFeature(featureElement);
            this.Add(iswixFeature);
            return iswixFeature;
        }
        public IsWiXFeature CreateSubFeature(string parentId, string id)
        {

            XElement featureElement = new XElement(ns + "Feature");
            featureElement.SetAttributeValue("Id", id);
            XElement parentElement = _documentManager.Document.Descendants(ns + "Feature").First(vw => (string)vw.Attribute("Id") == parentId);
            parentElement.Add(featureElement);
            IsWiXFeature iswixFeature = new IsWiXFeature(featureElement);
            return iswixFeature;
        }
    }
    public enum Absent { allow, disallow }
    public enum AllowAdvertise { no, system, yes }
    public enum InstallDefault { followParent, local, source }
    public enum TypicalDefault { advertise, install }


    public enum Display { collapse, expand, hidden }
    public class IsWiXFeature
    {
        XNamespace ns;
        XElement _featureElement;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public IsWiXFeature(XElement featureElement)
        {
            _featureElement = featureElement;
            ns = _documentManager.Document.GetWiXNameSpace();
        }
        public string Id
        {
            get
            {
                return _featureElement.Attribute("Id").Value;
            }
            set
            {
                var foo = from a in _documentManager.Document.Descendants(ns+"Feature")
                          where a.Attribute("Id").Value == value
                          select a;

                if(foo.Count()>0)
                {
                    throw new Exception("Duplicate Feature Name");
                }


                if (string.IsNullOrEmpty(value))
                {
                    value = this.Id;
                }
                else
                {
                    if (!IsWiXValidationHelper.IsValidIdentifier(value))
                    {
                        throw new Exception("Feature names must start with an '_' or letter and may only contain '_', '.', letters or numbers.");
                    }

                    if (value.Length > 38)
                    {
                        throw new Exception("Feature names must be less then 38 characters.");
                    }
                }
                _featureElement.Attribute("Id").Value = value;
            }
        }

        public Absent? Absent
        {
            get
            {

                Absent? absent = null;
                string attributeValue = _featureElement.GetOptionalAttribute("Absent");
                if (attributeValue != string.Empty)
                {
                    absent = (Absent)Enum.Parse(typeof(Absent), attributeValue, true);
                }
                return absent;
            }
            set
            {
                _featureElement.SetAttributeValue("Absent", value);
            }
        }
        public AllowAdvertise? AllowAdvertise
        {
            get
            {
                AllowAdvertise? allowAdvertise = null;
                string attributeValue = _featureElement.GetOptionalAttribute("AllowAdvertise");
                if (attributeValue != string.Empty)
                {
                    allowAdvertise = (AllowAdvertise)Enum.Parse(typeof(AllowAdvertise), attributeValue, true);
                }
                return allowAdvertise;
            }
            set
            {
                _featureElement.SetAttributeValue("AllowAdvertise", value);
            }
        }
        public string ConfigurableDirectory
        {
            get
            {
                return _featureElement.GetOptionalAttribute("ConfigurableDirectory");
            }
            set
            {
                string newValue;
                if (string.IsNullOrEmpty(value))
                {
                    newValue = null;
                }
                else
                {
                    newValue = value;
                }
                _featureElement.SetAttributeValue("ConfigurableDirectory", newValue);
            }
        }
        public InstallDefault? InstallDefault
        {
            get
            {

                InstallDefault? installDefault = null;
                string attributeValue = _featureElement.GetOptionalAttribute("InstallDefault");
                if (attributeValue != string.Empty)
                {
                    installDefault = (InstallDefault)Enum.Parse(typeof(InstallDefault), attributeValue, true);
                }
                return installDefault;
            }
            set
            {
                _featureElement.SetAttributeValue("InstallDefault", value);
            }
        }
        public Int16? Level
        {
            get
            {
                Int16? level = null;
                string attributeValue = _featureElement.GetOptionalAttribute("Level");
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    level = Convert.ToInt16(attributeValue);
                }
                return level;
            }
            set
            {
                _featureElement.SetAttributeValue("Level", value);
            }
        }
        public string Title
        {
            get
            {
                return _featureElement.GetOptionalAttribute("Title");
            }
            set
            {
                string newValue;
                if(string.IsNullOrEmpty(value))
                {
                    newValue = null;
                }
                else
                {
                    newValue = value;
                }
                _featureElement.SetAttributeValue("Title", newValue);
            }
        }
        public TypicalDefault? TypicalDefault
        {
            get
            {
                TypicalDefault? typicalDefault = null;
                string attributeValue = _featureElement.GetOptionalAttribute("TypicalDefault");
                if (attributeValue != string.Empty)
                {
                    typicalDefault = (TypicalDefault)Enum.Parse(typeof(TypicalDefault), attributeValue, true);
                }
                return typicalDefault;
            }
            set
            {
                _featureElement.SetAttributeValue("TypicalDefault", value);
            }    
        }
        public Display? Display
        {
            get
            {
                Display? displayDefault = null;
                string attributeValue = _featureElement.GetOptionalAttribute("Display");
                if (attributeValue != string.Empty)
                {
                    displayDefault = (Display)Enum.Parse(typeof(Display), attributeValue, true);
                }
                return displayDefault;
            }
            set
            {
                _featureElement.SetAttributeValue("Display", value);
            }    
        }
        public string Description
        {
            get
            {
                return _featureElement.GetOptionalAttribute("Description");
            }
            set
            {
                string newValue;
                if (string.IsNullOrEmpty(value))
                {
                    newValue = null;
                }
                else
                {
                    newValue = value;
                }
                _featureElement.SetAttributeValue("Description", newValue);
            }
        }

        public IsWiXMergeRefs MergeRefs
        {
            get
            {
                return new IsWiXMergeRefs(_featureElement);
            }
        }

        public void Delete()
        {
            var foo = from a in _documentManager.Document.Descendants(ns + "Feature")
                      where a.Attribute("Id").Value == this.Id
                      select a;
            foo.First().Remove();
        }
    }
}
