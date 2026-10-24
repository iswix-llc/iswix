using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FireworksFramework.Authoring;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace FeaturesDesigner
{
    // The features of a package or fragment and the merge modules they reference, read and written through the
    // typed model. Top-level features are in Id order (IsWiX's canonical order); sub-features keep theirs.
    internal static class FeatureDocument
    {
        /// <summary>Gets whether the document is one this designer edits: a WiX package or fragment.</summary>
        public static bool IsValidDocument(XDocument document)
        {
            var root = document?.Root;
            var first = root?.Elements().FirstOrDefault()?.Name;
            return root?.Name == Wxs.Wix.ElementName && (first == Wxs.Package.ElementName || first == Wxs.Fragment.ElementName);
        }

        // The <Package> or <Fragment> the features belong to.
        public static WixElement Container(XDocument document) => WixElement.Wrap(document.Root.Elements().First());

        public static IEnumerable<Wxs.Feature> Features(WixElement parent) => parent switch
        {
            Wxs.Package package => package.Features,
            Wxs.Fragment fragment => fragment.Features,
            Wxs.Feature feature => feature.Features,
            _ => Enumerable.Empty<Wxs.Feature>(),
        };

        public static string SuggestNextFeatureName(XDocument document)
        {
            var ids = new HashSet<string>(document.Descendants(Wxs.Feature.ElementName).Select(f => (string)f.Attribute("Id") ?? string.Empty));
            int i = 1;
            while (ids.Contains($"NewFeature{i}"))
            {
                i++;
            }

            return $"NewFeature{i}";
        }

        // Added through the model, so a top-level feature lands in Id order and a sub-feature after its siblings.
        public static Wxs.Feature Create(WixElement parent, string id) => Add(parent, new Wxs.Feature { Id = id });

        static Wxs.Feature Add(WixElement parent, Wxs.Feature feature) => parent switch
        {
            Wxs.Package package => package.Add(feature),
            Wxs.Fragment fragment => fragment.Add(feature),
            Wxs.Feature parentFeature => parentFeature.Add(feature),
            _ => throw new ArgumentException("Features belong to a package, fragment or feature.", nameof(parent)),
        };

        // Returns why the Id cannot be used, or null when it can.
        public static string ValidateId(XDocument document, Wxs.Feature feature, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return "A feature needs an Id.";
            }

            if (document.Descendants(Wxs.Feature.ElementName).Any(f => f != feature.Xml && (string)f.Attribute("Id") == id))
            {
                return "Duplicate Feature Name";
            }

            if (!WixIds.IsValidIdentifier(id))
            {
                return "Feature names must start with an '_' or letter and may only contain '_', '.', letters or numbers.";
            }

            if (id.Length > 38)
            {
                return "Feature names must be less then 38 characters.";
            }

            return null;
        }

        // For a top-level feature a new Id is a new canonical position: the feature (with its contents) is taken out
        // and added back. A sub-feature stays where it is.
        public static void Rename(Wxs.Feature feature, string id)
        {
            var parent = feature.Parent;
            feature.Id = id;
            if (!(parent is Wxs.Feature))
            {
                feature.RemoveFromParent();
                Add(parent, feature);
            }
        }

        public static void Delete(Wxs.Feature feature) => feature.RemoveFromParent();

        public static List<Wxs.Merge> Merges(XDocument document) =>
            document.Descendants(Wxs.Merge.ElementName).Select(e => (Wxs.Merge)WixElement.Wrap(e)).ToList();

        // The merge as written, without its namespace, for the list of merge modules.
        public static string Describe(Wxs.Merge merge) =>
            "<Merge " + string.Join(" ", merge.Xml.Attributes().Where(a => !a.IsNamespaceDeclaration).Select(a => $"{a.Name.LocalName}=\"{a.Value}\"")) + " />";

        public static bool References(Wxs.Feature feature, string mergeId) => feature.MergeRefs.Any(r => r.Id == mergeId);

        // Checking a merge module adds a MergeRef; unchecking removes just that one, so the others keep their attributes.
        public static void SetReference(Wxs.Feature feature, string mergeId, bool referenced)
        {
            if (referenced && !References(feature, mergeId))
            {
                feature.Add(new Wxs.MergeRef { Id = mergeId });
            }
            else if (!referenced)
            {
                foreach (var mergeRef in feature.MergeRefs.Where(r => r.Id == mergeId).ToList())
                {
                    mergeRef.RemoveFromParent();
                }
            }
        }
    }
}
