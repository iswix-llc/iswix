using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using IsWiXAutomationInterface;

namespace GeneralInformationDesigner
{
    public partial class Dependency : UserControl
    {
        IsWiXDependencies _dependencies;
        
        public Dependency()
        {
            InitializeComponent();
        }

        public void Read( XDocument Document )
        {
            _dependencies = new IsWiXDependencies(Document);

            dependencies.Tables[0].RowDeleting -= new DataRowChangeEventHandler(Dependencies_RowAddRemove);
            dependencies.Tables[0].RowChanging -= new DataRowChangeEventHandler(Dependencies_RowAddRemove);
            dependencies.Tables[0].Clear();
            
            try
            {
                foreach (var dependency in _dependencies)
                {
                    dependencies.Tables[0].Rows.Add(new object[] { dependency.RequiredId, dependency.RequiredLanguage, dependency.RequiredVersion });
                }

            }
            catch (Exception ex )
            {
                MessageBox.Show(ex.Message);
            }

            dependencies.Tables[0].RowChanging += new DataRowChangeEventHandler(Dependencies_RowAddRemove);
            dependencies.Tables[0].RowDeleting += new DataRowChangeEventHandler(Dependencies_RowAddRemove);
        }

        public void Dependencies_RowAddRemove(object sender, DataRowChangeEventArgs e)
        {

            var dependency = new IsWiXDependency(
                e.Row["RequiredId"].ToString(),
                e.Row["RequiredLanguage"].ToString(),
                e.Row["RequiredVersion"].ToString()
                );

            switch (e.Action)
            {
                case DataRowAction.Add:
                    _dependencies.Add(dependency);
                    break;

                case DataRowAction.Delete:
                    _dependencies.Remove(dependency);
                    break;
            }
        }

        private void dataGridViewDependencies_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewDependencies.SelectedRows.Count > 0)
            {
                buttonRemove.Enabled = true;
            }
            else
            {
                buttonRemove.Enabled = false;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            fileDialog.FileName = "";
            fileDialog.RestoreDirectory = true;
            fileDialog.Filter = "MSI Merge Modules (*.msm)|*.msm";
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in fileDialog.FileNames)
                {
                    try
                    {
                        var dependency = new IsWiXDependency(fileName);
                        dependencies.Tables[0].Rows.Add(new object[] { dependency.RequiredId, dependency.RequiredLanguage, dependency.RequiredVersion });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            foreach (var selectedRow in dataGridViewDependencies.SelectedRows)
            {
                dataGridViewDependencies.Rows.Remove((DataGridViewRow)selectedRow);
            }      
        }
    }

}
