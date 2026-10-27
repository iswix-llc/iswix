using System.Xml;
using System.Xml.Linq;
using IsWiX.Designer;
using Xunit;

namespace IsWiXExtension.Tests;

public class XmlPositionsTests
{
    // Text with the offset to ask about marked by a '|', which is removed: the XML used here never has one.
    static (string Text, int Offset) Marked(string marked)
    {
        var offset = marked.IndexOf('|');
        Assert.True(offset >= 0, "The text needs a | to mark the offset.");
        return (marked.Remove(offset, 1), offset);
    }

    static XmlPositions.Location? LocationAt(string marked)
    {
        var (text, offset) = Marked(marked);
        return XmlPositions.LocationAt(text, offset);
    }

    static string Slice(string text, int[] range) => text.Substring(range[0], range[1] - range[0]);

    const string Package =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<Wix xmlns=\"http://wixtoolset.org/schemas/v4/wxs\" xmlns:ui=\"http://wixtoolset.org/schemas/v4/wxs/ui\">\n" +
        "  <Package Name='My App' Version=\"1.0\">\n" +
        "    <Property Id=\"A\" Value=\"a &gt; b\" Admin=\"yes\" />\n" +
        "    <Property Id=\"B\">text</Property>\n" +
        "    <Feature Id=\"F\">\n" +
        "      <ComponentRef Id=\"C\" />\n" +
        "    </Feature>\n" +
        "  </Package>\n" +
        "</Wix>\n";

    public class LocationAtTests
    {
        [Fact]
        public void The_root_has_an_empty_path()
        {
            var location = LocationAt("<|Wix><Package /></Wix>");

            Assert.NotNull(location);
            Assert.Empty(location.Path);
            Assert.Null(location.Attribute);
        }

        [Theory]
        [InlineData("<Wix><|Package /></Wix>", new[] { 0 })]
        [InlineData("<Wix><Package><Feature /><|Feature /></Package></Wix>", new[] { 0, 1 })]
        [InlineData("<Wix><Package><Feature><Co|mponentRef /></Feature></Package></Wix>", new[] { 0, 0, 0 })]
        [InlineData("<Wix><A /><B><|C /></B></Wix>", new[] { 1, 0 })]
        public void A_path_is_the_index_among_the_parents_elements_from_the_root_down(string marked, int[] path)
        {
            Assert.Equal(path, LocationAt(marked)!.Path);
        }

        [Theory]
        [InlineData("<Wix><|Package Name=\"x\" /></Wix>")]
        [InlineData("<Wix><Package| Name=\"x\" /></Wix>")]
        [InlineData("<Wix><Package Name=\"x\" |/></Wix>")]
        [InlineData("<Wix><Package Name=\"x\" /|></Wix>")]
        [InlineData("<Wix><Package Name=\"x\" />|</Wix>")]
        public void The_whole_of_a_self_closing_tag_is_the_element_apart_from_its_attributes(string marked)
        {
            var location = LocationAt(marked)!;

            Assert.Equal(new[] { 0 }, location.Path);
            Assert.Null(location.Attribute);
        }

        [Theory]
        [InlineData("<Wix><|Package></Package></Wix>")]
        [InlineData("<Wix><Package|></Package></Wix>")]
        [InlineData("<Wix><Package>|</Package></Wix>")]
        [InlineData("<Wix><Package></|Package></Wix>")]
        [InlineData("<Wix><Package></Package|></Wix>")]
        public void The_start_and_end_tags_of_an_element_are_both_the_element(string marked)
        {
            Assert.Equal(new[] { 0 }, LocationAt(marked)!.Path);
        }

        [Theory]
        [InlineData("<Wix><Package Na|me=\"x\" Id=\"y\" /></Wix>", "Name")]
        [InlineData("<Wix><Package |Name=\"x\" Id=\"y\" /></Wix>", "Name")]
        [InlineData("<Wix><Package Name|=\"x\" Id=\"y\" /></Wix>", "Name")]
        [InlineData("<Wix><Package Name=|\"x\" Id=\"y\" /></Wix>", "Name")]
        [InlineData("<Wix><Package Name=\"x|\" Id=\"y\" /></Wix>", "Name")]
        [InlineData("<Wix><Package Name=\"x\"| Id=\"y\" /></Wix>", "Name")]
        [InlineData("<Wix><Package Name=\"x\" I|d=\"y\" /></Wix>", "Id")]
        [InlineData("<Wix><Package Name=\"x\" Id='y|' /></Wix>", "Id")]
        public void An_offset_from_the_start_of_an_attributes_name_to_its_closing_quote_is_in_the_attribute(string marked, string attribute)
        {
            var location = LocationAt(marked)!;

            Assert.Equal(new[] { 0 }, location.Path);
            Assert.Equal(attribute, location.Attribute);
        }

        [Fact]
        public void The_space_inside_a_tag_between_attributes_is_the_element_alone()
        {
            var location = LocationAt("<Wix><Package Name=\"x\"  | Id=\"y\" /></Wix>")!;

            Assert.Equal(new[] { 0 }, location.Path);
            Assert.Null(location.Attribute);
        }

        [Theory]
        [InlineData("<Wix><A Condition=\"a > |b\" Id=\"y\" /><B /></Wix>", new[] { 0 }, "Condition")]
        [InlineData("<Wix><A Condition=\"a > b\" Id=\"y\" /><|B /></Wix>", new[] { 1 }, null)]
        [InlineData("<Wix><A Condition='a \"quoted\" |b' Id=\"y\" /></Wix>", new[] { 0 }, "Condition")]
        [InlineData("<Wix><A Condition=\"it's |b\" Id=\"y\" /></Wix>", new[] { 0 }, "Condition")]
        public void A_greater_than_or_quote_in_a_value_does_not_end_the_attribute_or_the_tag(string marked, int[] path, string? attribute)
        {
            var location = LocationAt(marked)!;

            Assert.Equal(path, location.Path);
            Assert.Equal(attribute, location.Attribute);
        }

        [Fact]
        public void A_declaration_of_a_namespace_is_an_attribute_named_with_its_prefix()
        {
            var location = LocationAt("<Wix xmlns=\"urn:a\" xmlns:u|i=\"urn:b\"><Package /></Wix>")!;

            Assert.Empty(location.Path);
            Assert.Equal("xmlns:ui", location.Attribute);
        }

        [Fact]
        public void The_default_namespace_declaration_is_named_xmlns()
        {
            Assert.Equal("xmlns", LocationAt("<Wix xml|ns=\"urn:a\" />")!.Attribute);
        }

        [Fact]
        public void An_element_with_a_prefix_is_found_like_any_other()
        {
            var location = LocationAt("<Wix xmlns:ui=\"urn:b\"><ui:Dialog Id=\"D|\" /></Wix>")!;

            Assert.Equal(new[] { 0 }, location.Path);
            Assert.Equal("Id", location.Attribute);
        }

        [Fact]
        public void The_text_of_an_element_with_no_child_elements_is_the_element()
        {
            var location = LocationAt("<Wix><Property>te|xt</Property></Wix>")!;

            Assert.Equal(new[] { 0 }, location.Path);
            Assert.Null(location.Attribute);
        }

        [Theory]
        [InlineData("<Wix>\n  |\n  <Package />\n</Wix>")]
        [InlineData("<Wix>\n  <Package />\n  |\n</Wix>")]
        [InlineData("<Wix><Package>\n  |\n  <Feature />\n</Package></Wix>")]
        [InlineData("<Wix><!-- a |comment --><Package /></Wix>")]
        public void Space_between_the_children_of_an_element_is_no_element(string marked)
        {
            // It is the space around the children, so it isn't the parent, which would make every blank line in a
            // package select the package.
            Assert.Null(LocationAt(marked));
        }

        [Theory]
        [InlineData("<Wix>|<Package /></Wix>", new[] { 0 })]
        [InlineData("<Wix><Package>|<Feature /></Package></Wix>", new[] { 0, 0 })]
        [InlineData("<Wix><Package />|<Fragment /></Wix>", new[] { 1 })]
        [InlineData("<Wix>\n  <Package />\n|</Wix>", new int[0])]
        [InlineData("<?xml version=\"1.0\"?>|<Wix />\n", new int[0])]
        [InlineData("<Wix />|\n", new int[0])]
        public void A_caret_touching_a_tag_is_in_that_tag(string marked, int[] path)
        {
            // The tag after the caret wins over the one before it, as a click at the start of a line is on what follows.
            Assert.Equal(path, LocationAt(marked)!.Path);
        }

        [Theory]
        [InlineData("|<?xml version=\"1.0\"?><Wix />")]
        [InlineData("<?xml vers|ion=\"1.0\"?>\n<Wix />")]
        [InlineData("<Wix />\n|")]
        [InlineData("<Wix />\n<!-- after| -->")]
        public void Offsets_outside_the_root_are_in_no_element(string marked)
        {
            Assert.Null(LocationAt(marked));
        }

        [Fact]
        public void The_innermost_element_is_the_one_found()
        {
            var location = LocationAt("<Wix><A><B><C Id=\"1|\" /></B></A></Wix>")!;

            Assert.Equal(new[] { 0, 0, 0 }, location.Path);
            Assert.Equal("Id", location.Attribute);
        }

        [Fact]
        public void An_element_after_a_sibling_that_has_children_is_not_confused_with_it()
        {
            var location = LocationAt("<Wix><A><B /></A><|C /></Wix>")!;

            Assert.Equal(new[] { 1 }, location.Path);
        }

        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("\r")]
        public void Line_endings_of_any_kind_are_counted(string newline)
        {
            var marked = "<Wix>" + newline + "  <A Id=\"1\" />" + newline + "  <B" + newline + "    Name=\"na|me\"" + newline + "    Id=\"2\" />" + newline + "</Wix>";

            var location = LocationAt(marked)!;

            Assert.Equal(new[] { 1 }, location.Path);
            Assert.Equal("Name", location.Attribute);
        }

        [Fact]
        public void Tabs_and_non_ascii_text_before_the_offset_do_not_move_it()
        {
            var location = LocationAt("<Wix>\n\t<A Name=\"é日本語\" />\n\t<B Id=\"x|\" />\n</Wix>")!;

            Assert.Equal(new[] { 1 }, location.Path);
            Assert.Equal("Id", location.Attribute);
        }

        [Fact]
        public void Text_that_is_not_well_formed_throws()
        {
            var (text, offset) = Marked("<Wix><A |</Wix>");

            Assert.Throws<XmlException>(() => XmlPositions.LocationAt(text, offset));
        }

        [Fact]
        public void A_doctype_is_ignored_rather_than_processed()
        {
            var (text, offset) = Marked("<!DOCTYPE Wix [<!ENTITY e \"x\">]>\n<Wix><A Id=\"1|\" /></Wix>");

            var location = XmlPositions.LocationAt(text, offset)!;

            Assert.Equal(new[] { 0 }, location.Path);
            Assert.Equal("Id", location.Attribute);
        }
    }

    public class RangeOfTests
    {
        [Fact]
        public void An_attribute_is_its_name_value_and_quotes()
        {
            var range = XmlPositions.RangeOf(Package, new[] { 0, 0 }, "Value")!;

            Assert.Equal("Value=\"a &gt; b\"", Slice(Package, range));
        }

        [Fact]
        public void An_attribute_in_single_quotes_is_found()
        {
            var range = XmlPositions.RangeOf(Package, new[] { 0 }, "Name")!;

            Assert.Equal("Name='My App'", Slice(Package, range));
        }

        [Fact]
        public void A_namespace_declaration_is_found_by_its_prefixed_name()
        {
            var range = XmlPositions.RangeOf(Package, Array.Empty<int>(), "xmlns:ui")!;

            Assert.Equal("xmlns:ui=\"http://wixtoolset.org/schemas/v4/wxs/ui\"", Slice(Package, range));
        }

        [Fact]
        public void An_element_without_an_attribute_asked_for_is_its_start_tag()
        {
            var range = XmlPositions.RangeOf(Package, new[] { 0 })!;

            Assert.Equal("<Package Name='My App' Version=\"1.0\">", Slice(Package, range));
        }

        [Fact]
        public void The_start_tag_of_an_element_with_children_stops_before_them()
        {
            var range = XmlPositions.RangeOf(Package, new[] { 0, 2 })!;

            Assert.Equal("<Feature Id=\"F\">", Slice(Package, range));
        }

        [Fact]
        public void A_self_closing_element_is_the_whole_tag()
        {
            var range = XmlPositions.RangeOf(Package, new[] { 0, 0 })!;

            Assert.Equal("<Property Id=\"A\" Value=\"a &gt; b\" Admin=\"yes\" />", Slice(Package, range));
        }

        [Fact]
        public void An_attribute_the_element_does_not_have_falls_back_to_the_start_tag()
        {
            // So a designer's field for an attribute that isn't written yet still shows the element.
            var range = XmlPositions.RangeOf(Package, new[] { 0, 1 }, "Value")!;

            Assert.Equal("<Property Id=\"B\">", Slice(Package, range));
        }

        [Fact]
        public void The_root_is_found_by_an_empty_path()
        {
            var range = XmlPositions.RangeOf(Package, Array.Empty<int>())!;

            Assert.StartsWith("<Wix xmlns=", Slice(Package, range));
            Assert.EndsWith(">", Slice(Package, range));
        }

        [Theory]
        [InlineData(new[] { 1 })]
        [InlineData(new[] { 0, 9 })]
        [InlineData(new[] { 0, 0, 0 })]
        public void A_path_with_no_element_is_null(int[] path)
        {
            Assert.Null(XmlPositions.RangeOf(Package, path));
        }

        [Fact]
        public void An_attribute_that_contains_a_greater_than_does_not_end_the_start_tag_early()
        {
            const string text = "<Wix><A Condition=\"x > 1\" Id=\"y\"><B /></A></Wix>";

            var range = XmlPositions.RangeOf(text, new[] { 0 })!;

            Assert.Equal("<A Condition=\"x > 1\" Id=\"y\">", Slice(text, range));
        }

        [Fact]
        public void An_attribute_on_a_later_line_is_found()
        {
            const string text = "<Wix>\r\n  <A\r\n    Name=\"n\"\r\n    Id=\"y\" />\r\n</Wix>";

            var range = XmlPositions.RangeOf(text, new[] { 0 }, "Id")!;

            Assert.Equal("Id=\"y\"", Slice(text, range));
        }

        [Fact]
        public void Text_that_is_not_well_formed_throws()
        {
            Assert.Throws<XmlException>(() => XmlPositions.RangeOf("<Wix><A></Wix>", new[] { 0 }));
        }
    }

    public class LocationAndRangeTests
    {
        [Fact]
        public void Every_offset_in_an_attribute_is_found_as_that_attribute()
        {
            var name = Package.IndexOf("Admin=", StringComparison.Ordinal);

            for (var offset = name; offset < name + "Admin=\"yes\"".Length; offset++)
            {
                var location = XmlPositions.LocationAt(Package, offset)!;
                Assert.Equal("Admin", location.Attribute);
                Assert.Equal(new[] { 0, 0 }, location.Path);
            }
        }

        [Fact]
        public void The_range_of_what_is_found_at_an_offset_contains_the_offset()
        {
            // For every offset of a real file that is in an element: the attribute's range if it is in one, else the
            // element's start tag, end tag or text, which the start tag's range doesn't cover.
            for (var offset = 0; offset < Package.Length; offset++)
            {
                var location = XmlPositions.LocationAt(Package, offset);
                if (location?.Attribute == null)
                {
                    continue;
                }

                var range = XmlPositions.RangeOf(Package, location.Path, location.Attribute)!;
                Assert.True(
                    range[0] <= offset && offset <= range[1],
                    $"Offset {offset} ('{Package[offset]}') was found in the attribute {location.Attribute} of {string.Join("/", location.Path)} but is outside its range.");
            }
        }
    }

    public class PathTests
    {
        static IEnumerable<XElement> AllElements(XDocument document) => document.Root!.DescendantsAndSelf();

        [Fact]
        public void The_path_of_every_element_leads_back_to_it()
        {
            var document = XDocument.Parse(Package);

            foreach (var element in AllElements(document))
            {
                Assert.Same(element, XmlPositions.ElementAt(document, XmlPositions.PathOf(element)));
            }
        }

        [Fact]
        public void The_path_of_every_element_is_where_the_scanner_finds_it_in_the_text()
        {
            // The two are computed apart, from the text and from the parsed document: this is what makes a designer's
            // element and the editor's text agree.
            var document = XDocument.Parse(Package);

            foreach (var element in AllElements(document))
            {
                var range = XmlPositions.RangeOf(Package, XmlPositions.PathOf(element))!;
                Assert.StartsWith("<" + element.Name.LocalName, Slice(Package, range).Replace("<ui:", "<"));
            }
        }

        [Fact]
        public void A_location_in_the_text_leads_to_the_element_there()
        {
            var document = XDocument.Parse(Package);
            var offset = Package.IndexOf("Admin=", StringComparison.Ordinal);

            var location = XmlPositions.LocationAt(Package, offset)!;
            var element = XmlPositions.ElementAt(document, location.Path)!;

            Assert.Equal("Property", element.Name.LocalName);
            Assert.Equal("A", (string?)element.Attribute("Id"));
        }

        [Fact]
        public void The_root_has_an_empty_path()
        {
            var document = XDocument.Parse(Package);

            Assert.Empty(XmlPositions.PathOf(document.Root!));
            Assert.Same(document.Root, XmlPositions.ElementAt(document, Array.Empty<int>()));
        }

        [Theory]
        [InlineData(new[] { 7 })]
        [InlineData(new[] { 0, 0, 0 })]
        [InlineData(new[] { -1 })]
        public void A_path_with_no_element_leads_nowhere(int[] path)
        {
            var document = XDocument.Parse(Package);

            Assert.Null(XmlPositions.ElementAt(document, path));
        }

        [Fact]
        public void An_element_added_to_the_document_is_found_in_the_text_it_is_saved_as()
        {
            var ns = XNamespace.Get("http://wixtoolset.org/schemas/v4/wxs");
            var document = XDocument.Parse(Package);
            document.Root!.Element(ns + "Package")!.Add(new XElement(ns + "Fragment", new XAttribute("Id", "Added")));

            var added = AllElements(document).Last();
            var saved = document.ToString(SaveOptions.None);

            var range = XmlPositions.RangeOf(saved, XmlPositions.PathOf(added))!;
            Assert.Contains("Id=\"Added\"", Slice(saved, range));
        }
    }
}
