using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FireworksFramework.Services
{
    /// <summary>
    /// Asks which file a new item (a service, a shortcut) is made for, from a tree of where the files are installed, as
    /// the Files and Folders designer shows them: Destination Computer, the folders and, in them, the files.
    /// </summary>
    public static class CandidatePicker
    {
        // Positions in DesignerImages.Library.
        const int FolderOpen = 0;
        const int FolderClosed = 1;
        const int Computer = 2;
        const int FileIcon = 4;
        const int BlueFolderClosed = 9;
        const int BlueFolderOpen = 11;

        /// <summary>
        /// Shows <paramref name="candidates"/> in a dialog and returns the one chosen, or null if the dialog was
        /// cancelled. With no candidates it says <paramref name="noneText"/> instead.
        /// </summary>
        /// <param name="candidates">The files to choose from.</param>
        /// <param name="destinationPath">Where a candidate is installed, as <c>[Directory]Subdirectory\file</c>.</param>
        /// <param name="title">The dialog's title.</param>
        /// <param name="prompt">What the user is asked to choose.</param>
        /// <param name="noneText">What is said when there is nothing to choose from.</param>
        /// <param name="icon">The picture for the dialog's title bar, such as the designer's own icon; none for the default.</param>
        public static T? Pick<T>(IEnumerable<T> candidates, Func<T, string> destinationPath, string title, string prompt, string noneText, Image? icon = null)
            where T : class
        {
            var sorted = candidates.OrderBy(destinationPath, StringComparer.OrdinalIgnoreCase).ToList();
            if (sorted.Count == 0)
            {
                MessageBox.Show(noneText, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            using (var dialog = new PickerForm(title, prompt, icon))
            {
                foreach (var candidate in sorted)
                {
                    dialog.Add(destinationPath(candidate), candidate);
                }

                dialog.Prepare();
                return DesignerDialogs.ShowDialog(dialog) == DialogResult.OK ? dialog.Picked as T : null;
            }
        }

        // "[INSTALLLOCATION]bin\App.exe" is the parts "[INSTALLLOCATION]", "bin" and "App.exe".
        static IEnumerable<string> Parts(string path)
        {
            var bracket = path.IndexOf(']');
            if (path.StartsWith("[", StringComparison.Ordinal) && bracket > 0)
            {
                yield return path.Substring(0, bracket + 1);
                path = path.Substring(bracket + 1);
            }

            foreach (var part in path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return part;
            }
        }

        sealed class PickerForm : Form
        {
            readonly TreeView _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false, ShowLines = false, FullRowSelect = true };
            readonly ImageList _images = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
            readonly Button _ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Enabled = false };
            readonly Button _cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            IntPtr _iconHandle;
            readonly TreeNode _computer;
            readonly Dictionary<string, TreeNode> _folders = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            static extern bool DestroyIcon(IntPtr handle);

            // The title bar's icon, drawn from an image; the window keeps using the handle until it is disposed.
            void SetIcon(Image image)
            {
                using (var bitmap = new Bitmap(image, 32, 32))
                {
                    _iconHandle = bitmap.GetHicon();
                }

                Icon = Icon.FromHandle(_iconHandle);
                ShowIcon = true;
            }

            public PickerForm(string title, string prompt, Image? icon)
            {
                Text = title;
                ClientSize = new Size(520, 440);
                MinimumSize = new Size(360, 300);
                StartPosition = FormStartPosition.CenterParent;
                ShowInTaskbar = false;
                MinimizeBox = false;
                MaximizeBox = false;
                AcceptButton = _ok;
                CancelButton = _cancel;

                DesignerImages.AddLibrary(_images);
                _tree.ImageList = _images;
                _tree.TreeViewNodeSorter = new FoldersFirst();
                _tree.AfterSelect += (s, e) => _ok.Enabled = IsCandidate(e.Node);
                _tree.NodeMouseDoubleClick += (s, e) =>
                {
                    if (IsCandidate(e.Node))
                    {
                        DialogResult = DialogResult.OK;
                    }
                };
                _computer = _tree.Nodes.Add("Destination Computer");
                _computer.ImageIndex = _computer.SelectedImageIndex = Computer;

                if (icon != null)
                {
                    SetIcon(icon);
                }

                var label = new Label { Text = prompt, Dock = DockStyle.Top, Height = 34, Padding = new Padding(8, 10, 8, 0) };

                // The buttons sit at the right of a strip that grows to fit them, whatever the font or scaling: the first
                // added is the rightmost.
                var buttons = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    WrapContents = false,
                    Padding = new Padding(8),
                };
                foreach (var button in new[] { _cancel, _ok })
                {
                    button.AutoSize = true;
                    button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    button.MinimumSize = new Size(90, 28);
                    button.Margin = new Padding(6, 0, 0, 0);
                    buttons.Controls.Add(button);
                }

                // The tree fills what the label and the buttons leave, so it is added first.
                Controls.Add(_tree);
                Controls.Add(label);
                Controls.Add(buttons);
            }

            /// <summary>Gets the candidate the selected file is for, or null if a folder is selected.</summary>
            public object? Picked => IsCandidate(_tree.SelectedNode) ? _tree.SelectedNode.Tag : null;

            static bool IsCandidate(TreeNode? node) => node != null && node.Nodes.Count == 0 && node.Tag != null;

            public void Add(string destinationPath, object candidate)
            {
                var parts = Parts(destinationPath).ToList();
                var parent = _computer;
                var key = string.Empty;
                for (var i = 0; i < parts.Count - 1; i++)
                {
                    key += parts[i] + "\\";
                    if (!_folders.TryGetValue(key, out var folder))
                    {
                        folder = parent.Nodes.Add(parts[i]);
                        // The shortcut locations and install folders are the first level, in the blue folders.
                        folder.ImageIndex = i == 0 ? BlueFolderClosed : FolderClosed;
                        folder.SelectedImageIndex = i == 0 ? BlueFolderOpen : FolderOpen;
                        _folders.Add(key, folder);
                    }

                    parent = folder;
                }

                var file = parent.Nodes.Add(parts.Count > 0 ? parts[parts.Count - 1] : destinationPath);
                file.ImageIndex = file.SelectedImageIndex = FileIcon;
                file.Tag = candidate;
            }

            /// <summary>Opens every folder and selects the first file, so there is always something chosen to accept.</summary>
            public void Prepare()
            {
                _tree.ExpandAll();
                var first = FirstFile(_computer);
                _tree.SelectedNode = first ?? _computer;
                first?.EnsureVisible();
            }

            static TreeNode? FirstFile(TreeNode node)
            {
                if (IsCandidate(node))
                {
                    return node;
                }

                foreach (TreeNode child in node.Nodes)
                {
                    if (FirstFile(child) is { } found)
                    {
                        return found;
                    }
                }

                return null;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _images.Dispose();
                }

                base.Dispose(disposing);
                if (_iconHandle != IntPtr.Zero)
                {
                    DestroyIcon(_iconHandle);
                    _iconHandle = IntPtr.Zero;
                }
            }
        }

        // Folders before the files in them, each by name, as the Files and Folders designer lists a folder.
        sealed class FoldersFirst : IComparer
        {
            public int Compare(object? x, object? y)
            {
                var a = (TreeNode)x!;
                var b = (TreeNode)y!;
                var aFile = a.Tag != null;
                var bFile = b.Tag != null;
                return aFile != bFile ? (aFile ? 1 : -1) : string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
