using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.Win32;

namespace RegistryDesigner
{
    /// <summary>
    /// Reads this computer's registry, in the 32-bit or the 64-bit view, for the source side of the designer. Nothing is
    /// ever written to it. Keys that can't be read (no permission) report why instead of failing.
    /// </summary>
    internal static class LiveRegistry
    {
        /// <summary>The roots the source shows: the ones Windows Installer can write to.</summary>
        public static readonly string[] Roots = { "HKCR", "HKCU", "HKLM", "HKU" };

        /// <summary>The most values a drop reads at once: more is a key too big to mean to add.</summary>
        public const int Limit = 5000;

        static RegistryKey? Base(string root, bool view64)
        {
            var view = view64 ? RegistryView.Registry64 : RegistryView.Registry32;
            switch (root)
            {
                case "HKCR": return RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, view);
                case "HKCU": return RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view);
                case "HKLM": return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                case "HKU": return RegistryKey.OpenBaseKey(RegistryHive.Users, view);
                default: return null;
            }
        }

        static bool IsAccessProblem(Exception ex) => ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException;

        /// <summary>The names of the subkeys of a key; empty, with the reason in <paramref name="error"/>, if it can't be read.</summary>
        public static List<string> SubKeys(string root, string path, bool view64, out string? error)
        {
            error = null;
            try
            {
                using var baseKey = Base(root, view64);
                using var key = path.Length == 0 ? null : baseKey?.OpenSubKey(path);
                var names = (path.Length == 0 ? baseKey?.GetSubKeyNames() : key?.GetSubKeyNames()) ?? new string[0];
                var list = names.ToList();
                list.Sort(StringComparer.OrdinalIgnoreCase);
                return list;
            }
            catch (Exception ex) when (IsAccessProblem(ex))
            {
                error = ex.Message;
                return new List<string>();
            }
        }

        /// <summary>The values of a key, default value first, as regedit lists them.</summary>
        public static List<SourceValue> Values(string root, string path, bool view64, out string? error)
        {
            error = null;
            var values = new List<SourceValue>();
            try
            {
                using var baseKey = Base(root, view64);
                using var key = path.Length == 0 ? null : baseKey?.OpenSubKey(path);
                if (key == null)
                {
                    return values;
                }

                foreach (var name in key.GetValueNames().OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
                {
                    var kind = key.GetValueKind(name);
                    values.Add(SourceValues.Convert(root, path, name, key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames), kind));
                }
            }
            catch (Exception ex) when (IsAccessProblem(ex))
            {
                error = ex.Message;
            }

            return values;
        }

        /// <summary>
        /// Reads a key (its values, or only the named ones) and, if asked, the keys below it, as what a .reg file would hold,
        /// so adding it is the same as pasting one. Stops, with <paramref name="tooMany"/> set, past <see cref="Limit"/> values.
        /// </summary>
        /// <param name="names">The values to read; null for all of the key's.</param>
        public static RegImport Read(string root, string path, bool view64, IReadOnlyCollection<string>? names, bool subtree, out bool tooMany)
        {
            var import = new RegImport();
            tooMany = false;
            var count = 0;
            using var baseKey = Base(root, view64);
            if (baseKey == null)
            {
                import.Skipped.Add("The root " + root + " is not one Windows Installer writes.");
                return import;
            }

            Walk(baseKey, root, path, names, subtree, import, ref count, ref tooMany);
            return import;
        }

        static void Walk(RegistryKey baseKey, string root, string path, IReadOnlyCollection<string>? names, bool subtree, RegImport import, ref int count, ref bool tooMany)
        {
            try
            {
                using var key = baseKey.OpenSubKey(path);
                if (key == null)
                {
                    import.Skipped.Add("Key " + root + "\\" + path + " could not be opened.");
                    return;
                }

                var supported = 0;
                foreach (var name in names ?? key.GetValueNames())
                {
                    var value = SourceValues.Convert(root, path, name, key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames), key.GetValueKind(name));
                    if (value.Entry == null)
                    {
                        import.Skipped.Add(value.NotSupported ?? "A value was left out.");
                        continue;
                    }

                    import.Entries.Add(value.Entry);
                    supported++;
                    if (++count > Limit)
                    {
                        tooMany = true;
                        return;
                    }
                }

                var subKeys = subtree ? key.GetSubKeyNames() : new string[0];

                // A key with nothing to add in it, and nothing below it, is still a key.
                if (names == null && supported == 0 && subKeys.Length == 0)
                {
                    import.Entries.Add(new RegEntry { Root = root, Key = path, KeyOnly = true });
                }

                foreach (var sub in subKeys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
                {
                    Walk(baseKey, root, path + "\\" + sub, null, true, import, ref count, ref tooMany);
                    if (tooMany)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex) when (IsAccessProblem(ex))
            {
                import.Skipped.Add("Key " + root + "\\" + path + " can't be read: " + ex.Message);
            }
        }
    }
}
