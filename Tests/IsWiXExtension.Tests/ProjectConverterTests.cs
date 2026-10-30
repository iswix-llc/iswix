using System.Text;
using IsWiX.Conversion;
using Xunit;

namespace IsWiXExtension.Tests;

public class ProjectConverterTests : IDisposable
{
    readonly string _root = Path.Combine(Path.GetTempPath(), "iswix-convert-" + Guid.NewGuid().ToString("N"));

    public ProjectConverterTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }

    string P(string relative) => Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));

    void Write(string relative, string text, bool bom = false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(P(relative))!);
        File.WriteAllBytes(P(relative), (bom ? new byte[] { 0xEF, 0xBB, 0xBF } : Array.Empty<byte>()).Concat(Encoding.UTF8.GetBytes(text)).ToArray());
    }

    string Read(string relative) => File.ReadAllText(P(relative));

    ConversionResult Convert(ProjectKind to, string? sdk = null, bool eula = false, IEnumerable<string>? projects = null, bool dryRun = false) =>
        ProjectConverter.Convert(_root, projects, new ConversionOptions { To = to, SdkVersion = sdk, AcceptEula = eula, DryRun = dryRun });

    const string Msi = "<Project Sdk=\"WixToolset.Sdk/7.0.1\">\n  <PropertyGroup>\n    <AcceptEula>wix7</AcceptEula>\n    <OutputType>Package</OutputType>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"WixToolset.UI.wixext\" Version=\"7.0.1\" />\n  </ItemGroup>\n</Project>\n";
    const string Bundle = "<Project Sdk=\"WixToolset.Sdk/7.0.1\">\n  <PropertyGroup>\n    <OutputType>Bundle</OutputType>\n  </PropertyGroup>\n  <ItemGroup>\n    <ProjectReference Include=\"..\\Msi\\Msi.wixproj\" />\n  </ItemGroup>\n</Project>\n";
    const string Sln = "Project(\"{B7DD6F7E-DEF8-4E67-B5B7-07EF123DB6F0}\") = \"Msi\", \"Msi\\Msi.wixproj\", \"{11111111-1111-1111-1111-111111111111}\"\nEndProject\nProject(\"{B7DD6F7E-DEF8-4E67-B5B7-07EF123DB6F0}\") = \"Bundle\", \"Bundle\\Bundle.wixproj\", \"{22222222-2222-2222-2222-222222222222}\"\nEndProject\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"App\", \"App\\App.csproj\", \"{33333333-3333-3333-3333-333333333333}\"\nEndProject\n";
    const string Slnx = "<Solution>\n  <Project Path=\"Msi/Msi.wixproj\" />\n  <Project Path=\"Bundle/Bundle.wixproj\" Type=\"{B7DD6F7E-DEF8-4E67-B5B7-07EF123DB6F0}\" />\n  <Project Path=\"App/App.csproj\" Type=\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\" />\n</Solution>\n";

    void WriteSolution()
    {
        Write("Msi/Msi.wixproj", Msi);
        Write("Bundle/Bundle.wixproj", Bundle);
        Write("App/App.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"../Msi/Msi.wixproj\" />\n  </ItemGroup>\n</Project>\n");
        Write("Setup.sln", Sln);
        Write("Setup.slnx", Slnx);
    }

    [Fact]
    public void Wix_projects_become_IsWiX_projects()
    {
        WriteSolution();

        var result = Convert(ProjectKind.IsWiX);

        Assert.Equal(2, result.Renamed.Count);
        Assert.False(File.Exists(P("Msi/Msi.wixproj")));
        Assert.Contains("<DefaultProjectTypeGuid>{3A7B4E67-3CBB-402A-AC31-8EF9DCCEF68E}</DefaultProjectTypeGuid>", Read("Msi/Msi.iswixproj"));
        Assert.Contains("Include=\"..\\Msi\\Msi.iswixproj\"", Read("Bundle/Bundle.iswixproj"));
        Assert.Contains("Include=\"../Msi/Msi.iswixproj\"", Read("App/App.csproj"));
        Assert.Contains("Project(\"{3A7B4E67-3CBB-402A-AC31-8EF9DCCEF68E}\") = \"Msi\", \"Msi\\Msi.iswixproj\"", Read("Setup.sln"));
        Assert.Contains("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"App\", \"App\\App.csproj\"", Read("Setup.sln"));
        Assert.Contains("<Project Path=\"Bundle/Bundle.iswixproj\" />", Read("Setup.slnx"));
        Assert.Contains("Type=\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\"", Read("Setup.slnx"));
        // The SDK is left alone unless asked.
        Assert.Contains("WixToolset.Sdk/7.0.1", Read("Msi/Msi.iswixproj"));
        Assert.Contains("<AcceptEula>wix7</AcceptEula>", Read("Msi/Msi.iswixproj"));
    }

    [Fact]
    public void Converting_there_and_back_restores_the_files()
    {
        WriteSolution();

        Convert(ProjectKind.IsWiX);
        Convert(ProjectKind.Wix);

        Assert.Equal(Msi, Read("Msi/Msi.wixproj"));
        Assert.Equal(Bundle, Read("Bundle/Bundle.wixproj"));
        Assert.Equal(Sln, Read("Setup.sln"));
        Assert.False(File.Exists(P("Msi/Msi.iswixproj")));
        Assert.Contains("<Project Path=\"Bundle/Bundle.wixproj\" />", Read("Setup.slnx"));
    }

    [Fact]
    public void Converting_there_and_back_with_the_SDKs_restores_the_projects()
    {
        WriteSolution();

        Convert(ProjectKind.IsWiX, sdk: "7.0.1");
        Convert(ProjectKind.Wix, sdk: "7.0.1", eula: true);

        Assert.Equal(Msi, Read("Msi/Msi.wixproj"));
        Assert.Equal(Bundle.Replace("<OutputType>", "<AcceptEula>wix7</AcceptEula>\n    <OutputType>"), Read("Bundle/Bundle.wixproj"));
    }

    [Fact]
    public void A_group_IsWiX_added_goes_when_converting_back()
    {
        const string Original = "<Project Sdk=\"WixToolset.Sdk/5.0.2\">\n  <PropertyGroup Condition=\"x\">\n    <A>1</A>\n  </PropertyGroup>\n</Project>\n";
        Write("Pkg/Pkg.wixproj", Original);

        Convert(ProjectKind.IsWiX);
        Convert(ProjectKind.Wix);

        Assert.Equal(Original, Read("Pkg/Pkg.wixproj"));
    }

    [Fact]
    public void The_EULA_goes_into_an_empty_group_indented()
    {
        Write("Pkg/Pkg.iswixproj", "<Project Sdk=\"IsWiX.Sdk/7.0.1\">\n  <PropertyGroup>\n  </PropertyGroup>\n</Project>\n");

        Convert(ProjectKind.Wix, sdk: "7.0.1", eula: true);

        Assert.Equal("<Project Sdk=\"WixToolset.Sdk/7.0.1\">\n  <PropertyGroup>\n    <AcceptEula>wix7</AcceptEula>\n  </PropertyGroup>\n</Project>\n", Read("Pkg/Pkg.wixproj"));
    }

    [Fact]
    public void IsWiX_template_projects_lose_their_project_type_and_its_comment()
    {
        Write("Pkg/Pkg.iswixproj", "<Project Sdk=\"IsWiX.Sdk/7.0.1\">\r\n\t<PropertyGroup>\r\n\t\t<!-- The IsWiX project type, which \"dotnet sln add\" writes into solutions (the WiX SDK would give HeatWave's). -->\r\n\t\t<DefaultProjectTypeGuid>{3A7B4E67-3CBB-402A-AC31-8EF9DCCEF68E}</DefaultProjectTypeGuid>\r\n\t\t<OutputType>Package</OutputType>\r\n\t</PropertyGroup>\r\n</Project>\r\n");

        Convert(ProjectKind.Wix);

        Assert.Equal("<Project Sdk=\"IsWiX.Sdk/7.0.1\">\r\n\t<PropertyGroup>\r\n\t\t<OutputType>Package</OutputType>\r\n\t</PropertyGroup>\r\n</Project>\r\n", Read("Pkg/Pkg.wixproj"));
    }

    [Fact]
    public void Moving_to_IsWiX_Sdk_moves_the_extensions_and_drops_the_EULA()
    {
        WriteSolution();

        Convert(ProjectKind.IsWiX, sdk: "7.0.1");

        var msi = Read("Msi/Msi.iswixproj");
        Assert.Contains("Sdk=\"IsWiX.Sdk/7.0.1\"", msi);
        Assert.Contains("<PackageReference Include=\"IsWiX.UI.wixext\" Version=\"7.0.1\" />", msi);
        Assert.DoesNotContain("AcceptEula", msi);
    }

    [Fact]
    public void Moving_to_WixToolset_Sdk_7_needs_the_EULA_accepted()
    {
        Write("Pkg/Pkg.iswixproj", "<Project Sdk=\"IsWiX.Sdk/7.0.1\">\n  <PropertyGroup>\n    <OutputType>Package</OutputType>\n  </PropertyGroup>\n</Project>\n");

        var refused = Assert.Throws<ConversionException>(() => Convert(ProjectKind.Wix, sdk: "7.0.1"));
        Assert.Contains("Open Source Maintenance Fee", refused.Message);
        Assert.True(File.Exists(P("Pkg/Pkg.iswixproj")));

        Convert(ProjectKind.Wix, sdk: "7.0.1", eula: true);

        var pkg = Read("Pkg/Pkg.wixproj");
        Assert.Contains("Sdk=\"WixToolset.Sdk/7.0.1\"", pkg);
        Assert.Contains("<PropertyGroup>\n    <AcceptEula>wix7</AcceptEula>\n    <OutputType>Package</OutputType>", pkg);
    }

    [Fact]
    public void The_Bal_extension_moves_to_IsWiXs_BootstrapperApplications_extension()
    {
        Write("Bundle/Bundle.wixproj", "<Project Sdk=\"WixToolset.Sdk/7.0.1\">\n  <ItemGroup>\n    <PackageReference Include=\"WixToolset.Bal.wixext\" Version=\"7.0.1\" />\n    <PackageReference Include=\"WixToolset.BootstrapperApplications.wixext\" Version=\"7.0.1\" />\n  </ItemGroup>\n</Project>\n");

        Convert(ProjectKind.IsWiX, sdk: "7.0.1");

        var bundle = Read("Bundle/Bundle.iswixproj");
        Assert.DoesNotContain("Bal.wixext", bundle);
        Assert.Contains("<PackageReference Include=\"IsWiX.BootstrapperApplications.wixext\" Version=\"7.0.1\" />", bundle);

        Convert(ProjectKind.Wix, sdk: "7.0.1", eula: true);

        Assert.Contains("<PackageReference Include=\"WixToolset.BootstrapperApplications.wixext\" Version=\"7.0.1\" />", Read("Bundle/Bundle.wixproj"));
    }

    [Fact]
    public void Moving_to_WixToolset_Sdk_5_needs_no_EULA()
    {
        Write("Pkg/Pkg.iswixproj", "<Project Sdk=\"IsWiX.Sdk/7.0.1\">\n  <ItemGroup>\n    <PackageReference Include=\"IsWiX.Bal.wixext\" Version=\"7.0.1\"></PackageReference>\n  </ItemGroup>\n</Project>\n");

        Convert(ProjectKind.Wix, sdk: "5.0.2");

        var pkg = Read("Pkg/Pkg.wixproj");
        Assert.Contains("Sdk=\"WixToolset.Sdk/5.0.2\"", pkg);
        Assert.Contains("<PackageReference Include=\"WixToolset.Bal.wixext\" Version=\"5.0.2\"></PackageReference>", pkg);
        Assert.DoesNotContain("AcceptEula", pkg);
    }

    [Fact]
    public void A_project_without_a_plain_PropertyGroup_gets_one()
    {
        Write("Pkg/Pkg.wixproj", "<Project Sdk=\"WixToolset.Sdk/5.0.2\">\n  <PropertyGroup Condition=\"x\">\n    <A>1</A>\n  </PropertyGroup>\n</Project>\n");

        Convert(ProjectKind.IsWiX);

        Assert.Equal("<Project Sdk=\"WixToolset.Sdk/5.0.2\">\n  <PropertyGroup>\n    <DefaultProjectTypeGuid>{3A7B4E67-3CBB-402A-AC31-8EF9DCCEF68E}</DefaultProjectTypeGuid>\n  </PropertyGroup>\n  <PropertyGroup Condition=\"x\">\n    <A>1</A>\n  </PropertyGroup>\n</Project>\n", Read("Pkg/Pkg.iswixproj"));
    }

    [Fact]
    public void Files_keep_their_byte_order_mark_and_line_endings()
    {
        Write("Pkg/Pkg.wixproj", "<Project Sdk=\"WixToolset.Sdk/5.0.2\">\r\n  <PropertyGroup>\r\n    <OutputType>Package</OutputType>\r\n  </PropertyGroup>\r\n</Project>\r\n", bom: true);

        Convert(ProjectKind.IsWiX);

        var bytes = File.ReadAllBytes(P("Pkg/Pkg.iswixproj"));
        Assert.True(bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
        var text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        Assert.DoesNotContain("\n", text.Replace("\r\n", ""));
        Assert.Contains("<PropertyGroup>\r\n    <DefaultProjectTypeGuid>", text);
    }

    [Fact]
    public void Only_the_projects_named_are_converted()
    {
        WriteSolution();

        var result = Convert(ProjectKind.IsWiX, projects: new[] { P("Msi/Msi.wixproj") });

        Assert.Single(result.Renamed);
        Assert.True(File.Exists(P("Bundle/Bundle.wixproj")));
        Assert.Contains("Include=\"..\\Msi\\Msi.iswixproj\"", Read("Bundle/Bundle.wixproj"));
        Assert.Contains("\"Bundle\\Bundle.wixproj\"", Read("Setup.sln"));
    }

    [Fact]
    public void A_dry_run_changes_nothing()
    {
        WriteSolution();

        var result = Convert(ProjectKind.IsWiX, dryRun: true);

        Assert.Equal(2, result.Renamed.Count);
        Assert.Equal(5, result.Updated.Count);
        Assert.Equal(Msi, Read("Msi/Msi.wixproj"));
        Assert.Equal(Sln, Read("Setup.sln"));
    }

    [Fact]
    public void An_existing_target_stops_the_conversion_before_anything_changes()
    {
        WriteSolution();
        Write("Bundle/Bundle.iswixproj", "<Project />");

        Assert.Throws<ConversionException>(() => Convert(ProjectKind.IsWiX));

        Assert.Equal(Msi, Read("Msi/Msi.wixproj"));
        Assert.Equal(Sln, Read("Setup.sln"));
    }

    [Fact]
    public void A_projects_user_file_goes_with_it()
    {
        Write("Pkg/Pkg.wixproj", "<Project />");
        Write("Pkg/Pkg.wixproj.user", "<Project><PropertyGroup><ShowAllFiles>true</ShowAllFiles></PropertyGroup></Project>");

        Convert(ProjectKind.IsWiX);

        Assert.True(File.Exists(P("Pkg/Pkg.iswixproj.user")));
        Assert.False(File.Exists(P("Pkg/Pkg.wixproj.user")));
    }

    [Fact]
    public void Build_output_folders_are_left_alone()
    {
        Write("Pkg/Pkg.wixproj", "<Project />");
        Write("Pkg/obj/Stale.wixproj", "<Project />");

        var result = Convert(ProjectKind.IsWiX);

        Assert.Single(result.Renamed);
        Assert.True(File.Exists(P("Pkg/obj/Stale.wixproj")));
    }

    [Fact]
    public void References_to_projects_not_converted_are_left_alone()
    {
        Write("Pkg/Pkg.wixproj", "<Project>\n  <ItemGroup>\n    <ProjectReference Include=\"..\\..\\Elsewhere\\Other.wixproj\" />\n  </ItemGroup>\n</Project>\n");

        Convert(ProjectKind.IsWiX);

        Assert.Contains("Elsewhere\\Other.wixproj", Read("Pkg/Pkg.iswixproj"));
    }
}
