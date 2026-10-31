using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using FilesAndFoldersDesigner;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Services;
using static FireworksFramework.Types.Enums;

namespace RegistryDesigner
{
    /// <summary>
    /// The Registry designer: a tree of the keys of one component group, and a grid of the values of the selected key.
    /// What it edits is in <see cref="RegistryDocument"/>; this is only the window. It shares the group with Files and
    /// Folders, whose components it leaves alone (see the document class).
    /// </summary>
    public partial class Registry : UserControl, IFireworksDesigner, ISelectionLinked
    {
        const int ColIcon = 0, ColEntry = 1, ColName = 2, ColType = 3, ColValue = 4, ColAction = 5, ColKeyPath = 6, ColComponent = 7;

        // What the grid shows for the key's default value, which has no name.
        const string DefaultName = "(Default)";

        // What a tree node stands for: a root with a key under it (empty for the root itself).
        sealed class KeyRef
        {
            public KeyRef(string root, string key)
            {
                Root = root;
                Key = key;
            }

            public string Root { get; }

            public string Key { get; }

            public bool Is(string? root, string key) => string.Equals(Root, root, StringComparison.OrdinalIgnoreCase) && string.Equals(Key, key, StringComparison.OrdinalIgnoreCase);
        }

        // An issue in the list at the bottom.
        sealed class IssueEntry
        {
            public IssueEntry(ValidationIssue issue)
            {
                Issue = issue;
            }

            public ValidationIssue Issue { get; }

            public override string ToString()
            {
                var where = Issue.Item != null ? " [" + Describe(Issue.Item) + "]" : string.Empty;
                return (Issue.Severity == ValidationSeverity.Error ? "Error: " : "Warning: ") + Issue.Message + where + (Issue.Fix != null ? " " + Issue.Fix : string.Empty);
            }
        }

        readonly DocumentManager _documentManager;
        RegistryDocument? _registry;
        string? _groupId;
        List<RegistryItem> _items = new List<RegistryItem>();
        List<RegistryKeyInfo> _keys = new List<RegistryKeyInfo>();
        List<RegistryComponentInfo> _components = new List<RegistryComponentInfo>();
        List<ValidationIssue> _issues = new List<ValidationIssue>();
        bool _loading;

        readonly ComboBox _groupBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
        readonly Label _noGroups = new Label { Text = "This file has no component groups. Create one in Files and Folders first.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Visible = false };
        readonly TreeView _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false, LabelEdit = true };
        readonly ImageList _images = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };

        // The shared tree icons (see DesignerImages.Library): an open folder, a closed one, and the computer.
        const int FolderOpenImage = 0, FolderClosedImage = 1, ComputerImage = 2;
        readonly DataGridView _grid = new DataGridView();
        readonly Button _fixButton = new Button { Text = "Fix Components..." };
        readonly ListBox _issueBox = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        readonly SplitContainer _main = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
        // The destination's values above, and the component and issues below.
        readonly SplitContainer _right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        bool _splitsSet;
        // The source registry above, the destination (the tree and the values) below.
        readonly SplitContainer _outer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        ToolStripButton _deleteButton = null!;
        ToolStripDropDownButton _newValueButton = null!;
        readonly ComboBox _filterBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        List<RegistryItem> _viewItems = new List<RegistryItem>();
        List<RegistryKeyInfo> _viewKeys = new List<RegistryKeyInfo>();
        readonly List<string?> _filterComponentIds = new List<string?>();
        FindOptions _find = new FindOptions();
        (string Root, string Key, string? Name, string Where)? _lastFound;
        int _editingRow = -1;

        // The component panel.
        readonly Label _componentName = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
        readonly List<Control> _componentControls = new List<Control>();
        readonly List<Control> _keyControls = new List<Control>();
        readonly ComboBox _directoryBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        readonly ComboBox _viewBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        readonly TextBox _conditionBox = new TextBox();
        readonly CheckBox _reflectionBox = new CheckBox { Text = "Disable registry reflection", AutoSize = true, ThreeState = true };
        RegistryComponentInfo? _panelComponent;

        // The key panel, below the component's.
        readonly Label _keyName = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
        readonly CheckBox _forceCreate = new CheckBox { Text = "Force create on install", AutoSize = true, ThreeState = true };
        readonly CheckBox _forceDelete = new CheckBox { Text = "Force delete on uninstall", AutoSize = true, ThreeState = true };
        readonly Button _keyPermissions = new Button { Text = "Key Permissions...", AutoSize = true };
        KeyRef? _panelKey;

        public Registry(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            BuildLayout();
        }

        // ---------------------------------------------------------------- layout

        void BuildLayout()
        {
            SuspendLayout();

            // The group, above everything.
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true };
            bar.Controls.Add(new Label { Text = "Component Group:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(6, 6, 6, 0) });
            _groupBox.Margin = new Padding(0, 2, 6, 2);
            bar.Controls.Add(_groupBox);
            _groupBox.SelectedIndexChanged += (s, e) =>
            {
                if (!_loading && _groupBox.SelectedItem is string id)
                {
                    _groupId = id;
                    LoadData();
                }
            };

            bar.Controls.Add(new Label { Text = "View filter:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(12, 6, 6, 0) });
            _filterBox.Margin = new Padding(0, 2, 6, 2);
            bar.Controls.Add(_filterBox);
            _filterBox.SelectedIndexChanged += (s, e) =>
            {
                if (!_loading)
                {
                    Reload(null, null, null);
                }
            };

            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            AddButton(toolbar, "New Key", (s, e) => NewKey());
            _newValueButton = new ToolStripDropDownButton("New Value") { DisplayStyle = ToolStripItemDisplayStyle.Text, AutoToolTip = false };
            void AddNew(string text, RegKind? kind, bool isDefault) => _newValueButton.DropDownItems.Add(text, null, (s, e) => NewValue(kind, isDefault));
            AddNew("Value...", null, false);
            _newValueButton.DropDownItems.Add(new ToolStripSeparator());
            AddNew("Default Value", RegKind.String, true);
            AddNew("String Value", RegKind.String, false);
            AddNew("Binary Value", RegKind.Binary, false);
            AddNew("DWORD Value", RegKind.Integer, false);
            AddNew("Multi-String Value...", RegKind.MultiString, false);
            AddNew("Expandable String Value", RegKind.Expandable, false);
            toolbar.Items.Add(_newValueButton);
            AddButton(toolbar, "Remove Entry...", (s, e) => NewRemoval());
            _deleteButton = AddButton(toolbar, "Delete", (s, e) => DeleteSelected());
            _deleteButton.Enabled = false;
            AddButton(toolbar, "Find...", (s, e) => Find());
            toolbar.Items.Add(new ToolStripSeparator());
            var sourceButton = AddButton(toolbar, "Source Registry", (s, e) => { });
            sourceButton.CheckOnClick = true;
            sourceButton.Checked = true;
            sourceButton.Click += (s, e) => _outer.Panel1Collapsed = !sourceButton.Checked;
            AddButton(toolbar, "Import .reg...", (s, e) => ImportReg());
            AddButton(toolbar, "Export .reg...", (s, e) => ExportReg());

            // Keys on the left; on the right the values above, and the component and the issues below.
            _main.Panel1.Controls.Add(_tree);
            var right = _right;
            BuildGrid();
            right.Panel1.Controls.Add(_grid);
            right.Panel2.Controls.Add(BuildBottom());
            _main.Panel2.Controls.Add(right);

            _tree.AfterSelect += (s, e) =>
            {
                if (!_loading)
                {
                    ShowKeyValues();
                    RaiseSelectionChanged();
                }
            };
            _tree.NodeMouseClick += (s, e) =>
            {
                if (e.Node == _tree.SelectedNode)
                {
                    RaiseSelectionChanged();
                }
            };
            DesignerImages.AddLibrary(_images);
            _tree.ImageList = _images;
            _tree.BeforeLabelEdit += (s, e) =>
            {
                // Only a key can be renamed, not the computer or a root.
                if (e.Node?.Tag is not KeyRef { Key.Length: > 0 })
                {
                    e.CancelEdit = true;
                }
            };
            _tree.AfterLabelEdit += (s, e) => OnKeyRenamed(e);
            // A right-click should act on the key under the mouse, which a TreeView doesn't select for it.
            _tree.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && _tree.GetNodeAt(e.Location) is { } node)
                {
                    _tree.SelectedNode = node;
                }
            };
            _tree.KeyDown += (s, e) => OnTreeKey(e);
            _tree.ContextMenuStrip = BuildTreeMenu();

            _outer.Panel1.Controls.Add(BuildSourcePanel());
            _outer.Panel2.Controls.Add(_main);
            EnableDrops();
            Controls.Add(_outer);
            Controls.Add(_noGroups);
            Controls.Add(toolbar);
            Controls.Add(bar);
            ResumeLayout(true);
        }

        static ToolStripButton AddButton(ToolStrip strip, string text, EventHandler click)
        {
            var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, AutoToolTip = false };
            button.Click += click;
            strip.Items.Add(button);
            return button;
        }

        void BuildGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.RowHeadersVisible = false;
            _grid.MultiSelect = true;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _grid.Columns.Add(new DataGridViewImageColumn { HeaderText = string.Empty, Width = 26, SortMode = DataGridViewColumnSortMode.NotSortable, ImageLayout = DataGridViewImageCellLayout.Normal });
            // Narrow columns for what is short, and Value and Component (which have long text) share what is left, so the grid fits
            // the panel whatever its width, rather than scrolling sideways.
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Entry", ReadOnly = true, Width = 78, SortMode = DataGridViewColumnSortMode.NotSortable });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Width = 120, SortMode = DataGridViewColumnSortMode.NotSortable });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Type", Width = 105, SortMode = DataGridViewColumnSortMode.NotSortable });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 60F, MinimumWidth = 90, SortMode = DataGridViewColumnSortMode.NotSortable });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Action", Width = 100, SortMode = DataGridViewColumnSortMode.NotSortable });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "KeyPath", Width = 52, SortMode = DataGridViewColumnSortMode.NotSortable });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Component", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 40F, MinimumWidth = 90, SortMode = DataGridViewColumnSortMode.NotSortable });

            // Integer and binary data show the way regedit shows them (an integer in hex and decimal, bytes as hex pairs), until
            // the cell is edited, when it shows what is stored.
            _grid.CellBeginEdit += (s, e) => _editingRow = e.RowIndex;
            _grid.CellEndEdit += (s, e) => _editingRow = -1;
            _grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != ColValue || e.RowIndex == _editingRow || _grid.Rows[e.RowIndex].Tag is not RegistryItem { Kind: RegistryItemKind.Value } item)
                {
                    return;
                }

                if (item.Type == RegKind.Integer && uint.TryParse(item.Value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var number))
                {
                    e.Value = "0x" + number.ToString("x8", System.Globalization.CultureInfo.InvariantCulture) + " (" + number.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";
                    e.FormattingApplied = true;
                }
                else if (item.Type == RegKind.Binary && RegFile.FromHex(item.Value ?? string.Empty) is { } bytes)
                {
                    e.Value = SourceValues.HexDisplay(bytes);
                    e.FormattingApplied = true;
                }
            };

            // A combo box or a check box takes effect as it is chosen, not when the cell is left.
            _grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (_grid.IsCurrentCellDirty && (_grid.CurrentCell is DataGridViewComboBoxCell || _grid.CurrentCell is DataGridViewCheckBoxCell))
                {
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            _grid.CellValueChanged += (s, e) =>
            {
                if (_loading || e.RowIndex < 0 || _grid.Rows[e.RowIndex].Tag is not RegistryItem item)
                {
                    return;
                }

                // What the row says now, taken before the grid is rebuilt: changing the document redraws the grid, which can't be
                // done from inside its own change event.
                var row = _grid.Rows[e.RowIndex];
                var cells = new string[_grid.Columns.Count];
                for (var c = 0; c < cells.Length; c++)
                {
                    cells[c] = Convert.ToString(row.Cells[c].Value) ?? string.Empty;
                }

                var keyPath = row.Cells[ColKeyPath].Value is true;
                var column = e.ColumnIndex;
                BeginInvoke(new Action(() => ApplyCellChange(item, column, cells, keyPath)));
            };
            _grid.DataError += (s, e) => e.ThrowException = false;
            _grid.SelectionChanged += (s, e) =>
            {
                if (!_loading)
                {
                    ShowComponent();
                    _deleteButton.Enabled = _grid.SelectedRows.Count > 0;
                    RaiseSelectionChanged();
                }
            };
            _grid.CurrentCellChanged += (s, e) =>
            {
                if (!_loading)
                {
                    RaiseSelectionChanged();
                }
            };
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == ColValue && _grid.Rows[e.RowIndex].Tag is RegistryItem { Type: RegKind.MultiString, Kind: RegistryItemKind.Value } item)
                {
                    EditItem(item);
                }
            };
            _grid.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete && !_grid.IsCurrentCellInEditMode)
                {
                    DeleteSelected();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter && e.Control && _grid.CurrentRow?.Tag is RegistryItem item)
                {
                    EditItem(item);
                    e.Handled = true;
                }
                else if (HandleFindKey(e))
                {
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.C && !_grid.IsCurrentCellInEditMode)
                {
                    CopySelected();
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.V && !_grid.IsCurrentCellInEditMode)
                {
                    PasteSelected();
                    e.Handled = true;
                }
            };
            // Right-clicking a row selects it first, so the menu acts on the row under the mouse.
            _grid.CellMouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && !_grid.Rows[e.RowIndex].Selected)
                {
                    _grid.ClearSelection();
                    _grid.Rows[e.RowIndex].Selected = true;
                    _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
                }
            };
            var menu = new ContextMenuStrip();
            var newValue = new ToolStripMenuItem("New Value");
            void AddNewValue(string text, RegKind? kind, bool isDefault) => newValue.DropDownItems.Add(text, null, (s, e) => NewValue(kind, isDefault));
            AddNewValue("Value...", null, false);
            newValue.DropDownItems.Add(new ToolStripSeparator());
            AddNewValue("Default Value", RegKind.String, true);
            AddNewValue("String Value", RegKind.String, false);
            AddNewValue("Binary Value", RegKind.Binary, false);
            AddNewValue("DWORD Value", RegKind.Integer, false);
            AddNewValue("Multi-String Value...", RegKind.MultiString, false);
            AddNewValue("Expandable String Value", RegKind.Expandable, false);
            menu.Items.Add(newValue);
            menu.Items.Add(new ToolStripSeparator());
            var edit = menu.Items.Add("Edit...", null, (s, e) => { if (_grid.CurrentRow?.Tag is RegistryItem item) { EditItem(item); } });
            var valuePermissions = menu.Items.Add("Permissions...", null, (s, e) => { if (_grid.CurrentRow?.Tag is RegistryItem { Kind: RegistryItemKind.Value } item) { EditValuePermissions(item); } });
            var delete = menu.Items.Add("Delete", null, (s, e) => DeleteSelected());
            menu.Opening += (s, e) =>
            {
                // Edit, Permissions and Delete need a row.
                var row = _grid.CurrentRow?.Tag as RegistryItem;
                edit.Enabled = row != null;
                valuePermissions.Enabled = row is { Kind: RegistryItemKind.Value };
                delete.Enabled = _grid.SelectedRows.Count > 0;
            };
            _grid.ContextMenuStrip = menu;
        }

        Control BuildBottom()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var component = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true };
            component.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            component.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            // The component's rows and the key's rows each show only while there is one to show.
            var rowTarget = _componentControls;
            void Row(string text, Control control)
            {
                component.RowCount++;
                var label = new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 6, 3) };
                component.Controls.Add(label, 0, component.RowCount - 1);
                control.Dock = DockStyle.Fill;
                control.Margin = new Padding(0, 3, 6, 3);
                component.Controls.Add(control, 1, component.RowCount - 1);
                rowTarget.Add(label);
                rowTarget.Add(control);
            }

            _componentName.Margin = new Padding(0, 6, 0, 3);
            Row("Component:", _componentName);
            Row("Directory:", _directoryBox);
            _viewBox.Items.AddRange(Labels.Views.Cast<object>().ToArray());
            Row("Registry view:", _viewBox);
            Row("Condition:", _conditionBox);
            component.RowCount++;
            component.Controls.Add(_reflectionBox, 1, component.RowCount - 1);
            _componentControls.Add(_reflectionBox);

            // The selected key's own options.
            rowTarget = _keyControls;
            _keyName.Margin = new Padding(0, 12, 0, 3);
            Row("Key:", _keyName);
            component.RowCount++;
            component.Controls.Add(_forceCreate, 1, component.RowCount - 1);
            component.RowCount++;
            component.Controls.Add(_forceDelete, 1, component.RowCount - 1);
            component.RowCount++;
            component.Controls.Add(_keyPermissions, 1, component.RowCount - 1);
            _keyControls.Add(_forceCreate);
            _keyControls.Add(_forceDelete);
            _keyControls.Add(_keyPermissions);
            _forceCreate.CheckStateChanged += (s, e) => CommitKeyOptions();
            _forceDelete.CheckStateChanged += (s, e) => CommitKeyOptions();
            _keyPermissions.Click += (s, e) => EditKeyPermissions();
            SetKeyPanelEnabled(false);
            SetComponentPanelEnabled(false);

            _directoryBox.Leave += (s, e) => CommitDirectory();
            _directoryBox.SelectionChangeCommitted += (s, e) => BeginInvoke(new Action(CommitDirectory));
            _viewBox.SelectionChangeCommitted += (s, e) => CommitView();
            _conditionBox.Leave += (s, e) => CommitCondition();
            _reflectionBox.CheckStateChanged += (s, e) => CommitReflection();
            SetComponentPanelEnabled(false);

            _issueBox.DoubleClick += (s, e) =>
            {
                if (_issueBox.SelectedItem is IssueEntry entry)
                {
                    SelectIssue(entry.Issue);
                }
            };
            // The problems, with a button that makes the components legal where it can.
            _fixButton.AutoSize = true;
            _fixButton.Dock = DockStyle.Bottom;
            _fixButton.Enabled = false;
            _fixButton.Click += (s, e) => FixComponents();
            var issues = new Panel { Dock = DockStyle.Fill };
            issues.Controls.Add(_issueBox);
            issues.Controls.Add(_fixButton);
            panel.Controls.Add(component, 0, 0);
            panel.Controls.Add(issues, 1, 0);
            return panel;
        }

        // ---------------------------------------------------------------- splitters

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ApplySplits();
        }

        // The control may get its size before it is shown, when the containers inside have not been laid out: once it is shown,
        // after that layout, is when the positions fit.
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && IsHandleCreated)
            {
                BeginInvoke(new Action(ApplySplits));
            }
        }

        // Sets the splitters once the control has its real size: it is created 150 by 150, which these positions don't fit (and
        // a position that doesn't fit is refused). The source above and the destination below take half the height each, and
        // their trees are the same, wide, width. After this the user's own positions are kept.
        void ApplySplits()
        {
            if (_splitsSet || _registry == null || !_registry.HasGroup || _outer.Width < 500 || _outer.Height < 250)
            {
                return;
            }

            _splitsSet = true;
            var tree = Math.Max(360, Math.Min(480, Width / 4));
            SetSplit(_outer, _outer.Height / 2);
            SetSplit(_main, tree);
            SetSplit(_sourceSplit, tree);
            SetSplit(_right, (int)(_right.Height * 0.55));
        }

        static void SetSplit(SplitContainer container, int distance)
        {
            var size = container.Orientation == Orientation.Vertical ? container.Width : container.Height;
            var most = size - container.Panel2MinSize - container.SplitterWidth;
            var least = container.Panel1MinSize;
            if (most < least)
            {
                return;
            }

            try
            {
                container.SplitterDistance = Math.Max(least, Math.Min(distance, most));
            }
            catch (InvalidOperationException)
            {
            }
        }

        // ---------------------------------------------------------------- IFireworksDesigner

        public bool IsValidContext() => _documentManager.Document != null && RegistryDocument.IsValidDocument(_documentManager.Document);

        public void LoadData()
        {
            if (!IsValidContext())
            {
                return;
            }

            _loading = true;
            try
            {
                var wanted = _groupId ?? DefaultGroupId();
                _registry = new RegistryDocument(_documentManager.Document, wanted, () => _documentManager.Batch());
                _groupId = _registry.HasGroup ? _registry.GroupId : null;
                FillGroups();
                _noGroups.Visible = !_registry.HasGroup;
                _outer.Visible = _registry.HasGroup;
            }
            finally
            {
                _loading = false;
            }

            if (IsHandleCreated)
            {
                BeginInvoke(new Action(ApplySplits));
            }

            Reload(null, null, null);
        }

        // The group named after the file, as Files and Folders shows first; else the file's first.
        string? DefaultGroupId()
        {
            var fileName = Path.GetFileNameWithoutExtension(_documentManager.DocumentPath ?? string.Empty);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var probe = new RegistryDocument(_documentManager.Document);
            return probe.GetGroupIds().FirstOrDefault(id => string.Equals(id, fileName, StringComparison.OrdinalIgnoreCase));
        }

        void FillGroups()
        {
            _groupBox.Items.Clear();
            if (_registry == null)
            {
                return;
            }

            foreach (var id in _registry.GetGroupIds())
            {
                _groupBox.Items.Add(id);
            }

            _groupBox.SelectedItem = _registry.HasGroup ? _registry.GroupId : null;
            _groupBox.Enabled = _groupBox.Items.Count > 0;
        }

        public string PluginName => "Registry";

        public string PluginOrder => "iswix_group3_registry";

        public PluginType PluginType => PluginType.Designer;

        public Image PluginImage
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("RegistryDesigner.Registry.ico")!;
                return Image.FromStream(stream);
            }
        }

        // ---------------------------------------------------------------- showing the document

        // Reads the group again and fills the tree, grid, component panel and issues, keeping the selection where it can:
        // on the key, and then the entry whose element it was.
        void Reload(KeyRef? key, XElement? element, XElement? component)
        {
            if (_registry == null || !_registry.HasGroup)
            {
                return;
            }

            _loading = true;
            try
            {
                var selected = key ?? (_tree.SelectedNode?.Tag as KeyRef);
                _items = _registry.GetItems();
                _keys = _registry.GetKeyElements();
                _components = _registry.GetComponents();
                _issues = _registry.Validate();
                FillFilterBox();
                _viewItems = _items.Where(i => PassesFilter(i.Root, i.Component, i.Element)).ToList();
                _viewKeys = _keys.Where(k => PassesFilter(k.Root, k.Component, null)).ToList();
                BuildTree(selected);
                FillIssues();
            }
            finally
            {
                _loading = false;
            }

            ShowKeyValues(element);
            ShowComponent();
        }

        static readonly string[] RootOrder = { "HKCR", "HKCU", "HKLM", "HKU", "HKMU" };

        static void KeyIcons(TreeNode node)
        {
            node.ImageIndex = FolderClosedImage;
            node.SelectedImageIndex = FolderOpenImage;
        }

        // The computer, the roots under it (always, so a key can be added to any of them), and the keys below those.
        void BuildTree(KeyRef? select)
        {
            _tree.BeginUpdate();
            try
            {
                _tree.Nodes.Clear();
                var computer = _tree.Nodes.Add("Destination Computer");
                computer.ImageIndex = ComputerImage;
                computer.SelectedImageIndex = ComputerImage;
                var nodes = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
                foreach (var root in RootOrder)
                {
                    var node = computer.Nodes.Add(root, root);
                    node.Tag = new KeyRef(root, string.Empty);
                    KeyIcons(node);
                    nodes[root] = node;
                }

                var paths = new List<KeyRef>();
                paths.AddRange(_viewItems.Where(i => i.Root != null).Select(i => new KeyRef(i.Root!.ToUpperInvariant(), i.Key)));
                paths.AddRange(_viewKeys.Where(k => k.Root != null).Select(k => new KeyRef(k.Root!.ToUpperInvariant(), k.Key)));
                foreach (var path in paths.OrderBy(p => p.Root, StringComparer.OrdinalIgnoreCase).ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                {
                    if (!nodes.TryGetValue(path.Root, out var parent))
                    {
                        continue;
                    }

                    var built = path.Root;
                    foreach (var part in path.Key.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        built += "\\" + part;
                        if (!nodes.TryGetValue(built, out var child))
                        {
                            child = parent.Nodes.Add(built, part);
                            child.Tag = new KeyRef(path.Root, built.Substring(path.Root.Length + 1));
                            KeyIcons(child);
                            nodes[built] = child;
                        }

                        parent = child;
                    }
                }

                _tree.ExpandAll();
                var target = select == null ? null : FindNode(_tree.Nodes, select);
                var firstUsed = RootOrder.Select(r => nodes[r]).FirstOrDefault(n => n.Nodes.Count > 0);
                _tree.SelectedNode = target ?? firstUsed ?? computer;
            }
            finally
            {
                _tree.EndUpdate();
            }
        }

        static TreeNode? FindNode(TreeNodeCollection nodes, KeyRef key)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is KeyRef candidate && candidate.Is(key.Root, key.Key))
                {
                    return node;
                }

                if (FindNode(node.Nodes, key) is { } found)
                {
                    return found;
                }
            }

            return null;
        }

        // Fills the grid with the selected key's entries, and selects the one of this element if there is one.
        void ShowKeyValues(XElement? select = null)
        {
            var key = _tree.SelectedNode?.Tag as KeyRef;
            _loading = true;
            try
            {
                _grid.Rows.Clear();
                if (key == null)
                {
                    return;
                }

                foreach (var item in _viewItems.Where(i => key.Is(i.Root, i.Key)))
                {
                    AddRow(item);
                }

                _grid.ClearSelection();
                if (select != null)
                {
                    foreach (DataGridViewRow row in _grid.Rows)
                    {
                        if (row.Tag is RegistryItem item && item.Element == select)
                        {
                            row.Selected = true;
                            _grid.CurrentCell = row.Cells[ColName];
                        }
                    }
                }
            }
            finally
            {
                _loading = false;
            }

            _deleteButton.Enabled = _grid.SelectedRows.Count > 0;
            ShowComponent();
            ShowKey();
        }

        void AddRow(RegistryItem item)
        {
            var row = _grid.Rows[_grid.Rows.Add()];
            row.Tag = item;
            var isValue = item.Kind == RegistryItemKind.Value;
            row.Cells[ColIcon].Value = RowIcon(item);
            row.Cells[ColEntry].Value = isValue ? "Value" : item.Kind == RegistryItemKind.RemoveKey ? "Remove key" : "Remove value";
            row.Cells[ColName].Value = item.Kind == RegistryItemKind.RemoveKey ? string.Empty : string.IsNullOrEmpty(item.Name) ? DefaultName : item.Name;
            row.Cells[ColName].ReadOnly = !isValue;

            if (isValue)
            {
                row.Cells[ColType] = Combo(Labels.Kinds, Labels.Of(item.Type));
                var multi = item.Type == RegKind.MultiString;
                row.Cells[ColValue].Value = multi ? string.Join(" | ", item.MultiStrings) : item.Value ?? string.Empty;
                row.Cells[ColValue].ReadOnly = multi;
                row.Cells[ColValue].ToolTipText = multi ? "Double-click to change the strings" : string.Empty;
                row.Cells[ColAction] = Combo(Labels.ValueActions, Labels.ValueActionLabel(item.Action));
                row.Cells[ColKeyPath].Value = item.KeyPath;
            }
            else
            {
                row.Cells[ColType].ReadOnly = true;
                row.Cells[ColValue].ReadOnly = true;
                row.Cells[ColKeyPath].ReadOnly = true;
                row.Cells[ColKeyPath].Value = false;
                if (item.Kind == RegistryItemKind.RemoveKey)
                {
                    row.Cells[ColAction] = Combo(Labels.RemoveActions, Labels.RemoveActionLabel(item.Action));
                }
                else
                {
                    row.Cells[ColAction].ReadOnly = true;
                }
            }

            var choices = _components.Select(c => new ComponentChoice(c.Id, Labels.ComponentLabel(_groupId ?? string.Empty, c))).ToList();
            var componentCell = new DataGridViewComboBoxCell { FlatStyle = FlatStyle.Flat, DataSource = choices, DisplayMember = nameof(ComponentChoice.Label), ValueMember = nameof(ComponentChoice.Id) };
            row.Cells[ColComponent] = componentCell;
            componentCell.Value = (item.Component.Attribute("Id")?.Value);

            var messages = _issues.Where(i => i.Item != null && i.Item.Element == item.Element).Select(i => i.Message).ToList();
            row.ErrorText = string.Join("\n", messages);
        }

        static DataGridViewComboBoxCell Combo(string[] items, string value)
        {
            var cell = new DataGridViewComboBoxCell { FlatStyle = FlatStyle.Flat };
            cell.Items.AddRange(items.Cast<object>().ToArray());
            cell.Value = value;
            return cell;
        }

        // Makes the components legal where that can be done: per-user entries out of components they share with per-machine
        // entries or files, and KeyPaths that conflict.
        void FixComponents()
        {
            if (_registry == null || !_registry.CanMakeLegal())
            {
                return;
            }

            var question = "Make the components legal?\r\n\r\nPer-user (HKCU) entries that share a component with per-machine entries or files move to a component of their own, and registry KeyPaths that conflict are taken off.";
            if (ThemedMessageBox.Show(question, "Fix Components", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            string? directory = null;
            if (_registry.MakeLegalNeedsDirectory())
            {
                directory = Dialogs.PickDirectory(_registry.GetKnownDirectories(), _registry.SuggestDirectory(new[] { RegistryScope.PerUser }));
                if (directory == null)
                {
                    return;
                }
            }

            FixResult result;
            try
            {
                result = _registry.MakeLegal(directory);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ThemedMessageBox.Show(ex.Message, "Fix Components", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Reload(null, null, null);
            RaiseSelectionChanged();
            var report = new StringBuilder();
            report.Append(result.Moved).Append(result.Moved == 1 ? " entry moved" : " entries moved").Append(" to a component of their own; ");
            report.Append(result.KeyPathsCleared).Append(result.KeyPathsCleared == 1 ? " KeyPath" : " KeyPaths").Append(" taken off.");
            foreach (var note in result.Notes)
            {
                report.Append("\r\n\r\n").Append(note);
            }

            ThemedMessageBox.Show(report.ToString(), "Fix Components", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void FillIssues()
        {
            _fixButton.Enabled = _issues.Any(i => i.Code == ValidationCode.MixedScope || i.Code == ValidationCode.MultipleKeyPaths || i.Code == ValidationCode.KeyPathConflict || i.Code == ValidationCode.PerUserNoKeyPath);
            _issueBox.Items.Clear();
            foreach (var issue in _issues.OrderByDescending(i => i.Severity))
            {
                _issueBox.Items.Add(new IssueEntry(issue));
            }
        }

        static string Describe(RegistryItem item) =>
            (item.Root ?? "?") + "\\" + item.Key + (item.Kind == RegistryItemKind.RemoveKey ? string.Empty : "\\" + (string.IsNullOrEmpty(item.Name) ? "(default)" : item.Name));

        void SelectIssue(ValidationIssue issue)
        {
            var item = issue.Item ?? _items.FirstOrDefault(i => i.Component == issue.Component);
            if (item == null)
            {
                return;
            }

            var node = item.Root == null ? null : FindNode(_tree.Nodes, new KeyRef(item.Root.ToUpperInvariant(), item.Key));
            if (node != null)
            {
                _tree.SelectedNode = node;
            }

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is RegistryItem candidate && candidate.Element == item.Element)
                {
                    _grid.ClearSelection();
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[ColName];
                    break;
                }
            }
        }

        // ---------------------------------------------------------------- editing in the grid

        void ApplyCellChange(RegistryItem item, int column, string[] cells, bool keyPath)
        {
            if (_registry == null || IsDisposed)
            {
                return;
            }

            string Text(int c) => cells[c];
            try
            {
                if (column == ColComponent)
                {
                    var id = Text(ColComponent);
                    var target = _components.FirstOrDefault(c => c.Id == id);
                    if (target != null && target.Element != item.Component)
                    {
                        var moved = _registry.MoveToComponent(item, target.Element);
                        Reload(null, moved.Element, null);
                    }

                    return;
                }

                if (item.Kind == RegistryItemKind.RemoveKey && column == ColAction)
                {
                    _registry.SetRemoveAction(item, Labels.RemoveActionOf(Text(ColAction)));
                    Reload(null, item.Element, null);
                    return;
                }

                if (item.Kind != RegistryItemKind.Value)
                {
                    return;
                }

                if (column == ColKeyPath)
                {
                    if (keyPath)
                    {
                        _registry.SetKeyPath(item);
                    }
                    else
                    {
                        _registry.ClearKeyPath(item);
                    }

                    Reload(null, item.Element, null);
                    return;
                }

                var spec = new RegistryValueSpec
                {
                    Root = item.Root ?? "HKLM",
                    Key = item.Key,
                    Name = Text(ColName).Trim().Length == 0 || Text(ColName) == DefaultName ? null : Text(ColName),
                    Type = Labels.KindOf(Text(ColType)),
                    Value = Text(ColValue),
                    Action = Labels.ValueActionOf(Text(ColAction)),
                };
                if (spec.Type == RegKind.MultiString)
                {
                    // Becoming a multi-string keeps what the value was as its one string.
                    spec.MultiStrings.AddRange(item.Type == RegKind.MultiString ? item.MultiStrings : new List<string> { item.Value ?? string.Empty });
                    spec.Value = null;
                }
                else if (item.Type == RegKind.MultiString)
                {
                    spec.Value = item.MultiStrings.FirstOrDefault() ?? string.Empty;
                }

                var updated = _registry.UpdateValue(item, spec);
                Reload(null, updated.Element, null);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ThemedMessageBox.Show(ex.Message, "Registry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Reload(null, item.Element, null);
            }
        }

        // The whole entry, in a dialog: for a multi-string, whose strings don't fit a cell.
        void EditItem(RegistryItem item)
        {
            if (_registry == null || item.Kind != RegistryItemKind.Value)
            {
                return;
            }

            var initial = new RegistryValueSpec { Root = item.Root ?? "HKLM", Key = item.Key, Name = item.Name, Type = item.Type, Value = item.Value, Action = item.Action };
            initial.MultiStrings.AddRange(item.MultiStrings);
            if (Dialogs.EditEntry(EntryMode.EditValue, initial, _groupId ?? string.Empty, _components, _registry.GetReferences()) is not { } result)
            {
                return;
            }

            try
            {
                var updated = _registry.UpdateValue(item, result.Spec);
                Reload(new KeyRef(updated.Root!.ToUpperInvariant(), updated.Key), updated.Element, null);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ThemedMessageBox.Show(ex.Message, "Registry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------------------------------------------------------------- commands

        // The Directory to use for a new component of the group's own for this scope and view, asked for if there is none yet.
        // False if the user cancels.
        bool TryDirectory(string root, string bitness, XElement? component, out string? directory)
        {
            directory = null;
            if (_registry == null)
            {
                return false;
            }

            if (component != null || !_registry.NeedsDirectory(RegistryDocument.ScopeOf(root), bitness))
            {
                return true;
            }

            directory = Dialogs.PickDirectory(_registry.GetKnownDirectories(), _registry.SuggestDirectory(new[] { RegistryDocument.ScopeOf(root) }));
            return directory != null;
        }

        // A new value, of the type the menu item names (a default value has no name to ask for).
        void NewValue(RegKind? kind = null, bool isDefault = false)
        {
            if (_registry == null)
            {
                return;
            }

            var key = _tree.SelectedNode?.Tag as KeyRef;
            var initial = new RegistryValueSpec { Root = key?.Root ?? "HKLM", Key = key?.Key ?? string.Empty, Type = kind ?? RegKind.String };
            if (kind == RegKind.Integer)
            {
                initial.Value = "0";
            }

            if (Dialogs.EditEntry(EntryMode.NewValue, initial, _groupId ?? string.Empty, _components, _registry.GetReferences(), isDefault) is not { } result)
            {
                return;
            }

            var component = result.ComponentId == null ? null : _components.FirstOrDefault(c => c.Id == result.ComponentId)?.Element;
            if (!TryDirectory(result.Spec.Root, result.Bitness, component, out var directory))
            {
                return;
            }

            try
            {
                var added = _registry.AddValue(result.Spec, component, directory, result.Bitness);
                Reload(new KeyRef(added.Root!.ToUpperInvariant(), added.Key), added.Element, null);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ThemedMessageBox.Show(ex.Message, "Registry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void NewRemoval()
        {
            if (_registry == null)
            {
                return;
            }

            var key = _tree.SelectedNode?.Tag as KeyRef;
            var initial = new RegistryValueSpec { Root = key?.Root ?? "HKLM", Key = key?.Key ?? string.Empty };
            if (Dialogs.EditEntry(EntryMode.NewRemoval, initial, _groupId ?? string.Empty, _components) is not { } result)
            {
                return;
            }

            var component = result.ComponentId == null ? null : _components.FirstOrDefault(c => c.Id == result.ComponentId)?.Element;
            if (!TryDirectory(result.Spec.Root, result.Bitness, component, out var directory))
            {
                return;
            }

            try
            {
                var added = result.RemoveKey
                    ? _registry.AddRemoveKey(result.Spec.Root, result.Spec.Key, result.RemoveAction, component, directory, result.Bitness)
                    : _registry.AddRemoveValue(result.Spec.Root, result.Spec.Key, result.Spec.Name, component, directory, result.Bitness);
                Reload(new KeyRef(added.Root!.ToUpperInvariant(), added.Key), added.Element, null);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ThemedMessageBox.Show(ex.Message, "Registry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void DeleteSelected()
        {
            if (_registry == null || _grid.SelectedRows.Count == 0)
            {
                return;
            }

            var items = _grid.SelectedRows.Cast<DataGridViewRow>().Select(r => r.Tag).OfType<RegistryItem>().ToList();
            var message = items.Count == 1 ? "Do you really want to delete the selected entry?" : "Do you really want to delete the " + items.Count + " selected entries?";
            if (ThemedMessageBox.Show(message, "Delete Registry Entry", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            using (_documentManager.Batch())
            {
                foreach (var item in items)
                {
                    _registry.DeleteItem(item);
                }
            }

            Reload(null, null, null);
            RaiseSelectionChanged();
        }

        void DeleteSelectedKey()
        {
            if (_registry == null || _tree.SelectedNode is not { } node || node.Tag is not KeyRef key)
            {
                return;
            }

            if (key.Key.Length == 0)
            {
                ThemedMessageBox.Show("Select a key below " + key.Root + " to delete.", "Delete Registry Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // A key with keys below it goes with all of them; the question says which.
            var hasChildren = node.Nodes.Count > 0;
            var message = hasChildren
                ? "Do you really want to delete the " + node.Text + " key and all of its children?"
                : "Do you really want to delete the " + node.Text + " key?";
            if (ThemedMessageBox.Show(message, "Delete Registry Key", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            // The key above stays selected once this one is gone.
            var parent = node.Parent?.Tag as KeyRef;
            _registry.DeleteKey(key.Root, key.Key, includeSubkeys: true);
            Reload(parent, null, null);
            RaiseSelectionChanged();
        }

        // ---------------------------------------------------------------- tree commands

        ContextMenuStrip BuildTreeMenu()
        {
            var menu = new ContextMenuStrip();
            ToolStripMenuItem Item(string text, string keys, EventHandler click)
            {
                var item = new ToolStripMenuItem(text) { ShortcutKeyDisplayString = keys };
                item.Click += click;
                menu.Items.Add(item);
                return item;
            }

            var newKey = Item("New Key", "Ins", (s, e) => NewKey());
            menu.Items.Add(new ToolStripSeparator());
            var copy = Item("Copy", "Ctrl+C", (s, e) => CopySelected());
            var paste = Item("Paste", "Ctrl+V", (s, e) => PasteSelected());
            menu.Items.Add(new ToolStripSeparator());
            var delete = Item("Delete", "Del", (s, e) => DeleteSelectedKey());
            var rename = Item("Rename", "F2", (s, e) => RenameSelectedKey());
            var permissions = Item("Permissions...", string.Empty, (s, e) => EditKeyPermissions());
            menu.Items.Add(new ToolStripSeparator());
            Item("Import .reg...", string.Empty, (s, e) => ImportReg());
            Item("Export All...", string.Empty, (s, e) => ExportReg());
            var branch = Item("Export Selected Branch...", string.Empty, (s, e) => ExportBranch());
            menu.Opening += (s, e) =>
            {
                var key = _tree.SelectedNode?.Tag as KeyRef;
                newKey.Enabled = key != null;
                copy.Enabled = key != null && key.Key.Length > 0;
                paste.Enabled = key != null && Clipboard.ContainsText();
                delete.Enabled = key != null && key.Key.Length > 0;
                rename.Enabled = key != null && key.Key.Length > 0;
                permissions.Enabled = key != null && key.Key.Length > 0;
                branch.Enabled = key != null;
            };
            return menu;
        }

        void OnTreeKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Insert)
            {
                NewKey();
            }
            else if (e.KeyCode == Keys.F2)
            {
                RenameSelectedKey();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedKey();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelected();
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                PasteSelected();
            }
            else if (!HandleFindKey(e))
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        static string Join(string parent, string name) => parent.Length == 0 ? name : parent + "\\" + name;

        // A new key under the selected one (or root), named New Key, with its name ready to change.
        void NewKey()
        {
            if (_registry == null || _tree.SelectedNode is not { } node || node.Tag is not KeyRef parent)
            {
                return;
            }

            var name = "New Key";
            for (var n = 2; node.Nodes.Cast<TreeNode>().Any(child => string.Equals(child.Text, name, StringComparison.OrdinalIgnoreCase)); n++)
            {
                name = "New Key (" + n + ")";
            }

            var full = Join(parent.Key, name);
            if (!TryDirectory(parent.Root, "default", null, out var directory))
            {
                return;
            }

            try
            {
                _registry.AddKey(parent.Root, full, null, directory);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ThemedMessageBox.Show(ex.Message, "Registry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var created = new KeyRef(parent.Root, full);
            Reload(created, null, null);
            RaiseSelectionChanged();
            BeginInvoke(new Action(() =>
            {
                if (FindNode(_tree.Nodes, created) is { } added)
                {
                    _tree.SelectedNode = added;
                    added.BeginEdit();
                }
            }));
        }

        void RenameSelectedKey()
        {
            if (_tree.SelectedNode is { } node && node.Tag is KeyRef { Key.Length: > 0 })
            {
                node.BeginEdit();
            }
        }

        void OnKeyRenamed(NodeLabelEditEventArgs e)
        {
            // The tree is built again from the document either way.
            e.CancelEdit = true;
            if (_registry == null || e.Label == null || e.Node?.Tag is not KeyRef key)
            {
                return;
            }

            var name = e.Label.Trim();
            var slash = key.Key.LastIndexOf('\\');
            if (string.Equals(name, key.Key.Substring(slash + 1), StringComparison.Ordinal))
            {
                return;
            }

            if (!_registry.RenameKey(key.Root, key.Key, name))
            {
                ThemedMessageBox.Show("That name can't be used: it is empty, has a backslash, or another key at the same level already has it.", "Rename Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var renamed = new KeyRef(key.Root, slash < 0 ? name : key.Key.Substring(0, slash + 1) + name);
            BeginInvoke(new Action(() =>
            {
                Reload(renamed, null, null);
                RaiseSelectionChanged();
            }));
        }

        // The selected values of the grid when it has the focus, else the selected key and what is below it, as .reg text on the
        // clipboard, which regedit can paste too; with a line that says what it was so a paste here puts it in the right place.
        void CopySelected()
        {
            if (_registry == null || _tree.SelectedNode?.Tag is not KeyRef key)
            {
                return;
            }

            string text;
            if (_grid.ContainsFocus && _grid.SelectedRows.Count > 0)
            {
                var items = _grid.SelectedRows.Cast<DataGridViewRow>().Select(r => r.Tag).OfType<RegistryItem>().Where(i => i.Kind != RegistryItemKind.RemoveKey).ToList();
                if (items.Count == 0)
                {
                    return;
                }

                text = RegFile.WithCopyMarker(RegFile.Write(items).Text, "values", key.Root, key.Key);
            }
            else if (key.Key.Length > 0)
            {
                text = RegFile.WithCopyMarker(_registry.ExportRegBranch(key.Root, key.Key).Text, "keys", key.Root, key.Key);
            }
            else
            {
                return;
            }

            try
            {
                Clipboard.SetText(text);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                ThemedMessageBox.Show("The clipboard is in use by another program. Try again.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void PasteSelected()
        {
            if (_registry == null || _tree.SelectedNode?.Tag is not KeyRef target)
            {
                return;
            }

            string text;
            try
            {
                text = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                return;
            }

            var import = RegFile.Parse(text);
            if (import.Entries.Count == 0)
            {
                ThemedMessageBox.Show("The clipboard has no registry keys or values to paste.", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // From a copy here, it goes where the selection is; text from anywhere else (a .reg file's own paths) goes where it says.
            if (RegFile.TryReadCopyMarker(text, out var kind, out var baseRoot, out var baseKey))
            {
                RegFile.Retarget(import, kind, baseRoot, baseKey, target.Root, target.Key);
            }

            string? directory = null;
            if (import.Entries.Any(entry => _registry.NeedsDirectory(RegistryDocument.ScopeOf(entry.Root))))
            {
                directory = Dialogs.PickDirectory(_registry.GetKnownDirectories(), _registry.SuggestDirectory(import.Entries.Where(entry => _registry.NeedsDirectory(RegistryDocument.ScopeOf(entry.Root))).Select(entry => RegistryDocument.ScopeOf(entry.Root))));
                if (directory == null)
                {
                    return;
                }
            }

            ImportResult result;
            try
            {
                result = _registry.ImportReg(import, null, directory);
            }
            catch (ArgumentException ex)
            {
                ThemedMessageBox.Show(ex.Message, "Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Reload(target, null, null);
            RaiseSelectionChanged();
            if (result.AlreadyThere > 0 || result.Skipped.Count > 0)
            {
                var report = new StringBuilder("Pasted " + result.Added + (result.Added == 1 ? " entry." : " entries."));
                if (result.AlreadyThere > 0)
                {
                    report.Append("\r\n").Append(result.AlreadyThere).Append(result.AlreadyThere == 1 ? " value was" : " values were").Append(" already there and left as they are.");
                }

                foreach (var line in result.Skipped.Take(6))
                {
                    report.Append("\r\n- ").Append(line);
                }

                ThemedMessageBox.Show(report.ToString(), "Paste", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void ExportBranch()
        {
            if (_registry == null || _tree.SelectedNode?.Tag is not KeyRef key)
            {
                return;
            }

            var leaf = key.Key.Length == 0 ? key.Root : key.Key.Substring(key.Key.LastIndexOf('\\') + 1);
            using var dialog = new SaveFileDialog { Title = "Export the selected branch", Filter = "Registration files (*.reg)|*.reg", FileName = leaf + ".reg", OverwritePrompt = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            WriteExport(_registry.ExportRegBranch(key.Root, key.Key), dialog.FileName);
        }

        void WriteExport(RegExport export, string path)
        {
            try
            {
                // As regedit writes it: UTF-16 with a byte order mark.
                File.WriteAllText(path, export.Text, Encoding.Unicode);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                ThemedMessageBox.Show("Couldn't write " + path + ": " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (export.Warnings.Count > 0)
            {
                ThemedMessageBox.Show("Exported, with notes:\r\n\r\n" + string.Join("\r\n", export.Warnings.Take(10).Select(w => "- " + w)), "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void ImportReg()
        {
            if (_registry == null)
            {
                return;
            }

            using var dialog = new OpenFileDialog { Title = "Import a .reg file", Filter = "Registration files (*.reg)|*.reg|All files (*.*)|*.*", CheckFileExists = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            RegImport import;
            try
            {
                // A .reg file regedit writes is UTF-16 with a byte order mark, which ReadAllText recognizes.
                import = RegFile.Parse(File.ReadAllText(dialog.FileName));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                ThemedMessageBox.Show("Couldn't read " + dialog.FileName + ": " + ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // New components of the group's own: one Directory serves for all of them.
            string? directory = null;
            var needed = import.Entries.Any(e => _registry.NeedsDirectory(RegistryDocument.ScopeOf(e.Root)));
            if (needed)
            {
                directory = Dialogs.PickDirectory(_registry.GetKnownDirectories(), _registry.SuggestDirectory(import.Entries.Where(e => _registry.NeedsDirectory(RegistryDocument.ScopeOf(e.Root))).Select(e => RegistryDocument.ScopeOf(e.Root))));
                if (directory == null)
                {
                    return;
                }
            }

            ImportResult result;
            try
            {
                result = _registry.ImportReg(import, null, directory);
            }
            catch (ArgumentException ex)
            {
                ThemedMessageBox.Show(ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Reload(null, null, null);
            RaiseSelectionChanged();
            var report = new StringBuilder();
            report.Append("Added ").Append(result.Added).Append(result.Added == 1 ? " entry" : " entries").Append('.');
            if (result.AlreadyThere > 0)
            {
                report.Append("\r\n").Append(result.AlreadyThere).Append(result.AlreadyThere == 1 ? " value was" : " values were").Append(" already in the group and left as they are.");
            }

            if (result.Skipped.Count > 0)
            {
                report.Append("\r\n\r\nLeft out (").Append(result.Skipped.Count).Append("):");
                foreach (var line in result.Skipped.Take(8))
                {
                    report.Append("\r\n- ").Append(line);
                }

                if (result.Skipped.Count > 8)
                {
                    report.Append("\r\n...");
                }
            }

            ThemedMessageBox.Show(report.ToString(), "Import", MessageBoxButtons.OK, result.Skipped.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        void ExportReg()
        {
            if (_registry == null)
            {
                return;
            }

            using var dialog = new SaveFileDialog { Title = "Export a .reg file", Filter = "Registration files (*.reg)|*.reg", FileName = (_groupId ?? "registry") + ".reg", OverwritePrompt = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            WriteExport(_registry.ExportReg(), dialog.FileName);
        }

        // ---------------------------------------------------------------- the component panel

        // The component's rows are there only while an entry with a component is selected.
        void SetComponentPanelEnabled(bool enabled)
        {
            foreach (var control in _componentControls)
            {
                control.Visible = enabled;
            }

            _directoryBox.Enabled = enabled;
            _viewBox.Enabled = enabled;
            _conditionBox.Enabled = enabled;
            _reflectionBox.Enabled = enabled;
        }

        // Shows the component of the selected entry.
        void ShowComponent()
        {
            var item = _grid.CurrentRow?.Tag as RegistryItem;
            var info = item == null ? null : _components.FirstOrDefault(c => c.Element == item.Component);
            var was = _loading;
            _loading = true;
            try
            {
                _panelComponent = info;
                SetComponentPanelEnabled(info != null);
                if (info == null)
                {
                    _componentName.Text = string.Empty;
                    _directoryBox.Text = string.Empty;
                    _viewBox.SelectedIndex = -1;
                    _conditionBox.Text = string.Empty;
                    _reflectionBox.CheckState = CheckState.Indeterminate;
                    return;
                }

                _componentName.Text = Labels.ComponentLabel(_groupId ?? string.Empty, info);
                _directoryBox.Items.Clear();
                if (_registry != null)
                {
                    _directoryBox.Items.AddRange(_registry.GetKnownDirectories().Cast<object>().ToArray());
                }

                _directoryBox.Text = info.Directory ?? string.Empty;
                _viewBox.SelectedItem = Labels.ViewOf(info.Bitness);
                _conditionBox.Text = info.Condition ?? string.Empty;
                _reflectionBox.CheckState = info.DisableRegistryReflection == null ? CheckState.Indeterminate : info.DisableRegistryReflection.Value ? CheckState.Checked : CheckState.Unchecked;
            }
            finally
            {
                _loading = was;
            }
        }

        void CommitDirectory()
        {
            if (_loading || _registry == null || _panelComponent == null)
            {
                return;
            }

            var text = _directoryBox.Text.Trim();
            if (text == (_panelComponent.Directory ?? string.Empty))
            {
                return;
            }

            if (!Dialogs.IsIdentifier(text))
            {
                ThemedMessageBox.Show("An Id starts with a letter or underscore and has only letters, digits, underscores and periods.", "Directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _directoryBox.Text = _panelComponent.Directory ?? string.Empty;
                return;
            }

            _registry.SetDirectory(_panelComponent.Element, text);
            Reload(null, (_grid.CurrentRow?.Tag as RegistryItem)?.Element, null);
        }

        void CommitView()
        {
            if (_loading || _registry == null || _panelComponent == null)
            {
                return;
            }

            _registry.SetBitness(_panelComponent.Element, Labels.BitnessOf(_viewBox.SelectedItem as string));
            Reload(null, (_grid.CurrentRow?.Tag as RegistryItem)?.Element, null);
        }

        void CommitCondition()
        {
            if (_loading || _registry == null || _panelComponent == null || _conditionBox.Text == (_panelComponent.Condition ?? string.Empty))
            {
                return;
            }

            _registry.SetCondition(_panelComponent.Element, _conditionBox.Text);
            Reload(null, (_grid.CurrentRow?.Tag as RegistryItem)?.Element, null);
        }

        void CommitReflection()
        {
            if (_loading || _registry == null || _panelComponent == null)
            {
                return;
            }

            bool? value = _reflectionBox.CheckState == CheckState.Indeterminate ? (bool?)null : _reflectionBox.CheckState == CheckState.Checked;
            _registry.SetDisableRegistryReflection(_panelComponent.Element, value);
            Reload(null, (_grid.CurrentRow?.Tag as RegistryItem)?.Element, null);
        }

        // ---------------------------------------------------------------- the view filter

        // What the filter offers: all, the entries with problems, per-user or per-machine ones, or the entries of one component.
        void FillFilterBox()
        {
            var was = _filterBox.SelectedItem as string;
            var loading = _loading;
            _loading = true;
            try
            {
                _filterBox.Items.Clear();
                _filterBox.Items.Add("All entries");
                _filterBox.Items.Add("Entries with problems");
                _filterBox.Items.Add("Per-user entries (HKCU)");
                _filterBox.Items.Add("Per-machine entries");
                _filterComponentIds.Clear();
                foreach (var component in _components.Where(c => c.HasRegistry))
                {
                    _filterBox.Items.Add("Component: " + Labels.ComponentLabel(_groupId ?? string.Empty, component));
                    _filterComponentIds.Add(component.Id);
                }

                _filterBox.SelectedItem = was != null && _filterBox.Items.Contains(was) ? was : _filterBox.Items[0];
            }
            finally
            {
                _loading = loading;
            }
        }

        bool PassesFilter(string? root, XElement component, XElement? element)
        {
            switch (Math.Max(0, _filterBox.SelectedIndex))
            {
                case 0: return true;
                case 1: return _issues.Any(i => i.Item != null ? element != null && i.Item.Element == element : i.Component == component);
                case 2: return RegistryDocument.ScopeOf(root) == RegistryScope.PerUser;
                case 3: return RegistryDocument.ScopeOf(root) == RegistryScope.PerMachine;
                default: return component.Attribute("Id")?.Value == _filterComponentIds[_filterBox.SelectedIndex - 4];
            }
        }

        // ---------------------------------------------------------------- find

        bool HandleFindKey(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                Find();
            }
            else if (e.KeyCode == Keys.F3)
            {
                FindNext();
            }
            else
            {
                return false;
            }

            e.SuppressKeyPress = true;
            return true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                Find();
                return true;
            }

            if (keyData == Keys.F3)
            {
                FindNext();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Asks what to find, and goes to the first match.
        void Find()
        {
            if (_registry == null || !_registry.HasGroup)
            {
                return;
            }

            if (Dialogs.AskFind(_find) is not { } options)
            {
                return;
            }

            _find = options;
            _lastFound = null;
            FindNext();
        }

        // Goes to the match after the last one (the first, after the last of all).
        void FindNext()
        {
            if (_registry == null || !_registry.HasGroup)
            {
                return;
            }

            if (string.IsNullOrEmpty(_find.Text))
            {
                Find();
                return;
            }

            var matches = _registry.Find(_find);
            if (matches.Count == 0)
            {
                ThemedMessageBox.Show("\"" + _find.Text + "\" was not found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var index = 0;
            if (_lastFound is { } last)
            {
                var at = matches.FindIndex(m => string.Equals(m.Root, last.Root, StringComparison.OrdinalIgnoreCase) && string.Equals(m.Key, last.Key, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.Item?.Name, last.Name, StringComparison.OrdinalIgnoreCase) && m.Where == last.Where);
                index = at < 0 ? 0 : (at + 1) % matches.Count;
            }

            var match = matches[index];
            _lastFound = (match.Root, match.Key, match.Item?.Name, match.Where);
            SelectMatch(match);
        }

        void SelectMatch(FindMatch match)
        {
            var key = new KeyRef(match.Root, match.Key);
            var node = FindNode(_tree.Nodes, key);
            if (node == null && _filterBox.SelectedIndex > 0)
            {
                // The view filter hides it.
                _filterBox.SelectedIndex = 0;
                node = FindNode(_tree.Nodes, key);
            }

            if (node == null)
            {
                return;
            }

            _tree.SelectedNode = node;
            if (match.Item == null)
            {
                return;
            }

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is RegistryItem candidate && candidate.Element == match.Item.Element)
                {
                    _grid.ClearSelection();
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[match.Where == "data" ? ColValue : ColName];
                    _grid.Focus();
                    break;
                }
            }
        }

        // Puts a reference the user picks into the value being edited (starting an edit if there is none).
        void InsertPropertyIntoCell()
        {
            if (_registry == null || _grid.CurrentCell == null || _grid.CurrentCell.ColumnIndex != ColValue ||
                _grid.CurrentRow?.Tag is not RegistryItem { Kind: RegistryItemKind.Value } item || item.Type == RegKind.MultiString)
            {
                return;
            }

            if (Dialogs.PickReference(_registry.GetReferences()) is not { } text)
            {
                return;
            }

            if (!_grid.IsCurrentCellInEditMode)
            {
                _grid.BeginEdit(true);
            }

            if (_grid.EditingControl is TextBox box)
            {
                box.SelectedText = text;
            }
        }

        // ---------------------------------------------------------------- icons

        static readonly Dictionary<string, Image> Icons = new Dictionary<string, Image>();

        // A small icon for a kind of entry, as regedit has: letters on a colored square.
        static Image Icon(string text, Color color)
        {
            if (Icons.TryGetValue(text, out var found))
            {
                return found;
            }

            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(color))
            using (var font = new Font("Segoe UI", 6.5F, FontStyle.Bold))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.FillRectangle(brush, 1, 2, 14, 12);
                var size = graphics.MeasureString(text, font);
                graphics.DrawString(text, font, Brushes.White, (16 - size.Width) / 2 + 0.5F, (16 - size.Height) / 2 + 0.5F);
            }

            Icons[text] = bitmap;
            return bitmap;
        }

        static Image RowIcon(RegistryItem item)
        {
            if (item.Kind != RegistryItemKind.Value)
            {
                return Icon("x", Color.FromArgb(0xB0, 0x3A, 0x2E));
            }

            switch (item.Type)
            {
                case RegKind.Integer: return Icon("dw", Color.FromArgb(0x1F, 0x6F, 0xB5));
                case RegKind.Binary: return Icon("01", Color.FromArgb(0x2E, 0x7D, 0x32));
                case RegKind.Expandable: return Icon("%", Color.FromArgb(0xC7, 0x7C, 0x0E));
                case RegKind.MultiString: return Icon("ab+", Color.FromArgb(0x7B, 0x3F, 0xA0));
                default: return Icon("ab", Color.FromArgb(0xC0, 0x39, 0x2B));
            }
        }

        // ---------------------------------------------------------------- the key panel and permissions

        // The key's rows are there only while a key (not a root) is selected.
        void SetKeyPanelEnabled(bool enabled)
        {
            foreach (var control in _keyControls)
            {
                control.Visible = enabled;
            }

            _forceCreate.Enabled = enabled;
            _forceDelete.Enabled = enabled;
            _keyPermissions.Enabled = enabled;
        }

        // Shows the options of the selected key: those of its RegistryKey element, or none while it has none.
        void ShowKey()
        {
            var key = _tree.SelectedNode?.Tag as KeyRef;
            var was = _loading;
            _loading = true;
            try
            {
                _panelKey = key != null && key.Key.Length > 0 ? key : null;
                SetKeyPanelEnabled(_panelKey != null);
                if (_panelKey == null || _registry == null)
                {
                    _keyName.Text = string.Empty;
                    _forceCreate.CheckState = CheckState.Indeterminate;
                    _forceDelete.CheckState = CheckState.Indeterminate;
                    return;
                }

                var options = _registry.GetKeyOptions(_panelKey.Root, _panelKey.Key);
                _keyName.Text = _panelKey.Root + "\\" + _panelKey.Key;
                _forceCreate.CheckState = options.ForceCreateOnInstall == null ? CheckState.Indeterminate : options.ForceCreateOnInstall.Value ? CheckState.Checked : CheckState.Unchecked;
                _forceDelete.CheckState = options.ForceDeleteOnUninstall == null ? CheckState.Indeterminate : options.ForceDeleteOnUninstall.Value ? CheckState.Checked : CheckState.Unchecked;
            }
            finally
            {
                _loading = was;
            }
        }

        static bool? Flag(CheckBox box) => box.CheckState == CheckState.Indeterminate ? (bool?)null : box.CheckState == CheckState.Checked;

        // The Directory for a key's own element, asked for if the key has nowhere to put one. False if the user cancels.
        bool TryKeyDirectory(KeyRef key, out string? directory)
        {
            directory = null;
            if (_registry == null || !_registry.KeyNeedsDirectory(key.Root, key.Key))
            {
                return true;
            }

            directory = Dialogs.PickDirectory(_registry.GetKnownDirectories(), _registry.SuggestDirectory(new[] { RegistryDocument.ScopeOf(key.Root) }));
            return directory != null;
        }

        void CommitKeyOptions()
        {
            if (_loading || _registry == null || _panelKey == null)
            {
                return;
            }

            var key = _panelKey;
            if (!TryKeyDirectory(key, out var directory))
            {
                ShowKey();
                return;
            }

            try
            {
                _registry.SetKeyOptions(key.Root, key.Key, new KeyOptions { ForceCreateOnInstall = Flag(_forceCreate), ForceDeleteOnUninstall = Flag(_forceDelete) }, directory);
            }
            catch (ArgumentException ex)
            {
                ThemedMessageBox.Show(ex.Message, "Key Options", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Reload(key, null, null);
        }

        void EditKeyPermissions()
        {
            if (_registry == null || _tree.SelectedNode?.Tag is not KeyRef { Key.Length: > 0 } key)
            {
                return;
            }

            var existing = _keys.FirstOrDefault(k => key.Is(k.Root, k.Key));
            var current = existing == null ? new List<PermissionSpec>() : _registry.GetPermissions(existing.Element);
            if (PermissionsDialog.Edit("Permissions of " + key.Root + "\\" + key.Key, current) is not { } result)
            {
                return;
            }

            // Nothing to write for a key that has none and is given none.
            if (existing == null && result.Count == 0)
            {
                return;
            }

            if (!TryKeyDirectory(key, out var directory))
            {
                return;
            }

            try
            {
                _registry.SetPermissions(_registry.EnsureKeyElement(key.Root, key.Key, directory), result);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ThemedMessageBox.Show(ex.Message, "Permissions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Reload(key, null, null);
        }

        void EditValuePermissions(RegistryItem item)
        {
            if (_registry == null || item.Kind != RegistryItemKind.Value)
            {
                return;
            }

            var title = "Permissions of " + Describe(item);
            if (PermissionsDialog.Edit(title, _registry.GetPermissions(item.Element)) is not { } result)
            {
                return;
            }

            try
            {
                _registry.SetPermissions(item.Element, result);
            }
            catch (InvalidOperationException ex)
            {
                ThemedMessageBox.Show(ex.Message, "Permissions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Reload(null, item.Element, null);
        }

        // ---------------------------------------------------------------- ISelectionLinked

        public event EventHandler? SelectedElementChanged;

        void RaiseSelectionChanged() => SelectedElementChanged?.Invoke(this, EventArgs.Empty);

        public XElement? SelectedElement
        {
            get
            {
                if (_registry == null || !_registry.HasGroup)
                {
                    return null;
                }

                if (_grid.CurrentRow?.Tag is RegistryItem item)
                {
                    return item.Element;
                }

                return _tree.SelectedNode?.Tag is KeyRef key && _keys.FirstOrDefault(k => key.Is(k.Root, k.Key)) is { } info ? info.Element : _registry.GroupElement;
            }
        }

        public string? SelectedAttribute
        {
            get
            {
                if (_grid.CurrentRow?.Tag is not RegistryItem item || _grid.CurrentCell == null)
                {
                    return null;
                }

                switch (_grid.CurrentCell.ColumnIndex)
                {
                    case ColName: return item.Kind == RegistryItemKind.RemoveKey ? null : "Name";
                    case ColType: return item.Kind == RegistryItemKind.Value ? "Type" : null;
                    case ColValue: return item.Kind == RegistryItemKind.Value && item.Type != RegKind.MultiString ? "Value" : null;
                    case ColAction: return item.Kind == RegistryItemKind.RemoveValue ? null : "Action";
                    case ColKeyPath: return item.Kind == RegistryItemKind.Value ? "KeyPath" : null;
                    default: return null;
                }
            }
        }

        static bool IsRegistryElement(XElement element)
        {
            var name = element.Name.LocalName;
            return name == "RegistryKey" || name == "RegistryValue" || name == "RemoveRegistryKey" || name == "RemoveRegistryValue" || name == "MultiStringValue";
        }

        // From the XML rather than the tree, which is only filled once the designer has been shown.
        public bool Handles(XElement element, string? attribute) => element.AncestorsAndSelf().Any(IsRegistryElement);

        public void SelectElement(XElement element, string? attribute)
        {
            if (_registry == null)
            {
                return;
            }

            var entry = element.AncestorsAndSelf().FirstOrDefault(e => _items.Any(i => i.Element == e));
            if (entry != null)
            {
                var item = _items.First(i => i.Element == entry);
                var node = item.Root == null ? null : FindNode(_tree.Nodes, new KeyRef(item.Root.ToUpperInvariant(), item.Key));
                if (node != null && _tree.SelectedNode != node)
                {
                    _tree.SelectedNode = node;
                }

                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (row.Tag is RegistryItem candidate && candidate.Element == entry)
                    {
                        _grid.ClearSelection();
                        row.Selected = true;
                        var column = attribute == "Type" ? ColType : attribute == "Value" ? ColValue : attribute == "Action" ? ColAction : attribute == "KeyPath" ? ColKeyPath : ColName;
                        _grid.CurrentCell = row.Cells[column];
                        break;
                    }
                }

                return;
            }

            // A key of its own, with nothing but keys in it.
            var key = _keys.FirstOrDefault(k => k.Element == element || element.AncestorsAndSelf().Contains(k.Element));
            if (key?.Root != null && FindNode(_tree.Nodes, new KeyRef(key.Root.ToUpperInvariant(), key.Key)) is { } keyNode)
            {
                _tree.SelectedNode = keyNode;
            }
        }
    }
}
