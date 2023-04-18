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
using static FireworksFramework.Types.Enums;

namespace MSIXDesigner
{
    public partial class MSIXs : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        bool _fgWiXInstalled;
        IsWiXFGMSIXs _isWiXFGMSIXs;

        private enum ImageLibrary
        {
            FolderOpen,
            FolderClosed,
            Computer,
            Dll,
            File,
            Web,
            Sql,
            Services,
            Xml,
            BlueFolderClosed,
            FileNotPresent,
            BlueFolderOpen
        }


        public MSIXs()
        {
            InitializeComponent();
            _fgWiXInstalled = IsWiXFGMSIXs.IsFGWiXInstalled();
        }

        public bool SuppressFireGiantMessage
        {
            get
            {
                bool suppress = false;
                if (Properties.Settings.Default.SuppressFireGiantMessage.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    suppress = true;
                }
                return suppress;
            }
        }
        public bool IsValidContext()
        {
            bool valid = false;
            IsWiXDocumentType documentType = _documentManager.Document.GetDocumentType();

            if (documentType == IsWiXDocumentType.Product || documentType == IsWiXDocumentType.Fragment)
            {
                valid = true;
            }
            return valid;
        }

        public void LoadData()
        {
            if (_fgWiXInstalled)
            {
                linkLabelRequirements.Visible = false;
                LoadDocument();
            }
            else
            {
                contextMenuStripMSIX.Items.Clear();
            }

        }

        private void LoadDocument()
        {
            panelTop.Height = 0;
            _isWiXFGMSIXs = new IsWiXFGMSIXs(_documentManager.Document);
            contextMenuStripMSIX.Items["toolStripMenuItemRename"].Enabled = false;
            contextMenuStripMSIX.Items["toolStripMenuItemDelete"].Enabled = false;

            treeViewMSIXs.Nodes.Clear();
            foreach (var isWiXFGMSIX in _isWiXFGMSIXs)
            {
                contextMenuStripMSIX.Items["toolStripMenuItemRename"].Enabled = true;
                contextMenuStripMSIX.Items["toolStripMenuItemDelete"].Enabled = true;
                AddMSIXNode(isWiXFGMSIX);
            }


        }


        private void AddMSIXNode(IsWiXFGMSIX isWiXFGMSIX)
        {
            var subTreeNode = treeViewMSIXs.Nodes.Add(isWiXFGMSIX.Id);
            subTreeNode.ImageIndex = (int)ImageLibrary.Services;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Services;
            subTreeNode.Tag = isWiXFGMSIX;
            treeViewMSIXs.ExpandAll();
            treeViewMSIXs.Select();
            UpdatedSelectedNodeText();
        }

        private void UpdatedSelectedNodeText()
        {
            IsWiXFGMSIX msix = treeViewMSIXs.SelectedNode.Tag as IsWiXFGMSIX;
            if (msix != null)
            {
                treeViewMSIXs.SelectedNode.Text = msix.Id;
            }
        }

        private void DisplayFGWiXMessage()
        {
            FireGiantWiXMessage fireGiantWiXMessage = new FireGiantWiXMessage();
            fireGiantWiXMessage.ShowDialog();
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("MSIXDesigner.MSIX.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("MSIXDesigner.License.txt")).ReadToEnd();
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginName
        {
            get { return "MSIX"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group2_appx_msix"; }
        }

        private void linkLabelRequirements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DisplayFGWiXMessage();
        }

        private void treeViewMSIXs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.Enabled = true;
            propertyGrid1.SelectedObject = msix1;
            msix1.Read(e.Node.Tag as IsWiXFGMSIX);
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            msix1.Write(e.ChangedItem.Label);
            UpdatedSelectedNodeText();
        }

        private void toolStripMenuItemNewFeature_Click(object sender, EventArgs e)
        {
            IsWiXFGMSIXs msixs = new IsWiXFGMSIXs(_documentManager.Document);
            string msixName = IsWiXFGMSIXs.SuggestNextMSIXName(_documentManager.Document);
            IsWiXFGMSIX msix = msixs.Create(msixName, "CN=, O=, STREET=, L=, S=, PostalCode=, C=", TargetType.desktop);
            TreeNode node = treeViewMSIXs.Nodes.Add(msix.Id);
            node.SelectedImageIndex = (int)ImageLibrary.Services;
            node.ImageIndex = (int)ImageLibrary.Services;
            node.Tag = msix;
            treeViewMSIXs.SelectedNode = node;
            contextMenuStripMSIX.Items["toolStripMenuItemRename"].Enabled = true;
            contextMenuStripMSIX.Items["toolStripMenuItemDelete"].Enabled = true;

            _isWiXFGMSIXs.SortXML();
        }

        private void toolStripMenuItemRename_Click(object sender, EventArgs e)
        {
            treeViewMSIXs.SelectedNode.BeginEdit();
        }

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            IsWiXFGMSIX isWiXFGMSIX = treeViewMSIXs.SelectedNode.Tag as IsWiXFGMSIX;
            isWiXFGMSIX.Delete();
            treeViewMSIXs.SelectedNode.Remove();
            if (treeViewMSIXs.Nodes.Count > 0)
            {
                treeViewMSIXs.SelectedNode = treeViewMSIXs.Nodes[0];
            }
            else
            {
                propertyGrid1.Enabled = false;
                msix1 = new MSIX();
                propertyGrid1.SelectedObject = msix1;
            }

            if (treeViewMSIXs.Nodes.Count == 0)
            {
                contextMenuStripMSIX.Items["toolStripMenuItemRename"].Enabled = false;
                contextMenuStripMSIX.Items["toolStripMenuItemDelete"].Enabled = false;

            }
            _isWiXFGMSIXs.SortXML();
        }

        private void treeViewMSIXs_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Label))
                {
                    e.CancelEdit = true;
                }
                else
                {
                    IsWiXFGMSIX isWiXFGMSIX = treeViewMSIXs.SelectedNode.Tag as IsWiXFGMSIX;
                    isWiXFGMSIX.Id = e.Label;
                    msix1.Id = isWiXFGMSIX.Id;
                }
                propertyGrid1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                e.CancelEdit = true;
            }
        }
    }
}
