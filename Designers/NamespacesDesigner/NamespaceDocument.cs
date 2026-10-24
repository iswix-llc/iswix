using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FireworksFramework.Managers;

namespace NamespacesDesigner
{
    // The WiX extension namespaces declared on the document's root (xmlns:prefix="uri").
    internal class NamespaceDocument
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public bool IsDeclared(string prefix) => _documentManager.NameSpaces.ContainsKey(prefix);

        public void Add(string prefix, string uri)
        {
            if (!IsDeclared(prefix))
            {
                _documentManager.Document.Root.Add(new XAttribute(XNamespace.Xmlns + prefix, uri));
                _documentManager.RefreshNamespaces();
            }
        }

        // Returns false, removing nothing, while an element or attribute still uses the namespace: without its
        // declaration it would be saved with a made-up prefix.
        public bool Remove(string prefix)
        {
            var declaration = _documentManager.Document.Root.Attribute(XNamespace.Xmlns + prefix);
            if (declaration == null)
            {
                return true;
            }

            XNamespace ns = declaration.Value;
            var root = _documentManager.Document.Root;
            if (root.DescendantsAndSelf().Any(e => e.Name.Namespace == ns || e.Attributes().Any(a => !a.IsNamespaceDeclaration && a.Name.Namespace == ns)))
            {
                return false;
            }

            declaration.Remove();
            _documentManager.RefreshNamespaces();
            return true;
        }

        // The extensions IsWiX knows: the WiX v4+ extension schemas (fg4_*.xsd) in the Schemas folder next to IsWiX.
        public SortedDictionary<string, string> PossibleNamespaces
        {
            get
            {
                var extensions = new SortedDictionary<string, string>();

                try
                {
                    string currentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                    string schemasDir = Path.Combine(currentDirectory, "Schemas");

                    foreach (var file in new DirectoryInfo(schemasDir).GetFiles("fg4_*.xsd"))
                    {
                        string prefix = Path.GetFileNameWithoutExtension(file.Name).Replace("fg4_", "");
                        if (prefix.Equals("wix"))
                        {
                            continue;
                        }

                        string targetNameSpace = XDocument.Load(file.FullName).Root.Attribute("targetNamespace").Value;

                        // These schemas don't follow the file name pattern the others do.
                        switch (prefix)
                        {
                            case "fgwixappx":
                                prefix = "fga";
                                break;
                            case "hbt":
                                prefix = "fg";
                                break;
                            case "hbt_msix":
                                prefix = "msix";
                                break;
                        }

                        extensions.Add(prefix, targetNameSpace);
                    }
                }
                catch (Exception)
                {
                }

                return extensions;
            }
        }
    }
}
