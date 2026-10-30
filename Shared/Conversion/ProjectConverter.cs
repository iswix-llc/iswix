// Copyright (c) ISWIX LLC. Licensed under the MS-PL.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IsWiX.Conversion
{
    /// <summary>The two kinds of WiX SDK project: IsWiX's (.iswixproj) and the WiX Toolset's (.wixproj).</summary>
    public enum ProjectKind
    {
        /// <summary>An .iswixproj project, opened by IsWiX's project system.</summary>
        IsWiX,

        /// <summary>A .wixproj project, opened by HeatWave.</summary>
        Wix,
    }

    /// <summary>How to convert.</summary>
    public sealed class ConversionOptions
    {
        /// <summary>Gets or sets the kind of project to convert to.</summary>
        public ProjectKind To { get; set; } = ProjectKind.IsWiX;

        /// <summary>
        /// Gets or sets the SDK version to move the converted projects to, or null to leave their SDK alone. Converting
        /// to IsWiX moves them to IsWiX.Sdk and the IsWiX.*.wixext packages; converting to WiX, to WixToolset.Sdk and
        /// the WixToolset.*.wixext packages.
        /// </summary>
        public string? SdkVersion { get; set; }

        /// <summary>
        /// Gets or sets whether the WiX Toolset's Open Source Maintenance Fee EULA is accepted, which WixToolset.Sdk
        /// v7 and later require: the projects then get &lt;AcceptEula&gt;wix7&lt;/AcceptEula&gt;.
        /// </summary>
        public bool AcceptEula { get; set; }

        /// <summary>Gets or sets whether to report what would change without changing anything.</summary>
        public bool DryRun { get; set; }
    }

    /// <summary>What a conversion changed, or would change in a dry run.</summary>
    public sealed class ConversionResult
    {
        /// <summary>Gets the projects renamed, from their old path to their new one.</summary>
        public List<KeyValuePair<string, string>> Renamed { get; } = new List<KeyValuePair<string, string>>();

        /// <summary>Gets the files whose content changed, by the path they have after the conversion.</summary>
        public List<string> Updated { get; } = new List<string>();
    }

    /// <summary>Why a conversion was refused. Nothing was changed.</summary>
    public sealed class ConversionException : Exception
    {
        /// <summary>Creates the exception.</summary>
        public ConversionException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Converts WiX SDK projects between .wixproj and .iswixproj. It renames the projects; sets their
    /// DefaultProjectTypeGuid (which "dotnet sln add" writes into solutions) for the new kind; points ProjectReferences
    /// to the renamed projects, in every project file under the folder; updates the solutions there (.sln and .slnx:
    /// paths and project types); and, if asked, moves the projects to the other kind's SDK and extension packages.
    /// Files keep their encoding and line endings.
    /// </summary>
    public static class ProjectConverter
    {
        /// <summary>IsWiX's project type, for .iswixproj.</summary>
        public const string IsWiXProjectType = "{3A7B4E67-3CBB-402A-AC31-8EF9DCCEF68E}";

        /// <summary>The WiX Toolset's SDK-style project type, for .wixproj (HeatWave's).</summary>
        public const string WixProjectType = "{B7DD6F7E-DEF8-4E67-B5B7-07EF123DB6F0}";

        /// <summary>WiX v3's project type (Votive's), still found in old solutions.</summary>
        public const string WixV3ProjectType = "{930C7802-8A8C-48F9-8165-68863BCCD9DD}";

        static readonly string[] SkippedFolders = { "bin", "obj", "node_modules", "packages" };

        static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(10);

        /// <summary>Gets the extension, with its dot, of the kind's project files.</summary>
        public static string Extension(ProjectKind kind) => kind == ProjectKind.IsWiX ? ".iswixproj" : ".wixproj";

        /// <summary>Gets the other kind.</summary>
        public static ProjectKind Other(ProjectKind kind) => kind == ProjectKind.IsWiX ? ProjectKind.Wix : ProjectKind.IsWiX;

        /// <summary>
        /// Converts projects under a folder.
        /// </summary>
        /// <param name="root">The folder whose project files and solutions are updated to match.</param>
        /// <param name="projects">The projects to convert, or null for every project of the other kind under the
        /// folder.</param>
        /// <param name="options">How to convert.</param>
        /// <exception cref="ConversionException">The conversion was refused; nothing was changed.</exception>
        public static ConversionResult Convert(string root, IEnumerable<string>? projects, ConversionOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!Directory.Exists(root)) throw new ConversionException($"Folder not found: {root}");
            root = Path.GetFullPath(root);

            var fromExtension = Extension(Other(options.To));
            var toExtension = Extension(options.To);
            var files = Files(root).ToList();

            var sources = (projects ?? files.Where(f => HasExtension(f, fromExtension)))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var source in sources)
            {
                if (!HasExtension(source, fromExtension))
                    throw new ConversionException($"{Path.GetFileName(source)} is not a {fromExtension} project.");
                if (!File.Exists(source))
                    throw new ConversionException($"Project not found: {source}");
            }

            var renames = sources.ToDictionary(p => p, p => Path.ChangeExtension(p, toExtension), StringComparer.OrdinalIgnoreCase);
            foreach (var target in renames.Values.Where(File.Exists))
                throw new ConversionException($"{Relative(root, target)} already exists.");

            // Each project's .user file goes with it, unless one is already there for the new name.
            var userFiles = renames
                .Where(r => File.Exists(r.Key + ".user") && !File.Exists(r.Value + ".user"))
                .ToDictionary(r => r.Key + ".user", r => r.Value + ".user", StringComparer.OrdinalIgnoreCase);

            if (options.SdkVersion != null)
            {
                if (!Regex.IsMatch(options.SdkVersion, @"^\d+(\.\d+){1,3}(-[0-9A-Za-z.\-]+)?$"))
                    throw new ConversionException($"\"{options.SdkVersion}\" is not a package version.");
                if (options.To == ProjectKind.Wix && Major(options.SdkVersion) >= 7 && !options.AcceptEula)
                    throw new ConversionException(
                        $"WixToolset.Sdk {options.SdkVersion} requires accepting the WiX Toolset's Open Source Maintenance Fee EULA (https://wixtoolset.org/osmf/). Accept it to convert, or choose a v5 or v6 SDK.");
            }

            var result = new ConversionResult();
            var edits = new List<Edit>();

            foreach (var project in files.Where(IsProjectFile))
            {
                var converting = renames.TryGetValue(project, out var renamedTo);
                var edit = Edit.Read(project, converting ? renamedTo! : project);
                var projectFolder = Path.GetDirectoryName(project)!;

                edit.Text = Replace(edit.Text, @"(<ProjectReference\b[^>]*\bInclude\s*=\s*"")([^""]+?)(\.(?:wixproj|iswixproj))("")", m =>
                    renames.ContainsKey(Resolve(projectFolder, m.Groups[2].Value + m.Groups[3].Value))
                        ? m.Groups[1].Value + m.Groups[2].Value + toExtension + m.Groups[4].Value
                        : m.Value);

                if (converting)
                {
                    // The SDK first, so a property it adds lands where the project type is, not where it was.
                    if (options.SdkVersion != null)
                        edit.Text = MoveSdk(edit.Text, options, edit.NewLine);
                    edit.Text = SetProjectType(edit.Text, options.To, edit.NewLine);
                }

                edits.Add(edit);
            }

            foreach (var solution in files.Where(f => HasExtension(f, ".sln")))
            {
                var edit = Edit.Read(solution, solution);
                var folder = Path.GetDirectoryName(solution)!;
                edit.Text = Replace(edit.Text, @"^Project\(""(\{[^}]+\})""\)(\s*=\s*""[^""]*"",\s*"")([^""]+?)(\.(?:wixproj|iswixproj))("")", m =>
                {
                    if (!renames.ContainsKey(Resolve(folder, m.Groups[3].Value + m.Groups[4].Value))) return m.Value;
                    return $"Project(\"{(IsWixType(m.Groups[1].Value) ? ProjectType(options.To) : m.Groups[1].Value)}\"){m.Groups[2].Value}{m.Groups[3].Value}{toExtension}{m.Groups[5].Value}";
                }, RegexOptions.Multiline);
                edits.Add(edit);
            }

            foreach (var solution in files.Where(f => HasExtension(f, ".slnx")))
            {
                var edit = Edit.Read(solution, solution);
                var folder = Path.GetDirectoryName(solution)!;
                edit.Text = Replace(edit.Text, @"<Project\b[^>]*>", element =>
                {
                    var path = Regex.Match(element.Value, @"\bPath\s*=\s*""([^""]+?)(\.(?:wixproj|iswixproj))""", RegexOptions.IgnoreCase, RegexTimeout);
                    if (!path.Success || !renames.ContainsKey(Resolve(folder, path.Groups[1].Value + path.Groups[2].Value))) return element.Value;
                    var converted = element.Value.Substring(0, path.Groups[2].Index)
                        + toExtension
                        + element.Value.Substring(path.Groups[2].Index + path.Groups[2].Length);
                    // Visual Studio picks the project type from the extension; a WiX or IsWiX Type would override it.
                    return Replace(converted, @"\s+Type\s*=\s*""\{?([0-9A-Fa-f-]+)\}?""", type =>
                        IsWixType("{" + type.Groups[1].Value + "}") ? string.Empty : type.Value);
                });
                edits.Add(edit);
            }

            foreach (var edit in edits.Where(e => e.Changed))
            {
                result.Updated.Add(edit.ReportPath);
                if (!options.DryRun) edit.Write();
            }

            foreach (var rename in renames)
            {
                result.Renamed.Add(rename);
                if (!options.DryRun) File.Move(rename.Key, rename.Value);
            }

            if (!options.DryRun)
            {
                foreach (var userFile in userFiles)
                    File.Move(userFile.Key, userFile.Value);
            }

            return result;
        }

        /// <summary>Gets a path relative to a folder, for reporting.</summary>
        public static string Relative(string root, string path)
        {
            var prefix = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? root : root + Path.DirectorySeparatorChar;
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? path.Substring(prefix.Length) : path;
        }

        static string ProjectType(ProjectKind kind) => kind == ProjectKind.IsWiX ? IsWiXProjectType : WixProjectType;

        static bool IsWixType(string type) =>
            string.Equals(type, IsWiXProjectType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, WixProjectType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, WixV3ProjectType, StringComparison.OrdinalIgnoreCase);

        // The project's DefaultProjectTypeGuid for its new kind: IsWiX's for .iswixproj, which the WiX SDK would
        // otherwise default to HeatWave's; none for .wixproj, where the SDK's default is right.
        static string SetProjectType(string text, ProjectKind to, string newLine)
        {
            var existing = new Regex(@"<DefaultProjectTypeGuid>\s*(\{[^}]*\})\s*</DefaultProjectTypeGuid>", RegexOptions.IgnoreCase, RegexTimeout).Match(text);
            if (to == ProjectKind.IsWiX)
            {
                if (!existing.Success) return InsertProperty(text, $"<DefaultProjectTypeGuid>{IsWiXProjectType}</DefaultProjectTypeGuid>", newLine);
                return IsWixType(existing.Groups[1].Value)
                    ? text.Substring(0, existing.Index) + $"<DefaultProjectTypeGuid>{IsWiXProjectType}</DefaultProjectTypeGuid>" + text.Substring(existing.Index + existing.Length)
                    : text;
            }

            if (!existing.Success || !string.Equals(existing.Groups[1].Value, IsWiXProjectType, StringComparison.OrdinalIgnoreCase)) return text;
            // Remove the line, and the comment the IsWiX templates put above it; and the PropertyGroup, if it held
            // nothing else.
            const string Line = @"([ \t]*<!--[^\r\n]*IsWiX project type[^\r\n]*-->[ \t]*\r?\n)?[ \t]*<DefaultProjectTypeGuid>\s*\{3A7B4E67-3CBB-402A-AC31-8EF9DCCEF68E\}\s*</DefaultProjectTypeGuid>[ \t]*\r?\n?";
            var group = new Regex(@"[ \t]*<PropertyGroup\s*>[ \t]*\r?\n" + Line + @"[ \t]*</PropertyGroup>[ \t]*\r?\n?", RegexOptions.IgnoreCase, RegexTimeout);
            return group.IsMatch(text) ? group.Replace(text, string.Empty, 1) : Replace(text, Line, _ => string.Empty);
        }

        // Moves the project to the SDK and extension packages of the kind it's converted to.
        static string MoveSdk(string text, ConversionOptions options, string newLine)
        {
            var fromPrefix = options.To == ProjectKind.IsWiX ? "WixToolset" : "IsWiX";
            var toPrefix = options.To == ProjectKind.IsWiX ? "IsWiX" : "WixToolset";
            var version = options.SdkVersion!;

            text = Replace(text, $@"(\bSdk\s*=\s*""){fromPrefix}\.Sdk(/[^""]*)?("")", m => $"{m.Groups[1].Value}{toPrefix}.Sdk/{version}{m.Groups[3].Value}");
            text = Replace(text, $@"(<(?:Sdk|Import)\b[^>]*\b(?:Name|Sdk)\s*=\s*""){fromPrefix}\.Sdk("")", m => $"{m.Groups[1].Value}{toPrefix}.Sdk{m.Groups[2].Value}");
            text = Replace(text, $@"<PackageReference\b[^>]*>", element =>
            {
                var include = Regex.Match(element.Value, $@"(\bInclude\s*=\s*""){fromPrefix}\.([\w.]+?\.wixext)("")", RegexOptions.IgnoreCase, RegexTimeout);
                if (!include.Success) return element.Value;
                var name = ExtensionName(include.Groups[2].Value, options.To);
                var converted = element.Value.Substring(0, include.Index) + $"{include.Groups[1].Value}{toPrefix}.{name}{include.Groups[3].Value}" + element.Value.Substring(include.Index + include.Length);
                return Replace(converted, @"(\bVersion\s*=\s*"")[^""]*("")", v => $"{v.Groups[1].Value}{version}{v.Groups[2].Value}");
            });

            var hasEula = Regex.IsMatch(text, @"<AcceptEula>", RegexOptions.IgnoreCase, RegexTimeout);
            var needsEula = options.To == ProjectKind.Wix && Major(version) >= 7;
            if (!needsEula && hasEula)
                text = Replace(text, @"[ \t]*<AcceptEula>[^<]*</AcceptEula>[ \t]*\r?\n?", _ => string.Empty);
            else if (needsEula && !hasEula)
                text = InsertProperty(text, "<AcceptEula>wix7</AcceptEula>", newLine);
            return text;
        }

        // An extension package's name after the IsWiX. or WixToolset. prefix, for the kind it's converted to. The WiX
        // Toolset renamed Bal.wixext to BootstrapperApplications.wixext and kept Bal.wixext as a compatibility shim;
        // IsWiX publishes only BootstrapperApplications.wixext, so Bal.wixext moves to it.
        static string ExtensionName(string name, ProjectKind to) =>
            to == ProjectKind.IsWiX && string.Equals(name, "Bal.wixext", StringComparison.OrdinalIgnoreCase)
                ? "BootstrapperApplications.wixext"
                : name;

        // Adds a property at the top of the project's first unconditioned PropertyGroup, or in a new one.
        static string InsertProperty(string text, string property, string newLine)
        {
            var group = new Regex(@"([ \t]*)<PropertyGroup\s*>([ \t]*\r?\n)([ \t]*)", RegexOptions.None, RegexTimeout).Match(text);
            if (group.Success)
            {
                var at = group.Index + group.Length;
                var next = group.Groups[3].Value;
                // An empty group: indent the property one level deeper than the group.
                var propertyIndent = text.Substring(at).StartsWith("</PropertyGroup", StringComparison.OrdinalIgnoreCase)
                    ? group.Groups[1].Value + (group.Groups[1].Value.Length > 0 ? group.Groups[1].Value : "  ")
                    : next;
                return text.Substring(0, at - next.Length) + propertyIndent + property + newLine + next + text.Substring(at);
            }

            var project = new Regex(@"<Project\b[^>]*>([ \t]*\r?\n)?", RegexOptions.None, RegexTimeout).Match(text);
            if (!project.Success) return text;
            // The indentation of the project's first child element.
            var indent = new Regex(@"\n([ \t]+)<", RegexOptions.None, RegexTimeout).Match(text, project.Index);
            var unit = indent.Success ? indent.Groups[1].Value : "  ";
            var insert = (project.Groups[1].Success ? string.Empty : newLine)
                + $"{unit}<PropertyGroup>{newLine}{unit}{unit}{property}{newLine}{unit}</PropertyGroup>{newLine}";
            var end = project.Index + project.Length;
            return text.Substring(0, end) + insert + text.Substring(end);
        }

        static int Major(string version) => int.TryParse(version.Split('.')[0], out var major) ? major : 0;

        static string Replace(string text, string pattern, MatchEvaluator evaluator, RegexOptions options = RegexOptions.None) =>
            new Regex(pattern, options | RegexOptions.IgnoreCase, RegexTimeout).Replace(text, evaluator);

        static bool HasExtension(string path, string extension) =>
            string.Equals(Path.GetExtension(path), extension, StringComparison.OrdinalIgnoreCase);

        static bool IsProjectFile(string path) => Path.GetExtension(path).EndsWith("proj", StringComparison.OrdinalIgnoreCase);


        // The full path a path written in a file under folder refers to. Project files use either separator.
        static string Resolve(string folder, string path) =>
            Path.GetFullPath(Path.Combine(folder, path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)));

        static IEnumerable<string> Files(string folder)
        {
            string[] here;
            try { here = Directory.GetFiles(folder); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { yield break; }
            foreach (var file in here) yield return file;

            string[] subfolders;
            try { subfolders = Directory.GetDirectories(folder); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { yield break; }
            foreach (var sub in subfolders)
            {
                var name = Path.GetFileName(sub);
                if (name.StartsWith(".", StringComparison.Ordinal) || SkippedFolders.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
                foreach (var file in Files(sub)) yield return file;
            }
        }

        // A file being edited: its text, kept with the encoding, byte order mark and line endings it was read with.
        sealed class Edit
        {
            byte[] _preamble = new byte[0];
            Encoding _encoding = new UTF8Encoding(false);
            string _original = string.Empty;

            public string Path { get; private set; } = string.Empty;
            public string ReportPath { get; private set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public string NewLine => _original.Contains("\r\n") ? "\r\n" : "\n";
            public bool Changed => !string.Equals(Text, _original, StringComparison.Ordinal);

            public static Edit Read(string path, string reportPath)
            {
                var bytes = File.ReadAllBytes(path);
                var edit = new Edit { Path = path, ReportPath = reportPath };
                if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                    edit._preamble = new byte[] { 0xEF, 0xBB, 0xBF };
                else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                {
                    edit._preamble = new byte[] { 0xFF, 0xFE };
                    edit._encoding = new UnicodeEncoding(false, false);
                }
                else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                {
                    edit._preamble = new byte[] { 0xFE, 0xFF };
                    edit._encoding = new UnicodeEncoding(true, false);
                }

                edit._original = edit._encoding.GetString(bytes, edit._preamble.Length, bytes.Length - edit._preamble.Length);
                edit.Text = edit._original;
                return edit;
            }

            public void Write()
            {
                var body = _encoding.GetBytes(Text);
                var bytes = new byte[_preamble.Length + body.Length];
                Buffer.BlockCopy(_preamble, 0, bytes, 0, _preamble.Length);
                Buffer.BlockCopy(body, 0, bytes, _preamble.Length, body.Length);
                File.WriteAllBytes(Path, bytes);
            }
        }
    }
}
