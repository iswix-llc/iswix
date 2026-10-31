using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using IsWiX.WiXModel.Core;

namespace RegistryDesigner
{
    public enum ValidationSeverity { Warning, Error }

    public enum ValidationCode
    {
        MissingRoot,
        MissingKey,
        Duplicate,
        BadInteger,
        BadBinary,
        UnbalancedReference,
        MultipleKeyPaths,
        KeyPathConflict,
        PerUserInPerMachinePackage,
        MixedScope,
        DeprecatedKeyAction,
        PerUserNoKeyPath,
    }

    /// <summary>A problem with an entry or a component, for the designer to show beside it.</summary>
    public sealed class ValidationIssue
    {
        public ValidationCode Code { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        /// <summary>The entry the problem is with; null for a problem with a whole component.</summary>
        public RegistryItem? Item { get; set; }
        public XElement? Component { get; set; }
        /// <summary>What to do about it, when there is something to suggest.</summary>
        public string? Fix { get; set; }
    }

    /// <summary>What making the components legal did.</summary>
    public sealed class FixResult
    {
        /// <summary>The entries and keys moved to a component of their own.</summary>
        public int Moved { get; set; }

        /// <summary>The registry KeyPaths taken off values that could not keep them.</summary>
        public int KeyPathsCleared { get; set; }

        /// <summary>What was left for the user, and why.</summary>
        public List<string> Notes { get; } = new List<string>();
    }

    public sealed partial class RegistryDocument
    {
        /// <summary>
        /// Checks the group's registry entries: required fields, duplicates, the format of values by type, and the KeyPath
        /// rules. Nothing is changed; the designer shows the issues beside the entries and clears them as they are fixed.
        /// </summary>
        public List<ValidationIssue> Validate()
        {
            var issues = new List<ValidationIssue>();
            var items = GetItems();
            var perUserPackage = PackageIsPerUser();

            foreach (var item in items)
            {
                CheckFields(item, issues);
                if (item.Kind == RegistryItemKind.Value)
                {
                    CheckValue(item, issues);
                }
            }

            // A RegistryKey with an Action: deprecated, so it can't be used.
            foreach (var key in GetKeyElements().Where(k => k.Element.Attribute("Action") != null))
            {
                issues.Add(new ValidationIssue
                {
                    Code = ValidationCode.DeprecatedKeyAction,
                    Severity = ValidationSeverity.Warning,
                    Component = key.Component,
                    Message = "The Action of the key " + key.Root + "\\" + key.Key + " is deprecated and can't be used.",
                    Fix = "Set the key's options (Force create on install, Force delete on uninstall): that removes the Action.",
                });
            }

            CheckDuplicates(items, issues);
            var keyElements = GetKeyElements();
            foreach (var component in Components)
            {
                CheckComponent(component.Xml, items.Where(i => i.Component == component.Xml).ToList(), keyElements.Where(k => k.Component == component.Xml).ToList(), perUserPackage, issues);
            }

            return issues;
        }

        bool PackageIsPerUser()
        {
            var package = _wix.Xml.Elements().FirstOrDefault(e => e.Name.LocalName == "Package");
            return string.Equals((string?)package?.Attribute("Scope"), "perUser", StringComparison.OrdinalIgnoreCase);
        }

        static void CheckFields(RegistryItem item, List<ValidationIssue> issues)
        {
            if (string.IsNullOrEmpty(item.Root))
            {
                issues.Add(new ValidationIssue { Code = ValidationCode.MissingRoot, Severity = ValidationSeverity.Error, Item = item, Message = "The root (HKLM, HKCU, ...) is required." });
            }

            if (string.IsNullOrWhiteSpace(item.Key) && item.Kind != RegistryItemKind.RemoveKey)
            {
                issues.Add(new ValidationIssue { Code = ValidationCode.MissingKey, Severity = ValidationSeverity.Error, Item = item, Message = "The key is required." });
            }
            else if (string.IsNullOrWhiteSpace(item.Key))
            {
                issues.Add(new ValidationIssue { Code = ValidationCode.MissingKey, Severity = ValidationSeverity.Error, Item = item, Message = "The key to remove is required: removing a whole root is not allowed." });
            }
        }

        static void CheckValue(RegistryItem item, List<ValidationIssue> issues)
        {
            var texts = item.Type == RegKind.MultiString ? item.MultiStrings : new List<string> { item.Value ?? string.Empty };
            foreach (var text in texts)
            {
                if (!ReferencesBalanced(text))
                {
                    issues.Add(new ValidationIssue
                    {
                        Code = ValidationCode.UnbalancedReference,
                        Severity = ValidationSeverity.Error,
                        Item = item,
                        Message = "A [ or ] doesn't pair up. A literal bracket is written [\\[] or [\\]].",
                    });
                    break;
                }
            }

            var value = item.Value ?? string.Empty;
            if (RegFile.HasToken(value))
            {
                return;
            }

            if (item.Type == RegKind.Integer &&
                !(long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number >= 0 && number <= uint.MaxValue))
            {
                issues.Add(new ValidationIssue { Code = ValidationCode.BadInteger, Severity = ValidationSeverity.Error, Item = item, Message = "An integer is a whole number from 0 to 4294967295." });
            }

            if (item.Type == RegKind.Binary && RegFile.FromHex(value) == null)
            {
                issues.Add(new ValidationIssue { Code = ValidationCode.BadBinary, Severity = ValidationSeverity.Error, Item = item, Message = "Binary data is hex digits, two to a byte." });
            }
        }

        // Every [ starts a property reference that its ] ends, except the escaped literals [\[] and [\]].
        static bool ReferencesBalanced(string text)
        {
            var cleaned = text.Replace("[\\[]", string.Empty).Replace("[\\]]", string.Empty);
            var depth = 0;
            foreach (var c in cleaned)
            {
                if (c == '[')
                {
                    depth++;
                }
                else if (c == ']' && --depth < 0)
                {
                    return false;
                }
            }

            return depth == 0;
        }

        static void CheckDuplicates(List<RegistryItem> items, List<ValidationIssue> issues)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items.Where(i => i.Kind == RegistryItemKind.Value))
            {
                var view = BitnessOf(item.Component);
                if (!seen.Add(item.Root + "\u0001" + item.Key + "\u0001" + (item.Name ?? string.Empty) + "\u0001" + view))
                {
                    issues.Add(new ValidationIssue
                    {
                        Code = ValidationCode.Duplicate,
                        Severity = ValidationSeverity.Error,
                        Item = item,
                        Message = "Another value has the same root, key and name.",
                    });
                }
            }
        }

        static readonly ValidationCode[] Fixable = { ValidationCode.MixedScope, ValidationCode.MultipleKeyPaths, ValidationCode.KeyPathConflict, ValidationCode.PerUserNoKeyPath };

        /// <summary>Gets whether <see cref="MakeLegal"/> has anything to do: a component that mixes per-user and per-machine entries, or has KeyPaths that conflict.</summary>
        public bool CanMakeLegal() => Validate().Any(i => Array.IndexOf(Fixable, i.Code) >= 0);

        /// <summary>Gets whether <see cref="MakeLegal"/> needs a Directory: it has to make a component for per-user entries and there is none.</summary>
        public bool MakeLegalNeedsDirectory() =>
            Validate().Where(i => i.Code == ValidationCode.MixedScope && i.Component != null).Select(i => i.Component!).Distinct()
                .Any(c => NeedsDirectory(RegistryScope.PerUser, BitnessOf(c)));

        /// <summary>
        /// Makes the components legal. A component that mixes per-user (HKCU) entries with per-machine entries or files has
        /// the HKCU ones (values, keys and removals) moved to the group's per-user component, made if there is none (with
        /// <paramref name="directory"/>); that component gets a KeyPath if it has none and a value to give it. A component
        /// with more than one registry KeyPath keeps the first, and a registry KeyPath that is beside a file's or the
        /// component's own is taken off.
        /// </summary>
        public FixResult MakeLegal(string? directory)
        {
            var result = new FixResult();
            using var batch = _beginBatch();
            var mixed = Validate().Where(i => i.Code == ValidationCode.MixedScope && i.Component != null).Select(i => i.Component!).Distinct().ToList();
            foreach (var component in mixed)
            {
                var bitness = BitnessOf(component);
                var targetId = AutoComponentId(GroupId, RegistryScope.PerUser, bitness);
                if ((string?)component.Attribute("Id") == targetId)
                {
                    result.Notes.Add("The per-user component " + targetId + " also has per-machine entries; move those out by hand.");
                    continue;
                }

                var target = GetOrCreateAutoComponent(RegistryScope.PerUser, bitness, directory).Xml;
                foreach (var item in GetItems().Where(i => i.Component == component && !string.IsNullOrEmpty(i.Root) && ScopeOf(i.Root) == RegistryScope.PerUser).ToList())
                {
                    MoveToComponent(item, target);
                    result.Moved++;
                }

                // Keys of their own, and what is left of the keys that held values.
                foreach (var key in component.Elements().Where(e => e.Name.LocalName == "RegistryKey" && ScopeOf((string?)e.Attribute("Root")) == RegistryScope.PerUser && !string.IsNullOrEmpty((string?)e.Attribute("Root"))).ToList())
                {
                    if (!key.HasElements && key.Attributes().All(a => a.Name.LocalName == "Root" || a.Name.LocalName == "Key"))
                    {
                        // A key that only held values now moved: it creates nothing by itself.
                        DeleteElement(key, removeComponentIfEmpty: false);
                        continue;
                    }

                    var copy = new XElement(key);
                    DeleteElement(key, removeComponentIfEmpty: false);
                    WixElement.Wrap(target).AddElement(copy);
                    result.Moved++;
                }

                EnsurePerUserKeyPath(target, result);
            }

            // A per-user component with nothing to be its KeyPath has its Directory for one, which is per-machine.
            foreach (var component in Validate().Where(i => i.Code == ValidationCode.PerUserNoKeyPath && i.Component != null).Select(i => i.Component!).Distinct().ToList())
            {
                EnsurePerUserKeyPath(component, result);
            }

            // KeyPath rules, per component.
            foreach (var component in Components.Select(c => c.Xml).ToList())
            {
                var values = GetItems().Where(i => i.Component == component && i.Kind == RegistryItemKind.Value && i.KeyPath).ToList();
                var otherKeyPath = component.Elements().Any(e => e.Name.LocalName == "File" && IsYes((string?)e.Attribute("KeyPath"))) || IsYes((string?)component.Attribute("KeyPath"));
                foreach (var extra in values.Skip(otherKeyPath ? 0 : 1))
                {
                    ClearKeyPath(extra);
                    result.KeyPathsCleared++;
                }
            }

            return result;
        }

        // Gives a per-user component a registry value as its KeyPath: one it has, or a value made under its first key.
        void EnsurePerUserKeyPath(XElement component, FixResult result)
        {
            var values = GetItems().Where(i => i.Component == component && i.Kind == RegistryItemKind.Value && ScopeOf(i.Root) == RegistryScope.PerUser).ToList();
            if (values.Any(v => v.KeyPath) || IsYes((string?)component.Attribute("KeyPath")))
            {
                return;
            }

            if (values.Count > 0)
            {
                SetKeyPath(values[0]);
                return;
            }

            var firstKey = GetKeyElements().FirstOrDefault(k => k.Component == component && ScopeOf(k.Root) == RegistryScope.PerUser);
            if (firstKey != null)
            {
                var spec = new RegistryValueSpec { Root = firstKey.Root ?? "HKCU", Key = firstKey.Key, Name = "installed", Type = RegKind.Integer, Value = "1" };
                SetKeyPath(AddValue(spec, component));
                result.Notes.Add("The per-user component had only keys, so it had its Directory as its KeyPath, which is per-machine. Added the value " + spec.Root + "\\" + spec.Key + "\\installed = 1 as its KeyPath; rename it or change its data if you like.");
            }
        }

        static void CheckComponent(XElement component, List<RegistryItem> items, List<RegistryKeyInfo> keys, bool perUserPackage, List<ValidationIssue> issues)
        {
            if (items.Count == 0 && keys.Count == 0)
            {
                return;
            }

            var values = items.Where(i => i.Kind == RegistryItemKind.Value).ToList();
            var registryKeyPaths = values.Count(v => v.KeyPath);
            var fileKeyPaths = component.Elements().Count(e => e.Name.LocalName == "File" && IsYes((string?)e.Attribute("KeyPath")));
            var componentKeyPath = IsYes((string?)component.Attribute("KeyPath"));
            var hasFiles = component.Elements().Any(e => e.Name.LocalName == "File");

            if (registryKeyPaths > 1)
            {
                issues.Add(new ValidationIssue
                {
                    Code = ValidationCode.MultipleKeyPaths,
                    Severity = ValidationSeverity.Error,
                    Component = component,
                    Message = "More than one value of the component is its KeyPath.",
                    Fix = "Keep one KeyPath.",
                });
            }

            if (registryKeyPaths > 0 && (fileKeyPaths > 0 || componentKeyPath))
            {
                issues.Add(new ValidationIssue
                {
                    Code = ValidationCode.KeyPathConflict,
                    Severity = ValidationSeverity.Error,
                    Component = component,
                    Message = fileKeyPaths > 0
                        ? "A registry value and a file are both the component's KeyPath."
                        : "A registry value is the KeyPath of a component whose KeyPath is the component itself (as a folder's component).",
                    Fix = "Make only one of them the KeyPath.",
                });
            }

            // Scope: the entries' roots, the keys' own (a key kept on its own has no value to show it), and the files, which are per-machine.
            var perUser = values.Concat(items.Where(i => i.Kind != RegistryItemKind.Value)).Where(i => ScopeOf(i.Root) == RegistryScope.PerUser).ToList();
            var perMachine = items.Where(i => ScopeOf(i.Root) == RegistryScope.PerMachine && !string.IsNullOrEmpty(i.Root)).ToList();
            var perUserKeys = keys.Where(k => ScopeOf(k.Root) == RegistryScope.PerUser && !k.Element.Descendants().Any(e => e.Name.LocalName == "RegistryValue")).ToList();
            var perMachineKeys = keys.Where(k => ScopeOf(k.Root) == RegistryScope.PerMachine && !string.IsNullOrEmpty(k.Root)).ToList();
            if (perUser.Count + perUserKeys.Count > 0 && (perMachine.Count + perMachineKeys.Count > 0 || hasFiles))
            {
                issues.Add(new ValidationIssue
                {
                    Code = ValidationCode.MixedScope,
                    Severity = ValidationSeverity.Warning,
                    Component = component,
                    Message = "The component has per-user (HKCU) entries and also " + (perMachine.Count + perMachineKeys.Count > 0 ? "per-machine entries" : "files") + " (ICE57).",
                    Fix = "Move the HKCU entries to a component of their own.",
                });
            }

            // Only per-user entries and no registry KeyPath: the component's Directory is its KeyPath, which is per-machine (ICE57).
            if (perUser.Count + perUserKeys.Count > 0 && perMachine.Count + perMachineKeys.Count == 0 && !hasFiles &&
                registryKeyPaths == 0 && !componentKeyPath && fileKeyPaths == 0)
            {
                issues.Add(new ValidationIssue
                {
                    Code = ValidationCode.PerUserNoKeyPath,
                    Severity = ValidationSeverity.Warning,
                    Component = component,
                    Message = "The per-user (HKCU) component has no registry value as its KeyPath, so its Directory is its KeyPath, which is per-machine (ICE57).",
                    Fix = "Make one of its values the KeyPath, or use Fix Components.",
                });
            }

            // The component the warning below suggests (nothing but per-user entries, a registry value as its KeyPath) is not warned about.
            var cleanPerUser = perUser.Count + perUserKeys.Count > 0 && perMachine.Count + perMachineKeys.Count == 0 && !hasFiles && registryKeyPaths > 0 && !componentKeyPath && fileKeyPaths == 0;
            if (!perUserPackage && !cleanPerUser)
            {
                foreach (var item in perUser)
                {
                    issues.Add(new ValidationIssue
                    {
                        Code = ValidationCode.PerUserInPerMachinePackage,
                        Severity = ValidationSeverity.Warning,
                        Item = item,
                        Component = component,
                        Message = hasFiles
                            ? "A per-user (HKCU) entry in a per-machine package, in a component that also holds files."
                            : "A per-user (HKCU) entry in a per-machine package.",
                        Fix = "Put per-user entries in a component of their own, with a registry value of it as its KeyPath.",
                    });
                }

                foreach (var key in perUserKeys)
                {
                    issues.Add(new ValidationIssue
                    {
                        Code = ValidationCode.PerUserInPerMachinePackage,
                        Severity = ValidationSeverity.Warning,
                        Component = component,
                        Message = "The per-user (HKCU) key " + key.Root + "\\" + key.Key + " is in a per-machine package.",
                        Fix = "Put per-user entries in a component of their own, with a registry value of it as its KeyPath.",
                    });
                }
            }
        }
    }
}
