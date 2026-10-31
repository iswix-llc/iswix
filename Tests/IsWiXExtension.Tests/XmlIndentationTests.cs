using FireworksFramework.Managers;
using Xunit;

namespace IsWiXExtension.Tests;

public class XmlIndentationTests
{
    static string Lf(string text) => text.Replace("\r\n", "\n");

    [Fact]
    public void A_two_space_file_stays_two_spaces_and_its_odd_line_is_fixed()
    {
        var text = "<Wix>\n  <Package>\n    <Feature />\n      <Property />\n    <Component />\n  </Package>\n</Wix>";
        Assert.Equal("  ", XmlIndentation.DetectUnit(text));
        Assert.Equal("<Wix>\n  <Package>\n    <Feature />\n    <Property />\n    <Component />\n  </Package>\n</Wix>", XmlIndentation.Format(text));
    }

    [Fact]
    public void A_tab_file_stays_tabs()
    {
        var text = "<Wix>\n\t<Package>\n\t\t<Feature />\n    <Property />\n\t</Package>\n</Wix>";
        Assert.Equal("\t", XmlIndentation.DetectUnit(text));
        Assert.Equal("<Wix>\n\t<Package>\n\t\t<Feature />\n\t\t<Property />\n\t</Package>\n</Wix>", XmlIndentation.Format(text));
    }

    [Fact]
    public void A_file_with_nothing_indented_gets_four_spaces()
    {
        Assert.Equal("    ", XmlIndentation.DetectUnit("<Wix>\n<Package>\n</Package>\n</Wix>"));
        Assert.Equal("<Wix>\n    <Package>\n    </Package>\n</Wix>", XmlIndentation.Format("<Wix>\n<Package>\n</Package>\n</Wix>"));
    }

    [Fact]
    public void An_explicit_unit_overrides_the_detected_one()
    {
        Assert.Equal("<a>\n    <b>\n        <c />\n    </b>\n</a>", XmlIndentation.Format("<a>\n  <b>\n    <c />\n  </b>\n</a>", "    "));
    }

    [Fact]
    public void Formatting_is_idempotent_and_keeps_crlf_and_the_bom()
    {
        var text = "\uFEFF<?xml version=\"1.0\"?>\r\n<Wix>\r\n\t<Package>\r\n\t\t<Feature />\r\n<Property />\r\n\t</Package>\r\n</Wix>\r\n";
        var once = XmlIndentation.Format(text);
        Assert.StartsWith("\uFEFF<?xml", once);
        Assert.Equal("\uFEFF<?xml version=\"1.0\"?>\r\n<Wix>\r\n\t<Package>\r\n\t\t<Feature />\r\n\t\t<Property />\r\n\t</Package>\r\n</Wix>\r\n", once);
        Assert.Equal(once, XmlIndentation.Format(once));
    }

    [Fact]
    public void Text_content_and_the_end_tag_of_an_element_that_holds_text_are_not_touched()
    {
        var text = "<Wix>\n<Package>\n<Custom Action=\"A\">\n      NOT Installed\n      </Custom>\n</Package>\n</Wix>";
        Assert.Equal("<Wix>\n    <Package>\n        <Custom Action=\"A\">\n      NOT Installed\n      </Custom>\n    </Package>\n</Wix>", XmlIndentation.Format(text));
    }

    [Fact]
    public void Preserved_space_content_is_not_touched()
    {
        var text = "<a>\n<b xml:space=\"preserve\">\n   <c />\n  </b>\n</a>";
        Assert.Equal("<a>\n    <b xml:space=\"preserve\">\n   <c />\n  </b>\n</a>", XmlIndentation.Format(text));
    }

    [Fact]
    public void A_multi_line_comment_moves_as_a_block()
    {
        var text = "<a>\n<!--\n  note\n    more\n-->\n<b />\n</a>";
        Assert.Equal("<a>\n    <!--\n      note\n        more\n    -->\n    <b />\n</a>", XmlIndentation.Format(text));
    }

    [Fact]
    public void The_lines_of_a_multi_line_tag_move_with_it_but_attribute_value_lines_do_not()
    {
        var text = "<a>\n  <b>\n<c x=\"1\"\n   y=\"line one\n  line two\"\n   z=\"3\" />\n  </b>\n</a>";
        Assert.Equal("<a>\n  <b>\n    <c x=\"1\"\n       y=\"line one\n  line two\"\n       z=\"3\" />\n  </b>\n</a>", XmlIndentation.Format(text));
    }

    [Fact]
    public void Content_is_unchanged_apart_from_whitespace()
    {
        var text = "<Wix xmlns=\"urn:x\">\n<Package Name=\"a &amp; b\">\n<!-- c -->\n<![CDATA[ <x/> ]]>\n</Package>\n</Wix>";
        var formatted = XmlIndentation.Format(text);
        Assert.Equal(System.Xml.Linq.XDocument.Parse(text).Root!.Value.Trim(), System.Xml.Linq.XDocument.Parse(formatted).Root!.Value.Trim());
        Assert.Equal(System.Text.RegularExpressions.Regex.Replace(text, @"\n\s+", "\n"), System.Text.RegularExpressions.Regex.Replace(formatted, @"\n\s+", "\n"));
    }

    [Fact]
    public void Text_that_is_not_well_formed_is_returned_as_it_came()
    {
        const string text = "<a>\n<b>\n</a>";
        Assert.Equal(text, XmlIndentation.Format(text));
        Assert.Equal("    ", XmlIndentation.DetectUnit(text));
    }

    [Fact]
    public void Several_elements_on_one_line_only_move_the_line()
    {
        Assert.Equal("<a>\n    <b><c /></b>\n</a>", XmlIndentation.Format("<a>\n  <b><c /></b>\n</a>", "    "));
    }

    [Fact]
    public void Conditional_markers_before_the_root_stay_at_the_left()
    {
        var text = "<!--#if (x) -->\n<Wix>\n<!--#endif -->\n<Package />\n</Wix>";
        Assert.Equal("<!--#if (x) -->\n<Wix>\n    <!--#endif -->\n    <Package />\n</Wix>", XmlIndentation.Format(text));
    }
}
