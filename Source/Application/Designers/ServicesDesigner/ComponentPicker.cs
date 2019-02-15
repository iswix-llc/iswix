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
using IsWiXAutomationInterface;

namespace ServicesDesigner
{
    public partial class ComponentPicker : Form
    {
        string _fileKey = string.Empty;
        IsWiXServices _services;
        string _fileName = string.Empty;

        public string FileKey  
        { 
            get
            { 
                return _fileKey;
            }
        }


        public string FileName
        {
            get
            {
                return _fileName;
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
                TreeNode node = treeView1.Nodes.Add(candidate.DestinationFilePath);
                node.Tag = candidate;
            }

        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ServiceCandidate sc=treeView1.SelectedNode.Tag as ServiceCandidate;
            _fileKey = sc.Id;
            _fileName = sc.FileName;

            buttonSelect.Enabled=true;
        }

    }
}
