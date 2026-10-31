using System.Xml.Linq;
using RegistryDesigner;
using Xunit;

namespace IsWiXExtension.Tests;

public class RegistryDocumentTests
{
    const string Ns = "http://wixtoolset.org/schemas/v4/wxs";

    static XDocument Load(string body, string package = "<Package Name=\"P\" Version=\"1.0\" />") =>
        XDocument.Parse($"<Wix xmlns=\"{Ns}\">{package}<Fragment>{body}</Fragment></Wix>");

    static XDocument Basic() => Load(
        "<ComponentGroup Id=\"Main\">" +
        "<Component Id=\"files\" Directory=\"INSTALLFOLDER\" Guid=\"11111111-1111-1111-1111-111111111111\"><File Source=\"a.exe\" KeyPath=\"yes\" /></Component>" +
        "</ComponentGroup>" +
        "<ComponentGroup Id=\"Other\" />");

    static RegistryValueSpec Value(string root, string key, string? name, string value, RegKind type = RegKind.String)
    {
        var spec = new RegistryValueSpec { Root = root, Key = key, Name = name, Type = type, Value = value };
        return spec;
    }

    static List<XElement> Components(XDocument document, string group = "Main") =>
        document.Descendants(XName.Get("ComponentGroup", Ns)).First(g => (string?)g.Attribute("Id") == group).Elements(XName.Get("Component", Ns)).ToList();

    // ---------------------------------------------------------------- groups

    [Fact]
    public void It_shows_the_group_asked_for_else_the_first()
    {
        var document = Basic();
        Assert.Equal("Main", new RegistryDocument(document).GroupId);
        Assert.Equal("Other", new RegistryDocument(document, "Other").GroupId);
        Assert.Equal("Main", new RegistryDocument(document, "Missing").GroupId);
        Assert.Equal(2, new RegistryDocument(document).GetGroupIds().Count);
    }

    [Fact]
    public void A_file_with_no_group_has_nothing_to_edit()
    {
        var document = Load(string.Empty);
        var registry = new RegistryDocument(document);
        Assert.False(registry.HasGroup);
        Assert.Empty(registry.GetItems());
        Assert.Throws<InvalidOperationException>(() => registry.AddValue(Value("HKLM", "Software\\A", "x", "1"), null, "TARGETDIR"));
    }

    // ---------------------------------------------------------------- adding

    [Fact]
    public void A_new_value_needs_a_directory_when_the_group_has_no_registry_component()
    {
        var registry = new RegistryDocument(Basic());
        Assert.True(registry.NeedsDirectory(RegistryScope.PerMachine));
        Assert.Throws<ArgumentException>(() => registry.AddValue(Value("HKLM", "Software\\A", "x", "1")));
    }

    [Fact]
    public void The_first_value_makes_a_component_with_the_chosen_directory_and_the_owr_id()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var item = registry.AddValue(Value("HKLM", "Software\\A", "Name", "text"), directory: "INSTALLFOLDER");

        var component = Components(document).Single(c => ((string?)c.Attribute("Id"))!.StartsWith("owr"));
        Assert.Equal("INSTALLFOLDER", (string?)component.Attribute("Directory"));
        Assert.Equal(RegistryDocument.AutoComponentId("Main", RegistryScope.PerMachine, "default"), (string?)component.Attribute("Id"));
        Assert.NotNull((string?)component.Attribute("Guid"));
        Assert.Null(component.Attribute("Bitness"));
        Assert.Equal(item.Element, component.Elements().Single());
        Assert.False(registry.NeedsDirectory(RegistryScope.PerMachine));
    }

    [Fact]
    public void Later_values_reuse_the_component_and_ignore_the_directory()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        registry.AddValue(Value("HKLM", "Software\\A", "One", "1"), directory: "INSTALLFOLDER");
        registry.AddValue(Value("HKCR", "Software\\A", "Two", "2"), directory: "SOMEWHERE");

        var owr = Components(document).Where(c => ((string?)c.Attribute("Id"))!.StartsWith("owr")).ToList();
        Assert.Single(owr);
        Assert.Equal(2, owr[0].Elements().Count());
        Assert.Equal("INSTALLFOLDER", (string?)owr[0].Attribute("Directory"));
    }

    [Fact]
    public void Per_user_entries_and_other_views_get_components_of_their_own()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        registry.AddValue(Value("HKLM", "Software\\A", "m", "1"), directory: "TARGETDIR");
        registry.AddValue(Value("HKCU", "Software\\A", "u", "1"), directory: "TARGETDIR");
        registry.AddValue(Value("HKLM", "Software\\A", "w", "1"), directory: "TARGETDIR", bitness: "always64");

        var owr = Components(document).Where(c => ((string?)c.Attribute("Id"))!.StartsWith("owr")).ToList();
        Assert.Equal(3, owr.Count);
        Assert.Single(owr, c => (string?)c.Attribute("Bitness") == "always64");
        Assert.Equal(3, owr.Select(c => (string?)c.Attribute("Id")).Distinct().Count());
    }

    [Fact]
    public void A_value_can_be_attached_to_any_component_without_a_directory()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var files = Components(document).Single();
        var item = registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), files);

        Assert.Equal(files, item.Component);
        Assert.Equal(2, files.Elements().Count());
        Assert.DoesNotContain(Components(document), c => ((string?)c.Attribute("Id"))!.StartsWith("owr"));
    }

    [Fact]
    public void A_value_reads_back_as_it_was_written()
    {
        var registry = new RegistryDocument(Basic());
        var item = registry.AddValue(Value("HKLM", "Software\\A\\B", "Count", "42", RegKind.Integer), directory: "TARGETDIR");

        Assert.Equal("HKLM", item.Root);
        Assert.Equal("Software\\A\\B", item.Key);
        Assert.Equal("Count", item.Name);
        Assert.Equal(RegKind.Integer, item.Type);
        Assert.Equal("42", item.Value);
        Assert.Equal(RegistryItemKind.Value, item.Kind);
    }

    [Fact]
    public void A_multi_string_is_one_MultiStringValue_per_string_and_has_no_Value()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var spec = Value("HKLM", "Software\\A", "List", "ignored", RegKind.MultiString);
        spec.MultiStrings.AddRange(new[] { "one", "two", "three" });
        var item = registry.AddValue(spec, directory: "TARGETDIR");

        Assert.Equal(3, item.Element.Elements().Count(e => e.Name.LocalName == "MultiStringValue"));
        Assert.Null(item.Element.Attribute("Value"));
        Assert.Equal(new[] { "one", "two", "three" }, registry.GetItems().Single(i => i.Name == "List").MultiStrings.ToArray());
    }

    [Fact]
    public void Values_inside_registry_keys_read_with_their_full_key()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\">" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\\A\"><RegistryKey Key=\"B\"><RegistryValue Name=\"n\" Type=\"string\" Value=\"v\" /></RegistryKey>" +
            "<RegistryValue Key=\"C\" Name=\"m\" Type=\"integer\" Value=\"3\" /></RegistryKey>" +
            "<RegistryValue Root=\"HKCU\" Key=\"Software\\Z\" Name=\"z\" Type=\"string\" Value=\"1\" />" +
            "</Component></ComponentGroup>");
        var items = new RegistryDocument(document).GetItems();

        Assert.Equal("Software\\A\\B", items.Single(i => i.Name == "n").Key);
        Assert.Equal("HKLM", items.Single(i => i.Name == "n").Root);
        Assert.Equal("Software\\A\\C", items.Single(i => i.Name == "m").Key);
        Assert.Equal("HKCU", items.Single(i => i.Name == "z").Root);
    }

    [Fact]
    public void A_key_to_remove_has_an_action_the_user_sets()
    {
        var registry = new RegistryDocument(Basic());
        var item = registry.AddRemoveKey("HKLM", "Software\\Old", directory: "TARGETDIR");
        Assert.Null(item.Action);

        registry.SetRemoveAction(item, "RemoveOnUninstall");
        Assert.Equal("removeOnUninstall", registry.GetItems().Single().Action);
        registry.SetRemoveAction(item, null);
        Assert.Null(registry.GetItems().Single().Action);
        Assert.Throws<InvalidOperationException>(() => registry.SetRemoveAction(registry.AddRemoveValue("HKLM", "Software\\Old", "v", directory: "TARGETDIR"), "RemoveOnInstall"));
    }

    // ---------------------------------------------------------------- changing

    [Fact]
    public void A_value_can_be_changed()
    {
        var registry = new RegistryDocument(Basic());
        var item = registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR");
        var changed = registry.UpdateValue(item, Value("HKLM", "Software\\B", "m", "7", RegKind.Integer));

        Assert.Equal("Software\\B", changed.Key);
        Assert.Equal("m", changed.Name);
        Assert.Equal(RegKind.Integer, changed.Type);
        Assert.Equal("7", changed.Value);
        Assert.Single(registry.GetItems());
    }

    [Fact]
    public void Changing_the_type_to_multi_string_moves_the_value_into_children_and_back()
    {
        var registry = new RegistryDocument(Basic());
        var item = registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR");
        var spec = Value("HKLM", "Software\\A", "n", "x", RegKind.MultiString);
        spec.MultiStrings.AddRange(new[] { "a", "b" });
        var multi = registry.UpdateValue(item, spec);
        Assert.Null(multi.Element.Attribute("Value"));
        Assert.Equal(2, multi.MultiStrings.Count);

        var single = registry.UpdateValue(multi, Value("HKLM", "Software\\A", "n", "again"));
        Assert.Equal("again", single.Value);
        Assert.Empty(single.Element.Elements());
    }

    [Fact]
    public void A_nested_value_given_another_key_becomes_a_value_of_its_own_in_the_same_component()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\">" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\\A\"><RegistryValue Name=\"n\" Type=\"string\" Value=\"v\" KeyPath=\"yes\" /></RegistryKey>" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        var moved = registry.UpdateValue(registry.GetItems().Single(), Value("HKLM", "Software\\Other", "n", "v"));

        Assert.Equal("Software\\Other", moved.Key);
        Assert.True(moved.KeyPath);
        Assert.Equal(1, Components(document).Single().Elements().Count());
        Assert.DoesNotContain(document.Descendants(), e => e.Name.LocalName == "RegistryKey");
    }

    [Fact]
    public void The_user_marks_the_KeyPath_and_only_one_value_has_it()
    {
        var registry = new RegistryDocument(Basic());
        var a = registry.AddValue(Value("HKLM", "Software\\A", "a", "1"), directory: "TARGETDIR");
        var b = registry.AddValue(Value("HKLM", "Software\\A", "b", "1"));
        Assert.False(a.KeyPath);

        registry.SetKeyPath(a);
        registry.SetKeyPath(b);
        var items = registry.GetItems();
        Assert.False(items.Single(i => i.Name == "a").KeyPath);
        Assert.True(items.Single(i => i.Name == "b").KeyPath);

        registry.ClearKeyPath(b);
        Assert.DoesNotContain(registry.GetItems(), i => i.KeyPath);
    }

    [Fact]
    public void An_entry_can_move_to_another_component_and_leaves_an_empty_own_component_behind_removed()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var item = registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR");
        var files = Components(document).Single(c => (string?)c.Attribute("Id") == "files");
        registry.SetKeyPath(item);

        var moved = registry.MoveToComponent(item, files);
        Assert.Equal(files, moved.Component);
        Assert.False(moved.KeyPath);
        Assert.DoesNotContain(Components(document), c => ((string?)c.Attribute("Id"))!.StartsWith("owr"));
    }

    [Fact]
    public void Deleting_the_last_entry_removes_the_designers_component_but_not_one_with_files()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var own = registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR");
        var attached = registry.AddValue(Value("HKLM", "Software\\A", "m", "v"), Components(document).Single(c => (string?)c.Attribute("Id") == "files"));

        registry.DeleteItem(own);
        Assert.DoesNotContain(Components(document), c => ((string?)c.Attribute("Id"))!.StartsWith("owr"));
        registry.DeleteItem(attached);
        Assert.Contains(Components(document), c => (string?)c.Attribute("Id") == "files");
    }

    [Fact]
    public void Deleting_a_nested_value_removes_the_keys_left_empty()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\"><File Source=\"a\" />" +
            "<RegistryKey Root=\"HKLM\" Key=\"A\"><RegistryKey Key=\"B\"><RegistryValue Name=\"n\" Type=\"string\" Value=\"v\" /></RegistryKey></RegistryKey>" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        registry.DeleteItem(registry.GetItems().Single());

        Assert.Empty(document.Descendants().Where(e => e.Name.LocalName == "RegistryKey"));
        Assert.Single(Components(document).Single().Elements());
    }

    // ---------------------------------------------------------------- components

    [Fact]
    public void Component_properties_are_set_on_the_component()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR");
        var component = Components(document).Single(c => ((string?)c.Attribute("Id"))!.StartsWith("owr"));

        registry.SetBitness(component, "always64");
        registry.SetDisableRegistryReflection(component, true);
        registry.SetCondition(component, "VersionNT64");
        registry.SetDirectory(component, "INSTALLFOLDER");
        Assert.Equal("always64", (string?)component.Attribute("Bitness"));
        Assert.Equal("yes", (string?)component.Attribute("DisableRegistryReflection"));
        Assert.Equal("VersionNT64", (string?)component.Attribute("Condition"));
        Assert.Equal("INSTALLFOLDER", (string?)component.Attribute("Directory"));

        registry.SetBitness(component, "default");
        registry.SetDisableRegistryReflection(component, null);
        registry.SetCondition(component, " ");
        Assert.Null(component.Attribute("Bitness"));
        Assert.Null(component.Attribute("DisableRegistryReflection"));
        Assert.Null(component.Attribute("Condition"));
        Assert.Throws<ArgumentException>(() => registry.SetDirectory(component, ""));
        Assert.Throws<ArgumentException>(() => registry.SetBitness(component, "wide"));
    }

    [Fact]
    public void The_components_list_says_which_are_the_designers_and_which_have_files()
    {
        var registry = new RegistryDocument(Basic());
        registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR");
        var list = registry.GetComponents();

        Assert.True(list.Single(c => c.Id == "files").HasFiles);
        Assert.False(list.Single(c => c.Id == "files").IsAutoManaged);
        Assert.True(list.Single(c => c.IsAutoManaged).HasRegistry);
    }

    [Fact]
    public void Files_and_Folders_components_are_never_removed_by_a_delete_here()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var item = registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), Components(document).Single());
        registry.DeleteItem(item);
        Assert.Single(Components(document));
        Assert.Single(Components(document).Single().Elements());
    }

    [Fact]
    public void Keys_are_listed_with_their_full_path_and_a_key_can_be_deleted_by_its_path()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\"><File Source=\"f\" />" +
            "<RegistryKey Root=\"HKLM\" Key=\"A\"><RegistryKey Key=\"B\"><RegistryValue Name=\"n\" Type=\"string\" Value=\"v\" /></RegistryKey></RegistryKey>" +
            "<RegistryKey Root=\"HKLM\" Key=\"Empty\" ForceCreateOnInstall=\"yes\" />" +
            "<RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"top\" Type=\"string\" Value=\"t\" />" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        Assert.Equal(new[] { "A", "A\\B", "Empty" }, registry.GetKeyElements().Select(k => k.Key).OrderBy(k => k).ToArray());

        Assert.Equal(1, registry.DeleteKey("HKLM", "Empty"));
        Assert.Equal(1, registry.DeleteKey("hklm", "a"));
        Assert.Equal("n", registry.GetItems().Single().Name);
        Assert.Equal(1, registry.DeleteKey("HKLM", "A\\B"));
        Assert.Empty(registry.GetItems());
        Assert.Empty(registry.GetKeyElements());
    }

    [Fact]
    public void Deleting_a_key_with_its_subkeys_removes_everything_below_it()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"owrX\" Directory=\"X\">" +
            "<RegistryValue Root=\"HKLM\" Key=\"Software\\Acme\\Sub\\Deep\" Name=\"d\" Type=\"string\" Value=\"1\" />" +
            "<RegistryValue Root=\"HKLM\" Key=\"Software\\Acme\\Sub\" Name=\"s\" Type=\"string\" Value=\"1\" />" +
            "<RegistryValue Root=\"HKLM\" Key=\"Software\\Acme2\" Name=\"keep\" Type=\"string\" Value=\"1\" />" +
            "<RegistryValue Root=\"HKCU\" Key=\"Software\\Acme\\Sub\" Name=\"other root\" Type=\"string\" Value=\"1\" />" +
            "<RemoveRegistryKey Root=\"HKLM\" Key=\"Software\\Acme\\Old\" />" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\\Acme\\Empty\" ForceCreateOnInstall=\"yes\" />" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);

        // Without the subkeys, a key that only has keys below it deletes nothing.
        Assert.Equal(0, registry.DeleteKey("HKLM", "Software\\Acme"));
        Assert.Equal(6, registry.GetItems().Count + registry.GetKeyElements().Count - 0);

        Assert.Equal(4, registry.DeleteKey("HKLM", "Software\\Acme", includeSubkeys: true));
        var left = registry.GetItems().Select(i => i.Name).OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "keep", "other root" }, left);
        Assert.Empty(registry.GetKeyElements());
    }

    [Fact]
    public void A_key_is_renamed_with_everything_below_it_whether_standalone_or_nested()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\">" +
            "<RegistryValue Root=\"HKLM\" Key=\"Software\\Acme\\Sub\" Name=\"flat\" Type=\"string\" Value=\"1\" />" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\\Acme\"><RegistryValue Name=\"top\" Type=\"string\" Value=\"1\" /><RegistryKey Key=\"Deep\\Er\"><RegistryValue Name=\"nest\" Type=\"string\" Value=\"1\" /></RegistryKey></RegistryKey>" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\"><RegistryKey Key=\"Acme\\Split\"><RegistryValue Name=\"split\" Type=\"string\" Value=\"1\" /></RegistryKey></RegistryKey>" +
            "<RegistryValue Root=\"HKLM\" Key=\"Software\\Other\" Name=\"o\" Type=\"string\" Value=\"1\" />" +
            "<RemoveRegistryKey Root=\"HKLM\" Key=\"Software\\Acme\\Gone\" />" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);

        Assert.True(registry.RenameKey("HKLM", "Software\\Acme", "Renamed"));
        var keys = registry.GetItems().ToDictionary(i => i.Name ?? "gone" + i.Key, i => i.Key);
        Assert.Equal("Software\\Renamed\\Sub", keys["flat"]);
        Assert.Equal("Software\\Renamed", keys["top"]);
        Assert.Equal("Software\\Renamed\\Deep\\Er", keys["nest"]);
        Assert.Equal("Software\\Renamed\\Split", keys["split"]);
        Assert.Equal("Software\\Other", keys["o"]);
        Assert.Contains(registry.GetItems(), i => i.Kind == RegistryItemKind.RemoveKey && i.Key == "Software\\Renamed\\Gone");
        Assert.Empty(registry.GetItems().Where(i => i.Key.Contains("Acme")));
    }

    [Fact]
    public void Renaming_a_deeper_key_changes_only_that_part_and_a_taken_name_is_refused()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\">" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\\Acme\"><RegistryKey Key=\"Deep\\Er\"><RegistryValue Name=\"nest\" Type=\"string\" Value=\"1\" /></RegistryKey></RegistryKey>" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\\Taken\" ForceCreateOnInstall=\"yes\" />" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);

        Assert.True(registry.RenameKey("HKLM", "Software\\Acme\\Deep", "Shallow"));
        Assert.Equal("Software\\Acme\\Shallow\\Er", registry.GetItems().Single().Key);
        Assert.False(registry.RenameKey("HKLM", "Software\\Acme", "Taken"));
        Assert.False(registry.RenameKey("HKLM", "Software\\Acme", "a\\b"));
        Assert.False(registry.RenameKey("HKLM", "Software\\Acme", "  "));
        Assert.False(registry.RenameKey("HKLM", string.Empty, "X"));
        Assert.Equal("Software\\Acme\\Shallow\\Er", registry.GetItems().Single().Key);
        Assert.True(registry.RenameKey("HKLM", "Software\\Acme", "ACME"));
    }

    [Fact]
    public void A_branch_exports_with_its_empty_keys_and_the_whole_group_does_too()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\">" +
            "<RegistryValue Root=\"HKLM\" Key=\"Software\\Acme\\Sub\" Name=\"s\" Type=\"string\" Value=\"1\" />" +
            "<RegistryValue Root=\"HKLM\" Key=\"Software\\Other\" Name=\"o\" Type=\"string\" Value=\"2\" />" +
            "<RegistryKey Root=\"HKLM\" Key=\"Software\\Acme\\Empty\" ForceCreateOnInstall=\"yes\" />" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        var branch = registry.ExportRegBranch("HKLM", "Software\\Acme").Text;

        Assert.Contains("[HKEY_LOCAL_MACHINE\\Software\\Acme\\Sub]", branch);
        Assert.Contains("[HKEY_LOCAL_MACHINE\\Software\\Acme\\Empty]", branch);
        Assert.DoesNotContain(new[] { branch }, text => text.Contains("Other"));
        Assert.Contains("[HKEY_LOCAL_MACHINE\\Software\\Acme\\Empty]", registry.ExportReg().Text);
        Assert.Equal(2, registry.GetItemsUnder("HKLM", "Software").Count);
        Assert.Equal(2, registry.GetItemsUnder("HKLM", string.Empty).Count);
        Assert.Contains("Other", registry.ExportRegBranch("HKLM", string.Empty).Text);
        Assert.Single(registry.GetItemsUnder("HKLM", "Software\\Other"));
    }

    // ---------------------------------------------------------------- find and references

    static RegistryDocument Findable()
    {
        var registry = new RegistryDocument(Basic());
        registry.AddValue(Value("HKLM", "Software\\Acme\\Core", "InstallPath", "C:\\Acme"), directory: "TARGETDIR");
        registry.AddValue(Value("HKLM", "Software\\Acme\\Core", "Version", "1.2.3"));
        registry.AddValue(Value("HKCU", "Software\\Acme", "Theme", "acme dark"), directory: "TARGETDIR");
        registry.AddValue(Value("HKCR", ".acme", null, "AcmeFile"));
        var multi = Value("HKLM", "Software\\Other", "List", null!, RegKind.MultiString);
        multi.MultiStrings.AddRange(new[] { "first", "second ACME" });
        registry.AddValue(multi);
        registry.AddRemoveKey("HKLM", "Software\\Acme\\Old");
        return registry;
    }

    [Fact]
    public void Find_looks_in_keys_names_and_data_in_the_trees_order()
    {
        var found = Findable().Find(new FindOptions { Text = "acme" }).Select(m => m.Root + ":" + m.Key + ":" + (m.Item?.Name ?? "-") + ":" + m.Where).ToArray();

        Assert.Equal(new[]
        {
            "HKCR:.acme:-:key",
            "HKCR:.acme:-:data",
            "HKCU:Software\\Acme:-:key",
            "HKCU:Software\\Acme:Theme:data",
            "HKLM:Software\\Acme\\Core:-:key",
            "HKLM:Software\\Acme\\Core:InstallPath:data",
            "HKLM:Software\\Acme\\Old:-:key",
            "HKLM:Software\\Other:List:data",
        }, found);
    }

    [Fact]
    public void Find_can_be_limited_to_names_or_keys_and_can_match_case()
    {
        var registry = Findable();
        Assert.Equal(new[] { "InstallPath" }, registry.Find(new FindOptions { Text = "path", Keys = false, Data = false }).Select(m => m.Item!.Name!).ToArray());
        Assert.Empty(registry.Find(new FindOptions { Text = "path", Keys = false, Names = false }));
        Assert.Equal(4, registry.Find(new FindOptions { Text = "acme", Names = false, Data = false }).Count);
        Assert.Equal(new[] { "HKLM:Software\\Other:List" }, registry.Find(new FindOptions { Text = "ACME", MatchCase = true }).Select(m => m.Root + ":" + m.Key + ":" + m.Item!.Name).ToArray());
        Assert.Empty(registry.Find(new FindOptions { Text = string.Empty }));
    }

    [Fact]
    public void References_list_the_files_properties_directories_files_and_built_ins_once_each()
    {
        var document = XDocument.Parse(
            $"<Wix xmlns=\"{Ns}\"><Package Name=\"P\" Version=\"1\"><Property Id=\"MYPROP\" Value=\"x\" /><StandardDirectory Id=\"ProgramFiles6432Folder\"><Directory Id=\"INSTALLFOLDER\" Name=\"App\" /></StandardDirectory></Package>" +
            "<Fragment><ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"INSTALLFOLDER\"><File Id=\"App.exe\" Source=\"app.exe\" /></Component></ComponentGroup></Fragment></Wix>");
        var references = new RegistryDocument(document).GetReferences();

        Assert.Contains(references, r => r.Text == "[MYPROP]" && r.Kind == "Property" && r.Detail == "x");
        Assert.Contains(references, r => r.Text == "[INSTALLFOLDER]" && r.Kind == "Directory");
        Assert.Contains(references, r => r.Text == "[#App.exe]" && r.Kind == "File" && r.Detail == "app.exe");
        Assert.Contains(references, r => r.Text == "[ProductName]" && r.Kind == "Built-in");
        Assert.Equal(references.Count, references.Select(r => r.Text).Distinct().Count());
        // The file's own directory is the document's, so ProgramFiles6432Folder is listed as a directory, not again as a built-in.
        Assert.Equal(1, references.Count(r => r.Text == "[ProgramFiles6432Folder]"));
    }

    // ---------------------------------------------------------------- key options and permissions

    [Fact]
    public void Key_options_are_kept_on_a_RegistryKey_made_in_the_component_that_has_the_keys_values()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        registry.AddValue(Value("HKLM", "Software\\Acme", "n", "v"), directory: "TARGETDIR");
        Assert.False(registry.GetKeyOptions("HKLM", "Software\\Acme").HasKeyElement);
        Assert.False(registry.KeyNeedsDirectory("HKLM", "Software\\Acme"));

        registry.SetKeyOptions("HKLM", "Software\\Acme", new KeyOptions { ForceCreateOnInstall = true, ForceDeleteOnUninstall = false });
        var options = registry.GetKeyOptions("HKLM", "software\\ACME");
        Assert.True(options.HasKeyElement);
        Assert.Equal(true, options.ForceCreateOnInstall);
        Assert.Equal(false, options.ForceDeleteOnUninstall);

        var own = Components(document).Single(c => ((string?)c.Attribute("Id"))!.StartsWith("owr"));
        Assert.Equal(2, own.Elements().Count());
        var key = own.Elements().Single(e => e.Name.LocalName == "RegistryKey");
        Assert.Equal("yes", (string?)key.Attribute("ForceCreateOnInstall"));
        Assert.Equal("no", (string?)key.Attribute("ForceDeleteOnUninstall"));
        Assert.Null(key.Attribute("Action"));

        registry.SetKeyOptions("HKLM", "Software\\Acme", new KeyOptions());
        var cleared = registry.GetKeyOptions("HKLM", "Software\\Acme");
        Assert.Null(cleared.ForceCreateOnInstall);
        Assert.Null(cleared.ForceDeleteOnUninstall);
        Assert.Single(own.Elements().Where(e => e.Name.LocalName == "RegistryKey"));
    }

    [Fact]
    public void A_key_with_nothing_yet_needs_a_directory_for_its_own_element()
    {
        var registry = new RegistryDocument(Basic());
        Assert.True(registry.KeyNeedsDirectory("HKCU", "Software\\New"));
        Assert.Throws<ArgumentException>(() => registry.SetKeyOptions("HKCU", "Software\\New", new KeyOptions { ForceDeleteOnUninstall = true }));
        registry.SetKeyOptions("HKCU", "Software\\New", new KeyOptions { ForceDeleteOnUninstall = true }, "TARGETDIR");
        Assert.False(registry.KeyNeedsDirectory("HKCU", "Software\\New"));
        Assert.Equal(true, registry.GetKeyOptions("HKCU", "Software\\New").ForceDeleteOnUninstall);
    }

    [Fact]
    public void Deleting_a_key_removes_its_own_element_even_when_it_has_permissions()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        registry.AddKey("HKLM", "New Key", directory: "TARGETDIR");
        registry.SetKeyOptions("HKLM", "New Key", new KeyOptions { ForceCreateOnInstall = true });
        var key = registry.GetKeyElements().Single(k => k.Key == "New Key").Element;
        registry.SetPermissions(key, new List<PermissionSpec> { new PermissionSpec { Kind = PermissionKind.Permission, User = "Everyone" } });
        Assert.True(key.HasElements);
        Assert.True(RegistryDocument.HoldsOnlyPermissions(key));

        Assert.Equal(1, registry.DeleteKey("HKLM", "New Key", includeSubkeys: true));
        Assert.Empty(registry.GetKeyElements());
    }

    [Fact]
    public void A_new_key_is_created_on_install_by_a_flag_not_by_the_deprecated_Action()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var key = registry.AddKey("HKLM", "Software\\Empty", directory: "TARGETDIR");

        Assert.Equal("yes", (string?)key.Attribute("ForceCreateOnInstall"));
        Assert.Null(key.Attribute("Action"));
        Assert.Equal(true, registry.GetKeyOptions("HKLM", "Software\\Empty").ForceCreateOnInstall);
    }

    [Fact]
    public void A_key_with_the_deprecated_Action_is_warned_about_and_setting_its_options_removes_it()
    {
        var document = Load("<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\"><RegistryKey Root=\"HKLM\" Key=\"Software\\Old\" Action=\"createAndRemoveOnUninstall\" /></Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        var issue = registry.Validate().Single(i => i.Code == ValidationCode.DeprecatedKeyAction);
        Assert.Equal(ValidationSeverity.Warning, issue.Severity);
        Assert.Contains("deprecated", issue.Message);

        registry.SetKeyOptions("HKLM", "Software\\Old", new KeyOptions { ForceCreateOnInstall = true, ForceDeleteOnUninstall = true });
        Assert.DoesNotContain(registry.Validate(), i => i.Code == ValidationCode.DeprecatedKeyAction);
        var key = document.Descendants().Single(e => e.Name.LocalName == "RegistryKey");
        Assert.Null(key.Attribute("Action"));
        Assert.Equal("yes", (string?)key.Attribute("ForceDeleteOnUninstall"));
    }

    [Fact]
    public void Permissions_of_the_three_kinds_are_written_and_read_back_and_replace_the_old_ones()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var item = registry.AddValue(Value("HKLM", "Software\\Acme", "n", "v"), directory: "TARGETDIR");

        var plain = new PermissionSpec { Kind = PermissionKind.Permission, User = "Everyone", Domain = "NT AUTHORITY" };
        plain.Rights.Add("GenericRead");
        plain.Rights.Add("Notify");
        var util = new PermissionSpec { Kind = PermissionKind.UtilPermissionEx, User = "Administrators", Inheritable = true };
        util.Rights.Add("GenericAll");
        var sddl = new PermissionSpec { Kind = PermissionKind.Sddl, Sddl = "D:(A;;GA;;;BA)", Condition = "VersionNT" };
        registry.SetPermissions(item.Element, new[] { plain, util, sddl });

        var read = registry.GetPermissions(item.Element);
        Assert.Equal(3, read.Count);
        Assert.Equal(PermissionKind.Permission, read[0].Kind);
        Assert.Equal("Everyone", read[0].User);
        Assert.Equal("NT AUTHORITY", read[0].Domain);
        Assert.Equal(new[] { "GenericRead", "Notify" }, read[0].Rights.OrderBy(r => r).ToArray());
        Assert.Equal(PermissionKind.UtilPermissionEx, read[1].Kind);
        Assert.Equal(true, read[1].Inheritable);
        Assert.Equal(new[] { "GenericAll" }, read[1].Rights.ToArray());
        Assert.Equal(PermissionKind.Sddl, read[2].Kind);
        Assert.Equal("D:(A;;GA;;;BA)", read[2].Sddl);
        Assert.Equal("VersionNT", read[2].Condition);
        Assert.Equal("yes", (string?)item.Element.Elements().First().Attribute("GenericRead"));
        Assert.Contains(item.Element.Elements(), e => e.Name.LocalName == "PermissionEx" && e.Name.NamespaceName.EndsWith("/util"));

        registry.SetPermissions(item.Element, new[] { plain });
        Assert.Single(registry.GetPermissions(item.Element));
        registry.SetPermissions(item.Element, new PermissionSpec[0]);
        Assert.Empty(registry.GetPermissions(item.Element));
        Assert.Empty(item.Element.Elements());
    }

    [Fact]
    public void A_key_can_have_permissions_too_and_a_blank_user_is_not_written()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        registry.AddValue(Value("HKLM", "Software\\Acme", "n", "v"), directory: "TARGETDIR");
        var key = registry.EnsureKeyElement("HKLM", "Software\\Acme");
        var spec = new PermissionSpec { Kind = PermissionKind.Permission, User = "  " };
        spec.Rights.Add("Read");
        registry.SetPermissions(key, new[] { spec });

        Assert.Null(key.Elements().Single().Attribute("User"));
        Assert.Equal("yes", (string?)key.Elements().Single().Attribute("Read"));
        Assert.Throws<InvalidOperationException>(() => registry.SetPermissions(Components(document).First(), new[] { spec }));
    }

    [Fact]
    public void Known_directories_come_from_the_file_then_the_standard_ones_and_the_suggestion_follows_the_groups_components()
    {
        var document = Load(
            "<StandardDirectory Id=\"ProgramFiles6432Folder\"><Directory Id=\"INSTALLFOLDER\" Name=\"App\" /></StandardDirectory>" +
            "<ComponentGroup Id=\"Main\">" +
            "<Component Id=\"a\" Directory=\"INSTALLFOLDER\"><File Source=\"a\" /></Component>" +
            "<Component Id=\"b\" Directory=\"INSTALLFOLDER\"><File Source=\"b\" /></Component>" +
            "<Component Id=\"c\" Directory=\"DATADIR\"><File Source=\"c\" /></Component>" +
            "</ComponentGroup>");
        var registry = new RegistryDocument(document);
        var known = registry.GetKnownDirectories();

        Assert.Equal("ProgramFiles6432Folder", known[0]);
        Assert.Contains(known, d => d == "DATADIR");
        Assert.Contains(known, d => d == "TARGETDIR");
        Assert.Equal(known.Count, known.Distinct().Count());
        Assert.Equal("INSTALLFOLDER", registry.SuggestDirectory());
        Assert.Equal("LocalAppDataFolder", registry.SuggestDirectory(new[] { RegistryScope.PerUser }));
        Assert.Equal("INSTALLFOLDER", registry.SuggestDirectory(new[] { RegistryScope.PerUser, RegistryScope.PerMachine }));
        Assert.Equal("INSTALLFOLDER", registry.SuggestDirectory(new[] { RegistryScope.PerMachine }));
        Assert.Equal("TARGETDIR", new RegistryDocument(Load("<ComponentGroup Id=\"Main\" />")).SuggestDirectory());
    }

    [Fact]
    public void The_components_list_includes_the_reflection_setting_and_condition_and_the_group_element_is_exposed()
    {
        var registry = new RegistryDocument(Basic());
        registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR");
        var own = registry.GetComponents().Single(c => c.IsAutoManaged);
        Assert.Null(own.DisableRegistryReflection);
        Assert.Null(own.Condition);

        registry.SetDisableRegistryReflection(own.Element, true);
        registry.SetCondition(own.Element, "VersionNT64");
        own = registry.GetComponents().Single(c => c.IsAutoManaged);
        Assert.Equal(true, own.DisableRegistryReflection);
        Assert.Equal("VersionNT64", own.Condition);
        Assert.Equal("Main", (string?)registry.GroupElement!.Attribute("Id"));
    }

    // ---------------------------------------------------------------- validation

    static List<ValidationIssue> Issues(XDocument document) => new RegistryDocument(document).Validate();

    static bool Has(List<ValidationIssue> issues, ValidationCode code) => issues.Any(i => i.Code == code);

    [Fact]
    public void A_clean_registry_component_has_no_errors()
    {
        var registry = new RegistryDocument(Basic());
        registry.SetKeyPath(registry.AddValue(Value("HKLM", "Software\\A", "n", "v"), directory: "TARGETDIR"));
        Assert.Empty(registry.Validate());
    }

    [Fact]
    public void Missing_fields_are_errors_and_an_empty_name_is_the_default_value_and_fine()
    {
        var document = Load("<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\"><RegistryValue Type=\"string\" Value=\"v\" KeyPath=\"yes\" /></Component></ComponentGroup>");
        var issues = Issues(document);
        Assert.True(Has(issues, ValidationCode.MissingRoot));
        Assert.True(Has(issues, ValidationCode.MissingKey));
        Assert.Equal(2, issues.Count);

        var registry = new RegistryDocument(Basic());
        registry.SetKeyPath(registry.AddValue(Value("HKLM", "Software\\A", null, "default"), directory: "TARGETDIR"));
        Assert.Empty(registry.Validate());
    }

    [Fact]
    public void Duplicates_are_found_ignoring_case_and_differ_by_registry_view()
    {
        var registry = new RegistryDocument(Basic());
        registry.AddValue(Value("HKLM", "Software\\A", "n", "1"), directory: "TARGETDIR");
        registry.AddValue(Value("HKLM", "SOFTWARE\\a", "N", "2"));
        Assert.True(Has(registry.Validate(), ValidationCode.Duplicate));

        var other = new RegistryDocument(Basic());
        other.AddValue(Value("HKLM", "Software\\A", "n", "1"), directory: "TARGETDIR");
        other.AddValue(Value("HKLM", "Software\\A", "n", "1"), directory: "TARGETDIR", bitness: "always64");
        Assert.False(Has(other.Validate(), ValidationCode.Duplicate));
    }

    [Fact]
    public void Values_are_checked_against_their_type()
    {
        var registry = new RegistryDocument(Basic());
        registry.AddValue(Value("HKLM", "Software\\A", "bad", "-1", RegKind.Integer), directory: "TARGETDIR");
        registry.AddValue(Value("HKLM", "Software\\A", "toobig", "4294967296", RegKind.Integer));
        registry.AddValue(Value("HKLM", "Software\\A", "max", "4294967295", RegKind.Integer));
        registry.AddValue(Value("HKLM", "Software\\A", "token", "[SomeProperty]", RegKind.Integer));
        registry.AddValue(Value("HKLM", "Software\\A", "hexbad", "ABC", RegKind.Binary));
        registry.AddValue(Value("HKLM", "Software\\A", "hexok", "0AFF", RegKind.Binary));
        var issues = registry.Validate();

        Assert.Equal(new[] { "bad", "toobig" }, issues.Where(i => i.Code == ValidationCode.BadInteger).Select(i => i.Item!.Name).OrderBy(n => n).ToArray());
        Assert.Equal(new[] { "hexbad" }, issues.Where(i => i.Code == ValidationCode.BadBinary).Select(i => i.Item!.Name).ToArray());
    }

    [Fact]
    public void Brackets_must_pair_up_unless_they_are_escaped_literals()
    {
        var registry = new RegistryDocument(Basic());
        registry.AddValue(Value("HKLM", "Software\\A", "open", "[Prop"), directory: "TARGETDIR");
        registry.AddValue(Value("HKLM", "Software\\A", "close", "Prop]"));
        registry.AddValue(Value("HKLM", "Software\\A", "ok", "[INSTALLFOLDER]bin"));
        registry.AddValue(Value("HKLM", "Software\\A", "literal", "a[\\[]b[\\]]"));
        var names = registry.Validate().Where(i => i.Code == ValidationCode.UnbalancedReference).Select(i => i.Item!.Name).OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "close", "open" }, names);
    }

    [Fact]
    public void KeyPath_rules_are_checked_per_component()
    {
        var document = Load(
            "<ComponentGroup Id=\"Main\">" +
            "<Component Id=\"two\" Directory=\"X\"><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"a\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" /><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"b\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" /></Component>" +
            "<Component Id=\"withfile\" Directory=\"X\"><File Source=\"f\" KeyPath=\"yes\" /><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"c\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" /></Component>" +
            "<Component Id=\"folder\" Directory=\"X\" KeyPath=\"yes\"><CreateFolder /><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"d\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" /></Component>" +
            "<Component Id=\"none\" Directory=\"X\"><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"e\" Type=\"string\" Value=\"1\" /></Component>" +
            "<Component Id=\"filesonly\" Directory=\"X\"><File Source=\"g\" /><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"f\" Type=\"string\" Value=\"1\" /></Component>" +
            "</ComponentGroup>");
        var issues = Issues(document);
        string? IdOf(ValidationIssue i) => (string?)i.Component?.Attribute("Id");

        Assert.Equal("two", IdOf(issues.Single(i => i.Code == ValidationCode.MultipleKeyPaths)));
        Assert.Equal(new[] { "folder", "withfile" }, issues.Where(i => i.Code == ValidationCode.KeyPathConflict).Select(IdOf).OrderBy(x => x).ToArray());
        // A component with no KeyPath is not flagged: Windows Installer and WiX take care of it.
        Assert.DoesNotContain(issues, i => IdOf(i) == "none" || IdOf(i) == "filesonly");
    }

    [Fact]
    public void A_component_with_a_file_and_HKCU_and_HKLM_keys_is_flagged_though_the_keys_have_no_values()
    {
        var document = Load(
            "<ComponentGroup Id=\"test\"><Component Directory=\"X\" Id=\"c\"><File Source=\"f.dll\" KeyPath=\"yes\" />" +
            "<RegistryKey Root=\"HKLM\" Key=\"New Key\" ForceCreateOnInstall=\"yes\" />" +
            "<RegistryKey Root=\"HKCU\" Key=\"New Key\" ForceCreateOnInstall=\"yes\" />" +
            "</Component></ComponentGroup>");
        var issues = Issues(document);
        Assert.Contains(issues, i => i.Code == ValidationCode.MixedScope);
        Assert.Contains(issues, i => i.Code == ValidationCode.PerUserInPerMachinePackage && i.Message.Contains("HKCU\\New Key"));
    }

    [Fact]
    public void An_HKCU_key_beside_a_file_is_flagged_even_with_no_HKLM_entry()
    {
        var document = Load(
            "<ComponentGroup Id=\"test\"><Component Directory=\"X\" Id=\"c\"><File Source=\"f.dll\" KeyPath=\"yes\" />" +
            "<RegistryKey Root=\"HKCU\" Key=\"K\" ForceCreateOnInstall=\"yes\" /></Component></ComponentGroup>");
        var issue = Issues(document).Single(i => i.Code == ValidationCode.MixedScope);
        Assert.Contains("files", issue.Message);
    }

    [Fact]
    public void Making_components_legal_moves_HKCU_entries_into_a_component_of_their_own()
    {
        var document = Load(
            "<ComponentGroup Id=\"test\"><Component Directory=\"X\" Id=\"c\"><File Source=\"f.dll\" KeyPath=\"yes\" />" +
            "<RegistryKey Root=\"HKLM\" Key=\"New Key\" ForceCreateOnInstall=\"yes\" />" +
            "<RegistryKey Root=\"HKCU\" Key=\"New Key\" ForceCreateOnInstall=\"yes\" />" +
            "<RegistryValue Root=\"HKCU\" Key=\"Soft\" Name=\"a\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" />" +
            "</Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        Assert.True(registry.CanMakeLegal());
        Assert.True(registry.MakeLegalNeedsDirectory());
        Assert.Throws<ArgumentException>(() => registry.MakeLegal(null));

        var result = registry.MakeLegal("TARGETDIR");
        Assert.Equal(2, result.Moved);
        Assert.DoesNotContain(Issues(document), i => i.Code == ValidationCode.MixedScope || i.Code == ValidationCode.KeyPathConflict || i.Code == ValidationCode.MultipleKeyPaths);
        var original = Components(document, "test").Single(c => (string?)c.Attribute("Id") == "c");
        Assert.DoesNotContain(original.Elements(), e => (string?)e.Attribute("Root") == "HKCU");
        var own = Components(document, "test").Single(c => ((string?)c.Attribute("Id"))!.StartsWith("owr"));
        Assert.Equal(2, own.Elements().Count());
        Assert.False(registry.CanMakeLegal());
    }

    [Fact]
    public void Making_components_legal_with_only_HKCU_keys_adds_a_KeyPath_value()
    {
        var document = Load(
            "<ComponentGroup Id=\"test\"><Component Directory=\"X\" Id=\"c\"><File Source=\"f.dll\" KeyPath=\"yes\" />" +
            "<RegistryKey Root=\"HKCU\" Key=\"K\" ForceCreateOnInstall=\"yes\" /></Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        var result = registry.MakeLegal("TARGETDIR");
        Assert.Equal(1, result.Moved);
        Assert.Single(result.Notes);
        var value = registry.GetItems().Single();
        Assert.Equal("HKCU", value.Root);
        Assert.Equal("K", value.Key);
        Assert.Equal("installed", value.Name);
        Assert.True(value.KeyPath);
    }

    [Fact]
    public void A_per_user_component_of_its_own_with_a_registry_KeyPath_is_not_warned_about()
    {
        var document = Load(
            "<ComponentGroup Id=\"test\">" +
            "<Component Id=\"a\" Directory=\"X\"><RegistryKey Root=\"HKLM\" Key=\"New Key\" ForceCreateOnInstall=\"yes\" /></Component>" +
            "<Component Id=\"b\" Directory=\"LocalAppDataFolder\"><RegistryKey Root=\"HKCU\" Key=\"New Key\" ForceCreateOnInstall=\"yes\" /><RegistryValue Root=\"HKCU\" Key=\"New Key\" Name=\"installed\" Type=\"integer\" Value=\"1\" KeyPath=\"yes\" /></Component>" +
            "<Component Id=\"c\" Directory=\"X\"><File Source=\"f\" KeyPath=\"yes\" /></Component></ComponentGroup>");
        Assert.Empty(Issues(document));
    }

    [Fact]
    public void A_per_user_component_with_no_registry_KeyPath_is_flagged_and_made_legal()
    {
        var document = Load(
            "<ComponentGroup Id=\"test\"><Component Directory=\"X\" Id=\"u\"><RegistryKey Root=\"HKCU\" Key=\"K\" ForceCreateOnInstall=\"yes\" /></Component>" +
            "<Component Directory=\"X\" Id=\"u2\"><RegistryValue Root=\"HKCU\" Key=\"K2\" Name=\"n\" Type=\"string\" Value=\"v\" /></Component></ComponentGroup>");
        var registry = new RegistryDocument(document);
        Assert.Equal(new[] { "u", "u2" }, Issues(document).Where(i => i.Code == ValidationCode.PerUserNoKeyPath).Select(i => (string?)i.Component!.Attribute("Id")).OrderBy(x => x).ToArray());
        Assert.True(registry.CanMakeLegal());
        Assert.False(registry.MakeLegalNeedsDirectory());

        registry.MakeLegal(null);
        Assert.DoesNotContain(Issues(document), i => i.Code == ValidationCode.PerUserNoKeyPath);
        Assert.Equal(2, registry.GetItems().Count(i => i.KeyPath));
    }

    [Fact]
    public void Making_components_legal_trims_extra_registry_KeyPaths()
    {
        var document = Load(
            "<ComponentGroup Id=\"test\">" +
            "<Component Directory=\"X\" Id=\"two\"><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"a\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" /><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"b\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" /></Component>" +
            "<Component Directory=\"X\" Id=\"withfile\"><File Source=\"f\" KeyPath=\"yes\" /><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"c\" Type=\"string\" Value=\"1\" KeyPath=\"yes\" /></Component>" +
            "</ComponentGroup>");
        var registry = new RegistryDocument(document);
        var result = registry.MakeLegal(null);
        Assert.Equal(2, result.KeyPathsCleared);
        Assert.DoesNotContain(Issues(document), i => i.Code == ValidationCode.MultipleKeyPaths || i.Code == ValidationCode.KeyPathConflict);
        Assert.Equal(1, registry.GetItems().Count(i => i.KeyPath));
    }

    [Fact]
    public void Per_user_entries_in_a_per_machine_package_are_warned_about_and_mixed_scope_is_flagged()
    {
        var body = "<ComponentGroup Id=\"Main\"><Component Id=\"c\" Directory=\"X\"><File Source=\"f\" />" +
            "<RegistryValue Root=\"HKCU\" Key=\"A\" Name=\"u\" Type=\"string\" Value=\"1\" /><RegistryValue Root=\"HKLM\" Key=\"A\" Name=\"m\" Type=\"string\" Value=\"1\" /></Component></ComponentGroup>";
        var perMachine = Issues(Load(body));
        Assert.Contains(perMachine, i => i.Code == ValidationCode.PerUserInPerMachinePackage && i.Message.Contains("holds files"));
        Assert.True(Has(perMachine, ValidationCode.MixedScope));

        var perUser = Issues(Load(body, "<Package Name=\"P\" Version=\"1\" Scope=\"perUser\" />"));
        Assert.False(Has(perUser, ValidationCode.PerUserInPerMachinePackage));
        Assert.True(Has(perUser, ValidationCode.MixedScope));
    }

    [Fact]
    public void HKMU_counts_as_per_machine()
    {
        Assert.Equal(RegistryScope.PerMachine, RegistryDocument.ScopeOf("HKMU"));
        Assert.Equal(RegistryScope.PerMachine, RegistryDocument.ScopeOf("HKLM"));
        Assert.Equal(RegistryScope.PerUser, RegistryDocument.ScopeOf("hkcu"));
    }

    // ---------------------------------------------------------------- .reg import into the document

    [Fact]
    public void A_reg_file_is_imported_into_the_designers_components_and_existing_values_are_left_alone()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        registry.AddValue(Value("HKLM", "Software\\Acme", "Existing", "keep"), directory: "TARGETDIR");
        var import = RegFile.Parse(
            "Windows Registry Editor Version 5.00\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\Acme]\r\n\"Existing\"=\"new\"\r\n\"Added\"=dword:0000002a\r\n\r\n" +
            "[HKEY_CURRENT_USER\\Software\\Acme]\r\n\"Mine\"=\"x\"\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\Empty]\r\n\r\n[-HKEY_LOCAL_MACHINE\\Software\\Gone]\r\n");
        var result = registry.ImportReg(import, directory: "TARGETDIR");
        var items = registry.GetItems();

        Assert.Equal(1, result.AlreadyThere);
        Assert.Equal("keep", items.Single(i => i.Name == "Existing").Value);
        Assert.Equal("42", items.Single(i => i.Name == "Added").Value);
        Assert.Equal(RegKind.Integer, items.Single(i => i.Name == "Added").Type);
        Assert.Equal(RegistryScope.PerUser, RegistryDocument.ScopeOf(items.Single(i => i.Name == "Mine").Root));
        Assert.Contains(items, i => i.Kind == RegistryItemKind.RemoveKey && i.Key == "Software\\Gone");
        Assert.Contains(document.Descendants(), e => e.Name.LocalName == "RegistryKey" && (string?)e.Attribute("Key") == "Software\\Empty");
        Assert.NotEqual(items.Single(i => i.Name == "Added").Component, items.Single(i => i.Name == "Mine").Component);
    }

    [Fact]
    public void An_import_can_ask_for_a_registry_view_for_the_components_it_makes()
    {
        var document = Basic();
        var registry = new RegistryDocument(document);
        var import = RegFile.Parse("[HKEY_LOCAL_MACHINE\\Software\\Acme]\r\n\"A\"=\"1\"\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\Empty]\r\n\r\n[-HKEY_LOCAL_MACHINE\\Software\\Gone]\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\R]\r\n\"V\"=-\r\n");
        Assert.True(registry.NeedsDirectory(RegistryScope.PerMachine, "always64"));
        registry.ImportReg(import, directory: "TARGETDIR", bitness: "always64");

        var own = Components(document).Where(c => ((string?)c.Attribute("Id"))!.StartsWith("owr")).ToList();
        Assert.Single(own);
        Assert.Equal("always64", (string?)own[0].Attribute("Bitness"));
        Assert.Equal(4, own[0].Elements().Count());
        Assert.False(registry.NeedsDirectory(RegistryScope.PerMachine, "always64"));
        Assert.True(registry.NeedsDirectory(RegistryScope.PerMachine, "default"));
    }

    [Fact]
    public void Export_reads_the_documents_values()
    {
        var registry = new RegistryDocument(Basic());
        registry.AddValue(Value("HKLM", "Software\\Acme", "Path", "C:\\Program Files\\Acme"), directory: "TARGETDIR");
        registry.AddValue(Value("HKLM", "Software\\Acme", "Count", "10", RegKind.Integer));
        var export = registry.ExportReg();

        Assert.Contains("[HKEY_LOCAL_MACHINE\\Software\\Acme]", export.Text);
        Assert.Contains("\"Path\"=\"C:\\\\Program Files\\\\Acme\"", export.Text);
        Assert.Contains("\"Count\"=dword:0000000a", export.Text);
        Assert.Empty(export.Warnings);
    }
}
