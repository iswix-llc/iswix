using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FireworksFramework.Managers;
using IsWiX.WiXModel.Core;

namespace NamespacesDesigner
{
    // The WiX extension namespaces declared on the document's root (xmlns:prefix="uri"). A namespace counts as
    // declared whatever prefix the document gives it.
    internal class NamespaceDocument
    {
        // The extensions WiX ships, with the prefixes WiX's own sources use for them (from IsWiX.WiXModel, the same
        // prefixes it declares when the model adds an extension element).
        static readonly SortedDictionary<string, string> WixExtensions = new SortedDictionary<string, string>(
            WixNamespaces.ExtensionPrefixes.ToDictionary(e => e.Value, e => e.Key.NamespaceName));

        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        // The designer is only offered for an open WiX document, which has a root.
        XElement Root => _documentManager.Document.Root!;

        IEnumerable<XAttribute> Declarations(string uri) =>
            Root.Attributes().Where(a => a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.Xmlns && a.Value == uri);

        public bool IsDeclared(string uri) => Declarations(uri).Any();

        public void Add(string prefix, string uri)
        {
            if (!IsDeclared(uri))
            {
                Root.Add(new XAttribute(XNamespace.Xmlns + prefix, uri));
                _documentManager.RefreshNamespaces();
            }
        }

        // Returns false, removing nothing, while an element or attribute still uses the namespace: without its
        // declaration it would be saved with a made-up prefix.
        public bool Remove(string uri)
        {
            XNamespace ns = uri;
            if (Root.DescendantsAndSelf().Any(e => e.Name.Namespace == ns || e.Attributes().Any(a => !a.IsNamespaceDeclaration && a.Name.Namespace == ns)))
            {
                return false;
            }

            foreach (var declaration in Declarations(uri).ToList())
            {
                declaration.Remove();
            }
            _documentManager.RefreshNamespaces();
            return true;
        }

        // WiX's extensions, any extension schemas (fg4_*.xsd) in the Schemas folder next to IsWiX, and any other
        // namespace the document declares, by prefix.
        public SortedDictionary<string, string> PossibleNamespaces
        {
            get
            {
                // A namespace the document already declares is listed under the document's prefix.
                var declared = Root.Attributes()
                    .Where(a => a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.Xmlns)
                    .GroupBy(a => a.Value)
                    .ToDictionary(g => g.Key, g => g.First().Name.LocalName);
                var extensions = new SortedDictionary<string, string>();
                foreach (var extension in WixExtensions)
                {
                    extensions[declared.TryGetValue(extension.Value, out var prefix) ? prefix : extension.Key] = extension.Value;
                }

                void AddUnlisted(string prefix, string uri)
                {
                    if (!extensions.ContainsValue(uri) && !extensions.ContainsKey(prefix))
                    {
                        extensions.Add(prefix, uri);
                    }
                }

                foreach (var schema in SchemaNamespaces())
                {
                    AddUnlisted(schema.Key, schema.Value);
                }

                foreach (var declaration in Root.Attributes().Where(a => a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.Xmlns))
                {
                    AddUnlisted(declaration.Name.LocalName, declaration.Value);
                }

                return extensions;
            }
        }

        static IEnumerable<KeyValuePair<string, string>> SchemaNamespaces()
        {
            var schemas = new List<KeyValuePair<string, string>>();
            try
            {
                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                string schemasDir = Path.Combine(currentDirectory, "Schemas");
                if (!Directory.Exists(schemasDir))
                {
                    return schemas;
                }

                foreach (var file in new DirectoryInfo(schemasDir).GetFiles("fg4_*.xsd"))
                {
                    string prefix = Path.GetFileNameWithoutExtension(file.Name).Replace("fg4_", "");
                    if (prefix.Equals("wix"))
                    {
                        continue;
                    }

                    string? targetNameSpace = (string?)XDocument.Load(file.FullName).Root?.Attribute("targetNamespace");
                    if (targetNameSpace == null)
                    {
                        continue;
                    }

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

                    schemas.Add(new KeyValuePair<string, string>(prefix, targetNameSpace));
                }
            }
            catch (Exception)
            {
            }

            return schemas;
        }
    }
}
