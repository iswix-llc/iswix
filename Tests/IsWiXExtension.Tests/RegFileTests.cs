using RegistryDesigner;
using Xunit;

namespace IsWiXExtension.Tests;

public class RegFileTests
{
    static RegEntry Single(RegImport import, string? name) => import.Entries.Single(e => e.Name == name && !e.KeyOnly);

    [Fact]
    public void Strings_dwords_and_the_default_value_are_read()
    {
        var import = RegFile.Parse(
            "Windows Registry Editor Version 5.00\r\n\r\n; a comment\r\n[HKEY_LOCAL_MACHINE\\Software\\Acme]\r\n@=\"default\"\r\n\"Path\"=\"C:\\\\Dir\\\\\\\"q\\\"\"\r\n\"Count\"=dword:000000ff\r\n");

        Assert.Equal("HKLM", Single(import, null).Root);
        Assert.Equal("Software\\Acme", Single(import, null).Key);
        Assert.Equal("default", Single(import, null).Value);
        Assert.Equal("C:\\Dir\\\"q\"", Single(import, "Path").Value);
        Assert.Equal(RegKind.Integer, Single(import, "Count").Kind);
        Assert.Equal("255", Single(import, "Count").Value);
        Assert.Empty(import.Skipped);
    }

    [Fact]
    public void Literal_brackets_in_strings_are_escaped_so_they_are_not_property_references()
    {
        var import = RegFile.Parse("[HKEY_CURRENT_USER\\Software\\A]\r\n\"Pattern\"=\"[a-z]+\"\r\n");
        Assert.Equal("[\\[]a-z[\\]]+", Single(import, "Pattern").Value);
        Assert.False(RegFile.HasToken(Single(import, "Pattern").Value));
        Assert.True(RegFile.HasToken("[INSTALLFOLDER]x"));
        Assert.Equal("[a-z]+", RegFile.UnescapeBrackets("[\\[]a-z[\\]]+"));
    }

    [Fact]
    public void Binary_expandable_and_multi_string_values_are_read_from_hex()
    {
        var import = RegFile.Parse(
            "[HKEY_CLASSES_ROOT\\A]\r\n" +
            "\"Bin\"=hex:0a,ff,00\r\n" +
            "\"Exp\"=hex(2):25,00,50,00,41,00,54,00,48,00,25,00,00,00\r\n" +
            "\"Multi\"=hex(7):61,00,62,00,00,00,63,00,00,00,00,00\r\n");

        Assert.Equal("0AFF00", Single(import, "Bin").Value);
        Assert.Equal(RegKind.Binary, Single(import, "Bin").Kind);
        Assert.Equal("%PATH%", Single(import, "Exp").Value);
        Assert.Equal(RegKind.Expandable, Single(import, "Exp").Kind);
        Assert.Equal(new[] { "ab", "c" }, Single(import, "Multi").Strings.ToArray());
    }

    [Fact]
    public void Hex_lines_continued_with_a_backslash_are_joined()
    {
        var import = RegFile.Parse("[HKEY_USERS\\A]\r\n\"Bin\"=hex:01,02,\\\r\n  03,04,\\\r\n  05\r\n");
        Assert.Equal("0102030405", Single(import, "Bin").Value);
        Assert.Equal("HKU", Single(import, "Bin").Root);
    }

    [Fact]
    public void A_QWORD_and_other_unsupported_types_are_skipped_and_reported()
    {
        var import = RegFile.Parse("[HKEY_LOCAL_MACHINE\\A]\r\n\"Big\"=hex(b):01,00,00,00,00,00,00,00\r\n\"Link\"=hex(6):00\r\n\"Ok\"=\"x\"\r\n");

        Assert.Single(import.Entries.Where(e => !e.KeyOnly));
        Assert.Equal(2, import.Skipped.Count);
        Assert.Contains("QWORD", import.Skipped[0]);
        Assert.Contains("hex(6)", import.Skipped[1]);
    }

    [Fact]
    public void Keys_and_values_to_remove_and_keys_with_no_values_are_read()
    {
        var import = RegFile.Parse("[-HKEY_LOCAL_MACHINE\\Gone]\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\Acme]\r\n\"Old\"=-\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\Empty]\r\n");

        Assert.Contains(import.Entries, e => e.RemoveKey && e.Key == "Gone");
        Assert.Contains(import.Entries, e => e.RemoveValue && e.Name == "Old");
        Assert.Contains(import.Entries, e => e.KeyOnly && e.Key == "Software\\Empty");
        Assert.DoesNotContain(import.Entries, e => e.KeyOnly && e.Key == "Software\\Acme");
    }

    [Fact]
    public void A_root_windows_installer_does_not_write_is_skipped()
    {
        var import = RegFile.Parse("[HKEY_CURRENT_CONFIG\\A]\r\n\"x\"=\"1\"\r\n");
        Assert.Empty(import.Entries);
        Assert.Contains(import.Skipped, s => s.Contains("HKEY_CURRENT_CONFIG"));
    }

    [Fact]
    public void Export_writes_each_type_as_regedit_does()
    {
        RegistryItem Item(string name, RegKind type, string? value, params string[] strings)
        {
            var item = new RegistryItem { Kind = RegistryItemKind.Value, Root = "HKLM", Key = "Software\\A", Name = name, Type = type, Value = value };
            item.MultiStrings.AddRange(strings);
            return item;
        }

        var export = RegFile.Write(new[]
        {
            Item("S", RegKind.String, "a \"b\" c\\d"),
            Item("D", RegKind.Integer, "255"),
            Item("B", RegKind.Binary, "0AFF"),
            Item("E", RegKind.Expandable, "%PATH%"),
            Item("M", RegKind.MultiString, null, "ab", "c"),
            Item(string.Empty, RegKind.String, "default"),
        });

        Assert.StartsWith("Windows Registry Editor Version 5.00\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\A]\r\n", export.Text);
        Assert.Contains("\"S\"=\"a \\\"b\\\" c\\\\d\"", export.Text);
        Assert.Contains("\"D\"=dword:000000ff", export.Text);
        Assert.Contains("\"B\"=hex:0a,ff", export.Text);
        Assert.Contains("\"E\"=hex(2):25,00,50,00,41,00,54,00,48,00,25,00,00,00", export.Text);
        Assert.Contains("\"M\"=hex(7):61,00,62,00,00,00,63,00,00,00,00,00", export.Text);
        Assert.Contains("@=\"default\"", export.Text);
        Assert.Empty(export.Warnings);
    }

    [Fact]
    public void Export_unescapes_brackets_and_warns_about_property_references()
    {
        var export = RegFile.Write(new[]
        {
            new RegistryItem { Kind = RegistryItemKind.Value, Root = "HKCU", Key = "A", Name = "Lit", Type = RegKind.String, Value = "[\\[]x[\\]]" },
            new RegistryItem { Kind = RegistryItemKind.Value, Root = "HKCU", Key = "A", Name = "Ref", Type = RegKind.String, Value = "[INSTALLFOLDER]bin" },
        });

        Assert.Contains("\"Lit\"=\"[x]\"", export.Text);
        Assert.Contains("\"Ref\"=\"[INSTALLFOLDER]bin\"", export.Text);
        Assert.Single(export.Warnings);
        Assert.Contains("Ref", export.Warnings[0]);
    }

    [Fact]
    public void Export_writes_removals()
    {
        var export = RegFile.Write(new[]
        {
            new RegistryItem { Kind = RegistryItemKind.RemoveKey, Root = "HKLM", Key = "Gone" },
            new RegistryItem { Kind = RegistryItemKind.RemoveValue, Root = "HKLM", Key = "Software\\A", Name = "Old" },
        });

        Assert.Contains("[-HKEY_LOCAL_MACHINE\\Gone]", export.Text);
        Assert.Contains("\"Old\"=-", export.Text);
    }

    [Fact]
    public void What_is_exported_can_be_imported_again()
    {
        var items = new List<RegistryItem>();
        RegistryItem Add(string name, RegKind type, string? value, params string[] strings)
        {
            var item = new RegistryItem { Kind = RegistryItemKind.Value, Root = "HKLM", Key = "Software\\Round", Name = name, Type = type, Value = value };
            item.MultiStrings.AddRange(strings);
            items.Add(item);
            return item;
        }

        Add("s", RegKind.String, "he said \"hi\" \\ there");
        Add("d", RegKind.Integer, "4294967295");
        Add("b", RegKind.Binary, "000102FF");
        Add("e", RegKind.Expandable, "%TEMP%\\x");
        Add("m", RegKind.MultiString, null, "one", "two");
        var import = RegFile.Parse(RegFile.Write(items).Text);

        Assert.Equal("he said \"hi\" \\ there", Single(import, "s").Value);
        Assert.Equal("4294967295", Single(import, "d").Value);
        Assert.Equal("000102FF", Single(import, "b").Value);
        Assert.Equal("%TEMP%\\x", Single(import, "e").Value);
        Assert.Equal(new[] { "one", "two" }, Single(import, "m").Strings.ToArray());
        Assert.Empty(import.Skipped);
    }

    [Fact]
    public void Hex_helpers_round_trip_and_reject_bad_input()
    {
        Assert.Equal("0AFF", RegFile.ToHex(RegFile.FromHex("0aff")!));
        Assert.Null(RegFile.FromHex("ABC"));
        Assert.Null(RegFile.FromHex("GG"));
    }

    [Fact]
    public void Copied_keys_and_values_carry_a_marker_that_a_paste_reads()
    {
        var text = RegFile.WithCopyMarker("Windows Registry Editor Version 5.00\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\A\\B]\r\n\"x\"=\"1\"\r\n", "keys", "HKLM", "Software\\A\\B");
        Assert.StartsWith("Windows Registry Editor Version 5.00\r\n; IsWiX copy: keys HKLM\\Software\\A\\B\r\n", text);
        Assert.True(RegFile.TryReadCopyMarker(text, out var kind, out var root, out var key));
        Assert.Equal("keys", kind);
        Assert.Equal("HKLM", root);
        Assert.Equal("Software\\A\\B", key);
        Assert.Single(RegFile.Parse(text).Entries);
        Assert.False(RegFile.TryReadCopyMarker("[HKEY_LOCAL_MACHINE\\A]", out _, out _, out _));
    }

    [Fact]
    public void A_pasted_key_goes_under_the_target_by_its_own_name_and_pasted_values_into_the_target()
    {
        var import = RegFile.Parse("[HKEY_LOCAL_MACHINE\\Software\\A\\B]\r\n\"x\"=\"1\"\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\A\\B\\C]\r\n\"y\"=\"2\"\r\n\r\n[HKEY_LOCAL_MACHINE\\Software\\A\\Bee]\r\n\"z\"=\"3\"\r\n");
        RegFile.Retarget(import, "keys", "HKLM", "Software\\A\\B", "HKCU", "Software\\Target");

        Assert.Equal(new[] { "Software\\Target\\B", "Software\\Target\\B\\C", "Software\\A\\Bee" }, import.Entries.Select(e => e.Key).ToArray());
        Assert.Equal("HKCU", import.Entries[0].Root);
        Assert.Equal("HKLM", import.Entries[2].Root);

        var values = RegFile.Parse("[HKEY_LOCAL_MACHINE\\Software\\A]\r\n\"x\"=\"1\"\r\n");
        RegFile.Retarget(values, "values", "HKLM", "Software\\A", "HKLM", string.Empty);
        Assert.Equal(string.Empty, values.Entries[0].Key);

        var hive = RegFile.Parse("[HKEY_LOCAL_MACHINE\\Software\\A\\B]\r\n\"x\"=\"1\"\r\n");
        RegFile.Retarget(hive, "keys", "HKLM", "Software\\A\\B", "HKU", string.Empty);
        Assert.Equal("B", hive.Entries[0].Key);
    }

    [Fact]
    public void HKMU_survives_a_round_trip_with_a_warning()
    {
        var item = new RegistryItem { Kind = RegistryItemKind.Value, Root = "HKMU", Key = "Software\\A", Name = "n", Type = RegKind.String, Value = "v" };
        var export = RegFile.Write(new[] { item });
        Assert.Single(export.Warnings);
        var import = RegFile.Parse(export.Text);
        Assert.Equal("HKMU", import.Entries.Single().Root);
        Assert.Empty(import.Skipped);
    }

    [Fact]
    public void Extra_keys_are_written_with_no_values()
    {
        var export = RegFile.Write(new RegistryItem[0], new[] { new KeyValuePair<string, string>("HKLM", "Software\\Empty") });
        Assert.Contains("[HKEY_LOCAL_MACHINE\\Software\\Empty]", export.Text);
        Assert.Contains(RegFile.Parse(export.Text).Entries, e => e.KeyOnly && e.Key == "Software\\Empty");
    }
}
