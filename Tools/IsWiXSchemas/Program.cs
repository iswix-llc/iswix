// Copyright (c) ISWIX LLC. Licensed under the MS-PL.
//
//   dotnet run --project Tools/IsWiXSchemas -- <wix>/src/xsd <output folder>
//
// The WiX Toolset keeps its XML schemas as one fragment per element, type or attribute group, under
// src/xsd/<namespace>: ns.xsd holds the namespace and its description, and every other file one top-level schema
// construct, written without the XML Schema prefix. Visual Studio's XML editor needs one schema per namespace, so
// for each namespace folder this writes iswix_<folder>.xsd: an xs:schema with the target namespace as its default
// namespace, importing the main WiX namespace if it's an extension's, holding the fragments with their XML Schema
// elements put in the XML Schema namespace. The WiX-only appinfo (parents, see-also and MSI table links, used for the
// WiX documentation) is left out; documentation is kept as text. Every schema written is then compiled, together,
// with .NET's XML schema compiler, and any error fails the run.

using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: dotnet run --project Tools/IsWiXSchemas -- <wix>/src/xsd <output folder>");
    return 1;
}

XNamespace xs = "http://www.w3.org/2001/XMLSchema";
const string WixNamespace = "http://wixtoolset.org/schemas/v4/wxs";
// Attributes WiX adds to schema constructs for its documentation (the version that brought in or deprecated a
// construct), which XML Schema doesn't allow.
var WixOnlyAttributes = new HashSet<string> { "version", "deprecated" };
var source = Path.GetFullPath(args[0]);
var output = Path.GetFullPath(args[1]);
Directory.CreateDirectory(output);

var written = new List<string>();
foreach (var folder in Directory.GetDirectories(source).OrderBy(f => f, StringComparer.Ordinal))
{
    var nsFile = Path.Combine(folder, "ns.xsd");
    if (!File.Exists(nsFile)) continue;

    var ns = XElement.Load(nsFile);
    var target = (string?)ns.Attribute("targetNamespace") ?? throw new InvalidDataException($"{nsFile} has no targetNamespace.");
    XNamespace targetNs = target;

    var schema = new XElement(xs + "schema",
        new XAttribute(XNamespace.Xmlns + "xs", xs.NamespaceName),
        new XAttribute("targetNamespace", target),
        new XAttribute("xmlns", target));
    var isExtension = target != WixNamespace;
    if (isExtension) schema.Add(new XAttribute(XNamespace.Xmlns + "wxs", WixNamespace));

    var annotation = ns.Elements().FirstOrDefault(e => e.Name.LocalName == "annotation");
    if (annotation != null) schema.Add(Convert(annotation));
    if (isExtension) schema.Add(new XElement(xs + "import", new XAttribute("namespace", WixNamespace)));

    var fragments = Directory.GetFiles(folder, "*.xsd")
        .Where(f => !string.Equals(Path.GetFileName(f), "ns.xsd", StringComparison.OrdinalIgnoreCase))
        .OrderBy(f => f, StringComparer.Ordinal);
    foreach (var fragment in fragments)
        schema.Add(Convert(XElement.Load(fragment)));

    var file = Path.Combine(output, $"iswix_{Path.GetFileName(folder)}.xsd");
    Write(schema, file);
    written.Add(file);
    Console.WriteLine($"{Path.GetFileName(file)}: {target} ({schema.Elements(xs + "element").Count()} elements)");
}

// Compile them all together, as the XML editor sees them: extensions refer to the main namespace's types.
var set = new XmlSchemaSet { XmlResolver = null };
var errors = new List<string>();
set.ValidationEventHandler += (s, e) => errors.Add($"{e.Severity}: {e.Message} ({Path.GetFileName(e.Exception?.SourceUri ?? string.Empty)}:{e.Exception?.LineNumber})");
foreach (var file in written)
{
    using var reader = XmlReader.Create(file);
    var read = XmlSchema.Read(reader, (s, e) => errors.Add($"{Path.GetFileName(file)}: {e.Message}"));
    if (read != null) set.Add(read);
}
set.Compile();
foreach (var error in errors) Console.Error.WriteLine(error);
Console.WriteLine(errors.Count == 0 ? $"Compiled {written.Count} schemas, {set.GlobalElements.Count} global elements." : $"{errors.Count} schema error(s).");
return errors.Count == 0 ? 0 : 1;

// A fragment's construct, with its XML Schema elements in the XML Schema namespace, whatever namespace they were
// parsed in; documentation as text; WiX's appinfo dropped.
XElement Convert(XElement element)
{
    var converted = new XElement(xs + element.Name.LocalName,
        element.Attributes().Where(a => !a.IsNamespaceDeclaration && !WixOnlyAttributes.Contains(a.Name.LocalName)));
    foreach (var node in element.Nodes())
    {
        switch (node)
        {
            case XElement child when child.Name.LocalName == "appinfo":
                break;
            case XElement child when child.Name.LocalName == "documentation":
                converted.Add(new XElement(xs + "documentation", DocumentationText(child)));
                break;
            case XElement child:
                converted.Add(Convert(child));
                break;
            case XComment comment:
                converted.Add(new XComment(comment.Value));
                break;
        }
    }

    // An annotation left with nothing in it isn't valid XML Schema.
    if (converted.Name.LocalName == "annotation" && !converted.HasElements) return new XElement(xs + "annotation", new XElement(xs + "documentation"));
    return converted;
}

// Documentation as plain text: markup in it (examples, emphasis) is kept as written, as text.
static string DocumentationText(XElement documentation)
{
    var text = new StringBuilder();
    foreach (var node in documentation.Nodes())
        text.Append(node is XText t ? t.Value : node.ToString(SaveOptions.DisableFormatting));
    return text.ToString();
}

static void Write(XElement schema, string file)
{
    var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", Encoding = new UTF8Encoding(true), NewLineChars = "\r\n" };
    using var writer = XmlWriter.Create(file, settings);
    writer.WriteStartDocument();
    writer.WriteComment(" Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information. ");
    writer.WriteComment(" Generated by IsWiX's Tools/IsWiXSchemas from the WiX Toolset's src/xsd (iswix-llc/wix). Don't edit; regenerate. ");
    schema.WriteTo(writer);
}
