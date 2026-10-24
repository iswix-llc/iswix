using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using IsWiX.Designer;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace IsWiX
{
    /// <summary>
    /// Tools > Open in IsWiX Designer (Preview): opens a .wxs in the in-Visual Studio designer. The file is the one
    /// passed as an argument (<c>IsWiX.OpenDesigner path</c> in the Command Window), else the one selected in
    /// Solution Explorer, else the active document.
    /// </summary>
    internal sealed class CommandOpenDesigner
    {
        public const int CommandId = 0x0101;

        readonly AsyncPackage _package;
        readonly DTE _dte;

        CommandOpenDesigner(AsyncPackage package, OleMenuCommandService commandService, DTE dte)
        {
            _package = package;
            _dte = dte;
            var command = new OleMenuCommand(Execute, new CommandID(CommandLaunchIsWiX.CommandSet, CommandId))
            {
                ParametersDescription = "$",
            };
            commandService.AddCommand(command);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = (OleMenuCommandService)(await package.GetServiceAsync(typeof(IMenuCommandService)))!;
            var dte = (DTE)(await package.GetServiceAsync(typeof(DTE)))!;
            _ = new CommandOpenDesigner(package, commandService, dte);
        }

        void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var path = (e as OleMenuCmdEventArgs)?.InValue as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = SelectedOrActiveFile();
            }

            path = path?.Trim().Trim('"');
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                VsShellUtilities.ShowMessageBox(_package, "Select a .wxs file in Solution Explorer, or open one, first.", "IsWiX",
                    OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            VsShellUtilities.OpenDocumentWithSpecificEditor(_package, Path.GetFullPath(path), new Guid(WixDesignerEditorFactory.GuidString),
                VSConstants.LOGVIEWID_Primary, out _, out _, out var frame);
            ErrorHandler.ThrowOnFailure(frame.Show());
        }

        string? SelectedOrActiveFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (SelectedItem item in _dte.SelectedItems)
            {
                if (item.ProjectItem is { } projectItem && projectItem.FileCount > 0)
                {
                    return projectItem.FileNames[1];
                }
            }

            return _dte.ActiveDocument?.FullName;
        }
    }
}
