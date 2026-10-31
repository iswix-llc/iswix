using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace RegistryDesigner
{
    /// <summary>A value of the source registry, as the source list shows it and as a drop adds it.</summary>
    public sealed class SourceValue
    {
        /// <summary>The value's name; null for the key's default value.</summary>
        public string? Name { get; set; }

        /// <summary>The Windows type name: REG_SZ, REG_DWORD, ...</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>The data as regedit shows it.</summary>
        public string Display { get; set; } = string.Empty;

        /// <summary>What a drop adds; null for a value Windows Installer can't write.</summary>
        public RegEntry? Entry { get; set; }

        /// <summary>Why <see cref="Entry"/> is null.</summary>
        public string? NotSupported { get; set; }
    }

    /// <summary>Turns the registry's own value types into the designer's: pure, so it can be tested without a registry.</summary>
    public static class SourceValues
    {
        /// <summary>
        /// Converts a value read with <c>GetValue(name, null, DoNotExpandEnvironmentNames)</c>.
        /// </summary>
        /// <param name="root">HKCR, HKCU, HKLM or HKU.</param>
        /// <param name="key">The key's path below the root.</param>
        /// <param name="name">The value's name; null or empty for the default value.</param>
        public static SourceValue Convert(string root, string key, string? name, object? data, RegistryValueKind kind)
        {
            var label = string.IsNullOrEmpty(name) ? "(Default)" : name!;
            var where = "Value '" + label + "' of " + root + "\\" + key;
            var result = new SourceValue { Name = string.IsNullOrEmpty(name) ? null : name, Type = TypeName(kind) };
            var entry = new RegEntry { Root = root, Key = key, Name = result.Name };
            switch (kind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                {
                    var text = data as string ?? string.Empty;
                    entry.Kind = kind == RegistryValueKind.String ? RegKind.String : RegKind.Expandable;
                    entry.Value = RegFile.EscapeBrackets(text);
                    result.Display = text;
                    result.Entry = entry;
                    break;
                }

                case RegistryValueKind.DWord:
                {
                    var number = unchecked((uint)System.Convert.ToInt32(data, CultureInfo.InvariantCulture));
                    entry.Kind = RegKind.Integer;
                    entry.Value = number.ToString(CultureInfo.InvariantCulture);
                    result.Display = "0x" + number.ToString("x8", CultureInfo.InvariantCulture) + " (" + number.ToString(CultureInfo.InvariantCulture) + ")";
                    result.Entry = entry;
                    break;
                }

                case RegistryValueKind.Binary:
                {
                    var bytes = data as byte[] ?? new byte[0];
                    entry.Kind = RegKind.Binary;
                    entry.Value = RegFile.ToHex(bytes);
                    result.Display = HexDisplay(bytes);
                    result.Entry = entry;
                    break;
                }

                case RegistryValueKind.MultiString:
                {
                    var strings = data as string[] ?? new string[0];
                    entry.Kind = RegKind.MultiString;
                    entry.Strings.AddRange(strings.Select(RegFile.EscapeBrackets));
                    result.Display = string.Join(" | ", strings);
                    result.Entry = entry;
                    break;
                }

                case RegistryValueKind.QWord:
                {
                    var number = unchecked((ulong)System.Convert.ToInt64(data, CultureInfo.InvariantCulture));
                    result.Display = "0x" + number.ToString("x16", CultureInfo.InvariantCulture) + " (" + number.ToString(CultureInfo.InvariantCulture) + ")";
                    result.NotSupported = where + ": a QWORD (REG_QWORD) is not supported: Windows Installer has no 64-bit integer type.";
                    break;
                }

                default:
                {
                    var bytes = data as byte[] ?? new byte[0];
                    result.Display = HexDisplay(bytes);
                    result.NotSupported = where + ": the value type " + result.Type + " is not supported.";
                    break;
                }
            }

            return result;
        }

        public static string TypeName(RegistryValueKind kind)
        {
            switch (kind)
            {
                case RegistryValueKind.String: return "REG_SZ";
                case RegistryValueKind.ExpandString: return "REG_EXPAND_SZ";
                case RegistryValueKind.Binary: return "REG_BINARY";
                case RegistryValueKind.DWord: return "REG_DWORD";
                case RegistryValueKind.MultiString: return "REG_MULTI_SZ";
                case RegistryValueKind.QWord: return "REG_QWORD";
                case RegistryValueKind.None: return "REG_NONE";
                default: return "REG_UNKNOWN";
            }
        }

        // The first bytes as hex, as regedit shows binary data, and how many there are when they don't all show.
        /// <summary>The first bytes as hex, as regedit shows binary data.</summary>
        public static string HexDisplay(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return "(zero-length binary value)";
            }

            const int shown = 24;
            var builder = new StringBuilder();
            for (var i = 0; i < Math.Min(shown, bytes.Length); i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(bytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return bytes.Length > shown ? builder.Append(" ... (").Append(bytes.Length).Append(" bytes)").ToString() : builder.ToString();
        }
    }
}
