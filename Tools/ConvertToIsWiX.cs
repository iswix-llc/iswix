// Copyright (c) ISWIX LLC. Licensed under the MS-PL.
//
// Converts WiX SDK projects (.wixproj) to IsWiX projects (.iswixproj), so Visual Studio opens them with IsWiX's own
// project system instead of HeatWave's.
//
//   dotnet run Tools/ConvertToIsWiX.cs -- <folder> [--iswix-sdk <version>] [--dry-run]
//
// Under <folder> (build output and hidden folders aside) it:
//   - renames every .wixproj to .iswixproj, and sets its DefaultProjectTypeGuid to IsWiX's, which "dotnet sln add"
//     writes into solutions;
//   - points ProjectReferences to the renamed projects, in every project file;
//   - updates the solutions (.sln and .slnx): the projects' paths, and in .sln files their project type, to IsWiX's;
//   - with --iswix-sdk, also moves the projects from WixToolset.Sdk to IsWiX.Sdk at that version: the Sdk, the
//     WixToolset.*.wixext package references (to IsWiX.*.wixext) and the AcceptEula property, which IsWiX.Sdk
//     doesn't need.
// --dry-run reports what it would change and changes nothing. Files keep their encoding and line endings.

// Native AOT is the default for file-based apps and needs a package download; this tool has no use for it.
#:property PublishAot=false

using System.Text;
using System.Text.RegularExpressions;

const string IsWiXProjectType = "{3A7B4E67-3CBB-402A-AC31-8EF9DCCEF68E}";
string[] wixProjectTypes = { "{B7DD6F7E-DEF8-4E67-B5B7-07EF123DB6F0}", "{930C7802-8A8C-48F9-8165-68863BCCD9DD}" };
string[] skippedFolders = { "bin", "obj", "node_modules", "packages" };

string? root = null;
string? isWiXSdkVersion = null;
var dryRun = false;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--dry-run": dryRun = true; break;
        case "--iswix-sdk" when i + 1 < args.Length: isWiXSdkVersion = args[++i]; break;
        case "-h" or "--help": return Usage(0);
        default:
            if (root != null || args[i].StartsWith("-")) return Usage(1);
            root = args[i];
            break;
    }
}
if (root == null) return Usage(1);
if (!Directory.Exists(root)) { Console.Error.WriteLine($"Folder not found: {root}"); return 1; }
root = Path.GetFullPath(root);

var files = Files(root).ToList();
var wixProjects = files.Where(f => f.EndsWith(".wixproj", StringComparison.OrdinalIgnoreCase)).ToList();
if (wixProjects.Count == 0) { Console.WriteLine($"No .wixproj projects under {root}."); return 0; }

// Project files after the renames, which are the ones whose references and SDK get updated.
var renames = wixProjects.ToDictionary(p => p, p => Path.ChangeExtension(p, ".iswixproj"), StringComparer.OrdinalIgnoreCase);
foreach (var target in renames.Values.Where(File.Exists))
{
    Console.Error.WriteLine($"Not converting: {Relative(target)} already exists.");
    return 1;
}

var changed = 0;

// Project references and, if asked, the SDK. Done before the renames so a dry run reads the same files.
foreach (var project in files.Where(IsProjectFile))
{
    var renamedTo = renames.TryGetValue(project, out var to) ? to : project;
    Edit(project, text =>
    {
        text = Regex.Replace(text, @"(<ProjectReference\b[^>]*\bInclude\s*=\s*"")([^""]+?)\.wixproj("")", m =>
            Converts(Path.GetDirectoryName(project)!, m.Groups[2].Value + ".wixproj")
                ? m.Groups[1].Value + m.Groups[2].Value + ".iswixproj" + m.Groups[3].Value
                : m.Value, RegexOptions.IgnoreCase);
        if (renames.ContainsKey(project) && !text.Contains("<DefaultProjectTypeGuid>", StringComparison.OrdinalIgnoreCase))
        {
            // The WiX SDK's default project type is HeatWave's, which "dotnet sln add" would write into solutions.
            text = new Regex(@"<PropertyGroup>(\r?\n)([ \t]*)").Replace(text, m =>
                $"<PropertyGroup>{m.Groups[1].Value}{m.Groups[2].Value}<DefaultProjectTypeGuid>{IsWiXProjectType}</DefaultProjectTypeGuid>{m.Groups[1].Value}{m.Groups[2].Value}", 1);
        }
        if (isWiXSdkVersion != null && renames.ContainsKey(project))
        {
            text = Regex.Replace(text, @"(\bSdk\s*=\s*"")WixToolset\.Sdk(/[^""]*)?("")", $"${{1}}IsWiX.Sdk/{isWiXSdkVersion}${{3}}");
            text = Regex.Replace(text, @"(<Sdk\b[^>]*\bName\s*=\s*"")WixToolset\.Sdk("")", "$1IsWiX.Sdk$2");
            text = Regex.Replace(text, @"(<PackageReference\b[^>]*\bInclude\s*=\s*"")WixToolset\.([\w.]+?\.wixext)(""[^>]*?\bVersion\s*=\s*"")[^""]*("")",
                $"${{1}}IsWiX.${{2}}${{3}}{isWiXSdkVersion}${{4}}");
            text = Regex.Replace(text, @"(<PackageReference\b[^>]*\bInclude\s*=\s*"")WixToolset\.([\w.]+?\.wixext)("")", "$1IsWiX.$2$3");
            text = Regex.Replace(text, @"[ \t]*<AcceptEula>[^<]*</AcceptEula>[ \t]*\r?\n?", "");
        }
        return text;
    }, renamedTo);
}

// Solutions.
foreach (var solution in files.Where(f => f.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)))
{
    var folder = Path.GetDirectoryName(solution)!;
    Edit(solution, text => Regex.Replace(text, @"^Project\(""(\{[^}]+\})""\)(\s*=\s*""[^""]*"",\s*"")([^""]+?)\.wixproj("")", m =>
    {
        if (!Converts(folder, m.Groups[3].Value + ".wixproj")) return m.Value;
        var type = wixProjectTypes.Contains(m.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? IsWiXProjectType : m.Groups[1].Value;
        return $"Project(\"{type}\"){m.Groups[2].Value}{m.Groups[3].Value}.iswixproj{m.Groups[4].Value}";
    }, RegexOptions.Multiline | RegexOptions.IgnoreCase));
}
foreach (var solution in files.Where(f => f.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase)))
{
    var folder = Path.GetDirectoryName(solution)!;
    Edit(solution, text => Regex.Replace(text, @"<Project\b[^>]*>", element =>
    {
        var path = Regex.Match(element.Value, @"\bPath\s*=\s*""([^""]+?)\.wixproj""", RegexOptions.IgnoreCase);
        if (!path.Success || !Converts(folder, path.Groups[1].Value + ".wixproj")) return element.Value;
        var converted = element.Value.Remove(path.Index, path.Length)
            .Insert(path.Index, path.Value.Substring(0, path.Value.Length - ".wixproj\"".Length) + ".iswixproj\"");
        // A Type naming a WiX project type would send the project to HeatWave; IsWiX's is the default for .iswixproj.
        return Regex.Replace(converted, @"\s+Type\s*=\s*""(\{[^}]+\})""", type =>
            wixProjectTypes.Contains(type.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? "" : type.Value);
    }, RegexOptions.IgnoreCase));
}

// The renames.
foreach (var (from, to) in renames)
{
    Console.WriteLine($"{(dryRun ? "Would rename" : "Renamed")} {Relative(from)} -> {Path.GetFileName(to)}");
    if (!dryRun) File.Move(from, to);
}

Console.WriteLine(dryRun
    ? $"Dry run: {renames.Count} project(s) to rename, {changed} file(s) to update. Nothing was changed."
    : $"Converted {renames.Count} project(s); updated {changed} file(s).");
return 0;

// Whether a path written in a file under folder refers to one of the projects being converted.
bool Converts(string folder, string path) =>
    renames.ContainsKey(Path.GetFullPath(Path.Combine(folder, path.Replace('\\', Path.DirectorySeparatorChar))));

bool IsProjectFile(string path) => Path.GetExtension(path).EndsWith("proj", StringComparison.OrdinalIgnoreCase);

// Rewrites a file with what edit makes of its text, keeping its encoding and line endings; reports it under the
// name it will have.
void Edit(string path, Func<string, string> edit, string? reportAs = null)
{
    var bytes = File.ReadAllBytes(path);
    var encoding = DetectEncoding(bytes, out var preamble);
    var text = encoding.GetString(bytes, preamble, bytes.Length - preamble);
    var edited = edit(text);
    if (edited == text) return;
    changed++;
    Console.WriteLine($"{(dryRun ? "Would update" : "Updated")} {Relative(reportAs ?? path)}");
    if (dryRun) return;
    File.WriteAllBytes(path, bytes[..preamble].Concat(encoding.GetBytes(edited)).ToArray());
}

static Encoding DetectEncoding(byte[] bytes, out int preamble)
{
    if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) { preamble = 3; return new UTF8Encoding(false); }
    if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) { preamble = 2; return new UnicodeEncoding(false, false); }
    if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) { preamble = 2; return new UnicodeEncoding(true, false); }
    preamble = 0;
    return new UTF8Encoding(false);
}

IEnumerable<string> Files(string folder)
{
    IEnumerable<string> here;
    try { here = Directory.EnumerateFiles(folder).ToList(); }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { yield break; }
    foreach (var file in here) yield return file;

    IEnumerable<string> subfolders;
    try { subfolders = Directory.EnumerateDirectories(folder).ToList(); }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { yield break; }
    foreach (var sub in subfolders)
    {
        var name = Path.GetFileName(sub);
        if (name.StartsWith(".") || skippedFolders.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
        foreach (var file in Files(sub)) yield return file;
    }
}

string Relative(string path) => Path.GetRelativePath(root!, path);

static int Usage(int exitCode)
{
    (exitCode == 0 ? Console.Out : Console.Error).WriteLine(
        "Usage: dotnet run Tools/ConvertToIsWiX.cs -- <folder> [--iswix-sdk <version>] [--dry-run]");
    return exitCode;
}
