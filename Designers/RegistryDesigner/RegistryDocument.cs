using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace RegistryDesigner
{
    public enum RegistryItemKind { Value, RemoveKey, RemoveValue }

    /// <summary>Whether entries are written for the machine or for the installing user.</summary>
    public enum RegistryScope { PerMachine, PerUser }

    /// <summary>A registry value, or a registry key or value to remove, as the designer's grid shows it.</summary>
    public sealed class RegistryItem
    {
        public RegistryItemKind Kind { get; set; }
        /// <summary>HKCR, HKCU, HKLM, HKU or HKMU; null if the element has none.</summary>
        public string? Root { get; set; }
        /// <summary>The key's full path, below the root.</summary>
        public string Key { get; set; } = string.Empty;
        public string? Name { get; set; }
        public RegKind Type { get; set; }
        public string? Value { get; set; }
        public List<string> MultiStrings { get; } = new List<string>();
        public bool KeyPath { get; set; }
        /// <summary>For a value: Write, Append or Prepend. For a key to remove: RemoveOnInstall or RemoveOnUninstall.</summary>
        public string? Action { get; set; }
        public XElement Element { get; set; } = null!;
        public XElement Component { get; set; } = null!;
    }

    /// <summary>The fields of a registry value, for adding one or changing one.</summary>
    public sealed class RegistryValueSpec
    {
        public string Root { get; set; } = "HKLM";
        public string Key { get; set; } = string.Empty;
        public string? Name { get; set; }
        public RegKind Type { get; set; } = RegKind.String;
        public string? Value { get; set; }
        public List<string> MultiStrings { get; } = new List<string>();
        /// <summary>write, append or prepend; null for the default (write).</summary>
        public string? Action { get; set; }
    }

    /// <summary>A component of the group, for the attach-to-component choice and the component's properties.</summary>
    public sealed class RegistryComponentInfo
    {
        public string? Id { get; set; }
        public string? Directory { get; set; }
        /// <summary>default, always32 or always64.</summary>
        public string Bitness { get; set; } = "default";
        public bool HasFiles { get; set; }
        public bool HasRegistry { get; set; }
        /// <summary>One of the designer's own (see <see cref="RegistryDocument.AutoComponentPrefix"/>).</summary>
        public bool IsAutoManaged { get; set; }
        /// <summary>The component's DisableRegistryReflection; null while it is unset.</summary>
        public bool? DisableRegistryReflection { get; set; }
        public string? Condition { get; set; }
        public XElement Element { get; set; } = null!;
    }

    /// <summary>A RegistryKey element, which may hold values or stand on its own.</summary>
    public sealed class RegistryKeyInfo
    {
        public string? Root { get; set; }
        /// <summary>The key's full path, below the root.</summary>
        public string Key { get; set; } = string.Empty;
        public XElement Element { get; set; } = null!;
        public XElement Component { get; set; } = null!;
    }

    /// <summary>
    /// The registry entries of one component group, read and written through the typed model, without any user
    /// interface. Registry entries share the group with Files and Folders, so this only ever adds to or deletes from
    /// components that hold registry entries, and its own components have an Id of their own (<c>owr</c> and a hash).
    /// </summary>
    public sealed partial class RegistryDocument
    {
        /// <summary>The start of the Id of a component this designer made. Files and Folders' own start with owd.</summary>
        public const string AutoComponentPrefix = "owr";

        static readonly string[] BitnessNames = { "default", "always32", "always64" };

        readonly XDocument _document;
        readonly Wxs.Wix _wix;
        readonly Func<IDisposable> _beginBatch;
        Wxs.ComponentGroup? _group;

        /// <summary>
        /// Opens the registry entries of the group with <paramref name="groupId"/>, or failing that the first group of the file.
        /// </summary>
        /// <param name="beginBatch">Starts a batch of edits (such as <c>DocumentManager.Batch</c>); null for none.</param>
        public RegistryDocument(XDocument document, string? groupId = null, Func<IDisposable>? beginBatch = null)
        {
            _document = document;
            _wix = Wxs.WixDocument.FromXml(document).Root;
            _beginBatch = beginBatch ?? (() => new NoBatch());
            _group = (groupId == null ? null : FindGroup(groupId)) ?? AllGroups.FirstOrDefault();
        }

        sealed class NoBatch : IDisposable
        {
            public void Dispose()
            {
            }
        }

        /// <summary>Gets whether the document is one this designer edits: a WiX fragment or package.</summary>
        public static bool IsValidDocument(XDocument document)
        {
            var root = document?.Root;
            var first = root?.Elements().FirstOrDefault()?.Name;
            return root?.Name == Wxs.Wix.ElementName && (first == Wxs.Fragment.ElementName || first == Wxs.Package.ElementName);
        }

        // ---------------------------------------------------------------- groups

        IEnumerable<Wxs.ComponentGroup> AllGroups => _wix.Xml.Elements()
            .Where(e => e.Name == Wxs.Fragment.ElementName || e.Name == Wxs.Package.ElementName)
            .SelectMany(e => e.Elements(Wxs.ComponentGroup.ElementName))
            .Select(e => (Wxs.ComponentGroup)WixElement.Wrap(e));

        Wxs.ComponentGroup? FindGroup(string id) => AllGroups.FirstOrDefault(g => string.Equals(g.Id, id, StringComparison.Ordinal));

        public bool HasGroup => _group != null;

        public string GroupId => _group?.Id ?? string.Empty;

        /// <summary>Gets the Ids of the component groups of the file, in the file's order.</summary>
        public List<string> GetGroupIds() => AllGroups.Select(g => g.Id).Where(id => !string.IsNullOrEmpty(id)).Select(id => id!).Distinct(StringComparer.Ordinal).ToList();

        // ---------------------------------------------------------------- components

        IEnumerable<Wxs.Component> Components => _group?.Components ?? Enumerable.Empty<Wxs.Component>();

        static bool IsYes(string? text) => string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);

        static bool IsRegistryElement(XElement element)
        {
            var name = element.Name.LocalName;
            return name == "RegistryKey" || name == "RegistryValue" || name == "RemoveRegistryKey" || name == "RemoveRegistryValue";
        }

        static string BitnessOf(XElement component)
        {
            var text = (string?)component.Attribute("Bitness");
            return BitnessNames.FirstOrDefault(n => string.Equals(n, text, StringComparison.OrdinalIgnoreCase)) ?? "default";
        }

        /// <summary>Gets the components of the group, for choosing the one an entry belongs to.</summary>
        public List<RegistryComponentInfo> GetComponents() => Components.Select(c => new RegistryComponentInfo
        {
            Id = c.Id,
            Directory = c.Directory,
            Bitness = BitnessOf(c.Xml),
            HasFiles = c.Xml.Elements().Any(e => e.Name.LocalName == "File"),
            HasRegistry = c.Xml.Elements().Any(IsRegistryElement),
            IsAutoManaged = c.Id != null && c.Id.StartsWith(AutoComponentPrefix, StringComparison.Ordinal),
            DisableRegistryReflection = ReadBool((string?)c.Xml.Attribute("DisableRegistryReflection")),
            Condition = (string?)c.Xml.Attribute("Condition"),
            Element = c.Xml,
        }).ToList();

        static bool? ReadBool(string? text) => text == null ? (bool?)null : IsYes(text);

        /// <summary>Gets the component group's element, or null while the file has none.</summary>
        public XElement? GroupElement => _group?.Xml;

        /// <summary>Gets the RegistryKey elements of the group, with their full keys.</summary>
        public List<RegistryKeyInfo> GetKeyElements()
        {
            var keys = new List<RegistryKeyInfo>();
            foreach (var component in Components)
            {
                foreach (var child in component.Xml.Elements().Where(e => e.Name.LocalName == "RegistryKey"))
                {
                    CollectKeys(child, component.Xml, null, null, keys);
                }
            }

            return keys;
        }

        static void CollectKeys(XElement element, XElement component, string? parentRoot, string? parentKey, List<RegistryKeyInfo> keys)
        {
            var root = (string?)element.Attribute("Root") ?? parentRoot;
            var key = Join(parentKey, (string?)element.Attribute("Key"));
            keys.Add(new RegistryKeyInfo { Root = root, Key = key, Element = element, Component = component });
            foreach (var child in element.Elements().Where(e => e.Name.LocalName == "RegistryKey"))
            {
                CollectKeys(child, component, root, key, keys);
            }
        }

        /// <summary>
        /// Deletes what is at this key: its values, the keys and values to remove there, and a RegistryKey of its own that
        /// has nothing in it. With <paramref name="includeSubkeys"/> it also deletes all of that for the keys below it.
        /// Returns how many entries and keys went.
        /// </summary>
        public int DeleteKey(string root, string key, bool includeSubkeys = false)
        {
            using var batch = _beginBatch();
            bool Matches(string? candidateRoot, string candidateKey) =>
                string.Equals(candidateRoot, root, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(candidateKey, key, StringComparison.OrdinalIgnoreCase) ||
                    (includeSubkeys && candidateKey.StartsWith(key + "\\", StringComparison.OrdinalIgnoreCase)));

            var count = 0;
            foreach (var item in GetItems().Where(i => Matches(i.Root, i.Key)))
            {
                DeleteElement(item.Element, removeComponentIfEmpty: true);
                count++;
            }

            // A key left on its own, or that held only other keys: deepest first, so a parent that is empty by then goes too.
            foreach (var info in GetKeyElements().Where(k => Matches(k.Root, k.Key)).OrderByDescending(k => k.Key.Length).ToList())
            {
                if (info.Element.Parent != null && HoldsOnlyPermissions(info.Element))
                {
                    DeleteElement(info.Element, removeComponentIfEmpty: true);
                    count++;
                }
            }

            return count;
        }

        static readonly string[] StandardDirectories =
        {
            "TARGETDIR", "INSTALLFOLDER", "ProgramFiles6432Folder", "ProgramFilesFolder", "ProgramFiles64Folder", "CommonFiles6432Folder", "CommonFilesFolder",
            "CommonAppDataFolder", "AppDataFolder", "LocalAppDataFolder", "SystemFolder", "System64Folder", "WindowsFolder", "TempFolder", "DesktopFolder", "ProgramMenuFolder",
        };

        /// <summary>
        /// Gets the directories to offer for a component: those the file defines or its components use, then the standard ones.
        /// </summary>
        public List<string> GetKnownDirectories()
        {
            var known = new List<string>();
            void Add(string? id)
            {
                if (!string.IsNullOrWhiteSpace(id) && !known.Contains(id!, StringComparer.Ordinal))
                {
                    known.Add(id!);
                }
            }

            foreach (var element in _document.Descendants())
            {
                var name = element.Name.LocalName;
                if (name == "Directory" || name == "StandardDirectory" || name == "DirectoryRef")
                {
                    Add((string?)element.Attribute("Id"));
                }
                else if (name == "Component")
                {
                    Add((string?)element.Attribute("Directory"));
                }
            }

            foreach (var standard in StandardDirectories)
            {
                Add(standard);
            }

            return known;
        }

        /// <summary>
        /// Gets the directory to pre-select for a new component of the group's own: the one the group's other components
        /// (those Files and Folders makes) use most, else TARGETDIR.
        /// </summary>
        /// <summary>
        /// Gets the directory to pre-select for components of the given scopes: a folder in the user's profile when
        /// every one is per-user (a per-user component belongs there), else <see cref="SuggestDirectory()"/>.
        /// </summary>
        public string SuggestDirectory(IEnumerable<RegistryScope> scopes)
        {
            var list = scopes.ToList();
            return list.Count > 0 && list.All(s => s == RegistryScope.PerUser) ? "LocalAppDataFolder" : SuggestDirectory();
        }

        public string SuggestDirectory() => Components
            .Where(c => c.Id == null || !c.Id.StartsWith(AutoComponentPrefix, StringComparison.Ordinal))
            .Select(c => c.Directory)
            .Where(d => !string.IsNullOrEmpty(d))
            .GroupBy(d => d, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key!)
            .FirstOrDefault() ?? "TARGETDIR";

        /// <summary>
        /// The registry scope the root belongs to. HKCU is per user; the rest, including HKMU (which follows ALLUSERS),
        /// are per machine.
        /// </summary>
        public static RegistryScope ScopeOf(string? root) => string.Equals(root, "HKCU", StringComparison.OrdinalIgnoreCase) ? RegistryScope.PerUser : RegistryScope.PerMachine;

        /// <summary>The Id of the component this designer makes for a scope and registry view in a group.</summary>
        public static string AutoComponentId(string groupId, RegistryScope scope, string bitness)
        {
            using var sha = SHA256.Create();
            var data = sha.ComputeHash(Encoding.UTF8.GetBytes((groupId + "\\" + scope + "\\" + bitness).ToUpperInvariant()));
            return AutoComponentPrefix + string.Concat(data.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
        }

        /// <summary>Gets whether adding an entry for the scope and view must say which Directory to use: the group has no component for it yet.</summary>
        public bool NeedsDirectory(RegistryScope scope, string bitness = "default") =>
            !Components.Any(c => c.Id == AutoComponentId(GroupId, scope, bitness));

        Wxs.Component GetOrCreateAutoComponent(RegistryScope scope, string bitness, string? directory)
        {
            EnsureGroup();
            var id = AutoComponentId(GroupId, scope, bitness);
            var existing = Components.FirstOrDefault(c => c.Id == id);
            if (existing != null)
            {
                return existing;
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("The component for these entries doesn't exist yet: a Directory is needed.", nameof(directory));
            }

            var component = new Wxs.Component { Id = id, Directory = directory, Guid = Guid.NewGuid().ToString() };
            if (bitness == "always32")
            {
                component.Bitness = Wxs.BitnessType.Always32;
            }
            else if (bitness == "always64")
            {
                component.Bitness = Wxs.BitnessType.Always64;
            }

            return _group!.Add(component);
        }

        void EnsureGroup()
        {
            if (_group == null)
            {
                throw new InvalidOperationException("The file has no component group.");
            }
        }

        Wxs.Component ComponentFor(XElement? component, string? root, string bitness, string? directory) =>
            component != null ? (Wxs.Component)WixElement.Wrap(component) : GetOrCreateAutoComponent(ScopeOf(root), bitness, directory);

        // ---------------------------------------------------------------- reading entries

        /// <summary>Gets every registry value, and every registry key or value to remove, in the group.</summary>
        public List<RegistryItem> GetItems()
        {
            var items = new List<RegistryItem>();
            foreach (var component in Components)
            {
                foreach (var child in component.Xml.Elements())
                {
                    Collect(child, component.Xml, null, null, items);
                }
            }

            return items;
        }

        static string Join(string? parent, string? child)
        {
            if (string.IsNullOrEmpty(parent))
            {
                return child ?? string.Empty;
            }

            return string.IsNullOrEmpty(child) ? parent! : parent + "\\" + child;
        }

        static void Collect(XElement element, XElement component, string? parentRoot, string? parentKey, List<RegistryItem> items)
        {
            switch (element.Name.LocalName)
            {
                case "RegistryKey":
                {
                    var root = (string?)element.Attribute("Root") ?? parentRoot;
                    var key = Join(parentKey, (string?)element.Attribute("Key"));
                    foreach (var child in element.Elements())
                    {
                        Collect(child, component, root, key, items);
                    }

                    break;
                }

                case "RegistryValue":
                    items.Add(ReadValue(element, component, parentRoot, parentKey));
                    break;
                case "RemoveRegistryKey":
                    items.Add(new RegistryItem
                    {
                        Kind = RegistryItemKind.RemoveKey,
                        Root = (string?)element.Attribute("Root") ?? parentRoot,
                        Key = Join(parentKey, (string?)element.Attribute("Key")),
                        Action = (string?)element.Attribute("Action"),
                        Element = element,
                        Component = component,
                    });
                    break;
                case "RemoveRegistryValue":
                    items.Add(new RegistryItem
                    {
                        Kind = RegistryItemKind.RemoveValue,
                        Root = (string?)element.Attribute("Root") ?? parentRoot,
                        Key = Join(parentKey, (string?)element.Attribute("Key")),
                        Name = (string?)element.Attribute("Name"),
                        Element = element,
                        Component = component,
                    });
                    break;
            }
        }

        static RegistryItem ReadValue(XElement element, XElement component, string? parentRoot, string? parentKey)
        {
            var item = new RegistryItem
            {
                Kind = RegistryItemKind.Value,
                Root = (string?)element.Attribute("Root") ?? parentRoot,
                Key = Join(parentKey, (string?)element.Attribute("Key")),
                Name = (string?)element.Attribute("Name"),
                Type = ReadKind((string?)element.Attribute("Type")),
                Value = (string?)element.Attribute("Value"),
                KeyPath = IsYes((string?)element.Attribute("KeyPath")),
                Action = (string?)element.Attribute("Action"),
                Element = element,
                Component = component,
            };
            foreach (var child in element.Elements().Where(e => e.Name.LocalName == "MultiStringValue" || e.Name.LocalName == "MultiString"))
            {
                item.MultiStrings.Add((string?)child.Attribute("Value") ?? child.Value);
            }

            return item;
        }

        static RegKind ReadKind(string? text)
        {
            switch ((text ?? string.Empty).ToLowerInvariant())
            {
                case "integer": return RegKind.Integer;
                case "binary": return RegKind.Binary;
                case "expandable": return RegKind.Expandable;
                case "multistring": return RegKind.MultiString;
                default: return RegKind.String;
            }
        }

        static Wxs.RegistryValueType ToModel(RegKind kind)
        {
            switch (kind)
            {
                case RegKind.Integer: return Wxs.RegistryValueType.Integer;
                case RegKind.Binary: return Wxs.RegistryValueType.Binary;
                case RegKind.Expandable: return Wxs.RegistryValueType.Expandable;
                case RegKind.MultiString: return Wxs.RegistryValueType.MultiString;
                default: return Wxs.RegistryValueType.String;
            }
        }

        static Wxs.RegistryRootType ToRoot(string root)
        {
            if (!Enum.TryParse<Wxs.RegistryRootType>(root, true, out var parsed))
            {
                throw new ArgumentException("'" + root + "' is not a registry root (HKCR, HKCU, HKLM, HKU or HKMU).", nameof(root));
            }

            return parsed;
        }

        // ---------------------------------------------------------------- adding and changing entries

        /// <summary>
        /// Adds a value. Without a component it goes in the group's component for its scope and view (made if there is none,
        /// which takes <paramref name="directory"/>; see <see cref="NeedsDirectory"/>).
        /// </summary>
        /// <param name="component">The component to add it to, or null for the group's own registry component.</param>
        /// <param name="directory">The Directory of a new component of the group's own.</param>
        /// <param name="bitness">The registry view of a new component of the group's own: default, always32 or always64.</param>
        public RegistryItem AddValue(RegistryValueSpec spec, XElement? component = null, string? directory = null, string bitness = "default")
        {
            using var batch = _beginBatch();
            var target = ComponentFor(component, spec.Root, bitness, directory);
            var value = target.Add(new Wxs.RegistryValue());
            Apply(value, spec);
            return GetItems().First(i => i.Element == value.Xml);
        }

        /// <summary>Adds a registry key, which is kept even if it has no values (it is created on install).</summary>
        public XElement AddKey(string root, string key, XElement? component = null, string? directory = null, string bitness = "default")
        {
            using var batch = _beginBatch();
            var target = ComponentFor(component, root, bitness, directory);
            // The key's Action is deprecated; ForceCreateOnInstall is what makes WiX create a key that has nothing in it.
            var element = target.Add(new Wxs.RegistryKey { Root = ToRoot(root), Key = key, ForceCreateOnInstall = true });
            return element.Xml;
        }

        /// <summary>Adds a registry key to remove.</summary>
        /// <param name="action">RemoveOnInstall or RemoveOnUninstall; null leaves it for the user to choose.</param>
        public RegistryItem AddRemoveKey(string root, string key, string? action = null, XElement? component = null, string? directory = null, string bitness = "default")
        {
            using var batch = _beginBatch();
            var target = ComponentFor(component, root, bitness, directory);
            var element = target.Add(new Wxs.RemoveRegistryKey { Root = ToRoot(root), Key = key });
            if (action != null)
            {
                element.Action = ParseRemoveAction(action);
            }

            return GetItems().First(i => i.Element == element.Xml);
        }

        /// <summary>Adds a registry value to remove.</summary>
        public RegistryItem AddRemoveValue(string root, string key, string? name, XElement? component = null, string? directory = null, string bitness = "default")
        {
            using var batch = _beginBatch();
            var target = ComponentFor(component, root, bitness, directory);
            var element = target.Add(new Wxs.RemoveRegistryValue { Root = ToRoot(root), Key = key, Name = name });
            return GetItems().First(i => i.Element == element.Xml);
        }

        static Wxs.RemoveRegistryKeyAction ParseRemoveAction(string action)
        {
            if (!Enum.TryParse<Wxs.RemoveRegistryKeyAction>(action, true, out var parsed))
            {
                throw new ArgumentException("'" + action + "' is not a removal action (RemoveOnInstall or RemoveOnUninstall).", nameof(action));
            }

            return parsed;
        }

        /// <summary>Sets when a registry key is removed: RemoveOnInstall, RemoveOnUninstall, or null to leave it unset.</summary>
        public void SetRemoveAction(RegistryItem item, string? action)
        {
            if (item.Kind != RegistryItemKind.RemoveKey)
            {
                throw new InvalidOperationException("Only a registry key to remove has an action.");
            }

            using var batch = _beginBatch();
            var element = (Wxs.RemoveRegistryKey)WixElement.Wrap(item.Element);
            if (action == null)
            {
                element.Xml.SetAttributeValue("Action", null);
            }
            else
            {
                element.Action = ParseRemoveAction(action);
            }
        }

        /// <summary>Changes the fields of a value. A value inside a RegistryKey that is given another root or key becomes a value of its own in the same component.</summary>
        public RegistryItem UpdateValue(RegistryItem item, RegistryValueSpec spec)
        {
            if (item.Kind != RegistryItemKind.Value)
            {
                throw new InvalidOperationException("Only a registry value can be changed this way.");
            }

            using var batch = _beginBatch();
            var element = item.Element;
            var nested = element.Parent != null && element.Parent.Name.LocalName == "RegistryKey";
            var sameKey = string.Equals(item.Root, spec.Root, StringComparison.OrdinalIgnoreCase) && string.Equals(item.Key, spec.Key, StringComparison.Ordinal);
            if (nested && !sameKey)
            {
                var component = (Wxs.Component)WixElement.Wrap(item.Component);
                var keyPath = item.KeyPath;
                DeleteElement(element, removeComponentIfEmpty: false);
                var fresh = component.Add(new Wxs.RegistryValue());
                Apply(fresh, spec);
                if (keyPath)
                {
                    fresh.KeyPath = true;
                }

                return GetItems().First(i => i.Element == fresh.Xml);
            }

            var value = (Wxs.RegistryValue)WixElement.Wrap(element);
            if (nested)
            {
                // The root and key belong to the RegistryKey around it.
                ApplyContent(value, spec);
            }
            else
            {
                Apply(value, spec);
            }

            return GetItems().First(i => i.Element == element);
        }

        static void Apply(Wxs.RegistryValue value, RegistryValueSpec spec)
        {
            value.Root = ToRoot(spec.Root);
            value.Key = spec.Key;
            ApplyContent(value, spec);
        }

        static void ApplyContent(Wxs.RegistryValue value, RegistryValueSpec spec)
        {
            value.Name = string.IsNullOrEmpty(spec.Name) ? null : spec.Name;
            value.Type = ToModel(spec.Type);
            if (string.Equals(spec.Action, "append", StringComparison.OrdinalIgnoreCase))
            {
                value.Action = Wxs.RegistryValueAction.Append;
            }
            else if (string.Equals(spec.Action, "prepend", StringComparison.OrdinalIgnoreCase))
            {
                value.Action = Wxs.RegistryValueAction.Prepend;
            }
            else
            {
                // Write is the default: left out.
                value.Xml.SetAttributeValue("Action", null);
            }

            // A multi-string is one MultiStringValue per string; the others have their Value.
            foreach (var old in value.Xml.Elements().Where(e => e.Name.LocalName == "MultiStringValue" || e.Name.LocalName == "MultiString").ToList())
            {
                WixElement.Wrap(old).RemoveFromParent();
            }

            if (spec.Type == RegKind.MultiString)
            {
                value.Value = null;
                foreach (var text in spec.MultiStrings)
                {
                    value.Add(new Wxs.MultiStringValue { Value = text });
                }
            }
            else
            {
                value.Value = spec.Value;
            }
        }

        /// <summary>Makes a value its component's KeyPath, and not any other registry value of the component.</summary>
        public void SetKeyPath(RegistryItem item)
        {
            if (item.Kind != RegistryItemKind.Value)
            {
                throw new InvalidOperationException("Only a registry value can be a KeyPath.");
            }

            using var batch = _beginBatch();
            foreach (var other in GetItems().Where(i => i.Kind == RegistryItemKind.Value && i.Component == item.Component && i.KeyPath && i.Element != item.Element))
            {
                other.Element.SetAttributeValue("KeyPath", null);
            }

            ((Wxs.RegistryValue)WixElement.Wrap(item.Element)).KeyPath = true;
            item.KeyPath = true;
        }

        /// <summary>Clears a value's KeyPath.</summary>
        public void ClearKeyPath(RegistryItem item)
        {
            using var batch = _beginBatch();
            item.Element.SetAttributeValue("KeyPath", null);
            item.KeyPath = false;
        }

        /// <summary>Moves an entry to another component of the group. Its KeyPath stays only if the new component has none.</summary>
        public RegistryItem MoveToComponent(RegistryItem item, XElement component)
        {
            using var batch = _beginBatch();
            if (item.Component == component)
            {
                return item;
            }

            var targetHasKeyPath = GetItems().Any(i => i.Component == component && i.KeyPath) ||
                IsYes((string?)component.Attribute("KeyPath")) ||
                component.Elements().Any(e => e.Name.LocalName == "File" && IsYes((string?)e.Attribute("KeyPath")));
            var element = item.Element;
            var oldComponent = item.Component;

            // A value or key inside a RegistryKey is written out as one of its own: the key it sat in stays behind.
            var copy = new XElement(element);
            if (item.Kind == RegistryItemKind.Value || item.Kind == RegistryItemKind.RemoveKey || item.Kind == RegistryItemKind.RemoveValue)
            {
                copy.SetAttributeValue("Root", item.Root);
                copy.SetAttributeValue("Key", item.Key);
            }

            if (item.KeyPath && targetHasKeyPath)
            {
                copy.SetAttributeValue("KeyPath", null);
            }

            DeleteElement(element, removeComponentIfEmpty: true);
            var wrapped = WixElement.Wrap(component).AddElement(copy);
            _ = oldComponent;
            return GetItems().First(i => i.Element == wrapped.Xml);
        }

        /// <summary>Removes an entry. A component of the designer's own with nothing left is removed too.</summary>
        public void DeleteItem(RegistryItem item)
        {
            using var batch = _beginBatch();
            DeleteElement(item.Element, removeComponentIfEmpty: true);
        }

        void DeleteElement(XElement element, bool removeComponentIfEmpty)
        {
            var parent = element.Parent;
            WixElement.Wrap(element).RemoveFromParent();

            // A RegistryKey left with nothing in it goes too (unless it was added on its own: it has an Action of create).
            while (parent != null && parent.Name.LocalName == "RegistryKey" && !parent.HasElements)
            {
                var next = parent.Parent;
                WixElement.Wrap(parent).RemoveFromParent();
                parent = next;
            }

            if (removeComponentIfEmpty && parent != null && parent.Name.LocalName == "Component" && !parent.HasElements)
            {
                var id = (string?)parent.Attribute("Id");
                if (id != null && id.StartsWith(AutoComponentPrefix, StringComparison.Ordinal))
                {
                    WixElement.Wrap(parent).RemoveFromParent();
                }
            }
        }

        // ---------------------------------------------------------------- component properties

        /// <summary>Sets the registry view of a component: default, always32 or always64.</summary>
        public void SetBitness(XElement component, string bitness)
        {
            using var batch = _beginBatch();
            var wrapped = (Wxs.Component)WixElement.Wrap(component);
            switch (bitness)
            {
                case "always32": wrapped.Bitness = Wxs.BitnessType.Always32; break;
                case "always64": wrapped.Bitness = Wxs.BitnessType.Always64; break;
                case "default": component.SetAttributeValue("Bitness", null); break;
                default: throw new ArgumentException("'" + bitness + "' is not a registry view (default, always32 or always64).", nameof(bitness));
            }
        }

        /// <summary>Sets DisableRegistryReflection on a component; null leaves it unset.</summary>
        public void SetDisableRegistryReflection(XElement component, bool? disable)
        {
            using var batch = _beginBatch();
            if (disable == null)
            {
                component.SetAttributeValue("DisableRegistryReflection", null);
            }
            else
            {
                ((Wxs.Component)WixElement.Wrap(component)).DisableRegistryReflection = disable.Value;
            }
        }

        /// <summary>Sets the Condition of a component; empty clears it.</summary>
        public void SetCondition(XElement component, string? condition)
        {
            using var batch = _beginBatch();
            ((Wxs.Component)WixElement.Wrap(component)).Condition = string.IsNullOrWhiteSpace(condition) ? null : condition;
        }

        /// <summary>Sets the Directory of a component.</summary>
        public void SetDirectory(XElement component, string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("A component needs a Directory.", nameof(directory));
            }

            using var batch = _beginBatch();
            ((Wxs.Component)WixElement.Wrap(component)).Directory = directory;
        }

        // ---------------------------------------------------------------- key options

        static bool KeyIs(RegistryKeyInfo info, string root, string key) =>
            string.Equals(info.Root, root, StringComparison.OrdinalIgnoreCase) && string.Equals(info.Key, key, StringComparison.OrdinalIgnoreCase);

        /// <summary>Gets the options of the key, from the RegistryKey element at it (all unset when there is none).</summary>
        public KeyOptions GetKeyOptions(string root, string key)
        {
            var info = GetKeyElements().FirstOrDefault(k => KeyIs(k, root, key));
            if (info == null)
            {
                return new KeyOptions();
            }

            return new KeyOptions
            {
                HasKeyElement = true,
                ForceCreateOnInstall = ReadBool((string?)info.Element.Attribute("ForceCreateOnInstall")),
                ForceDeleteOnUninstall = ReadBool((string?)info.Element.Attribute("ForceDeleteOnUninstall")),
            };
        }

        /// <summary>
        /// Gets whether changing the key's options, or its permissions, must say which Directory to use: the key has no
        /// RegistryKey element and no entry to put one with, and the group has no registry component to put it in.
        /// </summary>
        public bool KeyNeedsDirectory(string root, string key) =>
            !GetKeyElements().Any(k => KeyIs(k, root, key)) &&
            !GetItems().Any(i => string.Equals(i.Root, root, StringComparison.OrdinalIgnoreCase) && string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase)) &&
            NeedsDirectory(ScopeOf(root));

        /// <summary>
        /// Gets the RegistryKey element at the key, making one if there is none: in the component that has the key's values,
        /// or else the group's own for the key's scope (which takes <paramref name="directory"/> if it has to be made).
        /// </summary>
        public XElement EnsureKeyElement(string root, string key, string? directory = null)
        {
            var existing = GetKeyElements().FirstOrDefault(k => KeyIs(k, root, key));
            if (existing != null)
            {
                return existing.Element;
            }

            using var batch = _beginBatch();
            var atKey = GetItems().FirstOrDefault(i => string.Equals(i.Root, root, StringComparison.OrdinalIgnoreCase) && string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));
            var component = atKey != null ? (Wxs.Component)WixElement.Wrap(atKey.Component) : GetOrCreateAutoComponent(ScopeOf(root), "default", directory);
            return component.Add(new Wxs.RegistryKey { Root = ToRoot(root), Key = key }).Xml;
        }

        /// <summary>Sets the options of a key, on its RegistryKey element (made if there is none, as <see cref="EnsureKeyElement"/>).</summary>
        public void SetKeyOptions(string root, string key, KeyOptions options, string? directory = null)
        {
            using var batch = _beginBatch();
            var element = EnsureKeyElement(root, key, directory);
            // The key's Action is deprecated and can't be used: a file that still has one loses it when the key's options are set,
            // and the two flags below say what it said.
            element.SetAttributeValue("Action", null);
            SetFlag(element, "ForceCreateOnInstall", options.ForceCreateOnInstall);
            SetFlag(element, "ForceDeleteOnUninstall", options.ForceDeleteOnUninstall);
        }

        static void SetFlag(XElement element, string attribute, bool? value) => element.SetAttributeValue(attribute, value == null ? null : value.Value ? "yes" : "no");

        // ---------------------------------------------------------------- permissions

        /// <summary>The rights a Permission or Util PermissionEx can have on a registry key or value, by attribute name.</summary>
        public static readonly string[] Rights =
        {
            "GenericAll", "GenericRead", "GenericWrite", "GenericExecute",
            "Read", "Write", "CreateSubkeys", "EnumerateSubkeys", "Notify", "CreateLink",
            "Delete", "ReadPermission", "ChangePermission", "TakeOwnership", "Synchronize",
        };

        const string UtilNamespace = "http://wixtoolset.org/schemas/v4/wxs/util";

        /// <summary>Gets whether a key's element holds nothing but permissions, which go with the key (it has no values or keys of its own).</summary>
        public static bool HoldsOnlyPermissions(XElement element) => element.Elements().All(IsPermission);

        static bool IsPermission(XElement element) => element.Name.LocalName == "Permission" || element.Name.LocalName == "PermissionEx";

        /// <summary>Gets the permissions of a registry key or value element.</summary>
        public List<PermissionSpec> GetPermissions(XElement target)
        {
            var list = new List<PermissionSpec>();
            foreach (var element in target.Elements().Where(IsPermission))
            {
                var spec = new PermissionSpec
                {
                    Kind = element.Name.LocalName == "Permission" ? PermissionKind.Permission : element.Name.NamespaceName == UtilNamespace ? PermissionKind.UtilPermissionEx : PermissionKind.Sddl,
                    User = (string?)element.Attribute("User"),
                    Domain = (string?)element.Attribute("Domain"),
                    Inheritable = ReadBool((string?)element.Attribute("Inheritable")),
                    Sddl = (string?)element.Attribute("Sddl"),
                    Condition = (string?)element.Attribute("Condition"),
                };
                foreach (var right in Rights.Where(r => IsYes((string?)element.Attribute(r))))
                {
                    spec.Rights.Add(right);
                }

                list.Add(spec);
            }

            return list;
        }

        /// <summary>Replaces the permissions of a registry key or value element with these.</summary>
        public void SetPermissions(XElement target, IReadOnlyList<PermissionSpec> permissions)
        {
            using var batch = _beginBatch();
            foreach (var old in target.Elements().Where(IsPermission).ToList())
            {
                WixElement.Wrap(old).RemoveFromParent();
            }

            var wrapped = WixElement.Wrap(target);
            foreach (var spec in permissions)
            {
                XElement added;
                switch (spec.Kind)
                {
                    case PermissionKind.Permission:
                        added = Add(wrapped, new Wxs.Permission { User = Blank(spec.User), Domain = Blank(spec.Domain) });
                        break;
                    case PermissionKind.UtilPermissionEx:
                        added = Add(wrapped, new Wxs.Util.PermissionEx { User = Blank(spec.User), Domain = Blank(spec.Domain) });
                        SetFlag(added, "Inheritable", spec.Inheritable);
                        break;
                    default:
                        added = Add(wrapped, new Wxs.PermissionEx { Sddl = Blank(spec.Sddl), Condition = Blank(spec.Condition) });
                        break;
                }

                if (spec.Kind != PermissionKind.Sddl)
                {
                    foreach (var right in Rights.Where(spec.Rights.Contains))
                    {
                        added.SetAttributeValue(right, "yes");
                    }
                }
            }
        }

        static string? Blank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text!.Trim();

        static XElement Add(WixElement parent, Wxs.Permission child) =>
            parent is Wxs.RegistryKey key ? key.Add(child).Xml : parent is Wxs.RegistryValue value ? value.Add(child).Xml : throw new InvalidOperationException("Only a registry key or value has permissions.");

        static XElement Add(WixElement parent, Wxs.Util.PermissionEx child) =>
            parent is Wxs.RegistryKey key ? key.Add(child).Xml : parent is Wxs.RegistryValue value ? value.Add(child).Xml : throw new InvalidOperationException("Only a registry key or value has permissions.");

        static XElement Add(WixElement parent, Wxs.PermissionEx child) =>
            parent is Wxs.RegistryKey key ? key.Add(child).Xml : parent is Wxs.RegistryValue value ? value.Add(child).Xml : throw new InvalidOperationException("Only a registry key or value has permissions.");

        // ---------------------------------------------------------------- finding

        static readonly string[] RootOrder = { "HKCR", "HKCU", "HKLM", "HKU", "HKMU" };

        /// <summary>
        /// Finds the text in the group's keys, value names and value data, in the order the tree shows them: by root, then
        /// key, then the key itself before its values, then the values by name.
        /// </summary>
        public List<FindMatch> Find(FindOptions options)
        {
            var matches = new List<FindMatch>();
            if (string.IsNullOrEmpty(options.Text))
            {
                return matches;
            }

            var comparison = options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            bool Has(string? text) => text != null && text.IndexOf(options.Text, comparison) >= 0;
            var items = GetItems();
            var paths = items.Where(i => i.Root != null).Select(i => (Root: i.Root!.ToUpperInvariant(), i.Key))
                .Concat(GetKeyElements().Where(k => k.Root != null).Select(k => (Root: k.Root!.ToUpperInvariant(), k.Key)))
                .Distinct()
                .OrderBy(p => Math.Max(0, Array.IndexOf(RootOrder, p.Root)))
                .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase);
            foreach (var path in paths)
            {
                if (options.Keys && Has(path.Key))
                {
                    matches.Add(new FindMatch { Root = path.Root, Key = path.Key });
                }

                foreach (var item in items.Where(i => string.Equals(i.Root, path.Root, StringComparison.OrdinalIgnoreCase) && string.Equals(i.Key, path.Key, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                {
                    var data = item.Type == RegKind.MultiString ? string.Join("\n", item.MultiStrings) : item.Value;
                    if (options.Names && Has(item.Name))
                    {
                        matches.Add(new FindMatch { Root = path.Root, Key = path.Key, Item = item, Where = "name" });
                    }
                    else if (options.Data && item.Kind == RegistryItemKind.Value && Has(data))
                    {
                        matches.Add(new FindMatch { Root = path.Root, Key = path.Key, Item = item, Where = "data" });
                    }
                }
            }

            return matches;
        }

        // ---------------------------------------------------------------- references for formatted values

        static readonly string[] BuiltInReferences =
        {
            "ProductName", "ProductVersion", "Manufacturer", "ProductCode", "UpgradeCode", "ProductLanguage", "TARGETDIR", "SourceDir", "ALLUSERS", "USERNAME", "ComputerName", "VersionNT", "VersionNT64",
            "ProgramFilesFolder", "ProgramFiles64Folder", "ProgramFiles6432Folder", "CommonFilesFolder", "CommonFiles64Folder", "CommonFiles6432Folder",
            "AppDataFolder", "LocalAppDataFolder", "CommonAppDataFolder", "SystemFolder", "System64Folder", "WindowsFolder", "TempFolder", "DesktopFolder", "ProgramMenuFolder", "StartMenuFolder", "StartupFolder",
        };

        /// <summary>
        /// Gets what a formatted value can refer to: the file's properties and directories, the paths of its files, and the
        /// properties and folders Windows Installer defines. A reference is its Id in brackets, or <c>[#Id]</c> for a file's path.
        /// </summary>
        public List<ReferenceChoice> GetReferences()
        {
            var references = new List<ReferenceChoice>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            void Add(string text, string kind, string? detail)
            {
                if (seen.Add(text))
                {
                    references.Add(new ReferenceChoice { Text = text, Kind = kind, Detail = detail });
                }
            }

            foreach (var element in _document.Descendants())
            {
                var id = (string?)element.Attribute("Id");
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                switch (element.Name.LocalName)
                {
                    case "Property": Add("[" + id + "]", "Property", (string?)element.Attribute("Value")); break;
                    case "Directory":
                    case "StandardDirectory":
                    case "DirectoryRef": Add("[" + id + "]", "Directory", (string?)element.Attribute("Name")); break;
                    case "File": Add("[#" + id + "]", "File", (string?)element.Attribute("Source")); break;
                }
            }

            foreach (var name in BuiltInReferences)
            {
                Add("[" + name + "]", "Built-in", null);
            }

            return references;
        }

        // ---------------------------------------------------------------- .reg files

        /// <summary>
        /// Adds what a .reg file held to the group's own registry components (or to <paramref name="component"/>). Values
        /// that already exist (the same root, key and name) are left as they are and counted in the result. A new component of the
        /// group's own has the registry view <paramref name="bitness"/> (default, always32 or always64).
        /// </summary>
        public ImportResult ImportReg(RegImport import, XElement? component = null, string? directory = null, string bitness = "default")
        {
            using var batch = _beginBatch();
            var result = new ImportResult();
            var existing = new HashSet<string>(GetItems().Where(i => i.Kind == RegistryItemKind.Value).Select(i => EntryKey(i.Root, i.Key, i.Name)), StringComparer.OrdinalIgnoreCase);
            foreach (var entry in import.Entries)
            {
                if (entry.RemoveKey)
                {
                    AddRemoveKey(entry.Root, entry.Key, null, component, directory, bitness);
                    result.Added++;
                }
                else if (entry.RemoveValue)
                {
                    AddRemoveValue(entry.Root, entry.Key, entry.Name, component, directory, bitness);
                    result.Added++;
                }
                else if (entry.KeyOnly)
                {
                    AddKey(entry.Root, entry.Key, component, directory, bitness);
                    result.Added++;
                }
                else if (!existing.Add(EntryKey(entry.Root, entry.Key, entry.Name)))
                {
                    result.AlreadyThere++;
                }
                else
                {
                    var spec = new RegistryValueSpec { Root = entry.Root, Key = entry.Key, Name = entry.Name, Type = entry.Kind, Value = entry.Kind == RegKind.MultiString ? null : entry.Value };
                    spec.MultiStrings.AddRange(entry.Strings);
                    AddValue(spec, component, directory, bitness);
                    result.Added++;
                }
            }

            result.Skipped.AddRange(import.Skipped);
            return result;
        }

        static string EntryKey(string? root, string key, string? name) => root + "\u0001" + key + "\u0001" + (name ?? string.Empty);

        /// <summary>Writes the group's registry values, keys to remove and keys of their own as a .reg file.</summary>
        public RegExport ExportReg() => RegFile.Write(GetItems(), EmptyKeys(null));

        /// <summary>Writes one key and everything below it as a .reg file.</summary>
        public RegExport ExportRegBranch(string root, string key) =>
            RegFile.Write(GetItems().Where(i => IsUnder(i.Root, i.Key, root, key)).ToList(), EmptyKeys((root, key)));

        // The keys of their own (a RegistryKey with nothing in it), as root and key, for the .reg file to list.
        List<KeyValuePair<string, string>> EmptyKeys((string Root, string Key)? within) => GetKeyElements()
            .Where(k => k.Root != null && !k.Element.HasElements && (within == null || IsUnder(k.Root, k.Key, within.Value.Root, within.Value.Key)))
            .Select(k => new KeyValuePair<string, string>(k.Root!, k.Key))
            .ToList();

        // The key, or a key below it; with no key, anything in the root.
        static bool IsUnder(string? candidateRoot, string candidateKey, string root, string key) =>
            string.Equals(candidateRoot, root, StringComparison.OrdinalIgnoreCase) &&
            (key.Length == 0 || string.Equals(candidateKey, key, StringComparison.OrdinalIgnoreCase) || candidateKey.StartsWith(key + "\\", StringComparison.OrdinalIgnoreCase));

        /// <summary>Gets the values and removals at this key and below it.</summary>
        public List<RegistryItem> GetItemsUnder(string root, string key) => GetItems().Where(i => IsUnder(i.Root, i.Key, root, key)).ToList();

        // ---------------------------------------------------------------- renaming a key

        /// <summary>
        /// Gives a key a new name (the last part of its path), moving everything at it and below it. False, with nothing
        /// changed, if the name is empty or has a backslash, or if there is already something at the new path.
        /// </summary>
        public bool RenameKey(string root, string key, string newName)
        {
            newName = (newName ?? string.Empty).Trim();
            if (key.Length == 0 || newName.Length == 0 || newName.IndexOf('\\') >= 0)
            {
                return false;
            }

            var oldSegments = key.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var newSegments = (string[])oldSegments.Clone();
            newSegments[newSegments.Length - 1] = newName;
            var newKey = string.Join("\\", newSegments);
            if (!string.Equals(newKey, key, StringComparison.OrdinalIgnoreCase) &&
                (GetItems().Any(i => IsUnder(i.Root, i.Key, root, newKey)) || GetKeyElements().Any(k => IsUnder(k.Root, k.Key, root, newKey))))
            {
                return false;
            }

            using var batch = _beginBatch();
            foreach (var component in Components)
            {
                foreach (var child in component.Xml.Elements())
                {
                    RenameIn(child, null, string.Empty, root, oldSegments, newName);
                }
            }

            return true;
        }

        // Walks the element's key elements. The attribute that holds the part of a path being renamed is the one to change: that
        // of the element whose own Key reaches the renamed part, not those of the elements inside it.
        static void RenameIn(XElement element, string? parentRoot, string parentKey, string root, string[] oldSegments, string newName)
        {
            var name = element.Name.LocalName;
            if (name != "RegistryKey" && name != "RegistryValue" && name != "RemoveRegistryKey" && name != "RemoveRegistryValue")
            {
                return;
            }

            var ownRoot = (string?)element.Attribute("Root") ?? parentRoot;
            var ownKey = (string?)element.Attribute("Key");
            var effective = Join(parentKey, ownKey);
            var effectiveSegments = effective.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var parentCount = parentKey.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var affected = string.Equals(ownRoot, root, StringComparison.OrdinalIgnoreCase) && effectiveSegments.Length >= oldSegments.Length &&
                !oldSegments.Where((segment, index) => !string.Equals(segment, effectiveSegments[index], StringComparison.OrdinalIgnoreCase)).Any();
            if (affected && parentCount < oldSegments.Length && ownKey != null)
            {
                var own = ownKey.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                own[oldSegments.Length - 1 - parentCount] = newName;
                element.SetAttributeValue("Key", string.Join("\\", own));
            }

            if (name == "RegistryKey")
            {
                // Inside it, the path is the one it had before it was changed.
                foreach (var child in element.Elements())
                {
                    RenameIn(child, ownRoot, effective, root, oldSegments, newName);
                }
            }
        }


        /// <summary>Puts the group's components, and what is in them, in IsWiX's canonical order.</summary>
        public void SortComponents()
        {
            using var batch = _beginBatch();
            var ordering = WixOrdering.Get(_document);
            if (ordering == null || _group == null)
            {
                return;
            }

            foreach (var component in Components.ToList())
            {
                ordering.Canonicalize(component.Xml);
            }

            ordering.Canonicalize(_group.Xml);
        }
    }

    /// <summary>The options of a registry key: what happens to the key itself when the component is installed or removed.</summary>
    public sealed class KeyOptions
    {
        public bool? ForceCreateOnInstall { get; set; }

        public bool? ForceDeleteOnUninstall { get; set; }

        /// <summary>The key has a RegistryKey element of its own (the options are kept on it).</summary>
        public bool HasKeyElement { get; set; }
    }

    /// <summary>The three kinds of permission on a registry key or value.</summary>
    public enum PermissionKind
    {
        /// <summary>A Permission: a user and rights.</summary>
        Permission,

        /// <summary>The Util extension's PermissionEx: a user, rights and whether they are inherited.</summary>
        UtilPermissionEx,

        /// <summary>A PermissionEx with a security descriptor in SDDL.</summary>
        Sddl,
    }

    /// <summary>One permission, for the Permissions dialog to edit and the document to write.</summary>
    public sealed class PermissionSpec
    {
        public PermissionKind Kind { get; set; }

        public string? User { get; set; }

        public string? Domain { get; set; }

        /// <summary>For a Util PermissionEx: whether the permission is inherited; null while not set.</summary>
        public bool? Inheritable { get; set; }

        /// <summary>For an SDDL permission.</summary>
        public string? Sddl { get; set; }

        public string? Condition { get; set; }

        /// <summary>The rights that are on, by attribute name (see <see cref="RegistryDocument.Rights"/>).</summary>
        public HashSet<string> Rights { get; } = new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>Something a formatted value can refer to, such as a property or a directory.</summary>
    public sealed class ReferenceChoice
    {
        /// <summary>What goes in the value: [Id], or [#Id] for a file's path.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Property, Directory, File or Built-in.</summary>
        public string Kind { get; set; } = string.Empty;

        public string? Detail { get; set; }
    }

    /// <summary>What to look for, and where.</summary>
    public sealed class FindOptions
    {
        public string Text { get; set; } = string.Empty;

        public bool MatchCase { get; set; }

        /// <summary>Look in the keys' paths.</summary>
        public bool Keys { get; set; } = true;

        /// <summary>Look in the values' names.</summary>
        public bool Names { get; set; } = true;

        /// <summary>Look in the values' data.</summary>
        public bool Data { get; set; } = true;
    }

    /// <summary>One place the text was found: a key, or a value (by its name or its data).</summary>
    public sealed class FindMatch
    {
        public string Root { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        /// <summary>The value found in; null for a key.</summary>
        public RegistryItem? Item { get; set; }

        /// <summary>key, name or data.</summary>
        public string Where { get; set; } = "key";
    }

    /// <summary>What <see cref="RegistryDocument.ImportReg"/> did.</summary>
    public sealed class ImportResult
    {
        public int Added { get; set; }
        public int AlreadyThere { get; set; }
        public List<string> Skipped { get; } = new List<string>();
    }
}
