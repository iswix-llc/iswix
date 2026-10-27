using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace IsWiX.Designer
{
    /// <summary>
    /// Sits in front of the XML editor's own handlers on its text view, so that Go to Definition run from the editor's
    /// context menu reaches the window: that menu's commands go to the view, not through the window the way a key does.
    /// Anything it doesn't take goes on to the editor.
    /// </summary>
    internal sealed class GoToDefinitionFilter : IOleCommandTarget
    {
        readonly WixDesignerPane _pane;

        public GoToDefinitionFilter(WixDesignerPane pane)
        {
            _pane = pane;
        }

        /// <summary>Gets or sets the handler that was first on the view, which gets what this one doesn't take.</summary>
        public IOleCommandTarget? Next { get; set; }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (cCmds == 1 && _pane.CanGoToDefinition(pguidCmdGroup, prgCmds[0].cmdID))
            {
                Tracing.Write("Command", () => "The text view was asked about Go to Definition: enabled");
                prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                return VSConstants.S_OK;
            }

            return Next?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText) ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_pane.TryGoToDefinition(pguidCmdGroup, nCmdID))
            {
                Tracing.Write("Command", () => "The text view ran Go to Definition");
                return VSConstants.S_OK;
            }

            return Next?.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut) ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }
    }
}
