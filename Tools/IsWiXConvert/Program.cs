// Copyright (c) ISWIX LLC. Licensed under the MS-PL.
//
//   dotnet run --project Tools/IsWiXConvert -- <folder> --to iswix|wix [--sdk <version>] [--accept-eula]
//                                             [--project <path>]... [--dry-run]
//
// Converts the .wixproj projects under <folder> to .iswixproj (--to iswix), or the .iswixproj ones to .wixproj
// (--to wix), and updates the project references and solutions under it to match. --project limits it to the
// projects named. --sdk also moves the projects to IsWiX.Sdk (to iswix) or WixToolset.Sdk (to wix) at that version,
// with the matching .wixext packages; WixToolset.Sdk v7 and later need --accept-eula, which accepts the WiX
// Toolset's Open Source Maintenance Fee EULA. --dry-run reports what would change and changes nothing.

using IsWiX.Conversion;

string? root = null;
var options = new ConversionOptions();
var to = (string?)null;
var projects = new List<string>();

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--to" when i + 1 < args.Length: to = args[++i]; break;
        case "--sdk" when i + 1 < args.Length: options.SdkVersion = args[++i]; break;
        case "--project" when i + 1 < args.Length: projects.Add(args[++i]); break;
        case "--accept-eula": options.AcceptEula = true; break;
        case "--dry-run": options.DryRun = true; break;
        case "-h" or "--help": return Usage(0);
        default:
            if (root != null || args[i].StartsWith('-')) return Usage(1);
            root = args[i];
            break;
    }
}

switch (to?.ToLowerInvariant())
{
    case "iswix": options.To = ProjectKind.IsWiX; break;
    case "wix": options.To = ProjectKind.Wix; break;
    default: return Usage(1);
}
if (root == null) return Usage(1);

try
{
    var result = ProjectConverter.Convert(root, projects.Count > 0 ? projects : null, options);
    var full = Path.GetFullPath(root);
    var verb = options.DryRun ? "Would" : null;
    foreach (var updated in result.Updated)
        Console.WriteLine($"{(verb == null ? "Updated" : "Would update")} {ProjectConverter.Relative(full, updated)}");
    foreach (var rename in result.Renamed)
        Console.WriteLine($"{(verb == null ? "Renamed" : "Would rename")} {ProjectConverter.Relative(full, rename.Key)} -> {Path.GetFileName(rename.Value)}");

    if (result.Renamed.Count == 0)
        Console.WriteLine($"No {ProjectConverter.Extension(ProjectConverter.Other(options.To))} projects under {full}.");
    else
        Console.WriteLine(options.DryRun
            ? $"Dry run: {result.Renamed.Count} project(s) to convert, {result.Updated.Count} file(s) to update. Nothing was changed."
            : $"Converted {result.Renamed.Count} project(s); updated {result.Updated.Count} file(s).");
    return 0;
}
catch (ConversionException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static int Usage(int exitCode)
{
    (exitCode == 0 ? Console.Out : Console.Error).WriteLine(
        "Usage: dotnet run --project Tools/IsWiXConvert -- <folder> --to iswix|wix [--sdk <version>] [--accept-eula] [--project <path>]... [--dry-run]");
    return exitCode;
}
