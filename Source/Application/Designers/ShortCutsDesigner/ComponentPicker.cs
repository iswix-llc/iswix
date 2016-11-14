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

namespace ShortCutsDesigner
{
    public partial class ComponentPicker : Form
    {
        string _fileKey = string.Empty;
        IsWiXShortCuts _shortCuts;

        public string FileKey  
        { 
            get
            { 
                return _fileKey;
            }
        }

        public ComponentPicker(IsWiXShortCuts shortCuts)
        {
            _shortCuts = shortCuts;
            InitializeComponent();
            PopulateListBox();
            if(treeView1.Nodes.Count==0)
            {
                label1.Text = "No files were found to be suitable for creating a shortcut.";
                treeView1.Visible = false;
            }
        }


        private void PopulateListBox()
        {

            foreach (var candidate in _shortCuts.GetShortCutCandidates())
            {
                TreeNode node = treeView1.Nodes.Add(candidate.Value);
                node.Tag = candidate.Key;
            }

        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _fileKey = treeView1.SelectedNode.Tag as string;
            buttonSelect.Enabled = true;
        }

    }
}
