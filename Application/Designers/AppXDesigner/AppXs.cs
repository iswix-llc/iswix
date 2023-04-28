﻿using System;
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

namespace AppXDesigner
{
    public partial class AppXs : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        bool _fgWiXInstalled;
        IsWiXFGAppXs _isWiXFGAppXs;

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


        public AppXs()
        {
            InitializeComponent();
            _fgWiXInstalled = IsWiXFGAppXs.IsFGWiXInstalled();
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
            DocumentManager documentManager = DocumentManager.DocumentManagerInstance;
            return (documentManager.Document.GetWiXVersion() == WiXVersion.v3 && documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product);
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
                contextMenuStripAppX.Items.Clear();
            }
        }

        private  void LoadDocument()
        {
            panelTop.Height = 0;
            _isWiXFGAppXs = new IsWiXFGAppXs(_documentManager.Document);

            contextMenuStripAppX.Items["toolStripMenuItemRename"].Enabled = false;
            contextMenuStripAppX.Items["toolStripMenuItemDelete"].Enabled = false;

            treeViewAppXs.Nodes.Clear();
            foreach (var isWiXFGAppX in _isWiXFGAppXs)
            {
                contextMenuStripAppX.Items["toolStripMenuItemRename"].Enabled = true;
                contextMenuStripAppX.Items["toolStripMenuItemDelete"].Enabled = true;
                AddAppXNode(isWiXFGAppX);
            }


        }


        private void AddAppXNode(IsWiXFGAppX isWiXFGAppX)
        {
            var subTreeNode = treeViewAppXs.Nodes.Add(isWiXFGAppX.Id);
            subTreeNode.ImageIndex = (int)ImageLibrary.Services;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Services;
            subTreeNode.Tag = isWiXFGAppX;
            treeViewAppXs.ExpandAll();
            treeViewAppXs.Select();
            UpdatedSelectedNodeText();
        }

        private void UpdatedSelectedNodeText()
        {
            IsWiXFGAppX appx = treeViewAppXs.SelectedNode.Tag as IsWiXFGAppX;
            if (appx != null)
            {
                treeViewAppXs.SelectedNode.Text = appx.Id;
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
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("AppXDesigner.AppX.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("AppXDesigner.License.txt")).ReadToEnd();
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginName
        {
            get { return "AppX"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group2_appx"; }
        }

        private void linkLabelRequirements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DisplayFGWiXMessage();
        }

        private void treeViewAppXs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.Enabled = true;
            propertyGrid1.SelectedObject = appX1;
            appX1.Read(e.Node.Tag as IsWiXFGAppX);
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            appX1.Write(e.ChangedItem.Label);
            UpdatedSelectedNodeText();
        }

        private void toolStripMenuItemNewFeature_Click(object sender, EventArgs e)
        {
            IsWiXFGAppXs appxs = new IsWiXFGAppXs(_documentManager.Document);
            string appxName = IsWiXFGAppXs.SuggestNextAppXName(_documentManager.Document);
            IsWiXFGAppX appx = appxs.Create(appxName, "CN=YourCompanyName", TargetType.desktop);
            TreeNode node = treeViewAppXs.Nodes.Add(appx.Id);
            node.SelectedImageIndex = (int)ImageLibrary.Services;
            node.ImageIndex = (int)ImageLibrary.Services;
            node.Tag = appx;
            treeViewAppXs.SelectedNode = node;
            contextMenuStripAppX.Items["toolStripMenuItemRename"].Enabled = true;
            contextMenuStripAppX.Items["toolStripMenuItemDelete"].Enabled = true;

            _isWiXFGAppXs.SortXML();
        }

        private void toolStripMenuItemRename_Click(object sender, EventArgs e)
        {
            treeViewAppXs.SelectedNode.BeginEdit();
        }

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            IsWiXFGAppX isWiXFGAppX = treeViewAppXs.SelectedNode.Tag as IsWiXFGAppX;
            isWiXFGAppX.Delete();
            treeViewAppXs.SelectedNode.Remove();
            if(treeViewAppXs.Nodes.Count>0)
            {
                treeViewAppXs.SelectedNode = treeViewAppXs.Nodes[0];
            }
            else
            {
                propertyGrid1.Enabled = false;
                appX1 = new AppX();
                propertyGrid1.SelectedObject = appX1;
            }

            if(treeViewAppXs.Nodes.Count == 0)
            {
                contextMenuStripAppX.Items["toolStripMenuItemRename"].Enabled = false;
                contextMenuStripAppX.Items["toolStripMenuItemDelete"].Enabled = false;

            }
            _isWiXFGAppXs.SortXML();
        }

        private void treeViewAppXs_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Label))
                {
                    e.CancelEdit = true;
                }
                else
                {
                    IsWiXFGAppX isWiXFGAppX = treeViewAppXs.SelectedNode.Tag as IsWiXFGAppX;
                    isWiXFGAppX.Id = e.Label;
                    appX1.Id = isWiXFGAppX.Id;
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
