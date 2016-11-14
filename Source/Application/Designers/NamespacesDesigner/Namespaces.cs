using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Xml.Linq;
using System.Text;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using IsWiXAutomationInterface;

namespace CustomTablesDesigner
{
    public partial class Namespaces : UserControl, IFireworksDesigner
    {
        XNamespace ns;
        IDesignerManager _mgr;
        WiXNamespaces _namespaces;

        public Namespaces()
        {
            InitializeComponent();
        }

        #region IFireworksDesigner Members

        public IDesignerManager DesignerManager
        {
            set
            {
                _mgr = value;
                ns = _mgr.DocumentManager.Document.GetWiXNameSpace();

            }
        }

        public bool IsValidContext()
        {
            if (_mgr.DocumentManager.DefaultNamespace == ns)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("NamespacesDesigner.Namespaces.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NamespacesDesigner.MS-PL.txt")).ReadToEnd();
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginName
        {
            get { return "Namespaces"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group3_namespaces"; }
        }

        #endregion

        public void LoadData()
        {
            _namespaces = new WiXNamespaces(_mgr.DocumentManager.Document);

            this.dataGridViewNamespaces.CurrentCellDirtyStateChanged -= new System.EventHandler(this.dataGridViewNamespaces_CurrentCellDirtyStateChanged);
            this.dataGridViewNamespaces.CellValueChanged -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewNamespaces_CellValueChanged);
            dataSetNamespaces.Tables[0].Clear();

            foreach (var item in _namespaces.PossibleNamespaces)
            {
                bool selected = false;
                if( _namespaces.ContainsKey( item.Key ))
                {
                    selected = true;
                }
                dataSetNamespaces.Tables[0].Rows.Add(new object[] { item.Key, item.Value, selected });
            }

            dataSetNamespaces.AcceptChanges();
            this.dataGridViewNamespaces.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewNamespaces_CellValueChanged);
            this.dataGridViewNamespaces.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewNamespaces_CurrentCellDirtyStateChanged);
        }

        private void dataGridViewNamespaces_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridViewNamespaces.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dataGridViewNamespaces_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if ((bool)dataGridViewNamespaces.CurrentCell.Value)
            {
                string name = dataGridViewNamespaces.CurrentRow.Cells[1].Value.ToString();
                string uri = dataGridViewNamespaces.CurrentRow.Cells[2].Value.ToString();
                _namespaces.Add(name, uri);
            }
            else
            {
                string name = dataGridViewNamespaces.CurrentRow.Cells[1].Value.ToString();
                _namespaces.Remove(name);
            }
        }
    }
}
