using System.Xml.Linq;
using FireworksFramework.Services;
using Xunit;

namespace IsWiXExtension.Tests;

public class ProjectComponentGroupsTests : IDisposable
{
    readonly string _root = Path.Combine(Path.GetTempPath(), "iswix-tests-" + Guid.NewGuid().ToString("N"));

    public ProjectComponentGroupsTests()
    {
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, true);
        }
        catch (IOException)
        {
        }
    }

    string Write(string relativePath, string content)
    {
        var path = Path.GetFullPath(Path.Combine(_root, relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    static string Wxs(params string[] groups) =>
        "<Wix xmlns=\"http://wixtoolset.org/schemas/v4/wxs\"><Fragment>" +
        string.Concat(groups.Select(g => $"<ComponentGroup Id=\"{g}\" />")) +
        "</Fragment></Wix>";

    static string[] Names(IEnumerable<string> paths, string root) =>
        paths.Select(p => p.Substring(root.Length).TrimStart('\\', '/').Replace('\\', '/')).ToArray();

    public class ProjectRootTests : ProjectComponentGroupsTests
    {
        [Fact]
        public void The_root_is_the_nearest_folder_above_with_a_project()
        {
            Write("Setup/Setup.wixproj", "<Project />");
            Write("Setup/Product/Features.wxs", Wxs());
            Write("Other.wixproj", "<Project />");

            var root = ProjectComponentGroups.ProjectRoot(Path.Combine(_root, "Setup", "Product", "Features.wxs"));

            Assert.Equal(Path.Combine(_root, "Setup"), root);
        }

        [Fact]
        public void A_file_in_the_projects_own_folder_is_in_that_project()
        {
            Write("Setup.wixproj", "<Project />");

            Assert.Equal(_root, ProjectComponentGroups.ProjectRoot(Path.Combine(_root, "Product.wxs")));
        }

        [Fact]
        public void With_no_project_above_the_root_is_the_files_own_folder()
        {
            var file = Write("Loose/Product.wxs", Wxs());

            // The temporary folder may itself be under a project on some machines; this one is not.
            var root = ProjectComponentGroups.ProjectRoot(file);

            Assert.True(root == Path.Combine(_root, "Loose") || Directory.EnumerateFiles(root, "*.wixproj").Any());
        }
    }

    public class ProjectFilesTests : ProjectComponentGroupsTests
    {
        [Fact]
        public void All_the_wxs_files_under_the_project_are_found()
        {
            Write("Setup.wixproj", "<Project />");
            var main = Write("Product.wxs", Wxs());
            Write("Parts/Files.wxs", Wxs());
            Write("Parts/Deep/More.wxs", Wxs());
            Write("Readme.txt", "no");

            var files = ProjectComponentGroups.ProjectFiles(main);

            Assert.Equal(new[] { "Parts/Deep/More.wxs", "Parts/Files.wxs", "Product.wxs" }, Names(files, _root));
        }

        [Fact]
        public void Build_output_and_hidden_folders_are_not_searched()
        {
            Write("Setup.wixproj", "<Project />");
            var main = Write("Product.wxs", Wxs());
            Write("bin/Debug/Generated.wxs", Wxs());
            Write("obj/Generated.wxs", Wxs());
            Write(".vs/Cache.wxs", Wxs());
            Write("node_modules/x/Y.wxs", Wxs());

            Assert.Equal(new[] { "Product.wxs" }, Names(ProjectComponentGroups.ProjectFiles(main), _root));
        }

        [Fact]
        public void A_file_with_no_project_stands_with_the_files_in_its_folder_only()
        {
            var main = Write("Loose/Product.wxs", Wxs());
            Write("Loose/Beside.wxs", Wxs());
            Write("Loose/Below/Deeper.wxs", Wxs());

            var root = ProjectComponentGroups.ProjectRoot(main);
            if (root != Path.Combine(_root, "Loose"))
            {
                // Under a project after all, which this test is not about.
                return;
            }

            Assert.Equal(new[] { "Loose/Beside.wxs", "Loose/Product.wxs" }, Names(ProjectComponentGroups.ProjectFiles(main), _root));
        }
    }

    public class ScanTests : ProjectComponentGroupsTests
    {
        [Fact]
        public void The_ids_of_the_component_groups_are_read()
        {
            var file = Write("A.wxs", Wxs("One", "Two"));

            Assert.Equal(new[] { "One", "Two" }, ProjectComponentGroups.Scan(file));
        }

        [Fact]
        public void References_and_other_elements_are_not_component_groups()
        {
            var file = Write("A.wxs",
                "<Wix xmlns=\"http://wixtoolset.org/schemas/v4/wxs\"><Package><Feature Id=\"F\"><ComponentGroupRef Id=\"Refd\" /></Feature></Package>" +
                "<Fragment><ComponentGroup Id=\"Real\" /><Component Id=\"C\" /></Fragment></Wix>");

            Assert.Equal(new[] { "Real" }, ProjectComponentGroups.Scan(file));
        }

        [Fact]
        public void A_group_with_no_id_is_skipped()
        {
            var file = Write("A.wxs", "<Wix><Fragment><ComponentGroup /><ComponentGroup Id=\"\" /><ComponentGroup Id=\"Ok\" /></Fragment></Wix>");

            Assert.Equal(new[] { "Ok" }, ProjectComponentGroups.Scan(file));
        }

        [Fact]
        public void A_file_that_is_not_there_has_none()
        {
            Assert.Empty(ProjectComponentGroups.Scan(Path.Combine(_root, "Missing.wxs")));
        }

        [Fact]
        public void A_file_that_is_not_well_formed_gives_what_came_before_the_error()
        {
            var file = Write("A.wxs", "<Wix><Fragment><ComponentGroup Id=\"Before\" /><ComponentGroup Id=\"Broken\"</Fragment></Wix>");

            Assert.Equal(new[] { "Before" }, ProjectComponentGroups.Scan(file));
        }

        [Fact]
        public void An_edited_file_is_read_again()
        {
            var file = Write("A.wxs", Wxs("Old"));
            Assert.Equal(new[] { "Old" }, ProjectComponentGroups.Scan(file));

            File.WriteAllText(file, Wxs("New", "Newer"));
            File.SetLastWriteTimeUtc(file, DateTime.UtcNow.AddMinutes(1));

            Assert.Equal(new[] { "New", "Newer" }, ProjectComponentGroups.Scan(file));
        }

        [Fact]
        public void The_list_returned_is_the_callers_own()
        {
            var file = Write("A.wxs", Wxs("One"));

            ProjectComponentGroups.Scan(file).Add("Changed");

            Assert.Equal(new[] { "One" }, ProjectComponentGroups.Scan(file));
        }

        [Fact]
        public void A_doctype_is_ignored_rather_than_processed()
        {
            var file = Write("A.wxs", "<!DOCTYPE Wix [<!ENTITY e \"x\">]><Wix><ComponentGroup Id=\"G\" /></Wix>");

            Assert.Equal(new[] { "G" }, ProjectComponentGroups.Scan(file));
        }
    }

    public class FindTests : ProjectComponentGroupsTests
    {
        [Fact]
        public void The_other_files_groups_are_found_with_their_files_and_the_open_one_is_left_out()
        {
            Write("Setup.wixproj", "<Project />");
            var main = Write("Product.wxs", Wxs("FromDisk"));
            var other = Write("Parts/Files.wxs", Wxs("Shared1", "Shared2"));

            var groups = ProjectComponentGroups.FindInOtherFiles(main);

            Assert.Equal(new[] { "Shared1", "Shared2" }, groups.Select(g => g.Id));
            Assert.All(groups, g => Assert.Equal(other, g.FilePath));
        }

        [Fact]
        public void The_open_files_groups_come_from_memory_not_disk()
        {
            var main = Write("Product.wxs", Wxs("OnDisk"));
            var inMemory = XDocument.Parse(Wxs("Unsaved"));

            var groups = ProjectComponentGroups.FindInDocument(inMemory, main);

            Assert.Equal(new[] { "Unsaved" }, groups.Select(g => g.Id));
            Assert.All(groups, g => Assert.Equal(main, g.FilePath));
        }

        [Fact]
        public void A_definition_in_the_open_file_is_found_there_first()
        {
            Write("Setup.wixproj", "<Project />");
            var main = Write("Product.wxs", Wxs());
            Write("Other.wxs", Wxs("Both"));
            var inMemory = XDocument.Parse(Wxs("Both"));

            Assert.Equal(main, ProjectComponentGroups.FindDefinition(main, inMemory, "Both"));
        }

        [Fact]
        public void A_definition_in_another_file_is_found_there()
        {
            Write("Setup.wixproj", "<Project />");
            var main = Write("Product.wxs", Wxs());
            var other = Write("Parts/Files.wxs", Wxs("Elsewhere"));
            var inMemory = XDocument.Parse(Wxs());

            Assert.Equal(other, ProjectComponentGroups.FindDefinition(main, inMemory, "Elsewhere"));
        }

        [Fact]
        public void A_group_nothing_defines_has_no_file()
        {
            Write("Setup.wixproj", "<Project />");
            var main = Write("Product.wxs", Wxs());
            var inMemory = XDocument.Parse(Wxs());

            Assert.Null(ProjectComponentGroups.FindDefinition(main, inMemory, "Nowhere"));
        }

        [Fact]
        public void A_group_in_a_file_outside_the_project_is_not_found()
        {
            Write("A/Setup.wixproj", "<Project />");
            var main = Write("A/Product.wxs", Wxs());
            Write("B/Other.wxs", Wxs("InAnotherProject"));
            var inMemory = XDocument.Parse(Wxs());

            Assert.Null(ProjectComponentGroups.FindDefinition(main, inMemory, "InAnotherProject"));
        }

        [Fact]
        public void The_groups_of_a_document_are_all_its_component_groups_wherever_they_are()
        {
            var document = XDocument.Parse(
                "<Wix xmlns=\"http://wixtoolset.org/schemas/v4/wxs\"><Fragment><ComponentGroup Id=\"A\" /></Fragment>" +
                "<Fragment><ComponentGroup Id=\"B\"><ComponentGroupRef Id=\"A\" /></ComponentGroup></Fragment></Wix>");

            Assert.Equal(new[] { "A", "B" }, ProjectComponentGroups.Read(document));
        }
    }
}
