using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryDesigner
{
    /// <summary>The words the window uses for the document's values, in one place so the grid and the dialogs agree.</summary>
    internal static class Labels
    {
        public static readonly string[] Roots = { "HKCR", "HKCU", "HKLM", "HKU", "HKMU" };

        public static readonly string[] Kinds = { "String", "Integer (DWORD)", "Binary", "Expandable string", "Multi-string" };

        public static string Of(RegKind kind) => Kinds[(int)kind];

        public static RegKind KindOf(string? label)
        {
            var index = Array.IndexOf(Kinds, label);
            return index < 0 ? RegKind.String : (RegKind)index;
        }

        public static readonly string[] ValueActions = { "Write", "Append", "Prepend" };

        public static readonly string NotSet = "(not set)";

        public static readonly string[] RemoveActions = { NotSet, "RemoveOnInstall", "RemoveOnUninstall" };

        /// <summary>The value action for the document: null for write (the default), else append or prepend.</summary>
        public static string? ValueActionOf(string? label) =>
            string.Equals(label, "Append", StringComparison.OrdinalIgnoreCase) ? "append" :
            string.Equals(label, "Prepend", StringComparison.OrdinalIgnoreCase) ? "prepend" : null;

        public static string ValueActionLabel(string? action) =>
            ValueActions.FirstOrDefault(a => string.Equals(a, action, StringComparison.OrdinalIgnoreCase)) ?? ValueActions[0];

        public static string RemoveActionLabel(string? action) =>
            RemoveActions.FirstOrDefault(a => string.Equals(a, action, StringComparison.OrdinalIgnoreCase)) ?? NotSet;

        /// <summary>The removal action for the document: null while it is not set.</summary>
        public static string? RemoveActionOf(string? label) => label == null || label == NotSet ? null : label;

        public static readonly string[] Views = { "Default", "32-bit", "64-bit" };

        public static string ViewOf(string bitness) => bitness == "always32" ? Views[1] : bitness == "always64" ? Views[2] : Views[0];

        public static string BitnessOf(string? view) => view == Views[1] ? "always32" : view == Views[2] ? "always64" : "default";

        /// <summary>A short name for a component, for a list.</summary>
        public static string ComponentLabel(string groupId, RegistryComponentInfo component)
        {
            foreach (var scope in new[] { RegistryScope.PerMachine, RegistryScope.PerUser })
            {
                foreach (var bitness in new[] { "default", "always32", "always64" })
                {
                    if (component.Id == RegistryDocument.AutoComponentId(groupId, scope, bitness))
                    {
                        return "Registry: " + (scope == RegistryScope.PerUser ? "per-user" : "per-machine") + (bitness == "default" ? string.Empty : ", " + ViewOf(bitness));
                    }
                }
            }

            return string.IsNullOrEmpty(component.Directory) ? component.Id ?? "(no Id)" : component.Id + " (" + component.Directory + ")";
        }
    }

    /// <summary>A component to choose in a list: its Id and what to show for it.</summary>
    internal sealed class ComponentChoice
    {
        public ComponentChoice(string? id, string label)
        {
            Id = id;
            Label = label;
        }

        public string? Id { get; }

        public string Label { get; }

        public override string ToString() => Label;
    }
}
