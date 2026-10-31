using Microsoft.Win32;
using RegistryDesigner;
using Xunit;

namespace IsWiXExtension.Tests;

public class SourceValuesTests
{
    [Fact]
    public void Strings_keep_their_text_with_the_brackets_escaped_and_expandable_ones_stay_unexpanded()
    {
        var text = SourceValues.Convert("HKLM", "Software\\A", "Pattern", "[a-z]+", RegistryValueKind.String);
        Assert.Equal("REG_SZ", text.Type);
        Assert.Equal("[a-z]+", text.Display);
        Assert.Equal(RegKind.String, text.Entry!.Kind);
        Assert.Equal("[\\[]a-z[\\]]+", text.Entry.Value);
        Assert.Equal("HKLM", text.Entry.Root);
        Assert.Equal("Software\\A", text.Entry.Key);

        var expand = SourceValues.Convert("HKLM", "Software\\A", "Path", "%SystemRoot%\\x", RegistryValueKind.ExpandString);
        Assert.Equal(RegKind.Expandable, expand.Entry!.Kind);
        Assert.Equal("%SystemRoot%\\x", expand.Entry.Value);
    }

    [Fact]
    public void A_DWORD_is_read_as_unsigned_and_shown_in_hex_and_decimal()
    {
        var value = SourceValues.Convert("HKCU", "A", "n", -1, RegistryValueKind.DWord);
        Assert.Equal(RegKind.Integer, value.Entry!.Kind);
        Assert.Equal("4294967295", value.Entry.Value);
        Assert.Equal("0xffffffff (4294967295)", value.Display);
        Assert.Equal("0x0000002a (42)", SourceValues.Convert("HKCU", "A", "n", 42, RegistryValueKind.DWord).Display);
    }

    [Fact]
    public void Binary_and_multi_string_values_convert_and_the_default_value_has_no_name()
    {
        var binary = SourceValues.Convert("HKCR", "A", string.Empty, new byte[] { 0x0A, 0xFF }, RegistryValueKind.Binary);
        Assert.Null(binary.Name);
        Assert.Null(binary.Entry!.Name);
        Assert.Equal("0AFF", binary.Entry.Value);
        Assert.Equal("0a ff", binary.Display);

        var multi = SourceValues.Convert("HKU", "A", "List", new[] { "one", "[two]" }, RegistryValueKind.MultiString);
        Assert.Equal(new[] { "one", "[\\[]two[\\]]" }, multi.Entry!.Strings.ToArray());
        Assert.Equal("one | [two]", multi.Display);
        Assert.Equal("(zero-length binary value)", SourceValues.Convert("HKCR", "A", "e", new byte[0], RegistryValueKind.Binary).Display);
        Assert.EndsWith("(40 bytes)", SourceValues.Convert("HKCR", "A", "big", new byte[40], RegistryValueKind.Binary).Display);
    }

    [Fact]
    public void A_QWORD_and_other_types_are_shown_but_not_added()
    {
        var qword = SourceValues.Convert("HKLM", "A", "Big", 1L, RegistryValueKind.QWord);
        Assert.Null(qword.Entry);
        Assert.Contains("QWORD", qword.NotSupported!);
        Assert.Equal("0x0000000000000001 (1)", qword.Display);

        var none = SourceValues.Convert("HKLM", "A", "n", new byte[] { 1 }, RegistryValueKind.None);
        Assert.Null(none.Entry);
        Assert.Contains("REG_NONE", none.NotSupported!);
    }
}
