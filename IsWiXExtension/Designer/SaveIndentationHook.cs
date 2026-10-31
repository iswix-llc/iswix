using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace IsWiX.Designer
{
    /// <summary>
    /// Calls back just before Visual Studio saves one file, which is when the file's text is brought to an even
    /// indentation.
    /// </summary>
    internal sealed class SaveIndentationHook : IVsRunningDocTableEvents3, IDisposable
    {
        readonly IVsRunningDocumentTable _table;
        readonly string _filePath;
        readonly Action _beforeSave;
        uint _cookie;

        public SaveIndentationHook(IVsRunningDocumentTable table, string filePath, Action beforeSave)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _table = table;
            _filePath = filePath;
            _beforeSave = beforeSave;
            ErrorHandler.ThrowOnFailure(_table.AdviseRunningDocTableEvents(this, out _cookie));
        }

        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_cookie != 0)
            {
                _table.UnadviseRunningDocTableEvents(_cookie);
                _cookie = 0;
            }
        }

        public int OnBeforeSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_table is IVsRunningDocumentTable4 table && IsThisFile(table.GetDocumentMoniker(docCookie)))
            {
                _beforeSave();
            }

            return VSConstants.S_OK;
        }

        bool IsThisFile(string? moniker)
        {
            if (string.IsNullOrEmpty(moniker))
            {
                return false;
            }

            try
            {
                return string.Equals(Path.GetFullPath(moniker), Path.GetFullPath(_filePath), StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnAfterSave(uint docCookie) => VSConstants.S_OK;

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) => VSConstants.S_OK;

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew) => VSConstants.S_OK;
    }
}
