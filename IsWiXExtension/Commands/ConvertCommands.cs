// Copyright (c) ISWIX LLC. Licensed under the MS-PL.

using IsWiX.Conversion;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

namespace IsWiX.Commands
{
    /// <summary>The command IDs in IsWiXCommands.vsct.</summary>
    internal static class CommandIds
    {
        public static readonly Guid CommandSet = new Guid("f9d8589d-bf0e-4b75-ab03-eb3d9cffdc9e");
        public const int ConvertProjectToIsWiX = 0x0100;
        public const int ConvertProjectToWix = 0x0101;
        public const int ConvertSolutionToIsWiX = 0x0102;
        public const int ConvertSolutionToWix = 0x0103;
    }

    /// <summary>
    /// Convert to IsWiX Project and Convert to WiX Project, on a project (loaded or not) or on the solution. Visual
    /// Studio can't rename a loaded project's file, so they save and close the solution, convert, and open it again.
    /// </summary>
    internal sealed class ConvertCommands
    {
        const string Title = "IsWiX";

        readonly AsyncPackage _package;

        ConvertCommands(AsyncPackage package) => _package = package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            if (!(await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commands)) return;

            var handler = new ConvertCommands(package);
            handler.Add(commands, CommandIds.ConvertProjectToIsWiX, ProjectKind.IsWiX, wholeSolution: false);
            handler.Add(commands, CommandIds.ConvertProjectToWix, ProjectKind.Wix, wholeSolution: false);
            handler.Add(commands, CommandIds.ConvertSolutionToIsWiX, ProjectKind.IsWiX, wholeSolution: true);
            handler.Add(commands, CommandIds.ConvertSolutionToWix, ProjectKind.Wix, wholeSolution: true);
        }

        void Add(OleMenuCommandService commands, int id, ProjectKind to, bool wholeSolution)
        {
            var command = new OleMenuCommand((s, e) => Execute(to, wholeSolution), new CommandID(CommandIds.CommandSet, id));
            command.BeforeQueryStatus += (s, e) => QueryStatus(command, to, wholeSolution);
            commands.AddCommand(command);
        }

        // A project command shows on projects it can convert; a solution command shows on any solution, and says so
        // if there is nothing to convert.
        void QueryStatus(OleMenuCommand command, ProjectKind to, bool wholeSolution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (wholeSolution)
            {
                command.Visible = command.Enabled = true;
                return;
            }

            var project = SelectedProject();
            command.Visible = command.Enabled = project != null && HasExtension(project, ProjectConverter.Extension(ProjectConverter.Other(to)));
        }

        void Execute(ProjectKind to, bool wholeSolution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = Service<SVsSolution, IVsSolution>();
            if (solution == null || ErrorHandler.Failed(solution.GetSolutionInfo(out var solutionFolder, out var solutionFile, out _))
                || string.IsNullOrEmpty(solutionFile))
            {
                Show("Open a solution to convert its projects.", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                return;
            }

            var fromExtension = ProjectConverter.Extension(ProjectConverter.Other(to));
            var projects = wholeSolution
                ? SolutionProjects(solution).Where(p => HasExtension(p, fromExtension)).ToList()
                : new List<string> { SelectedProject() }.Where(p => p != null && HasExtension(p, fromExtension)).Select(p => p!).ToList();
            if (projects.Count == 0)
            {
                Show($"The solution has no {fromExtension} projects to convert.", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                return;
            }

            var what = projects.Count == 1 ? Path.GetFileName(projects[0]) : $"the solution's {projects.Count} {fromExtension} projects";
            var kind = to == ProjectKind.IsWiX
                ? "IsWiX projects (.iswixproj), which IsWiX opens"
                : "WiX projects (.wixproj), which HeatWave opens";
            var confirm = Show(
                $"Convert {what} to {kind}?\n\n" +
                "Visual Studio will save and close the solution, rename the projects and update the project references " +
                "and solutions in its folder, and open it again. The projects keep their SDK; to change it too, use " +
                "Tools\\IsWiXConvert with --sdk.",
                OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL);
            if (confirm != (int)VSConstants.MessageBoxResult.IDOK) return;

            if (ErrorHandler.Failed(solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, null, 0))
                || solution.CloseSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_PromptSave, null, 0) != VSConstants.S_OK)
            {
                return;
            }

            string message;
            var icon = OLEMSGICON.OLEMSGICON_INFO;
            try
            {
                var result = ProjectConverter.Convert(solutionFolder, projects, new ConversionOptions { To = to });
                message = $"Converted {result.Renamed.Count} project(s) to {ProjectConverter.Extension(to)}; updated {result.Updated.Count} file(s).";
                if (to == ProjectKind.Wix)
                    message += "\n\n.wixproj projects open with HeatWave; without it installed, Visual Studio shows them as unloaded.";
            }
            catch (ConversionException ex)
            {
                message = ex.Message + "\n\nNothing was changed.";
                icon = OLEMSGICON.OLEMSGICON_WARNING;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                message = "The conversion stopped partway: " + ex.Message + "\n\nCheck the projects and solution in " + solutionFolder + ".";
                icon = OLEMSGICON.OLEMSGICON_CRITICAL;
            }

            solution.OpenSolutionFile((uint)__VSSLNOPENOPTIONS.SLNOPENOPT_Silent, solutionFile);
            Show(message, icon, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }

        // The project file of the project selected in Solution Explorer, loaded or not; null for anything else.
        string? SelectedProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selection = Service<SVsShellMonitorSelection, IVsMonitorSelection>();
            var solution = Service<SVsSolution, IVsSolution>();
            if (selection == null || solution == null) return null;

            var hierarchyPointer = IntPtr.Zero;
            var containerPointer = IntPtr.Zero;
            try
            {
                if (ErrorHandler.Failed(selection.GetCurrentSelection(out hierarchyPointer, out var itemId, out var multiple, out containerPointer))
                    || hierarchyPointer == IntPtr.Zero || multiple != null || itemId != (uint)VSConstants.VSITEMID.Root)
                {
                    return null;
                }

                return Marshal.GetObjectForIUnknown(hierarchyPointer) is IVsHierarchy hierarchy ? ProjectFile(solution, hierarchy) : null;
            }
            finally
            {
                if (hierarchyPointer != IntPtr.Zero) Marshal.Release(hierarchyPointer);
                if (containerPointer != IntPtr.Zero) Marshal.Release(containerPointer);
            }
        }

        // The project files of every project in the solution, loaded or not.
        static IEnumerable<string> SolutionProjects(IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var none = Guid.Empty;
            if (ErrorHandler.Failed(solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLINSOLUTION, ref none, out var projects)) || projects == null)
                yield break;

            var one = new IVsHierarchy[1];
            while (projects.Next(1, one, out var fetched) == VSConstants.S_OK && fetched == 1)
            {
                var file = ProjectFile(solution, one[0]);
                if (file != null) yield return file;
            }
        }

        // A project's file: its unique name in the solution, which is its path relative to the solution's folder.
        static string? ProjectFile(IVsSolution solution, IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (ErrorHandler.Failed(solution.GetUniqueNameOfProject(hierarchy, out var uniqueName)) || string.IsNullOrEmpty(uniqueName)
                || !Path.GetExtension(uniqueName).EndsWith("proj", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (Path.IsPathRooted(uniqueName)) return uniqueName;
            return ErrorHandler.Succeeded(solution.GetSolutionInfo(out var folder, out _, out _)) && !string.IsNullOrEmpty(folder)
                ? Path.GetFullPath(Path.Combine(folder, uniqueName))
                : null;
        }

        static bool HasExtension(string path, string extension) =>
            string.Equals(Path.GetExtension(path), extension, StringComparison.OrdinalIgnoreCase);

        TInterface? Service<TService, TInterface>() where TInterface : class
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IServiceProvider)_package).GetService(typeof(TService)) as TInterface;
        }

        int Show(string message, OLEMSGICON icon, OLEMSGBUTTON buttons) =>
            VsShellUtilities.ShowMessageBox(_package, message, Title, icon, buttons, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
}
