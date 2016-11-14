using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using IsWiXAutomationInterface;

namespace ServicesDesigner
{
    public partial class ComponentPicker : Form
    {
        string _fileKey = string.Empty;
        IsWiXServices _services;

        public string FileKey  
        { 
            get
            { 
                return _fileKey;
            }
        }

        public ComponentPicker(IsWiXServices services)
        {
            _services = services;
            InitializeComponent();
            PopulateListBox();
            if(treeView1.Nodes.Count==0)
            {
                label1.Text = "No files were found to be suitable for defining a service.";
           
                treeView1.Visible = false;
            }
        }


        private void PopulateListBox()
        {

            foreach (var candidate in _services.GetServiceCandidates())
            {
                TreeNode node = treeView1.Nodes.Add(candidate.Value);
                node.Tag = candidate.Key;
            }

        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _fileKey = treeView1.SelectedNode.Tag as string;
            buttonSelect.Enabled=true;
        }

    }
}
