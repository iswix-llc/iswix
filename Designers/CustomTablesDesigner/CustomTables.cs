using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Services;
using IsWiX.WiXModel.Core;
using static FireworksFramework.Types.Enums;
using Wxs = IsWiX.WiXModel;

namespace CustomTablesDesigner
{
    public partial class CustomTablesDesigner : UserControl, IFireworksDesigner, ISelectionLinked
    {
        DataTable _customTable = new DataTable();
        // Set when a table is selected; the column and rename commands need one.
        Wxs.CustomTable _customTableElement = null!;
        // The Row element behind each grid row.
        Dictionary<DataRow, Wxs.Row> _rowElements = new Dictionary<DataRow, Wxs.Row>();
        readonly DocumentManager _documentManager;

        public CustomTablesDesigner(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            InitializeComponent();
            DesignerImages.Add(imageListIcons, typeof(CustomTablesDesigner).Assembly, "CustomTablesDesigner.Images.", "PrimaryKey");
            dataGridViewRows.DataSource = _customTable;

            // The column list and schema box are placed in pixels and grow with the font they get at run time (Visual
            // Studio's, or the system's), while the panels holding them keep their designed size. Once those sizes are
            // known, and whenever they change, the panels are grown to fit them.
            Load += (s, e) => FitColumnEditor();
            groupBoxColumnSchema.SizeChanged += (s, e) => FitColumnEditor();
            splitContainer1.SizeChanged += (s, e) => FitColumnEditor();
            splitContainer2.SizeChanged += (s, e) => FitColumnEditor();

            LinkSelection();

            AddToolbar(splitContainer2.Panel1, toolStripMenuItemTableCreate, toolStripMenuItemRenameTable, toolStripMenuItemTableDrop);
            AddToolbar(splitContainer1.Panel2, toolStripMenuItemRowAdd, toolStripMenuItemRowRemove);

            setKeyToolStripMenuItem.Click += (s, e) => SetKey(true);
            clearKeyToolStripMenuItem.Click += (s, e) => SetKey(false);
            insertAboveToolStripMenuItem.Click += (s, e) => InsertColumn(below: false);
            insertBelowToolStripMenuItem.Click += (s, e) => InsertColumn(below: true);
            deleteToolStripMenuItem.Click += (s, e) => DeleteColumn();
            renameToolStripMenuItem.Click += (s, e) => RenameColumn();
            moveUpToolStripMenuItem.Click += (s, e) => MoveColumn(-1);
            moveDownToolStripMenuItem.Click += (s, e) => MoveColumn(1);
            var columnsToolbar = AddToolbar(splitContainer2.Panel2, insertBelowToolStripMenuItem, deleteToolStripMenuItem, moveUpToolStripMenuItem, moveDownToolStripMenuItem);
            // The columns' label, list and schema box are placed in pixels: they move down below the toolbar.
            foreach (Control control in splitContainer2.Panel2.Controls)
            {
                if (control != columnsToolbar)
                {
                    control.Top += columnsToolbar.Height;
                }
            }
        }

        #region ISelectionLinked Members

        // What the selection is on, by what the user last used: a table, one of its columns (and an attribute of it, if
        // chosen in the property grid, whose items are named for them), or a row (and a cell of it).
        enum SelectionLevel
        {
            Table,
            Column,
            Row,
        }

        SelectionLevel _level;
        string? _property;
        bool _showingTable;

        public event EventHandler? SelectedElementChanged;

        // The row of the grid's current cell and the Data element behind the cell, if it has one.
        (Wxs.Row Row, Wxs.Data? Data)? CurrentRow()
        {
            if (dataGridViewRows.CurrentRow?.DataBoundItem is DataRowView view && _rowElements.TryGetValue(view.Row, out var row))
            {
                var column = dataGridViewRows.CurrentCell?.OwningColumn?.Name;
                return (row, row.DataElements.FirstOrDefault(d => d.Column == column));
            }

            return null;
        }

        public XElement? SelectedElement
        {
            get
            {
                if (listBoxTables.SelectedItem is null || _customTableElement is null)
                {
                    return null;
                }

                switch (_level)
                {
                    case SelectionLevel.Column when SelectedColumnId is string id:
                        return FindColumn(id).Xml;
                    case SelectionLevel.Row when CurrentRow() is { } current:
                        return current.Data?.Xml ?? current.Row.Xml;
                    default:
                        return _customTableElement.Xml;
                }
            }
        }

        public string? SelectedAttribute
        {
            get
            {
                switch (_level)
                {
                    case SelectionLevel.Column:
                        return _property;
                    case SelectionLevel.Row when CurrentRow() is { Data: { } data }:
                        // A cell's value is the attribute, unless an older source wrote it as the element's text.
                        return data.Xml.Attribute("Value") != null ? "Value" : null;
                    default:
                        return null;
                }
            }
        }

        // From the XML rather than the lists, which are only filled once the designer has been shown. Anything in a
        // table, a column or a row, counts as the table.
        public bool Handles(XElement element, string? attribute) => element.AncestorsAndSelf().Any(e => e.Name == Wxs.CustomTable.ElementName);

        public void SelectElement(XElement element, string? attribute)
        {
            var table = element.AncestorsAndSelf().FirstOrDefault(e => e.Name == Wxs.CustomTable.ElementName);
            if ((string?)table?.Attribute("Id") is not string tableId)
            {
                return;
            }

            if (!Equals(listBoxTables.SelectedItem, tableId))
            {
                listBoxTables.SelectedItem = tableId;
            }

            if (listBoxTables.SelectedItem is null)
            {
                return;
            }

            var level = SelectionLevel.Table;
            string? property = null;
            var columnElement = element.AncestorsAndSelf().FirstOrDefault(e => e.Name == Wxs.Column.ElementName);
            var rowElement = element.AncestorsAndSelf().FirstOrDefault(e => e.Name == Wxs.Row.ElementName);
            if (columnElement != null)
            {
                var item = listViewColumns.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Text == (string?)columnElement.Attribute("Id"));
                if (item != null)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    level = SelectionLevel.Column;
                    property = attribute;
                    if (attribute != null && propertyGridColumn.SelectedGridItem is { } selected)
                    {
                        var root = selected;
                        while (root.Parent != null)
                        {
                            root = root.Parent;
                        }

                        AllItems(root).FirstOrDefault(i => i.GridItemType == GridItemType.Property && i.PropertyDescriptor?.Name == attribute)?.Select();
                    }
                }
            }
            else if (rowElement != null)
            {
                var dataRow = _rowElements.FirstOrDefault(r => r.Value.Xml == rowElement).Key;
                var gridRow = dataRow is null ? null : dataGridViewRows.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => (r.DataBoundItem as DataRowView)?.Row == dataRow);
                if (gridRow != null)
                {
                    var column = element.Name == Wxs.Data.ElementName ? (string?)element.Attribute("Column") : null;
                    var cell = column != null && dataGridViewRows.Columns.Contains(column) ? gridRow.Cells[column] : gridRow.Cells[0];
                    dataGridViewRows.ClearSelection();
                    gridRow.Selected = true;
                    dataGridViewRows.CurrentCell = cell;
                    dataGridViewRows.FirstDisplayedScrollingRowIndex = Math.Max(0, Math.Min(gridRow.Index, dataGridViewRows.Rows.Count - 1));
                    level = SelectionLevel.Row;
                }
            }

            // Set last: selecting above raised the designer's own events, which set these as a user's choice would.
            _level = level;
            _property = property;
        }

        private static IEnumerable<GridItem> AllItems(GridItem item)
        {
            yield return item;
            foreach (GridItem child in item.GridItems)
            {
                foreach (var descendant in AllItems(child))
                {
                    yield return descendant;
                }
            }
        }

        private void ShowSelectionInXml(SelectionLevel level, string? property = null)
        {
            _level = level;
            _property = property;
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
        }

        // Whoever selected it, the user or the designer showing a table. The window ignores this while it reloads the
        // designers for a change made in the XML, so typing there doesn't move the XML's selection.
        private void LinkSelection()
        {
            // Run after the designer's own handlers, which show the table the list selects.
            listBoxTables.SelectedIndexChanged += (s, e) => ShowSelectionInXml(SelectionLevel.Table);
            listBoxTables.Click += (s, e) => ShowSelectionInXml(SelectionLevel.Table);
            // So the selected column still shows when the XML editor has the focus.
            listViewColumns.HideSelection = false;
            listViewColumns.SelectedIndexChanged += (s, e) =>
            {
                if (SelectedColumnId != null)
                {
                    ShowSelectionInXml(SelectionLevel.Column);
                }
            };
            listViewColumns.Click += (s, e) =>
            {
                if (SelectedColumnId != null)
                {
                    ShowSelectionInXml(SelectionLevel.Column);
                }
            };
            propertyGridColumn.SelectedGridItemChanged += (s, e) =>
            {
                if (propertyGridColumn.ContainsFocus && e.NewSelection?.GridItemType == GridItemType.Property && SelectedColumnId != null)
                {
                    ShowSelectionInXml(SelectionLevel.Column, e.NewSelection.PropertyDescriptor?.Name);
                }
            };
            // Not while a table is being shown: the grid then selects its first cell, which nobody chose.
            dataGridViewRows.CurrentCellChanged += (s, e) =>
            {
                if (!_showingTable && dataGridViewRows.CurrentCell != null)
                {
                    ShowSelectionInXml(SelectionLevel.Row);
                }
            };
            dataGridViewRows.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    ShowSelectionInXml(SelectionLevel.Row);
                }
            };
        }

        #endregion

        // The tables', columns' and rows' commands as buttons above them too: nothing else shows that right-clicking
        // adds one. The buttons run the menu items and are enabled as they are.
        static ToolStrip AddToolbar(Control panel, params ToolStripMenuItem[] menuItems)
        {
            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            foreach (var menuItem in menuItems)
            {
                // No tooltip: it would only repeat the button's text.
                var button = new ToolStripButton(menuItem.Text) { DisplayStyle = ToolStripItemDisplayStyle.Text, Enabled = menuItem.Enabled, AutoToolTip = false };
                button.Click += (s, e) => menuItem.PerformClick();
                menuItem.EnabledChanged += (s, e) => button.Enabled = menuItem.Enabled;
                toolbar.Items.Add(button);
            }

            panel.Controls.Add(toolbar);
            return toolbar;
        }

        bool _fitting;

        // Widens the left side to the schema box's right edge and makes the columns area tall enough for the list and
        // the box, as far as the designer's own size allows. It only ever grows them; the splitters can still be moved.
        // With no table selected there are no columns or rows to show: their panes are collapsed, not left empty.
        void ShowPanesForSelection()
        {
            var hasTable = listBoxTables.SelectedItems.Count > 0;
            var changed = splitContainer2.Panel2Collapsed == hasTable;
            splitContainer2.Panel2Collapsed = !hasTable;
            splitContainer1.Panel2Collapsed = !hasTable;
            if (changed && hasTable)
            {
                // Back at their sizes, which have to be fitted again.
                FitColumnEditor();
            }
        }

        void FitColumnEditor()
        {
            if (_fitting || splitContainer1.Panel2Collapsed || splitContainer2.Panel2Collapsed)
            {
                return;
            }

            _fitting = true;
            try
            {
                const int margin = 8;
                var width = groupBoxColumnSchema.Right + margin;
                var widest = splitContainer1.Width - splitContainer1.SplitterWidth - splitContainer1.Panel2MinSize;
                if (splitContainer1.SplitterDistance < width && widest > splitContainer1.SplitterDistance)
                {
                    splitContainer1.SplitterDistance = Math.Min(width, widest);
                }

                var height = Math.Max(groupBoxColumnSchema.Bottom, listViewColumns.Bottom) + margin;
                if (splitContainer2.Panel2.Height < height)
                {
                    var distance = splitContainer2.Height - splitContainer2.SplitterWidth - height;
                    splitContainer2.SplitterDistance = Math.Max(splitContainer2.Panel1MinSize, Math.Min(splitContainer2.SplitterDistance, distance));
                }
            }
            finally
            {
                _fitting = false;
            }
        }

        void _customTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            SaveTable();
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {
            return CustomTableDocument.IsValidDocument(_documentManager.Document);
        }

        public void LoadData()
        {
            _customTable.RowChanged -= new DataRowChangeEventHandler(_customTable_RowChanged);

            _customTable.Clear();

            listBoxTables.Items.Clear();
            listViewColumns.Items.Clear();
            // Until a table is selected (there may be none).
            toolStripMenuItemRenameTable.Enabled = false;
            toolStripMenuItemTableDrop.Enabled = false;
            toolStripMenuItemRowAdd.Enabled = false;

            try
            {
                foreach (var table in CustomTableDocument.Tables(_documentManager.Document))
                {
                    listBoxTables.Items.Add(table.Id ?? string.Empty);
                }

                if (listBoxTables.Items.Count > 0)
                {
                    listBoxTables.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Malformed XML Detected (Get Table Names)" + ex.Message);
                listBoxTables.Items.Clear();
            }

            // A document with no tables raises no selection change to do this.
            ShowPanesForSelection();
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomTablesDesigner.CustomTables.ico")!);
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomTablesDesigner.License.txt")!).ReadToEnd();
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginName
        {
            get { return "Custom Tables"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group4_customtables"; }
        }

        #endregion

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) => ShowTable(null);

        // Shows the selected table: its columns, with the column named (else the first) selected and its schema in the
        // property grid, and its rows in the grid.
        private void ShowTable(string? selectColumn)
        {
            _showingTable = true;
            try
            {
                ShowTableCore(selectColumn);
            }
            finally
            {
                _showingTable = false;
            }
        }

        private void ShowTableCore(string? selectColumn)
        {
            this.dataGridViewRows.CellValueChanged -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewRows_CellValueChanged);

            toolStripMenuItemTableDrop.Enabled = false;
            toolStripMenuItemRenameTable.Enabled = false;
            // Rows can be added to a selected table only.
            toolStripMenuItemRowAdd.Enabled = false;
            _customTable = new DataTable();
            _rowElements = new Dictionary<DataRow, Wxs.Row>();
            dataGridViewRows.DataSource = _customTable;
            listViewColumns.Items.Clear();
            propertyGridColumn.SelectedObject = null;
            if (listBoxTables.SelectedItems.Count > 0)
            {

                _customTableElement = CustomTableDocument.Find(_documentManager.Document, (string)listBoxTables.SelectedItem!);

                foreach (var column in _customTableElement.Columns)
                {
                    var item = listViewColumns.Items.Add(column.Id);
                    if (IsYes(column.PrimaryKey))
                    {
                        item.ImageKey = "PrimaryKey";
                    }
                }

                bool isPreviousPK = true;
                foreach (ListViewItem item in listViewColumns.Items)
                {
                    if (item.Index == 0)
                    {
                        if (item.ImageKey != "PrimaryKey")
                        {
                            MessageBox.Show("Notice: Marking the first column as Primary due to Windows Installer requirements.");
                            item.ImageKey = "PrimaryKey";
                            FindColumn(item.Text).PrimaryKey = true;
                        }
                    }
                    else
                    {
                        if (item.ImageKey == "PrimaryKey")
                        {
                            if (isPreviousPK == false)
                            {
                                MessageBox.Show("Notice: Marking column '" + item.Text + "' as non-primary due to Window Installer requirements.");
                                item.ImageKey = string.Empty;
                                FindColumn(item.Text).PrimaryKey = false;
                            }
                        }
                        else
                        {
                            isPreviousPK = false;
                        }
                    }
                }
                toolStripMenuItemRenameTable.Enabled = true;
                toolStripMenuItemTableDrop.Enabled = true;
                toolStripMenuItemRowAdd.Enabled = true;
                try
                {
                    _customTable = CustomTableDocument.BuildDataTable(_customTableElement, out _rowElements);
                    dataGridViewRows.DataSource = _customTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The rows of the table '{listBoxTables.Text}' don't fit its columns, so they can't be shown: {ex.Message}");
                    toolStripMenuItemRowAdd.Enabled = false;
                }

                var selected = listViewColumns.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Text == selectColumn)
                    ?? listViewColumns.Items.Cast<ListViewItem>().FirstOrDefault();
                if (selected != null)
                {
                    selected.Selected = true;
                    selected.Focused = true;
                    selected.EnsureVisible();
                }
            }

            UpdateColumnCommands();
            ShowPanesForSelection();
            this.dataGridViewRows.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewRows_CellValueChanged);
        }

        private void toolStripMenuItemRowRemove_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow selectedRow in dataGridViewRows.SelectedRows)
            {
                try
                {
                    dataGridViewRows.Rows.Remove(selectedRow);

                }
                catch (Exception)
                {
                }
            }
            _customTable.AcceptChanges();
            SaveTable();

            // The row next to the one removed is current, if there is one; else the XML shows the table.
            ShowSelectionInXml(dataGridViewRows.CurrentRow != null ? SelectionLevel.Row : SelectionLevel.Table);
        }

        private void dataGridViewRows_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewRows.SelectedRows.Count == 0)
            {
                toolStripMenuItemRowRemove.Enabled = false;
            }
            else
            {
                toolStripMenuItemRowRemove.Enabled = true;
            }
        }

        private void SaveTable()
        {
            CustomTableDocument.SaveRows(CustomTableDocument.Find(_documentManager.Document, listBoxTables.Text), _customTable, _rowElements);
        }

        // A row with the first free key ("Id1", or 1 in a number column, then 2...) and the columns that can't be empty
        // given an empty or zero value.
        private void toolStripMenuItemRowAdd_Click(object sender, EventArgs e)
        {
            var keys = new HashSet<DataColumn>(_customTable.PrimaryKey);
            for (int index = 1; index <= 10000; index++)
            {
                DataRow dr = _customTable.NewRow();
                foreach (DataColumn dc in _customTable.Columns)
                {
                    bool number = dc.DataType != typeof(string);
                    if (keys.Contains(dc))
                    {
                        dr[dc] = number ? index : (object)("Id" + index);
                    }
                    else if (!dc.AllowDBNull)
                    {
                        dr[dc] = number ? 0 : (object)string.Empty;
                    }
                }

                try
                {
                    _customTable.Rows.Add(dr);
                    break;
                }
                catch (ConstraintException)
                {
                    // That key is taken: try the next.
                }
            }
            _customTable.AcceptChanges();
            SaveTable();

            // The new row, selected so the XML shows it.
            if (dataGridViewRows.Rows.Count > 0)
            {
                dataGridViewRows.ClearSelection();
                var newRow = dataGridViewRows.Rows[dataGridViewRows.Rows.Count - 1];
                newRow.Selected = true;
                dataGridViewRows.CurrentCell = newRow.Cells[0];
                ShowSelectionInXml(SelectionLevel.Row);
            }
        }

        private void toolStripMenuItemTableDrop_Click(object sender, EventArgs e)
        {
            CustomTableDocument.Drop(_documentManager.Document, CustomTableDocument.Find(_documentManager.Document, (string)listBoxTables.SelectedItem!));
            listBoxTables.Items.Remove(listBoxTables.SelectedItem!);
        }

        private void listBoxTables_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point p = new Point(e.X, e.Y);
                listBoxTables.SelectedIndex = listBoxTables.IndexFromPoint(p);
            }
        }
        private void dataGridViewRows_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                _customTable.AcceptChanges();
                SaveTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static bool IsYes(WixValue<bool>? value) => value is { IsLiteral: true } literal && literal.Value;

        private Wxs.Column FindColumn(string id) => _customTableElement.Columns.First(c => c.Id == id);
        private void dataGridViewRows_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(e.Exception?.Message);
        }

        // The selected column's schema, in the property grid.
        private void listViewColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGridColumn.SelectedObject = SelectedColumnId is string id ? new ColumnProperties(FindColumn(id)) : null;
            propertyGridColumn.ExpandAllGridItems();
            UpdateColumnCommands();
        }

        private string? SelectedColumnId => listViewColumns.SelectedItems.Count > 0 ? listViewColumns.SelectedItems[0].Text : null;

        private void propertyGridColumn_PropertyValueChanged(object? s, PropertyValueChangedEventArgs e)
        {
            if (propertyGridColumn.SelectedObject is not ColumnProperties properties || e.ChangedItem?.PropertyDescriptor is not { } property)
            {
                return;
            }

            object? value = e.ChangedItem.Value;
            string select = property.Name == nameof(ColumnProperties.Id) ? ((string?)value ?? string.Empty).Trim() : properties.ColumnId;
            ChangeSchema(table => ColumnProperties.Write(table, properties.ColumnId, property.Name, value), select);
        }

        // Makes a change to the selected table's columns: first to a copy, which must still meet Windows Installer's key
        // rules and fit the rows, then (if it does) to the document, and shows the table again. When it doesn't, the
        // document is left as it was and the reason shown.
        private void ChangeSchema(Action<Wxs.CustomTable> change, string? selectColumn)
        {
            dataGridViewRows.EndEdit();
            try
            {
                var copy = (Wxs.CustomTable)WixElement.Wrap(new XElement(_customTableElement.Xml));
                change(copy);
                CustomTableDocument.CheckKeys(copy);
                CustomTableDocument.BuildDataTable(copy, out _);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or DataException or FormatException or OverflowException)
            {
                MessageBox.Show(ex.Message, "Custom Tables", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ShowTable(SelectedColumnId);
                return;
            }

            change(_customTableElement);
            ShowTable(selectColumn);
        }

        // The column commands, enabled as Windows Installer's key rules allow for the selected column: the key's columns
        // come first, the first column is always in it.
        private void UpdateColumnCommands()
        {
            var columns = _customTableElement?.Columns.ToList() ?? new List<Wxs.Column>();
            int index = SelectedColumnId is string id ? columns.FindIndex(c => c.Id == id) : -1;
            bool selected = index >= 0;
            bool key = selected && IsYes(columns[index].PrimaryKey);
            bool previousKey = index > 0 && IsYes(columns[index - 1].PrimaryKey);
            bool nextKey = selected && index < columns.Count - 1 && IsYes(columns[index + 1].PrimaryKey);

            setKeyToolStripMenuItem.Enabled = selected && !key && previousKey;
            clearKeyToolStripMenuItem.Enabled = key && index > 0 && !nextKey;
            insertAboveToolStripMenuItem.Enabled = selected && !key;
            insertBelowToolStripMenuItem.Enabled = selected && !nextKey;
            deleteToolStripMenuItem.Enabled = selected && columns.Count > 1 && !(index == 0 && !nextKey);
            renameToolStripMenuItem.Enabled = selected;
            moveUpToolStripMenuItem.Enabled = index > 0 && key == previousKey;
            moveDownToolStripMenuItem.Enabled = selected && index < columns.Count - 1 && key == nextKey;
        }

        private void InsertColumn(bool below)
        {
            if (SelectedColumnId is not string beside)
            {
                return;
            }

            string id = CustomTableDocument.NewColumnId(_customTableElement);
            ChangeSchema(table => CustomTableDocument.InsertColumn(table, id, beside, below), id);
        }

        private void DeleteColumn()
        {
            if (SelectedColumnId is not string id)
            {
                return;
            }

            int values = _customTableElement.Rows.SelectMany(r => r.DataElements).Count(d => d.Column == id);
            if (values > 0 && MessageBox.Show($"Delete the column '{id}', and its values in {values} rows?", "Custom Tables",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            var ids = _customTableElement.Columns.Select(c => c.Id).ToList();
            int index = ids.IndexOf(id);
            string? select = index + 1 < ids.Count ? ids[index + 1] : index > 0 ? ids[index - 1] : null;
            ChangeSchema(table => CustomTableDocument.DeleteColumn(table, id), select);
        }

        private void MoveColumn(int offset)
        {
            if (SelectedColumnId is string id)
            {
                ChangeSchema(table => CustomTableDocument.MoveColumn(table, id, offset), id);
            }
        }

        private void SetKey(bool key)
        {
            if (SelectedColumnId is string id)
            {
                ChangeSchema(table => CustomTableDocument.SetPrimaryKey(table, id, key), id);
            }
        }

        // The name is edited in the property grid: its row is selected there, ready to type in.
        private void RenameColumn()
        {
            var root = propertyGridColumn.SelectedGridItem;
            while (root?.Parent != null)
            {
                root = root.Parent;
            }

            var idItem = root?.GridItems.Cast<GridItem>().SelectMany(c => c.GridItems.Cast<GridItem>().Prepend(c))
                .FirstOrDefault(i => i.PropertyDescriptor?.Name == nameof(ColumnProperties.Id));
            if (idItem != null)
            {
                propertyGridColumn.Focus();
                idItem.Select();
            }
        }

        private void toolStripMenuItemRenameTable_Click(object sender, EventArgs e)
        {
            List<string> tables = new List<string>();

            foreach (var item in listBoxTables.Items)
            {
                tables.Add((string)item);
            }
            var dialog = new FormTableName( listBoxTables.Text, tables );

            if (DesignerDialogs.ShowDialog(dialog) == DialogResult.OK)
            {
                CustomTableDocument.Rename(_documentManager.Document, _customTableElement, dialog.TableName);
                LoadData();
                // The renamed table, not the first one that LoadData selects.
                listBoxTables.SelectedItem = dialog.TableName;
            }
        }

        private void toolStripMenuItemTableCreate_Click(object sender, EventArgs e)
        {
            List<string> tables = new List<string>();

            foreach (var item in listBoxTables.Items)
            {
                tables.Add((string)item);
            }
            var dialog = new FormTableName(string.Empty, tables);

            if (DesignerDialogs.ShowDialog(dialog) == DialogResult.OK)
            { 
                CustomTableDocument.Create(_documentManager.Document, dialog.TableName);
                LoadData();
                // The new table, not the first one that LoadData selects.
                listBoxTables.SelectedItem = dialog.TableName;
            }
        }
    }
}