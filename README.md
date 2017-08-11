# IsWiX Windows Service Tutorial

This tutorial will walk you through how to build a [Windows Installer](https://msdn.microsoft.com/en-us/library/windows/desktop/cc185688(v=vs.85).aspx) (aka .MSI) setup package for a typical Windows Service.  It is assumed you have read the [Overview](https://github.com/iswix-llc/iswix-tutorials) instructions first.

## Create the Windows Service

### Launch Visual Studio and create a new Windows Service:

![Application New Project](/Images/windows-service/Service-ProjectType.png)

* Use File | New | Project and select the Windows Service C# template.
* Target .NET Framework 4. You can select a different version but you will have an additional step later in the process.
* Call the project WindowsService
* Point the Location to the root of your GitHub repos or to a location in your version control system.
   * For tools like Team Foundation Version Control you may want to point it to something like `C:\workspace\TeamProject\ApplicationName\main\` as this will give you the folder `main` that can the be branched to say `dev` and used by build definitions.   This isn't needed in GitHub since the branching model is implemented differently.
* Make sure the "Create directory for solution" option is selected.
* Make sure the "Add Files to Source Control" option is selected if using TFVC.

### Author a postbuild copy:

![Mode](/Images/windows-service/Service-Postbuild.png)

Using the Solution Explorer, bring up the project properties for your main EXE project.  Author the following postbuild event:

`xcopy /iery "$(TargetDir)*.*" "$(SolutionDir)..\Installer\Deploy"`

This will help us create a directory structure that models what your deployed application should look like.  Go ahead and build the project and take a look at that directory in your workspace.

![Mode](/Images/windows-service/Service-InstallerModel.png)

We are done creating the application.   Close the Application.sln as we will be creating a new solution for the installer.

## Create the Installer

### Launch Visual Studio and create a new IsWiX Solution:

![Installer New Project](/Images/windows-service/Installer-ProjectType.png)

* Use File | New | Project and select the IsWiX Solution for WiX v3 template.
* The .NET Framework version isn't important here.  Perhaps one day someone will submit a pull request to help automate a step down the road based on the selection here.
* Call the project DesktopApplication.  (This will result in your installer being called DesktopApplication.msi.)
* Point the Location to the root of your GitHub repos or to a location in your version control system.
   * For tools like Team Foundation Version Control you may want to point it to something like `C:\workspace\TeamProject\ApplicationName\main\` as this will give you the folder `main` that can the be branched to say `dev` and used by build definitions.   This isn't needed in GitHub since the branching model is implemented differently.
* Make sure the "Create directory for solution" option is selected.
* Make sure the "Add Files to Source Control" option is selected if using TFVC.

### Project Structure Overview:

![Mode](/Images/desktop-application/Installer-ProjectStructure.png)

### The IsWiX solution template created a solution with two projects based on the name:

* Setup Project 
   * Named DesktopApplication
   * Creates an .MSI (Microsoft Installer) database that is installable by a user.
   * Expresses the Manufacturer, Product Name, Upgrade story, Feature Tree,  User Interface,  Installation Requirements and Installation path of the product.

* Merge Module Project
   * NamedDesktopApplicationMM  (Merge Module)
   * Creates an .MSM (Micrsoft Merge Module) database that is not installable by a user.
   * Similar to a .LIB in C/C++.  Database contents get merged into the .MSI at build time.
   * Expresses the files, subfolders, registry entries, shortcuts, services, environment variables and so on that will make up your product.
   * Somewhat serves as an atomic abstract encapsulation of a portion of your installer logic.  You may have more then one merge module consumed by one or more features across one consumed by one or more setup projects.  This is an advanced topic that may be covered by another tutorial some day.  For now just keep it simple with a 1 product with 1 feature consuming 1 merge module.
   
### Author the merge module:

![Merge Module Source before](/Images/desktop-application/Installer-MergeModuleBefore.png)

Select the DesktopApplicationMM.wxs (.wix source) in the DesktopApplication MM project.

Observe line 5: `<?define SourceDir="..\Deploy"?>` This will guide IsWiX in knowing where to find your applications files relative to the location of this wix project (.wixproj).  Our project structure choices cause this to automatically align with the output of our previous xcopy command.

### Launch IsWiX:

![Launch IsWiX](/Images/desktop-application/Installer-LaunchIsWiX.png)

Select Tools | Launch IsWiX to send the current .wxs document to IsWiX.

![IsWiX Launched](/Images/desktop-application/Installer-IsWiX.png)

* Our DesktopApplicationMM.wxs document is now loaded in IsWiX.  
* We are given a single document interface (SDI) with multiple designers along the left side. 
* Each designer expresses a particular view of our .wxs file.  
* The default designer is the General Information designer.  
   * This designer tells us about the module signature and dependencies of this module.  
   * Every merge module has a unique identity similar to a strong name in .NET.  
   * Merge modules can also have dependencies on other merge modules.  This may be covered in another tutorial one day.

### Click on the Files and Folders designer:

![IsWiX Files and Folder designer - before](/Images/desktop-application/Installer-IsWiXFFBefore.png)

* The top  two quadrants of this designer represent source files and folders that are on your developer machine  that can be included in the installation.  
* The bottom two quadrants represent what will be installed on the destination machine.  
* `[MergeRedirectFolder]` is an abstract folder which represents the root of this merge module.  Later it will be associated with your products installation directory: `[ProgramFilesFolder]\Company\Product`.
* The merge module has no knowledge of what product will consume it. Therefore, folder name abstractions like above are used.
* Notice that `<?define SourceDir="..\Deploy"?>` has been resolved to a fully qualified path and your files are in the upper right quadrant.
   * The use of relative path filenames will be critical when we get to build automation as all of your file path dependencies must be found on the build server for your solution to successfully build.
   * Our choice of Application.sln and Installer.sln is important also as we avoid tricky project dependency / build order race conditions.
   
### Author your files:

![IsWiX Files and Folder designer - after](/Images/desktop-application/Installer-IsWiXFFAfter.png)

* Click on the `[MergeRedirectFolder]` in the destination view.
* Drag the file DesktopApplication.exe from the source files to the destination files quadrant.
   * We don't need to ship the other files.
   
### Author your Shortcut:

![IsWiX ShortCuts designer](/Images/desktop-application/Installer-IsWiXShortCuts.png)

* Click on the ShortCuts designer. A view of your merge modules shortcuts will appear.

![IsWiX ShortCuts designer - create desktop folder](/Images/desktop-application/Installer-IsWiXShortCutsDesktopFolder.png)

* Right click `Destination Computer` and select the `DesktopFolder` option.
* Expand `Desitnation Computer` and select the `DesktopFolder` node.  (Maybe a Pull Request will fix this some day.)

![IsWiX ShortCuts designer - create shortcut](/Images/desktop-application/Installer-IsWiXShortCutsCreateShortCut.png)

* Right click `DesktopFolder` and select the `Create ShortCut option`.

![IsWiX ShortCuts designer - component picker](/Images/desktop-application/Installer-IsWiXShortCutsComponentPicker.png)

* Select the file that will serve as the target of this shortcut. `[MergeRedirectFolder\DesktopApplication.exe`  and click Select.

![IsWiX ShortCuts designer - shortcut properties](/Images/desktop-application/Installer-IsWiXShortCutsProperties.png)

* Use the property grid to give the shortcut a friendly name such as `Desktop Application`.

![Visual Studio Reload Document](/Images/desktop-application/Installer-VSReload.png)

* Press Control-S to save the changes back to disk and close the IsWiX application.
* This should cause Visual Studio to detect that the .wxs file was modified out of process.  Click Yes to cause it to reload in the IDE.

### Observe the XML changes

![IsWiX Merge Module - After](/Images/desktop-application/Installer-MergeModuleAfter.png)

* If you compare the XML before/after you should notice that IsWiX authored multiple elements for you. 
   * A `Directory` element referencing the DesktopFolder location.
   * `Component`, `File`, and `Shortcut` elements defining your new file and a shortcut pointing to it. 
   * IsWiX manages this document using a hashing and sorting alogorythm.  You should not modify these elements.
   * You may however add additional child elements and/or author elements in the `-custom.wxs` fragment.  This is an advanced topic that may be covered by another tutorial one day.
   
### Build the MSI

![Installer - Built MSI](/Images/desktop-application/Installer-BuiltMSI.png)

### Different .NET Version Scenario

![Installer - Built MSI](/Images/desktop-application/Installer-DotNetVersion.png)

* The IsWiX project template includes a .NET 4.0 prerequisite check by default.  This ensures that .NET 4.0 is installed  before a user can install your MSI.
* You may have choosen a different .NET version when building your EXE.  In this case find the Product.wxs file in the DesktopApplication wix project.
   * Visit the URL mentioned on line 19 to get the name of the property of the version of .NET you wish to require.
   * Update the `ProperfyRef` on line 20 and update the condition on line 21.
   * The `Installed or` portion of the condition is to ensure tha we don't require .NET during the uninstall as this would be a poor user experience.
   
### Build considerations

Now that you have a source tree of application and installer code you will likely want to build it with a continuous integration system. WiX suports msbuild and as such is easily compiled.  While IsWiX merely authors the WiX projects and isn't technically part of the build process (and not needed on the build server)  it does include some XML tags in the DesktopApplication.wixproj that can be useful for versioning.

`<MSIProductVersion Condition="'$(MSIProductVersion)' == ''">$([System.Text.RegularExpressions.Regex]::Match($(TF_BUILD_BUILDNUMBER), "\d+.\d+.\d+.\d+"))</MSIProductVersion>`

This allows you to pass an MSIProductVersion property into the build or directly infer it in the case of a TFS XAML build definition. For TFS v.Next builds change `TF_BUILD_BUILDNUMBER` to `BUILD_BUILDNUMBER`.   As Windows Installer ignores the fourth field, you should always change one of the first three fields in order to ensure Major Upgrades will work correctly.  

`1.0.0.0, 1.0.1.0, 1.0.2.0, 1.1.0.0, 1.1.1.0, 2.0.0.0, 2.0.1.0` and so on...

### Conclusion

Hopefully this tutorial gives you a complete picture of how to quickly come up to speed on creating an MSI installer using WiX / IsWiX. This is only the beginning though as eventually you'll need to learn more about the underlying technologies as you attempt more complicated authoring.
