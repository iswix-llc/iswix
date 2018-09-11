using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DocumentManagement.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXMergeRefs : Dictionary<string, IsWiXMergeRef>
    {
        XNamespace ns;
        XElement _parentFeature;
        public IsWiXMergeRefs( XElement parentFeature)
        {
            _parentFeature = parentFeature;
            Load();
        }

        public void Load()
        {
            ns = _parentFeature.Document.GetWiXNameSpace();
            this.Clear();
            foreach (var mergeRefElement in _parentFeature.Elements(ns + "MergeRef"))
            {
                IsWiXMergeRef mergeRef = new IsWiXMergeRef();
                mergeRef.Id = mergeRefElement.Attribute("Id").Value;

                YesNo? yesno = null;
                string attributeValue = mergeRefElement.GetOptionalAttribute("Primary");
                if (attributeValue != string.Empty)
                {
                    yesno = (YesNo)Enum.Parse(typeof(YesNo), attributeValue, true);
                }

                mergeRef.Primary = yesno;
                this.Add(mergeRef.Id, mergeRef);
            }
        }

        public void Save()
        {
            _parentFeature.Elements(ns + "MergeRef").Remove();

            foreach (IsWiXMergeRef mergeRef in this.Values)
            {
                var mergeRefElement = new XElement(ns + "MergeRef");
                mergeRefElement.SetAttributeValue("Id", mergeRef.Id);
                mergeRefElement.SetAttributeValue("Primary", mergeRef.Primary);
                _parentFeature.Add(mergeRefElement);
            }
        }
    }

    public class IsWiXMergeRef
    {        
        public string Id { get; set; }
        public YesNo? Primary { get; set; }
    }
}
