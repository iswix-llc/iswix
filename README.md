Industrial Strength Windows Installer XML ( IsWiX ) makes authoring Windows Installer XML (WiX) Windows Installers (.MSI) a breeze.  It includes Visual Studio project templates and graphical designers to aid you in authoring 95% of your source code.  From there you can author additional code using standard WiX programming techniques.

Starting with v6, IsWiX is only distributed via the Visual Studio Marketplace as a Visual Studio 2022/2026 Extension.  You can find it [here](https://marketplace.visualstudio.com/items?itemName=ISWIX-LLC.IsWiX).


## IsWiX projects (.iswixproj)

IsWiX opens its own project type, `.iswixproj`, without HeatWave. An `.iswixproj` is an ordinary WiX SDK project: it
builds with `dotnet build` or MSBuild on either `WixToolset.Sdk` or `IsWiX.Sdk`, and the IsWiX project templates create
them. In Visual Studio, IsWiX's project system (`IsWiXExtension/ProjectSystem`) gives it Solution Explorer, property
pages, project references and NuGet restore. `.wixproj` projects still open with HeatWave if it is installed; the IsWiX
designers work in either.

To convert projects either way, use **Convert to IsWiX Project** or **Convert to WiX Project** on a project or the solution
in Solution Explorer, or run from a clone of this repository (needs the .NET 10 SDK):

```
dotnet run --project Tools/IsWiXConvert -- <folder> --to iswix|wix [--sdk <version>] [--accept-eula] [--project <path>]... [--dry-run]
```

It renames the projects under the folder (or just those given with `--project`) and updates the project references and
solutions there to match. `--sdk` also moves the projects to IsWiX.Sdk (`--to iswix`) or WixToolset.Sdk (`--to wix`) at
that version, with the matching `.wixext` packages; WixToolset.Sdk v7 and later need `--accept-eula`, which accepts the
WiX Toolset's [Open Source Maintenance Fee EULA](https://wixtoolset.org/osmf/). Converting there and back restores the
files.

## Designers

How the designers behave is documented under [docs](docs):

* [Features designer](docs/features-designer.md): features, feature groups and their references across the documents of a
  project, and how a project's files are found (for every designer).
* [Components designer](docs/components-designer.md): the components of a component group and what each holds, across the
  documents of a project.

## WiX XML schemas

The extension installs the WiX XML schemas (`IsWiXExtension/Schemas/iswix_*.xsd`) into Visual Studio's XML schema cache,
for IntelliSense and validation in `.wxs`, `.wxi` and `.wxl` files. The WiX Toolset keeps its schemas as one fragment
per element under `src/xsd`; regenerate the combined schemas from a WiX checkout at the release IsWiX ships with:

```
dotnet run --project Tools/IsWiXSchemas -- <wix>/src/xsd IsWiXExtension/Schemas
```

It compiles the schemas it writes and fails on any error.

## Custom action templates

The template pack also has two custom action projects, which ask for IsWiX's or the WiX Toolset's packages the same way
the installer template does:

- **IsWiX C# Custom Action** (`dotnet new iswix-ca-cs`): a .NET Framework 4.7.2 class library on the `Dtf.CustomAction`
  package. The build packs it into `<name>.CA.dll`, the file the installer's `Binary` element references.
- **IsWiX C++ Custom Action** (`dotnet new iswix-ca-cpp`): a native DLL on the `WcaUtil` and `DUtil` packages, restored
  with `packages.config`. Restore NuGet packages for the solution before the first build. IsWiX's 7.x native libraries
  are built with the v145 toolset (Visual Studio 2026).


