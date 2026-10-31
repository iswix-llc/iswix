using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RegistryDesigner
{
    /// <summary>The kinds of registry value the designer works with: the types Windows Installer can write.</summary>
    public enum RegKind { String, Integer, Binary, Expandable, MultiString }

    /// <summary>One key or value read from a .reg file.</summary>
    public sealed class RegEntry
    {
        /// <summary>HKCR, HKCU, HKLM or HKU.</summary>
        public string Root { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        /// <summary>The value's name; null for the key's default value.</summary>
        public string? Name { get; set; }
        public RegKind Kind { get; set; }
        /// <summary>The text of a value: the string, the decimal number, or the hex digits. Strings have their brackets escaped.</summary>
        public string Value { get; set; } = string.Empty;
        public List<string> Strings { get; } = new List<string>();
        /// <summary>The line was <c>[-key]</c>: the key is to be removed.</summary>
        public bool RemoveKey { get; set; }
        /// <summary>The line was <c>"name"=-</c>: the value is to be removed.</summary>
        public bool RemoveValue { get; set; }
        /// <summary>The key appears with no values of its own.</summary>
        public bool KeyOnly { get; set; }
    }

    /// <summary>What a .reg file held, and what it held that the designer can't write.</summary>
    public sealed class RegImport
    {
        public List<RegEntry> Entries { get; } = new List<RegEntry>();
        /// <summary>The lines left out, each with the reason.</summary>
        public List<string> Skipped { get; } = new List<string>();
    }

    /// <summary>A .reg file's text and what could not be written to it exactly.</summary>
    public sealed class RegExport
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Warnings { get; } = new List<string>();
    }

    /// <summary>Reads and writes regedit's .reg text format.</summary>
    public static class RegFile
    {
        const string Header = "Windows Registry Editor Version 5.00";

        static readonly string[,] Roots =
        {
            { "HKEY_CLASSES_ROOT", "HKCR" },
            { "HKEY_CURRENT_USER", "HKCU" },
            { "HKEY_LOCAL_MACHINE", "HKLM" },
            { "HKEY_USERS", "HKU" },
            // Not a root regedit has; written and read as it is, so what is copied can be pasted.
            { "HKMU", "HKMU" },
        };

        /// <summary>
        /// Escapes the brackets of a string so it is written literally: in a formatted value a [ starts a property
        /// reference, so a literal one is [\[] and a literal ] is [\]].
        /// </summary>
        public static string EscapeBrackets(string text)
        {
            if (text.IndexOf('[') < 0 && text.IndexOf(']') < 0)
            {
                return text;
            }

            var builder = new StringBuilder(text.Length + 8);
            foreach (var c in text)
            {
                if (c == '[')
                {
                    builder.Append("[\\[]");
                }
                else if (c == ']')
                {
                    builder.Append("[\\]]");
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        /// <summary>Undoes <see cref="EscapeBrackets"/>.</summary>
        public static string UnescapeBrackets(string text) => text.Replace("[\\[]", "[").Replace("[\\]]", "]");

        /// <summary>Gets whether the text has a property reference: a bracket that is not an escaped literal.</summary>
        public static bool HasToken(string text) => UnescapeBrackets(text.Replace("[\\[]", "\u0001").Replace("[\\]]", "\u0002")).IndexOf('[') >= 0;

        // ---------------------------------------------------------------- copy and paste

        static readonly System.Text.RegularExpressions.Regex CopyMarker = new System.Text.RegularExpressions.Regex(
            @"^; IsWiX copy: (keys|values) (HK[A-Z]+)(?:\\([^\r\n]*))?\s*$", System.Text.RegularExpressions.RegexOptions.Multiline);

        /// <summary>
        /// The text to put on the clipboard for a copied key (<paramref name="kind"/> "keys": the key and what is below it) or
        /// copied values of a key ("values"): the .reg file, with a comment line saying what was copied so a paste can put it
        /// somewhere else. Regedit pastes the same text as a .reg file.
        /// </summary>
        public static string WithCopyMarker(string regText, string kind, string root, string key)
        {
            var marker = "; IsWiX copy: " + kind + " " + root + (key.Length == 0 ? string.Empty : "\\" + key);
            var firstBreak = regText.IndexOf("\r\n", StringComparison.Ordinal);
            return firstBreak < 0 ? regText + "\r\n" + marker : regText.Substring(0, firstBreak) + "\r\n" + marker + regText.Substring(firstBreak);
        }

        /// <summary>Reads what <see cref="WithCopyMarker"/> wrote; false for text that is not from a copy here.</summary>
        public static bool TryReadCopyMarker(string text, out string kind, out string root, out string key)
        {
            var match = CopyMarker.Match(text);
            kind = match.Success ? match.Groups[1].Value : string.Empty;
            root = match.Success ? match.Groups[2].Value : string.Empty;
            key = match.Success && match.Groups[3].Success ? match.Groups[3].Value : string.Empty;
            return match.Success;
        }

        /// <summary>
        /// Moves what was copied to a key: for copied "keys", the copied key (by its own name) goes under the target with
        /// everything that was below it; for copied "values", the values go in the target key itself.
        /// </summary>
        public static void Retarget(RegImport import, string kind, string baseRoot, string baseKey, string targetRoot, string targetKey)
        {
            var name = baseKey.Substring(baseKey.LastIndexOf('\\') + 1);
            foreach (var entry in import.Entries)
            {
                if (!string.Equals(entry.Root, baseRoot, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (kind == "values")
                {
                    if (string.Equals(entry.Key, baseKey, StringComparison.OrdinalIgnoreCase))
                    {
                        entry.Root = targetRoot;
                        entry.Key = targetKey;
                    }

                    continue;
                }

                string relative;
                if (string.Equals(entry.Key, baseKey, StringComparison.OrdinalIgnoreCase))
                {
                    relative = string.Empty;
                }
                else if (entry.Key.StartsWith(baseKey + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    relative = entry.Key.Substring(baseKey.Length + 1);
                }
                else
                {
                    continue;
                }

                entry.Root = targetRoot;
                entry.Key = string.Join("\\", new[] { targetKey, name, relative }.Where(part => part.Length > 0));
            }
        }

        // ---------------------------------------------------------------- reading

        public static RegImport Parse(string text)
        {
            var import = new RegImport();
            var logical = Logical(text);
            string? root = null;
            string? key = null;
            var keyIsRemoved = false;
            var keyEntry = default(RegEntry);
            var keyHasValues = false;

            void CloseKey()
            {
                if (keyEntry != null && !keyHasValues)
                {
                    keyEntry.KeyOnly = true;
                    import.Entries.Add(keyEntry);
                }

                keyEntry = null;
            }

            foreach (var line in logical)
            {
                if (line.Length == 0 || line[0] == ';' || line.StartsWith("Windows Registry Editor", StringComparison.OrdinalIgnoreCase) || line.StartsWith("REGEDIT", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (line[0] == '[' && line[line.Length - 1] == ']')
                {
                    CloseKey();
                    var path = line.Substring(1, line.Length - 2);
                    keyIsRemoved = path.StartsWith("-", StringComparison.Ordinal);
                    if (keyIsRemoved)
                    {
                        path = path.Substring(1);
                    }

                    if (!SplitKey(path, out root, out key))
                    {
                        import.Skipped.Add("Key '" + path + "': its root is not one Windows Installer writes (HKEY_CLASSES_ROOT, HKEY_CURRENT_USER, HKEY_LOCAL_MACHINE or HKEY_USERS).");
                        root = null;
                        key = null;
                        continue;
                    }

                    if (keyIsRemoved)
                    {
                        import.Entries.Add(new RegEntry { Root = root!, Key = key!, RemoveKey = true });
                        root = null;
                        key = null;
                    }
                    else
                    {
                        keyEntry = new RegEntry { Root = root!, Key = key! };
                        keyHasValues = false;
                    }

                    continue;
                }

                if (root == null || key == null)
                {
                    if (!keyIsRemoved)
                    {
                        import.Skipped.Add("Value outside a supported key: " + Shorten(line));
                    }

                    continue;
                }

                ParseValue(line, root, key, import, ref keyHasValues);
            }

            CloseKey();
            return import;
        }

        // The file's lines, with a line that ends in a backslash joined to the next, and each trimmed.
        static List<string> Logical(string text)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            foreach (var raw in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
            {
                var line = current.Length == 0 ? raw.Trim() : raw.TrimStart();
                if (line.EndsWith("\\", StringComparison.Ordinal) && IsHexLine(current.Length == 0 ? line : current.ToString()))
                {
                    current.Append(line.Substring(0, line.Length - 1));
                    continue;
                }

                current.Append(line);
                result.Add(current.ToString().Trim());
                current.Length = 0;
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString().Trim());
            }

            return result;
        }

        static bool IsHexLine(string line) => line.IndexOf("=hex", StringComparison.OrdinalIgnoreCase) >= 0 || line.IndexOf("hex(", StringComparison.OrdinalIgnoreCase) >= 0 ||
            line.StartsWith("\"", StringComparison.Ordinal) == false && line.IndexOf(',') >= 0;

        static bool SplitKey(string path, out string? root, out string? key)
        {
            root = null;
            key = null;
            var slash = path.IndexOf('\\');
            var first = slash < 0 ? path : path.Substring(0, slash);
            for (var i = 0; i < Roots.GetLength(0); i++)
            {
                if (string.Equals(first, Roots[i, 0], StringComparison.OrdinalIgnoreCase))
                {
                    root = Roots[i, 1];
                    key = slash < 0 ? string.Empty : path.Substring(slash + 1);
                    return true;
                }
            }

            return false;
        }

        static void ParseValue(string line, string root, string key, RegImport import, ref bool keyHasValues)
        {
            string? name;
            int equals;
            if (line[0] == '@')
            {
                name = null;
                equals = line.IndexOf('=');
            }
            else if (line[0] == '"')
            {
                var close = EndOfQuoted(line, 0);
                if (close < 0)
                {
                    import.Skipped.Add("Not understood: " + Shorten(line));
                    return;
                }

                name = UnescapeQuoted(line.Substring(1, close - 1));
                equals = line.IndexOf('=', close);
            }
            else
            {
                import.Skipped.Add("Not understood: " + Shorten(line));
                return;
            }

            if (equals < 0)
            {
                import.Skipped.Add("Not understood: " + Shorten(line));
                return;
            }

            var data = line.Substring(equals + 1).Trim();
            var label = "Value '" + (name ?? "@") + "' of " + root + "\\" + key;
            keyHasValues = true;

            if (data == "-")
            {
                import.Entries.Add(new RegEntry { Root = root, Key = key, Name = name, RemoveValue = true });
                return;
            }

            if (data.StartsWith("\"", StringComparison.Ordinal))
            {
                var close = EndOfQuoted(data, 0);
                if (close < 0)
                {
                    import.Skipped.Add(label + ": the string is not closed.");
                    return;
                }

                import.Entries.Add(new RegEntry { Root = root, Key = key, Name = name, Kind = RegKind.String, Value = EscapeBrackets(UnescapeQuoted(data.Substring(1, close - 1))) });
                return;
            }

            if (data.StartsWith("dword:", StringComparison.OrdinalIgnoreCase))
            {
                if (uint.TryParse(data.Substring(6).Trim(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var number))
                {
                    import.Entries.Add(new RegEntry { Root = root, Key = key, Name = name, Kind = RegKind.Integer, Value = number.ToString(CultureInfo.InvariantCulture) });
                }
                else
                {
                    import.Skipped.Add(label + ": the number is not a DWORD.");
                }

                return;
            }

            if (data.StartsWith("hex", StringComparison.OrdinalIgnoreCase))
            {
                var colon = data.IndexOf(':');
                if (colon < 0)
                {
                    import.Skipped.Add("Not understood: " + Shorten(line));
                    return;
                }

                var type = data.Substring(3, colon - 3).Trim();
                var bytes = ParseBytes(data.Substring(colon + 1));
                if (bytes == null)
                {
                    import.Skipped.Add(label + ": the hex data is not valid.");
                    return;
                }

                switch (type)
                {
                    case "":
                        import.Entries.Add(new RegEntry { Root = root, Key = key, Name = name, Kind = RegKind.Binary, Value = ToHex(bytes) });
                        break;
                    case "(2)":
                        import.Entries.Add(new RegEntry { Root = root, Key = key, Name = name, Kind = RegKind.Expandable, Value = EscapeBrackets(DecodeString(bytes)) });
                        break;
                    case "(7)":
                    {
                        var entry = new RegEntry { Root = root, Key = key, Name = name, Kind = RegKind.MultiString };
                        foreach (var part in DecodeString(bytes, keepNulls: true).Split('\0'))
                        {
                            entry.Strings.Add(EscapeBrackets(part));
                        }

                        while (entry.Strings.Count > 0 && entry.Strings[entry.Strings.Count - 1].Length == 0)
                        {
                            entry.Strings.RemoveAt(entry.Strings.Count - 1);
                        }

                        import.Entries.Add(entry);
                        break;
                    }

                    case "(b)":
                        import.Skipped.Add(label + ": a QWORD (REG_QWORD) is not supported: Windows Installer has no 64-bit integer type.");
                        break;
                    default:
                        import.Skipped.Add(label + ": the value type hex" + type + " is not supported.");
                        break;
                }

                return;
            }

            import.Skipped.Add("Not understood: " + Shorten(line));
        }

        static string Shorten(string line) => line.Length <= 80 ? line : line.Substring(0, 77) + "...";

        // The index of the quote that closes the string opening at start.
        static int EndOfQuoted(string text, int start)
        {
            for (var i = start + 1; i < text.Length; i++)
            {
                if (text[i] == '\\')
                {
                    i++;
                }
                else if (text[i] == '"')
                {
                    return i;
                }
            }

            return -1;
        }

        static string UnescapeQuoted(string text)
        {
            if (text.IndexOf('\\') < 0)
            {
                return text;
            }

            var builder = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    i++;
                }

                builder.Append(text[i]);
            }

            return builder.ToString();
        }

        static byte[]? ParseBytes(string text)
        {
            var bytes = new List<byte>();
            foreach (var part in text.Split(','))
            {
                var hex = part.Trim();
                if (hex.Length == 0)
                {
                    continue;
                }

                if (!byte.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var b))
                {
                    return null;
                }

                bytes.Add(b);
            }

            return bytes.ToArray();
        }

        static string DecodeString(byte[] bytes, bool keepNulls = false)
        {
            var text = Encoding.Unicode.GetString(bytes);
            return keepNulls ? text : text.TrimEnd('\0');
        }

        /// <summary>The hex digits of the bytes, without separators.</summary>
        public static string ToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        /// <summary>The bytes of hex digits; null when they are not an even number of hex digits.</summary>
        public static byte[]? FromHex(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                return null;
            }

            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                if (!byte.TryParse(hex.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out bytes[i]))
                {
                    return null;
                }
            }

            return bytes;
        }

        // ---------------------------------------------------------------- writing

        /// <summary>Writes the values and keys as a .reg file. Property references are written as they are, with a warning.</summary>
        /// <param name="extraKeys">Keys (root and key) to write with no values, such as a key kept on its own.</param>
        public static RegExport Write(IEnumerable<RegistryItem> items, IEnumerable<KeyValuePair<string, string>>? extraKeys = null)
        {
            var export = new RegExport();
            var builder = new StringBuilder();
            builder.Append(Header).Append("\r\n");
            var groups = new List<KeyValuePair<string, List<RegistryItem>>>();
            var index = new Dictionary<string, List<RegistryItem>>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                if (item.Root == null)
                {
                    export.Warnings.Add("An entry without a root was left out.");
                    continue;
                }

                var path = FullRoot(item.Root) + (item.Key.Length == 0 ? string.Empty : "\\" + item.Key);
                if (item.Kind == RegistryItemKind.RemoveKey)
                {
                    path = "-" + path;
                }

                if (!index.TryGetValue(path, out var list))
                {
                    list = new List<RegistryItem>();
                    index[path] = list;
                    groups.Add(new KeyValuePair<string, List<RegistryItem>>(path, list));
                }

                if (item.Kind != RegistryItemKind.RemoveKey)
                {
                    list.Add(item);
                }
            }

            if (extraKeys != null)
            {
                foreach (var pair in extraKeys)
                {
                    var path = FullRoot(pair.Key) + (pair.Value.Length == 0 ? string.Empty : "\\" + pair.Value);
                    if (!index.ContainsKey(path))
                    {
                        var list = new List<RegistryItem>();
                        index[path] = list;
                        groups.Add(new KeyValuePair<string, List<RegistryItem>>(path, list));
                    }
                }
            }

            if (groups.Any(g => g.Key.StartsWith("HKMU", StringComparison.Ordinal) || g.Key.StartsWith("-HKMU", StringComparison.Ordinal)))
            {
                export.Warnings.Add("HKMU is not a root regedit has: it is written as HKMU, which only IsWiX reads back.");
            }

            foreach (var group in groups)
            {
                builder.Append("\r\n[").Append(group.Key).Append("]\r\n");
                foreach (var item in group.Value)
                {
                    builder.Append(Line(item, export)).Append("\r\n");
                }
            }

            export.Text = builder.ToString();
            return export;
        }

        static string FullRoot(string root)
        {
            for (var i = 0; i < Roots.GetLength(0); i++)
            {
                if (string.Equals(root, Roots[i, 1], StringComparison.OrdinalIgnoreCase))
                {
                    return Roots[i, 0];
                }
            }

            return root;
        }

        static string Line(RegistryItem item, RegExport export)
        {
            var name = item.Name == null || item.Name.Length == 0 ? "@" : "\"" + Escape(item.Name) + "\"";
            if (item.Kind == RegistryItemKind.RemoveValue)
            {
                return name + "=-";
            }

            var label = (item.Name == null || item.Name.Length == 0 ? "@" : item.Name) + " of " + item.Root + "\\" + item.Key;
            var texts = item.Type == RegKind.MultiString ? item.MultiStrings : new List<string> { item.Value ?? string.Empty };
            foreach (var text in texts)
            {
                if (HasToken(text))
                {
                    export.Warnings.Add("Value " + label + " has a property reference, which is written as it is.");
                    break;
                }
            }

            switch (item.Type)
            {
                case RegKind.Integer:
                    return uint.TryParse(item.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                        ? name + "=dword:" + number.ToString("x8", CultureInfo.InvariantCulture)
                        : name + "=\"" + Escape(item.Value ?? string.Empty) + "\"";
                case RegKind.Binary:
                {
                    var bytes = FromHex(item.Value ?? string.Empty);
                    if (bytes == null)
                    {
                        export.Warnings.Add("Value " + label + " is not valid hex and was written as a string.");
                        return name + "=\"" + Escape(item.Value ?? string.Empty) + "\"";
                    }

                    return Wrap(name + "=hex:", bytes);
                }

                case RegKind.Expandable:
                    return Wrap(name + "=hex(2):", Encoding.Unicode.GetBytes(UnescapeBrackets(item.Value ?? string.Empty) + "\0"));
                case RegKind.MultiString:
                {
                    var builder = new StringBuilder();
                    foreach (var text in item.MultiStrings)
                    {
                        builder.Append(UnescapeBrackets(text)).Append('\0');
                    }

                    builder.Append('\0');
                    return Wrap(name + "=hex(7):", Encoding.Unicode.GetBytes(builder.ToString()));
                }

                default:
                    return name + "=\"" + Escape(UnescapeBrackets(item.Value ?? string.Empty)) + "\"";
            }
        }

        static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\"", "\\\"");

        // The bytes as comma-separated hex, broken onto lines with a trailing backslash as regedit does.
        static string Wrap(string prefix, byte[] bytes)
        {
            var builder = new StringBuilder(prefix);
            var column = prefix.Length;
            for (var i = 0; i < bytes.Length; i++)
            {
                var hex = bytes[i].ToString("x2", CultureInfo.InvariantCulture) + (i < bytes.Length - 1 ? "," : string.Empty);
                if (column + hex.Length > 78)
                {
                    builder.Append("\\\r\n  ");
                    column = 2;
                }

                builder.Append(hex);
                column += hex.Length;
            }

            return builder.ToString();
        }
    }
}
