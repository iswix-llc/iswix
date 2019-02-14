using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiXAutomationInterface;
using FireworksFramework.Managers;
using static FireworksFramework.Types.Enums;

namespace PropertiesDesigner
{
    public partial class Properties : UserControl, IFireworksDesigner
    {
        IsWiXProperties _properties;
        string _selectedPropertyId;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;


        public Properties()
        {
            InitializeComponent();
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {
            if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.None)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void LoadData()
        {
            this.dataGridViewProperties.CurrentCellDirtyStateChanged -= new System.EventHandler(this.dataGridViewProperties_CurrentCellDirtyStateChanged);
            this.dataGridViewProperties.CellValueChanged -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewProperties_CellValueChanged);
            LoadProperties();
            dataGridViewProperties.Sort(dataGridViewProperties.Columns[0], ListSortDirection.Ascending);
            dataSetProperties.AcceptChanges();
            this.dataGridViewProperties.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewProperties_CellValueChanged);
            this.dataGridViewProperties.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewProperties_CurrentCellDirtyStateChanged);
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("PropertiesDesigner.Properties.ico"));
            }
        }

        public string PluginInformation
        {
            get 
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("PropertiesDesigner.License.txt")).ReadToEnd();
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginName
        {
            get { return "Properties"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group2_properties"; }
        }

        #endregion

        private void LoadProperties()
        {
            dataSetProperties.Tables[0].Rows.Clear();
            try
            {
                _properties = new IsWiXProperties();
                foreach (var property in _properties)
                {
                    dataSetProperties.Tables[0].Rows.Add(new object[] {
                        property.Id,
                        property.Value,
                        property.SuppressModularization,
                        property.Secure,
                        property.Hidden,
                        property.Admin });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridViewProperties_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var properties = from a in _properties
                             where a.Id == _selectedPropertyId
                             select a;
            IsWiXProperty iswixProperty = properties.First();

            // Unset all attributes so they can be recreated in order
            iswixProperty.Value = null;
            iswixProperty.Admin = false;
            iswixProperty.Secure = false;
            iswixProperty.SuppressModularization = false;
            iswixProperty.Hidden = false;
            
            // Add the attributes back in order
            iswixProperty.Id = dataGridViewProperties["idDataGridViewTextBoxColumn", e.RowIndex].Value.ToString();
            iswixProperty.Value = dataGridViewProperties["valueDataGridViewTextBoxColumn", e.RowIndex].Value.ToString();
            iswixProperty.Admin = (bool)dataGridViewProperties["adminDataGridViewCheckBoxColumn", e.RowIndex].Value;
            iswixProperty.Hidden = (bool)dataGridViewProperties["hiddenDataGridViewCheckBoxColumn", e.RowIndex].Value;
            iswixProperty.Secure = (bool)dataGridViewProperties["secureDataGridViewCheckBoxColumn", e.RowIndex].Value;
            iswixProperty.SuppressModularization = (bool)dataGridViewProperties["suppressModularizationDataGridViewCheckBoxColumn", e.RowIndex].Value;
 
            CommitChanges();
        }

        private void dataGridViewProperties_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {

            if (dataGridViewProperties.CurrentCell.ColumnIndex > 1 && dataGridViewProperties.IsCurrentCellDirty)
            {
                dataGridViewProperties.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool added = false;
            Int16 index = 0;

            do
            {
                try
                {
                    index++;
                    dataSetProperties.Tables[0].Rows.Add(new object[] { "NewProperty" + index.ToString(), string.Empty, false, false, false, false });
                    _properties.Create("NewProperty" + index.ToString());
                    added = true;
                }
                catch(Exception)
                {
                }
            }
            while (added == false);
            CommitChanges();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var selectedRow in dataGridViewProperties.SelectedRows)
            {
                DataGridViewRow row = selectedRow as DataGridViewRow;
                
                var properties = from a in _properties
                                 where a.Id == row.Cells[0].Value.ToString()
                                 select a;
                IsWiXProperty iswixProperty = properties.First();
                iswixProperty.Delete();
                _properties.Remove(iswixProperty);

                try
                {
                    string propertyId = row.Cells[0].Value.ToString();
                    
                    dataGridViewProperties.Rows.Remove(row);
                }
                catch (Exception)
                {
                }
            }
            CommitChanges();
        }

        private void CommitChanges()
        {
            try
            {
                dataSetProperties.AcceptChanges();
                dataSetProperties.Tables[0].AcceptChanges();
                _properties.SortXML();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridViewProperties_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
           // _selectedPropertyId = dataGridViewProperties[0, e.RowIndex].Value.ToString();

            if (e.ColumnIndex == 0)
            {
                string newId = e.FormattedValue.ToString();
                var regex = new Regex("^[a-z_]", RegexOptions.IgnoreCase);
                if (regex.Matches(newId).Count == 0)
                {
                    MessageBox.Show("Property names must begin a letter or underscore. (Additional restrictions may apply.)");
                    e.Cancel = true;
                }
            }
        }

        private void dataGridViewProperties_SelectionChanged(object sender, EventArgs e)
        {
            
            if (dataGridViewProperties.SelectedRows.Count == 0)
            {
                removeToolStripMenuItem.Enabled = false;
            }
            else
            {
                removeToolStripMenuItem.Enabled = true;
            }
        }

        private void dataGridViewProperties_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            _selectedPropertyId = dataGridViewProperties[0, e.RowIndex].Value.ToString();
        }

    }
}
