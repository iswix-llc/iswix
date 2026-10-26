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
        }

        /// <summary>Gets the path of the innermost element whose tags contain <paramref name="offset"/>, or null if none does.</summary>
        public static int[]? PathAt(string text, int offset)
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

            return found?.Path;
        }

        /// <summary>Gets the offset of the '&lt;' that starts the element at <paramref name="path"/>, or null if there is none.</summary>
        public static int? OffsetOf(string text, int[] path)
        {
            foreach (var span in Scan(text))
            {
                if (span.Path.Length == path.Length && PathEquals(span.Path, path))
                {
                    return span.Start;
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
                        var parent = open.Count > 0 ? open.Peek().Path : null;
                        var span = new Span { Start = start, Path = Append(parent, index, open.Count == 0 && spans.Count == 0) };
                        spans.Add(span);
                        if (reader.IsEmptyElement)
                        {
                            span.End = TagEnd(text, start);
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
                        open.Pop().End = TagEnd(text, at);
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
