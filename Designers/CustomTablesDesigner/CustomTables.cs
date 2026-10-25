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
    public partial class CustomTablesDesigner : UserControl, IFireworksDesigner
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

            AddToolbar(splitContainer2.Panel1, toolStripMenuItemTableCreate, toolStripMenuItemRenameTable, toolStripMenuItemTableDrop);
            AddToolbar(splitContainer1.Panel2, toolStripMenuItemRowAdd, toolStripMenuItemRowRemove);
        }

        // The tables' and rows' commands as buttons above them too: nothing else shows that right-clicking adds one.
        // The buttons run the menu items and are enabled as they are.
        static void AddToolbar(Control panel, params ToolStripMenuItem[] menuItems)
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
        }

        bool _fitting;

        // Widens the left side to the schema box's right edge and makes the columns area tall enough for the list and
        // the box, as far as the designer's own size allows. It only ever grows them; the splitters can still be moved.
        void FitColumnEditor()
        {
            if (_fitting)
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.dataGridViewRows.CellValueChanged -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewRows_CellValueChanged);

            toolStripMenuItemTableDrop.Enabled = false;
            toolStripMenuItemRenameTable.Enabled = false;
            // Rows can be added to a selected table only.
            toolStripMenuItemRowAdd.Enabled = false;
            _customTable.Clear();
            _customTable.Rows.Clear();
            _customTable.Constraints.Clear();
            _customTable.Columns.Clear();
            listViewColumns.Items.Clear();
            
            // The column schema fields only show the selected column; they are read-only rather than disabled, so
            // they stay readable in Visual Studio's dark theme.
            textBoxName.Text = "";
            textBoxType.Text = "";
            textBoxLength.Text = "";
            checkBoxNullable.Checked = false;
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
                listViewColumns.Items[0].Selected = true;

                if (!String.IsNullOrEmpty(listBoxTables.Text))
                {
                    toolStripMenuItemRenameTable.Enabled = true;
                    toolStripMenuItemTableDrop.Enabled = true;
                    toolStripMenuItemRowAdd.Enabled = true;
                    _customTable.TableName = listBoxTables.Text;
                    try
                    {

                        foreach (var column in _customTableElement.Columns.Select(c => c.Xml))
                        {
                            var dc = new DataColumn((string?)column.Attribute("Id") ?? string.Empty);

                            ColumnDataType colDT = ColumnDataType.Unknown;
                            int width = 0;
                            int length = 0;

                            foreach (var attribute in column.Attributes())
                            {
                                switch (attribute.Name.LocalName)
                                {
                                    case "Nullable":
                                        if (attribute.Value == "no")
                                            dc.AllowDBNull = false;
                                        else
                                            dc.AllowDBNull = true;
                                        break;

                                    case "PrimaryKey":
                                        if (attribute.Value == "no")
                                            dc.Unique = false;
                                        else
                                            dc.Unique = true;
                                        break;

                                    case "Type":
                                        switch (attribute.Value)
                                        {
                                            case "binary":
                                                colDT = ColumnDataType.String; // We don't really store blobs in wxs files.
                                                break;
                                            case "integer":
                                                colDT = ColumnDataType.Int;
                                                break;
                                            case "string":
                                                colDT = ColumnDataType.String;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;

                                    case "Width":
                                        width = Convert.ToInt32(attribute.Value);
                                        break;

                                    case "Length":
                                        length = Convert.ToInt32(attribute.Value);
                                        break;

                                    default:
                                        break;
                                }
                            }

                            if (colDT == ColumnDataType.Int)
                            {
                                if (width == 2)
                                {
                                    dc.DataType = typeof(UInt32);
                                }

                                if (width == 4)
                                {
                                    dc.DataType = typeof(UInt64);
                                }
                            }

                            if (colDT == ColumnDataType.String)
                            {
                                dc.DataType = typeof(string);
                                if (length > 0)
                                {
                                    dc.MaxLength = length;
                                }
                            }


                            _customTable.Columns.Add(dc);
                        }

                        _rowElements = CustomTableDocument.LoadRows(_customTableElement, _customTable);
                        _customTable.AcceptChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Malformed XML detected (Get Table '" + listBoxTables.Text + "' )" + ex.Message);
                        dataGridViewRows.DataSource = null;
                    }
                }
            }
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

        private void toolStripMenuItemRowAdd_Click(object sender, EventArgs e)
        {
            bool added = false;
            Int16 index = 0;

            do
            {
                try
                {
                    index++;
                    DataRow dr = _customTable.NewRow();

                    foreach (DataColumn dc in _customTable.Columns)
                    {
                        if (!dc.AllowDBNull)
                        {
                            if (dc.Unique)
                            {
                                dr[dc] = "Id" + index.ToString();
                            }
                            else
                            {
                                dr[dc] = "0";
                            }
                        }
                    }
                    _customTable.Rows.Add(dr);
                    added = true;
                }
                catch (Exception)
                {
                }
            }
            while (added == false);
            _customTable.AcceptChanges();
            SaveTable();
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

        public enum ColumnDataType
        {
            Unknown,
            String,
            Int,
            Binary,
        } ;

        private static bool IsYes(WixValue<bool>? value) => value is { IsLiteral: true } literal && literal.Value;

        private Wxs.Column FindColumn(string id) => _customTableElement.Columns.First(c => c.Id == id);
        private void dataGridViewRows_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(e.Exception?.Message);
        }

        private void listViewColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewColumns.SelectedItems.Count > 0)
            {
                var column = FindColumn(listViewColumns.SelectedItems[0].Text);

                textBoxName.Text = column.Id;
                textBoxType.Text = column.GetAttributeValue("Type") ?? string.Empty;
                textBoxLength.Text = column.GetAttributeValue("Width") ?? string.Empty;
                checkBoxNullable.Checked = IsYes(column.Nullable);

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

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CustomTableDocument.Rename(_documentManager.Document, _customTableElement, dialog.TableName);
                LoadData();
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

            if (dialog.ShowDialog() == DialogResult.OK)
            { 
                CustomTableDocument.Create(_documentManager.Document, dialog.TableName);
                LoadData();
            }
        }

        private void labelName_Click(object sender, EventArgs e)
        {

        }
    }
}