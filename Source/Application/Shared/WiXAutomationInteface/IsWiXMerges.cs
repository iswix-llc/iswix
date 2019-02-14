using FireworksFramework.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public class IsWiXMerges : List<IsWiXMerge>
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public IsWiXMerges()
        {
            Load();
        }

        private void Load()
        {
            Clear();
            ns = _documentManager.Document.GetWiXNameSpace();

            try
            {

                foreach (var element in _documentManager.Document.Descendants(ns + "Merge"))
                {
                    Add(new IsWiXMerge(element));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing XML. Please check your Merge elements.\r\n" + ex.Message);
            }
        }
    }
    public class IsWiXMerge
    {
        XElement _mergeElement;

        public IsWiXMerge(XElement mergeElement)
        {
            _mergeElement = mergeElement;
        }
        public string Id
        {
            get
            {
                return _mergeElement.Attribute("Id").Value;
            }
        }
        public string SourceFile
        {
            get
            {
                return _mergeElement.Attribute("SourceFile").Value;
            }
        }

        public override string ToString()
        {
            XElement strippedElement = stripNS(_mergeElement);
            strippedElement.Value = string.Empty;
            return strippedElement.ToString(SaveOptions.DisableFormatting);
        }
        static XElement stripNS(XElement root)
        {
            XElement res = new XElement(
                root.Name.LocalName,
                root.HasElements ?
                    root.Elements().Select(el => stripNS(el)) :
                    (object)root.Value
            );

            res.ReplaceAttributes(
                root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));

            return res;
        }

    }
}
