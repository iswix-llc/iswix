REM  Release build of ISWIX is  done using  Visual Studio Online Team Build
REM  The build definition builds the following solutions / platforms / releases

REM  Place MSBuild in the path
SET PATH=%PATH%;C:\Program Files\MSBuild\14.0\Bin;C:\Program Files (x86)\MSBuild\14.0\Bin

REM  Build the application
REM  Note: Release build automation is responsible for versioning all AssemblyInfo.cs files by calling the ApplyVersionToAssemblies.ps1 script
REM  Developer builds are versioned as whatever is checked into source control
msbuild ..\Source\Application\Application.sln /m /t:Rebuild /p:Configuration=Release /p:Platform=x86 /p:OutDir=%CD%\Build\Application\ > dev_build_application.log

REM  Build the installer
REM  Note: Release build automation will pass in /MSIProductVersion=Major.Minor.YYDDD.R to version the MSI. 
REM  Developer builds default to version 0.0.1 as to never upgrade an official release build
msbuild ..\Source\Installer\Installer.sln /m /t:Rebuild /p:Configuration=Release /p:Platform=x86 /p:OutDir=%CD%\Build\Installer\ > dev_build_installer.log