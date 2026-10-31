///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FireworksFramework.Managers
{
    /// <summary>
    /// Works out how an XML file is indented and re-indents the whole file to that style. The style is the most common
    /// one among the file's markup lines (tabs, or a number of spaces per level); a file with nothing to go by gets four
    /// spaces. Only whitespace before markup that starts a line is changed. Text content, CDATA, attribute values,
    /// <c>xml:space="preserve"</c> content and elements that hold text are left exactly as they are, so the document's
    /// content never changes. Text that is not well-formed XML is returned as it came.
    /// </summary>
    public static class XmlIndentation
    {
        /// <summary>The indent used when a file gives nothing to go by: four spaces.</summary>
        public const string DefaultUnit = "    ";

        // Candidates in the order a tie is settled: four spaces, two, a tab, then the rest.
        static readonly string[] Candidates =
        {
            "    ", "  ", "\t", "        ", "   ", " ", "     ", "      ", "       ",
        };

        /// <summary>
        /// Gets the indent for one level that the text uses most: a tab, or a number of spaces. Gives
        /// <see cref="DefaultUnit"/> when the text is not well-formed or has no indented markup.
        /// </summary>
        public static string DetectUnit(string text)
        {
            text = WithoutBom(text, out _);
            return Scan(text) is { } scan ? Detect(scan) : DefaultUnit;
        }

        /// <summary>
        /// Re-indents the text to the style it mostly uses already, so a file's own convention wins and its odd lines
        /// are brought in line.
        /// </summary>
        public static string Format(string text) => Format(text, null);

        /// <summary>Re-indents the text to the given indent for one level (a tab, or spaces).</summary>
        /// <param name="text">The XML text.</param>
        /// <param name="unit">The indent for one level, or null to use the style the text mostly uses.</param>
        public static string Format(string text, string? unit)
        {
            var body = WithoutBom(text, out var bom);
            if (HasLoneCarriageReturn(body) || Scan(body) is not { } scan)
            {
                return text;
            }

            unit ??= Detect(scan);
            var starts = LineStarts(body);
            var lines = body.Split('\n');
            var changed = false;
            foreach (var item in scan.StartLines)
            {
                var index = item.Line - 1;
                if (index < 0 || index >= lines.Length)
                {
                    continue;
                }

                var line = lines[index];
                var length = LeadingWhitespace(line);
                if (length == line.Length || line[length] != '<')
                {
                    continue;
                }

                var oldIndent = line.Substring(0, length);
                var newIndent = Repeat(unit, item.Depth);
                if (oldIndent == newIndent)
                {
                    continue;
                }

                lines[index] = newIndent + line.Substring(length);
                changed = true;

                // A comment, processing instruction or tag that runs over several lines keeps its shape: the lines
                // after the first move with it.
                var valueLines = new HashSet<int>();
                var last = LastLine(body, starts, starts[index] + length, index, valueLines);
                for (var k = index + 1; k <= last && k < lines.Length; k++)
                {
                    if (!valueLines.Contains(k) && lines[k].StartsWith(oldIndent, StringComparison.Ordinal))
                    {
                        lines[k] = newIndent + lines[k].Substring(oldIndent.Length);
                    }
                }
            }

            return changed ? bom + string.Join("\n", lines) : text;
        }

        sealed class Item
        {
            public int Line;
            public int Depth;
            public int Parent = -1;
            public int Owner = -1;
            public bool HasText;
            public bool Skip;
            public bool IsEnd;
        }

        sealed class ScanResult
        {
            // The markup that starts a line and may be re-indented, in document order.
            public List<Item> StartLines = new List<Item>();

            // The text's lines, for reading how the markup is indented now.
            public string[] Lines = Array.Empty<string>();
        }

        static ScanResult? Scan(string text)
        {
            var items = new List<Item>();
            var open = new Stack<int>();
            var preserveDepth = -1;
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
                using var input = new System.IO.StringReader(text);
                using var reader = XmlReader.Create(input, settings);
                var info = (IXmlLineInfo)reader;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                        {
                            var item = new Item
                            {
                                Line = info.LineNumber,
                                Depth = reader.Depth,
                                Parent = open.Count > 0 ? open.Peek() : -1,
                                Skip = preserveDepth >= 0,
                            };
                            items.Add(item);
                            if (!reader.IsEmptyElement)
                            {
                                open.Push(items.Count - 1);
                                if (preserveDepth < 0 && reader.XmlSpace == XmlSpace.Preserve)
                                {
                                    preserveDepth = reader.Depth;
                                }
                            }

                            break;
                        }

                        case XmlNodeType.EndElement:
                        {
                            var owner = open.Pop();
                            items.Add(new Item
                            {
                                Line = info.LineNumber,
                                Depth = reader.Depth,
                                Owner = owner,
                                IsEnd = true,
                                Skip = preserveDepth >= 0,
                            });
                            if (preserveDepth == reader.Depth)
                            {
                                preserveDepth = -1;
                            }

                            break;
                        }

                        case XmlNodeType.Comment:
                        case XmlNodeType.ProcessingInstruction:
                            items.Add(new Item
                            {
                                Line = info.LineNumber,
                                Depth = reader.Depth,
                                Parent = open.Count > 0 ? open.Peek() : -1,
                                Skip = preserveDepth >= 0,
                            });
                            break;

                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.EntityReference:
                            if (open.Count > 0)
                            {
                                items[open.Peek()].HasText = true;
                            }

                            break;
                    }
                }
            }
            catch (XmlException)
            {
                return null;
            }

            // The lines whose indentation is not content: the first markup on its line, in an element that holds no text.
            var lines = text.Split('\n');
            var result = new ScanResult { Lines = lines };
            var seen = new HashSet<int>();
            foreach (var item in items)
            {
                if (!seen.Add(item.Line))
                {
                    continue;
                }

                var holder = item.IsEnd ? item.Owner : item.Parent;
                if (item.Skip || (holder >= 0 && items[holder].HasText))
                {
                    continue;
                }

                var index = item.Line - 1;
                if (index < 0 || index >= lines.Length)
                {
                    continue;
                }

                var line = lines[index];
                var length = LeadingWhitespace(line);
                if (length < line.Length && line[length] == '<')
                {
                    result.StartLines.Add(item);
                }
            }

            return result;
        }

        static string Detect(ScanResult scan)
        {
            // The candidate whose indentation matches the most lines below the top level wins; a tie goes to the one
            // listed first. Without a match (a flat or empty file) the default stands.
            var best = DefaultUnit;
            var bestScore = 0;
            foreach (var candidate in Candidates)
            {
                var score = 0;
                foreach (var item in scan.StartLines)
                {
                    if (item.Depth < 1)
                    {
                        continue;
                    }

                    var line = scan.Lines[item.Line - 1];
                    var length = LeadingWhitespace(line);
                    if (string.CompareOrdinal(line, 0, Repeat(candidate, item.Depth), 0, length) == 0
                        && length == candidate.Length * item.Depth)
                    {
                        score++;
                    }
                }

                if (score > bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            return best;
        }

        static string WithoutBom(string text, out string bom)
        {
            if (text.Length > 0 && text[0] == '﻿')
            {
                bom = "﻿";
                return text.Substring(1);
            }

            bom = string.Empty;
            return text;
        }

        static bool HasLoneCarriageReturn(string text)
        {
            for (var i = text.IndexOf('\r'); i >= 0; i = text.IndexOf('\r', i + 1))
            {
                if (i + 1 >= text.Length || text[i + 1] != '\n')
                {
                    return true;
                }
            }

            return false;
        }

        static int LeadingWhitespace(string line)
        {
            var i = 0;
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
            {
                i++;
            }

            return i;
        }

        static string Repeat(string unit, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(unit.Length * count);
            for (var i = 0; i < count; i++)
            {
                builder.Append(unit);
            }

            return builder.ToString();
        }

        static int[] LineStarts(string text)
        {
            var starts = new List<int> { 0 };
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    starts.Add(i + 1);
                }
            }

            return starts.ToArray();
        }

        static int LineOf(int[] starts, int position)
        {
            var index = Array.BinarySearch(starts, position);
            return index >= 0 ? index : ~index - 1;
        }

        // The last line of the comment, processing instruction or tag that begins at the given position. Lines that start
        // inside a quoted attribute value are added to valueLines: their text is content, not layout.
        static int LastLine(string text, int[] starts, int position, int line, HashSet<int> valueLines)
        {
            bool At(string s) => string.CompareOrdinal(text, position, s, 0, s.Length) == 0;

            if (At("<!--"))
            {
                var end = text.IndexOf("-->", position + 4, StringComparison.Ordinal);
                return LineOf(starts, end < 0 ? text.Length : end);
            }

            if (At("<?"))
            {
                var end = text.IndexOf("?>", position + 2, StringComparison.Ordinal);
                return LineOf(starts, end < 0 ? text.Length : end);
            }

            if (At("<![CDATA[") || At("</") || At("<!"))
            {
                return line;
            }

            var quote = '\0';
            for (var i = position + 1; i < text.Length; i++)
            {
                var c = text[i];
                if (quote != '\0')
                {
                    if (c == quote)
                    {
                        quote = '\0';
                    }
                    else if (c == '\n')
                    {
                        valueLines.Add(LineOf(starts, i) + 1);
                    }
                }
                else if (c == '"' || c == '\'')
                {
                    quote = c;
                }
                else if (c == '>')
                {
                    return LineOf(starts, i);
                }
            }

            return line;
        }
    }
}
