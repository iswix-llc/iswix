using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace IsWiX.Designer
{
    /// <summary>
    /// Hosts a Visual Studio code window, the same window the XML editor opens in, inside WPF. A code window is what
    /// language services attach to: colorizing, IntelliSense and the rest come with it, as they don't for a bare text view.
    /// </summary>
    internal sealed class CodeWindowHost : HwndHost
    {
        readonly IVsCodeWindow _codeWindow;

        public CodeWindowHost(IVsCodeWindow codeWindow)
        {
            _codeWindow = codeWindow;
        }

        /// <summary>Gets the code window's text view, once the window exists.</summary>
        public IVsTextView? TextView
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return ErrorHandler.Succeeded(_codeWindow.GetPrimaryView(out var view)) ? view : null;
            }
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.ThrowOnFailure(((IVsWindowPane)_codeWindow).CreatePaneWindow(hwndParent.Handle, 0, 0, 1, 1, out var hwnd));
            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ((IVsWindowPane)_codeWindow).ClosePane();
        }
    }
}
