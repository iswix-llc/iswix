using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace IsWiX.Designer
{
    /// <summary>
    /// Finds elements in XML text by position. An element is named by its path: the index of each element among its
    /// parent's child elements, from the root down (the root's path is empty). The same path names the element in an
    /// XDocument parsed from the same text.
    /// </summary>
    internal static class XmlPositions
    {
        sealed class Span
        {
            public int[] Path = Array.Empty<int>();
            public int Start;
            public int End;
            // Where the start tag ends and the end tag starts; the same as End for an element with no end tag.
            public int OpenEnd;
            public int CloseStart;
            public bool HasChildren;
            public List<AttributeSpan> Attributes = new List<AttributeSpan>();
        }

        // An attribute from the start of its name to the end of its closing quote.
        sealed class AttributeSpan
        {
            public string Name = string.Empty;
            public int Start;
            public int End;
        }

        /// <summary>Where an offset is: an element, and the attribute of it if the offset is in one.</summary>
        public sealed class Location
        {
            public int[] Path = Array.Empty<int>();
            public string? Attribute;
        }

        /// <summary>
        /// Gets the innermost element that <paramref name="offset"/> is in, or null if there is none. An offset between
        /// the tags of an element that has child elements is in none of them: it is just space around the children.
        /// </summary>
        public static Location? LocationAt(string text, int offset)
        {
            Span? found = null;
            foreach (var span in Scan(text))
            {
                // Spans come in document order, so the last one containing the offset is the innermost.
                if (span.Start <= offset && offset <= span.End)
                {
                    found = span;
                }
            }

            if (found == null || (found.HasChildren && offset > found.OpenEnd && offset < found.CloseStart))
            {
                return null;
            }

            var location = new Location { Path = found.Path };
            foreach (var attribute in found.Attributes)
            {
                if (attribute.Start <= offset && offset <= attribute.End)
                {
                    location.Attribute = attribute.Name;
                }
            }

            return location;
        }

        /// <summary>
        /// Gets the text of <paramref name="attribute"/> (its name, value and quotes) in the element at
        /// <paramref name="path"/>, or the element's start tag if it has no such attribute or none is asked for, as
        /// the start and end offsets. It is null if there is no such element.
        /// </summary>
        public static int[]? RangeOf(string text, int[] path, string? attribute = null)
        {
            foreach (var span in Scan(text))
            {
                if (span.Path.Length == path.Length && PathEquals(span.Path, path))
                {
                    foreach (var candidate in span.Attributes)
                    {
                        if (candidate.Name == attribute)
                        {
                            return new[] { candidate.Start, candidate.End };
                        }
                    }

                    return new[] { span.Start, span.OpenEnd };
                }
            }

            return null;
        }

        static bool PathEquals(int[] a, int[] b)
        {
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        // Every element's start tag to the end of its end tag. Throws XmlException if the text isn't well-formed.
        static List<Span> Scan(string text)
        {
            var lineStarts = LineStarts(text);
            var spans = new List<Span>();
            var open = new Stack<Span>();
            // Elements seen so far under each open element, and under the document.
            var counts = new Stack<int>();
            counts.Push(0);
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
            using (var reader = XmlReader.Create(new StringReader(text), settings))
            {
                var info = (IXmlLineInfo)reader;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // The reader is at the name, one past the '<'.
                        var start = lineStarts[info.LineNumber - 1] + info.LinePosition - 2;
                        var index = counts.Pop();
                        counts.Push(index + 1);
                        var parent = open.Count > 0 ? open.Peek() : null;
                        if (parent != null)
                        {
                            parent.HasChildren = true;
                        }

                        var span = new Span { Start = start, Path = Append(parent?.Path, index, open.Count == 0 && spans.Count == 0) };
                        span.OpenEnd = TagEnd(text, start);
                        spans.Add(span);
                        var isEmpty = reader.IsEmptyElement;
                        if (reader.MoveToFirstAttribute())
                        {
                            do
                            {
                                // The reader is at the attribute's name.
                                var attributeStart = lineStarts[info.LineNumber - 1] + info.LinePosition - 1;
                                span.Attributes.Add(new AttributeSpan { Name = reader.Name, Start = attributeStart, End = ValueEnd(text, attributeStart) });
                            }
                            while (reader.MoveToNextAttribute());
                            reader.MoveToElement();
                        }

                        if (isEmpty)
                        {
                            span.End = span.CloseStart = span.OpenEnd;
                        }
                        else
                        {
                            open.Push(span);
                            counts.Push(0);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        // The reader is at the name, two past the "</".
                        var at = lineStarts[info.LineNumber - 1] + info.LinePosition - 3;
                        var closed = open.Pop();
                        closed.CloseStart = at;
                        closed.End = TagEnd(text, at);
                        counts.Pop();
                    }
                }
            }

            return spans;
        }

        static int[] Append(int[]? parent, int index, bool isRoot)
        {
            if (isRoot)
            {
                return Array.Empty<int>();
            }

            var path = new int[(parent?.Length ?? 0) + 1];
            parent?.CopyTo(path, 0);
            path[path.Length - 1] = index;
            return path;
        }

        // The offset just past the closing quote of the attribute whose name starts at from.
        static int ValueEnd(string text, int from)
        {
            var open = text.IndexOfAny(new[] { '"', '\'' }, from);
            if (open < 0)
            {
                return text.Length;
            }

            var close = text.IndexOf(text[open], open + 1);
            return close < 0 ? text.Length : close + 1;
        }

        // The offset just past the '>' that closes the tag starting at from: not one inside a quoted attribute value.
        static int TagEnd(string text, int from)
        {
            var quote = '\0';
            for (var i = from; i < text.Length; i++)
            {
                var c = text[i];
                if (quote != '\0')
                {
                    if (c == quote)
                    {
                        quote = '\0';
                    }
                }
                else if (c == '"' || c == '\'')
                {
                    quote = c;
                }
                else if (c == '>')
                {
                    return i + 1;
                }
            }

            return text.Length;
        }

        // The offset each line starts at; a line ends with \r\n, \n or \r, as the XML reader counts them.
        static List<int> LineStarts(string text)
        {
            var starts = new List<int> { 0 };
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                    starts.Add(i + 1);
                }
                else if (text[i] == '\n' || text[i] == '\r')
                {
                    starts.Add(i + 1);
                }
            }

            return starts;
        }
    }
}
