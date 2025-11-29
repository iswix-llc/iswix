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
using IsWiXAutomationInterface;
using static FireworksFramework.Types.Enums;

namespace CustomTablesDesigner
{
    public partial class CustomTablesDesigner : UserControl, IFireworksDesigner
    {
        XNamespace ns;
        DataTable _customTable = new DataTable();
        XElement _customTableElement;
        DocumentManager _documentManager;

        public CustomTablesDesigner()
        {
            InitializeComponent();
            dataGridViewRows.DataSource = _customTable;
            _documentManager = DocumentManager.DocumentManagerInstance;
        }

        void _customTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            SaveTable();
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {

            IsWiXDocumentType docType = _documentManager.Document.GetDocumentType();

            if (docType != IsWiXDocumentType.None)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadData()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            _customTable.RowChanged -= new DataRowChangeEventHandler(_customTable_RowChanged);

            _customTable.Clear();

            listBoxTables.Items.Clear();
            listViewColumns.Items.Clear();

            try
            {
                XElement moduleElement = _documentManager.Document.GetSecondOrderRoot();
                foreach (var tableElement in moduleElement.Elements(ns + "CustomTable"))
                {
                    listBoxTables.Items.Add(tableElement.Attribute("Id").Value);
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
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomTablesDesigner.CustomTables.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomTablesDesigner.License.txt")).ReadToEnd();
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
            _customTable.Clear();
            _customTable.Rows.Clear();
            _customTable.Constraints.Clear();
            _customTable.Columns.Clear();
            listViewColumns.Items.Clear();
            
            textBoxName.Text = "";
            textBoxName.Enabled = false;

            comboBoxType.Text = "";
            comboBoxType.Enabled = false;

            textBoxLength.Text = "";
            textBoxLength.Enabled = false;

            checkBoxNullable.Checked = false;
            checkBoxNullable.Enabled = false;
            if (listBoxTables.SelectedItems.Count > 0)
            {

                _customTableElement = (from item in _documentManager.Document.Descendants(ns + "CustomTable")
                                       where
                                           item.Attribute("Id") != null &&
                                           item.Attribute("Id").Value == listBoxTables.SelectedItem.ToString()
                                       select item).First();

                foreach (var column in _customTableElement.Elements(ns + "Column"))
                {
                    var item = listViewColumns.Items.Add(column.Attribute("Id").Value);

                    try
                    {
                        if (column.Attribute("PrimaryKey").Value == "yes")
                        {
                            item.ImageKey = "PrimaryKey";
                        }
                    }
                    catch (Exception)
                    {
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
                            XElement columnElement = (from myitem in _customTableElement.Elements(ns + "Column")
                                                      where myitem.Attribute("Id").Value == item.Text
                                                      select myitem).First();
                            columnElement.Attribute("PrimaryKey").Value = "yes";
                        }
                    }
                    else
                    {
                        if (item.ImageKey == "PrimaryKey")
                        {
                            if (isPreviousPK == false)
                            {
                                MessageBox.Show("Notice: Marking column '" + item.Text + "' as non-primary due to Window Installer requirements.");
                                item.ImageKey = null;
                                XElement columnElement = (from myitem in _customTableElement.Elements(ns + "Column")
                                                          where myitem.Attribute("Id").Value == item.Text
                                                          select myitem).First();
                                columnElement.Attribute("PrimaryKey").Value = "no";
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
                    _customTable.TableName = listBoxTables.Text;
                    try
                    {

                        XElement customTableElement = (from item in _documentManager.Document.Descendants(ns + "CustomTable")
                                                       where
                                                           item.Attribute("Id") != null &&
                                                           item.Attribute("Id").Value == listBoxTables.Text
                                                       select item).First();

                        foreach (var column in customTableElement.Elements(ns + "Column"))
                        {
                            var dc = new DataColumn(column.Attribute("Id").Value);

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

                        foreach (var row in customTableElement.Elements(ns + "Row"))
                        {
                            DataRow dr = _customTable.NewRow();

                            foreach (var data in row.Elements(ns + "Data"))
                            {
                                string columnName = data.Attribute("Column").Value;
                                if (_documentManager.Document.GetWiXVersion() == WiXVersion.v4)
                                {
                                    dr[columnName] = data.GetOptionalAttribute("Value");
                                }
                                else
                                {
                                    dr[columnName] = data.Value;
                                }
                            }
                            _customTable.Rows.Add(dr);

                        }
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
            XElement customTableElement = (from item in _documentManager.Document.Descendants(ns + "CustomTable")
                                           where
                                               item.Attribute("Id") != null &&
                                               item.Attribute("Id").Value == listBoxTables.Text
                                           select item).First();


            var rows = customTableElement.Elements(ns + "Row");
            rows.Remove();

            foreach (DataRow row in _customTable.Rows)
            {
                var xrow = new XElement(ns + "Row");
                foreach (DataColumn column in _customTable.Columns)
                {
                    var xcolumn = new XElement(ns + "Data");
                    xcolumn.Add(new XAttribute("Column", column.ColumnName));
                    if (_documentManager.Document.GetWiXVersion() == WiXVersion.v4)
                    {
                        xcolumn.SetAttributeValue("Value", row[column.ColumnName].ToString());
                    }
                    else
                    {
                        xcolumn.Value = row[column.ColumnName].ToString();
                    }
                    xrow.Add(xcolumn);

                }
                customTableElement.Add(xrow);
            }

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
            XElement customTableElement = (from item in _documentManager.Document.Descendants(ns + "CustomTable")
                                           where
                                               item.Attribute("Id") != null &&
                                               item.Attribute("Id").Value == listBoxTables.SelectedItem.ToString()
                                           select item).First();

            customTableElement.Remove();
            listBoxTables.Items.Remove(listBoxTables.SelectedItem);
        }

        private void listBoxTables_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point p = new Point(e.X, e.Y);
                listBoxTables.SelectedIndex = listBoxTables.IndexFromPoint(p);
            }
        }
        private void dataGridViewRows_CellValueChanged(object sender, DataGridViewCellEventArgs e)
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

        private bool IsAttributeYes(XElement element, string attributeName)
        {
            bool set = false;

            try
            {
                if (element.Attribute(attributeName).Value == "yes")
                {
                    set = true;
                }
            }
            catch (Exception)
            {
            }

            return set;
        }
        private void dataGridViewRows_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        private void listViewColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewColumns.SelectedItems.Count > 0)
            {
                XElement columnElement = (from item in _customTableElement.Elements(ns + "Column")
                                          where item.Attribute("Id").Value == listViewColumns.SelectedItems[0].Text
                                          select item).First();

                textBoxName.Text = columnElement.Attribute("Id").Value;
                //textBoxName.Enabled = true;

                comboBoxType.Text = columnElement.Attribute("Type").Value;
                //comboBoxType.Enabled = true;

                textBoxLength.Text = columnElement.Attribute("Width").Value;
                //textBoxLength.Enabled = true;

                checkBoxNullable.Checked = IsAttributeYes(columnElement, "Nullable");
                //checkBoxNullable.Enabled = true;

            }
        }

        private void toolStripMenuItemRenameTable_Click(object sender, EventArgs e)
        {
            List<string> tables = new List<string>();

            foreach (var item in listBoxTables.Items)
            {
                tables.Add(item.ToString());
            }
            var dialog = new FormTableName( listBoxTables.Text, tables );

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _customTableElement.Attribute("Id").Value = dialog.TableName;
                LoadData();
            }
        }

        private void toolStripMenuItemTableCreate_Click(object sender, EventArgs e)
        {
            List<string> tables = new List<string>();

            foreach (var item in listBoxTables.Items)
            {
                tables.Add(item.ToString());
            }
            var dialog = new FormTableName(string.Empty, tables);

            if (dialog.ShowDialog() == DialogResult.OK)
            { 
                string identifier = "Identifier";
                if(_documentManager.Document.GetWiXVersion() == WiXVersion.v4)
                {
                    identifier = identifier.ToLower();
                }
                XElement previousElement = _documentManager.Document.GetElementToAddAfterSelf("CustomTable");
                XElement newElement =    new XElement(ns + "CustomTable", new XAttribute("Id", dialog.TableName),
                    new XElement( ns + "Column",
                        new XAttribute( "Id", "Id" ),
                        new XAttribute( "Category", identifier ),
                        new XAttribute( "PrimaryKey", "yes" ),
                        new XAttribute( "Nullable", "no" ),
                        new XAttribute( "Type", "string" ),
                        new XAttribute( "Width", "72" )));

                if (previousElement == null)
                {
                    _documentManager.Document.GetSecondOrderRoot().AddFirst(newElement);
                }
                else
                {
                    previousElement.AddAfterSelf(newElement);
                }
                if (_documentManager.Document.GetWiXVersion() == WiXVersion.v4)
                {
                    newElement.AddAfterSelf(new XElement(ns + "EnsureTable", new XAttribute("Id", dialog.TableName)));
                }
                LoadData();
            }
        }

        private void labelName_Click(object sender, EventArgs e)
        {

        }
    }
}