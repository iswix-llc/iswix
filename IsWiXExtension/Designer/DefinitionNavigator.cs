using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace IsWiX.Designer
{
    /// <summary>
    /// Opens the file that defines something (a component group, say) and selects it there: a designer's Go to Definition,
    /// and F12 on a reference in the XML.
    /// </summary>
    internal static class DefinitionNavigator
    {
        /// <summary>
        /// Opens <paramref name="path"/> in Visual Studio and selects the element with the given name (without its
        /// namespace) and Id. Returns false if the file couldn't be opened.
        /// </summary>
        public static bool GoTo(string path, string elementName, string id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!File.Exists(path))
            {
                Tracing.Write("GoTo", () => $"{path} does not exist");
                return false;
            }

            try
            {
                Tracing.Write("GoTo", () => $"Opening {path} for {elementName} '{id}'");
                VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, path, VSConstants.LOGVIEWID_Primary, out _, out _, out var frame);
                frame.Show();

                // The IsWiX editor selects it in its XML and its designers; a file opened some other way is a text editor.
                if (ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var docView)) && docView is WixDesignerPane pane)
                {
                    Tracing.Write("GoTo", () => "Opened in the IsWiX editor");
                    pane.ShowDefinition(elementName, id);
                }
                else
                {
                    Tracing.Write("GoTo", () => $"Opened in another editor ({docView?.GetType().Name ?? "no view"}): selecting in its text");
                    SelectInTextView(path, frame, elementName, id);
                }

                return true;
            }
            catch (Exception ex) when (ex is COMException || ex is InvalidOperationException || ex is IOException)
            {
                Tracing.Write("GoTo", () => $"Could not open {path}: {ex.Message}");
                return false;
            }
        }

        // For a file in a plain text editor: the element's start tag, from the file's text.
        static void SelectInTextView(string path, IVsWindowFrame frame, string elementName, string id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var view = VsShellUtilities.GetTextView(frame);
            if (view is null || ErrorHandler.Failed(view.GetBuffer(out var lines)))
            {
                return;
            }

            XmlPositions.Found? found;
            try
            {
                found = XmlPositions.FindElement(File.ReadAllText(path), elementName, "Id", id);
            }
            catch (XmlException)
            {
                return;
            }

            if (found is null
                || ErrorHandler.Failed(lines.GetLineIndexOfPosition(found.Start, out var startLine, out var startColumn))
                || ErrorHandler.Failed(lines.GetLineIndexOfPosition(found.End, out var endLine, out var endColumn)))
            {
                return;
            }

            view.SetSelection(startLine, startColumn, endLine, endColumn);
            view.EnsureSpanVisible(new TextSpan { iStartLine = startLine, iStartIndex = startColumn, iEndLine = endLine, iEndIndex = endColumn });
            view.SendExplicitFocus();
        }
    }
}
