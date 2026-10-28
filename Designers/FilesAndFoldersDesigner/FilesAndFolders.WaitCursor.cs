using System;
using System.Windows.Forms;

namespace Designers.FilesAndFolders
{
    // The hourglass for what takes a while. The operations here run on the user interface thread, so nothing is drawn or
    // answered until they finish; the wait cursor is set before they start, on the designer and what is in it (setting
    // UseWaitCursor updates the cursor at once, which setting Cursor does not do under the mouse). Scopes nest: the
    // cursor goes back when the outermost ends.
    public partial class FilesAndFolders
    {
        int _waitDepth;

        // Use as: using var wait = Waiting();
        private IDisposable Waiting()
        {
            if (_waitDepth++ == 0)
            {
                UseWaitCursor = true;
                Cursor.Current = Cursors.WaitCursor;
            }

            return new WaitScope(this);
        }

        sealed class WaitScope : IDisposable
        {
            FilesAndFolders? _owner;

            public WaitScope(FilesAndFolders owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner is { } owner)
                {
                    _owner = null;
                    if (--owner._waitDepth == 0 && !owner.IsDisposed)
                    {
                        owner.UseWaitCursor = false;
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }
    }
}
