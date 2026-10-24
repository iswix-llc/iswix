using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace IsWiX.Designer
{
    /// <summary>
    /// Opens a .wxs file in the IsWiX designer (preview), offered under Open With. The designer edits Visual Studio's
    /// in-memory copy of the file, a text buffer it shares with the XML editor, so undo, the unsaved-changes marker,
    /// save and source control all work as they do for text.
    /// </summary>
    [Guid(GuidString)]
    internal sealed class WixDesignerEditorFactory : IVsEditorFactory
    {
        public const string GuidString = "2c930e52-76a4-4f4d-b21d-e231228d788c";

        readonly AsyncPackage _package;
        IOleServiceProvider? _site;

        public WixDesignerEditorFactory(AsyncPackage package)
        {
            _package = package;
        }

        public int SetSite(IOleServiceProvider psp)
        {
            _site = psp;
            return VSConstants.S_OK;
        }

        public int Close() => VSConstants.S_OK;

        public int MapLogicalView(ref Guid rguidLogicalView, out string? pbstrPhysicalView)
        {
            pbstrPhysicalView = null;
            // One view: the designer, which is also what the primary view means for this editor.
            return rguidLogicalView == VSConstants.LOGVIEWID_Primary || rguidLogicalView == VSConstants.LOGVIEWID_Designer
                ? VSConstants.S_OK
                : VSConstants.E_NOTIMPL;
        }

        public int CreateEditorInstance(
            uint grfCreateDoc,
            string pszMkDocument,
            string pszPhysicalView,
            IVsHierarchy pvHier,
            uint itemid,
            IntPtr punkDocDataExisting,
            out IntPtr ppunkDocView,
            out IntPtr ppunkDocData,
            out string pbstrEditorCaption,
            out Guid pguidCmdUI,
            out int pgrfCDW)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pbstrEditorCaption = string.Empty;
            pguidCmdUI = GetType().GUID;
            pgrfCDW = 0;

            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            IVsTextLines textLines;
            if (punkDocDataExisting != IntPtr.Zero)
            {
                // The file is already open, usually in the XML editor: share its buffer, or ask Visual Studio to
                // close the other editor first if it holds the document some other way.
                if (Marshal.GetObjectForIUnknown(punkDocDataExisting) is not IVsTextLines existing)
                {
                    return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
                }

                textLines = existing;
            }
            else
            {
                // A new buffer, which Visual Studio loads the file into after this returns.
                textLines = CreateTextBuffer();
            }

            var pane = new WixDesignerPane(_package, textLines, pszMkDocument);
            ppunkDocView = Marshal.GetIUnknownForObject(pane);
            ppunkDocData = Marshal.GetIUnknownForObject(textLines);
            pbstrEditorCaption = " [IsWiX]";
            return VSConstants.S_OK;
        }

        IVsTextLines CreateTextBuffer()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var registry = ((IServiceProvider)_package).GetService(typeof(SLocalRegistry)) as ILocalRegistry
                ?? throw new InvalidOperationException("Visual Studio's local registry service is not available.");
            var clsid = typeof(VsTextBufferClass).GUID;
            var iid = typeof(IVsTextLines).GUID;
            ErrorHandler.ThrowOnFailure(registry.CreateInstance(clsid, null, ref iid, (uint)CLSCTX.CLSCTX_INPROC_SERVER, out var unknown));
            try
            {
                var textLines = (IVsTextLines)Marshal.GetObjectForIUnknown(unknown);
                if (textLines is IObjectWithSite withSite && _site != null)
                {
                    withSite.SetSite(_site);
                }

                return textLines;
            }
            finally
            {
                Marshal.Release(unknown);
            }
        }
    }
}
