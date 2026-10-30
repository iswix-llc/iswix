Industrial Strength Windows Installer XML ( IsWiX ) makes authoring Windows Installer XML (WiX) Windows Installers (.MSI) a breeze.  It includes Visual Studio project templates and graphical designers to aid you in authoring 95% of your source code.  From there you can author additional code using standard WiX programming techniques.

Starting with v6, IsWiX is only distributed via the Visual Studio Marketplace as a Visual Studio 2022/2026 Extension.  You can find it [here](https://marketplace.visualstudio.com/items?itemName=ISWIX-LLC.IsWiX).

Please see our [tutorials](https://github.com/iswix-llc/iswix-tutorials) to learn how to get started.

## IsWiX projects (.iswixproj)

IsWiX opens its own project type, `.iswixproj`, without HeatWave. An `.iswixproj` is an ordinary WiX SDK project: it
builds with `dotnet build` or MSBuild on either `WixToolset.Sdk` or `IsWiX.Sdk`, and the IsWiX project templates create
them. In Visual Studio, IsWiX's project system (`IsWiXExtension/ProjectSystem`) gives it Solution Explorer, property
pages, project references and NuGet restore. `.wixproj` projects still open with HeatWave if it is installed; the IsWiX
designers work in either.

To convert existing `.wixproj` projects, run from a clone of this repository (needs the .NET 10 SDK):

```
dotnet run Tools/ConvertToIsWiX.cs -- <folder> [--iswix-sdk <version>] [--dry-run]
```

It renames the projects under the folder, updates project references and solutions, and with `--iswix-sdk` moves them
from WixToolset.Sdk to IsWiX.Sdk.

## Building and releasing

The version lives in `Directory.Build.props`; the release workflow also writes it into the VSIX manifest
(`IsWiXExtension/source.extension.vsixmanifest`) and `IsWiXExtension/Properties/AssemblyInfo.cs`.

- **CI** (`.github/workflows/ci.yml`) runs on every push to `dev` and every pull request to `dev` or
  `main`: it builds `IsWiX.sln`, publishes IsWiX into `IsWiXExtension\IsWiX` (the `FolderProfile` publish
  profile), builds `IsWiXExtension.sln` and uploads the unsigned VSIX as a build artifact.
- **Publish** (`.github/workflows/publish.yml`) is run by hand from the Actions tab on `main`: choose
  `patch`, `minor` or `major`. It increments the version, builds, signs the VSIX and IsWiX's own
  assemblies inside it with Azure Artifact Signing (the `sign` CLI; third-party DLLs keep their
  publishers' signatures), commits `Release x.y.z` to `main`, tags it
  `vx.y.z` and creates a GitHub release with the signed VSIX attached. Uploading that VSIX to the
  Visual Studio Marketplace is done by hand.

The publish workflow needs the `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID`
repository secrets, and an Azure federated credential for this repository's `main` branch with the
Artifact Signing Certificate Profile Signer role on the `ISWIX-LLC` account.

## Diagnosing the designer window

Set the environment variable `ISWIX_TRACE` to `1` before starting Visual Studio and the extension writes what the .wxs
editor and its designers are doing to an **IsWiX** pane of the Output window: when a designer's selection and the XML
caret are linked, why one was not followed, the commands (such as Go to Definition) the editor was asked to run, and the
component groups found in the project. It is off, and costs nothing, otherwise. Visual Studio's activity log
(`devenv /log`) also records the editor's main events.
