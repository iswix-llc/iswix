using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FilesAndFoldersDesigner;

namespace RegistryDesigner
{
    // The source side: this computer's registry, read-only, above the destination. Keys and values are dragged from here
    // into the destination tree.
    public partial class Registry
    {
        const string DragFormat = "IsWiXRegistrySource";

        // What a source node stands for: a root with a key under it (empty for the root itself).
        sealed class SourceKey
        {
            public SourceKey(string root, string path)
            {
                Root = root;
                Path = path;
            }

            public string Root { get; }

            public string Path { get; }
        }

        // What is being dragged out of the source: a key and what is below it, or values of a key.
        sealed class SourceDrag
        {
            public string Kind { get; set; } = "keys";

            public string Root { get; set; } = string.Empty;

            public string Path { get; set; } = string.Empty;

            public bool View64 { get; set; }

            public List<string> Names { get; } = new List<string>();
        }

        const string Placeholder = "placeholder";

        readonly TreeView _sourceTree = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
        readonly ListView _sourceList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = true, HideSelection = false };
        readonly ComboBox _sourceView = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
        readonly ComboBox _addAs = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
        TreeNode? _hoverNode;
        readonly SplitContainer _sourceSplit = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };

        bool SourceIs64 => _sourceView.SelectedIndex != 1;

        Control BuildSourcePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            _sourceView.Items.AddRange(new object[] { "64-bit registry", "32-bit registry" });
            _sourceView.SelectedIndex = 0;
            _addAs.Items.AddRange(new object[] { "Package default", "32-bit view", "64-bit view" });
            _addAs.SelectedIndex = 0;
            var header = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true };
            header.Controls.Add(new Label { Text = "Source computer's registry:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(6, 6, 6, 0) });
            _sourceView.Margin = new Padding(0, 2, 12, 2);
            header.Controls.Add(_sourceView);
            header.Controls.Add(new Label { Text = "Add dragged entries as:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(6, 6, 6, 0) });
            _addAs.Margin = new Padding(0, 2, 6, 2);
            header.Controls.Add(_addAs);

            _sourceSplit.Panel1.Controls.Add(_sourceTree);
            _sourceSplit.Panel2.Controls.Add(_sourceList);
            panel.Controls.Add(_sourceSplit);
            panel.Controls.Add(header);

            _sourceList.Columns.Add("Name", 150);
            _sourceList.Columns.Add("Type", 110);
            _sourceList.Columns.Add("Data", 260);
            _sourceTree.ImageList = _images;

            _sourceView.SelectedIndexChanged += (s, e) =>
            {
                FillSourceRoots();
            };
            _sourceTree.BeforeExpand += (s, e) =>
            {
                if (e.Node != null)
                {
                    LoadSourceChildren(e.Node);
                }
            };
            _sourceTree.AfterSelect += (s, e) =>
            {
                if (e.Node != null)
                {
                    ShowSourceValues(e.Node);
                }
            };
            _sourceTree.ItemDrag += (s, e) =>
            {
                if (e.Item is TreeNode { Tag: SourceKey { Path.Length: > 0 } key })
                {
                    var drag = new SourceDrag { Kind = "keys", Root = key.Root, Path = key.Path, View64 = SourceIs64 };
                    _sourceTree.DoDragDrop(new DataObject(DragFormat, drag), DragDropEffects.Copy);
                }
            };
            _sourceList.ItemDrag += (s, e) =>
            {
                if (_sourceTree.SelectedNode?.Tag is SourceKey { Path.Length: > 0 } key)
                {
                    var drag = new SourceDrag { Kind = "values", Root = key.Root, Path = key.Path, View64 = SourceIs64 };
                    foreach (ListViewItem item in _sourceList.SelectedItems)
                    {
                        if (item.Tag is SourceValue { Entry: not null } value)
                        {
                            drag.Names.Add(value.Name ?? string.Empty);
                        }
                    }

                    if (drag.Names.Count > 0)
                    {
                        _sourceList.DoDragDrop(new DataObject(DragFormat, drag), DragDropEffects.Copy);
                    }
                }
            };

            FillSourceRoots();
            return panel;
        }

        void FillSourceRoots()
        {
            _sourceTree.BeginUpdate();
            try
            {
                _sourceTree.Nodes.Clear();
                _sourceList.Items.Clear();
                var computer = _sourceTree.Nodes.Add("My Computer");
                computer.ImageIndex = ComputerImage;
                computer.SelectedImageIndex = ComputerImage;
                foreach (var root in LiveRegistry.Roots)
                {
                    var node = computer.Nodes.Add(root, root);
                    node.Tag = new SourceKey(root, string.Empty);
                    KeyIcons(node);
                    node.Nodes.Add(new TreeNode(string.Empty) { Tag = Placeholder });
                }

                computer.Expand();
            }
            finally
            {
                _sourceTree.EndUpdate();
            }
        }

        // The subkeys are read when a key is first opened, not all at once.
        void LoadSourceChildren(TreeNode node)
        {
            if (node.Tag is not SourceKey key || node.Nodes.Count != 1 || !Equals(node.Nodes[0].Tag, Placeholder))
            {
                return;
            }

            node.Nodes.Clear();
            var names = LiveRegistry.SubKeys(key.Root, key.Path, SourceIs64, out var error);
            if (error != null)
            {
                node.ForeColor = SystemColors.GrayText;
                node.ToolTipText = "This key can't be read: " + error;
                return;
            }

            foreach (var name in names)
            {
                var child = node.Nodes.Add(name);
                child.Tag = new SourceKey(key.Root, key.Path.Length == 0 ? name : key.Path + "\\" + name);
                KeyIcons(child);
                child.Nodes.Add(new TreeNode(string.Empty) { Tag = Placeholder });
            }
        }

        void ShowSourceValues(TreeNode node)
        {
            _sourceList.BeginUpdate();
            try
            {
                _sourceList.Items.Clear();
                if (node.Tag is not SourceKey key || key.Path.Length == 0)
                {
                    return;
                }

                var values = LiveRegistry.Values(key.Root, key.Path, SourceIs64, out var error);
                if (error != null)
                {
                    var item = new ListViewItem("(cannot be read)") { ForeColor = SystemColors.GrayText };
                    item.SubItems.Add(string.Empty);
                    item.SubItems.Add(error);
                    _sourceList.Items.Add(item);
                    return;
                }

                foreach (var value in values)
                {
                    var item = new ListViewItem(value.Name ?? "(Default)") { Tag = value };
                    item.SubItems.Add(value.Type);
                    item.SubItems.Add(value.Display);
                    if (value.Entry == null)
                    {
                        // Shown, but it can't be added.
                        item.ForeColor = SystemColors.GrayText;
                        item.ToolTipText = value.NotSupported ?? string.Empty;
                    }

                    _sourceList.Items.Add(item);
                }
            }
            finally
            {
                _sourceList.EndUpdate();
            }
        }

        // ---------------------------------------------------------------- dropping on the destination

        void EnableDrops()
        {
            _tree.AllowDrop = true;
            _tree.DragEnter += (s, e) => OnSourceDragOver(e);
            _tree.DragOver += (s, e) => OnSourceDragOver(e);
            _tree.DragLeave += (s, e) => ClearHover();
            _tree.DragDrop += (s, e) =>
            {
                ClearHover();
                if (e.Data?.GetData(DragFormat) is SourceDrag drag)
                {
                    DropSource(drag);
                }
            };
        }

        // What is dropped keeps its fully qualified path (the same root and key it has in the source), wherever on the tree it lands.
        void OnSourceDragOver(DragEventArgs e)
        {
            e.Effect = e.Data?.GetDataPresent(DragFormat) == true ? DragDropEffects.Copy : DragDropEffects.None;
        }

        void ClearHover()
        {
            if (_hoverNode != null)
            {
                _hoverNode.BackColor = Color.Empty;
                _hoverNode.ForeColor = Color.Empty;
                _hoverNode = null;
            }
        }

        // Reads what was dragged and adds it under the same root and key path it has in the source, the same way as importing a .reg file.
        void DropSource(SourceDrag drag)
        {
            if (_registry == null)
            {
                return;
            }

            RegImport import;
            bool tooMany;
            var previous = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                import = LiveRegistry.Read(drag.Root, drag.Path, drag.View64, drag.Kind == "values" ? drag.Names : null, drag.Kind == "keys", out tooMany);
            }
            finally
            {
                Cursor = previous;
            }

            if (tooMany)
            {
                ThemedMessageBox.Show("That key has more than " + LiveRegistry.Limit + " values, which is too many to add at once. Drag a smaller key or some of its subkeys.", "Add from the Registry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (import.Entries.Count == 0)
            {
                ThemedMessageBox.Show("There is nothing there that can be added." + SkippedText(import.Skipped), "Add from the Registry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The registry view the new entries are written in: the package's own unless asked otherwise.
            var bitness = _addAs.SelectedIndex == 1 ? "always32" : _addAs.SelectedIndex == 2 ? "always64" : "default";
            string? directory = null;
            if (import.Entries.Any(entry => _registry.NeedsDirectory(RegistryDocument.ScopeOf(entry.Root), bitness)))
            {
                directory = Dialogs.PickDirectory(_registry.GetKnownDirectories(), _registry.SuggestDirectory(import.Entries.Where(entry => _registry.NeedsDirectory(RegistryDocument.ScopeOf(entry.Root), bitness)).Select(entry => RegistryDocument.ScopeOf(entry.Root))));
                if (directory == null)
                {
                    return;
                }
            }

            ImportResult result;
            try
            {
                result = _registry.ImportReg(import, null, directory, bitness);
            }
            catch (ArgumentException ex)
            {
                ThemedMessageBox.Show(ex.Message, "Add from the Registry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Reload(new KeyRef(drag.Root, drag.Path), null, null);
            RaiseSelectionChanged();
            if (result.AlreadyThere > 0 || import.Skipped.Count > 0)
            {
                var report = new StringBuilder("Added " + result.Added + (result.Added == 1 ? " entry." : " entries."));
                if (result.AlreadyThere > 0)
                {
                    report.Append("\r\n").Append(result.AlreadyThere).Append(result.AlreadyThere == 1 ? " value was" : " values were").Append(" already there and left as they are.");
                }

                report.Append(SkippedText(import.Skipped));
                ThemedMessageBox.Show(report.ToString(), "Add from the Registry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        static string SkippedText(List<string> skipped)
        {
            if (skipped.Count == 0)
            {
                return string.Empty;
            }

            var text = new StringBuilder("\r\n\r\nLeft out (" + skipped.Count + "):");
            foreach (var line in skipped.Take(6))
            {
                text.Append("\r\n- ").Append(line);
            }

            return skipped.Count > 6 ? text.Append("\r\n...").ToString() : text.ToString();
        }
    }
}
