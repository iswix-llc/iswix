using System;
using System.Collections;
using System.Collections.Generic;

using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.ComponentModel;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiXAutomationInterface;
using FireworksFramework.Managers;
using static FireworksFramework.Types.Enums;

namespace FeaturesDesigner
{
    public partial class Features : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        bool _refresh = false;
        public Features()
        {
            InitializeComponent();
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {
            var docType = _documentManager.Document.GetDocumentType();

            if (docType == IsWiXDocumentType.Product || docType == IsWiXDocumentType.Fragment)
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
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("FeaturesDesigner.Features.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("FeaturesDesigner.License.txt")).ReadToEnd();
            }
        }

        public string PluginName
        {
            get { return "Features"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group2_Features"; }
        }

        public PluginType PluginType
        {
            get { return PluginType.Designer; }
        }

        public void LoadData()
        {
            LoadData2();
        }

        #endregion

        private void LoadData2()
        {
            _refresh = false;
            IsWiXFeatures features = new IsWiXFeatures();

            treeViewFeatures.Nodes.Clear();
            ClearPropertyGrid();
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemRename.Enabled = false;
            toolStripMenuItemNewSubFeature.Enabled = false;

            foreach (var feature in features)
            {
                var treeNode = treeViewFeatures.Nodes.Add(feature.Id, string.Format("{0}", feature.Id));
                treeNode.Tag = feature;
                LoadDataRecurse(treeNode, feature.Id);

            }
            UpdateMergeModules();
            if (treeViewFeatures.Nodes.Count > 0)
            {
                _refresh = true;
                treeViewFeatures.Select();
                treeViewFeatures.SelectedNode = treeViewFeatures.Nodes[0];
                treeViewFeatures.ExpandAll();
                
            }
        }

        private void LoadDataRecurse(TreeNode parentNode, string parentId)
        {
            IsWiXFeatures features = new IsWiXFeatures(parentId);
            foreach (var feature in features)
            {
                var treeNode = parentNode.Nodes.Add(feature.Id, string.Format("{0}", feature.Id));
                treeNode.Tag = feature;
                LoadDataRecurse(treeNode, feature.Id);

            }
        }

        private void UpdateMergeModules()
        {

            if (treeViewFeatures.Nodes.Count > 0)
            {
                treeViewMerges.Enabled = true;
            }
            else
            {
                treeViewMerges.Enabled = false;
            }

            treeViewMerges.Nodes.Clear();
            IsWiXMerges merges = new IsWiXMerges();
            foreach (var merge in merges)
            {
                TreeNode mergeNode = treeViewMerges.Nodes.Add(merge.Id, merge.ToString(), 1);
                mergeNode.Tag = merge;
            }
            if(merges.Count>0)
            {
                textBox1.Text = "The above merge modules were found in this WXS file.\r\nPlease select which files should be referenced by the selected feature.";
            }
            else
            {
                textBox1.Text = "No merge modules were found in this WXS file.";
            }


        }
        private void treeViewFeatures_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_refresh)
            {
                IsWiXFeature iswixFeature = treeViewFeatures.SelectedNode.Tag as IsWiXFeature;
                feature.Read(iswixFeature);

                UpdatePropertyGrid();
                UpdateMergeModules();

                foreach (var mergeRef in iswixFeature.MergeRefs.Values)
                {
                    var nodes = treeViewMerges.Nodes.Find(mergeRef.Id, true);
                    foreach (var node in nodes)
                    {
                        node.Checked = true;
                    }
                }
                toolStripMenuItemNewSubFeature.Enabled = true;
                toolStripMenuItemRename.Enabled = true;
                toolStripMenuItemDelete.Enabled = true;

                UpdateMoveOptions();
                propertyGridFeatureProperties.ExpandAllGridItems();
            }
        }

        private void propertyGridFeatureProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            feature.Write(e.ChangedItem.Label);
        }
        private void treeViewMerges_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (treeViewFeatures.SelectedNode != null)
            {
                IsWiXFeature feature = treeViewFeatures.SelectedNode.Tag as IsWiXFeature;
                IsWiXMerge merge = e.Node.Tag as IsWiXMerge;
                IsWiXMergeRefs mergeRefs = feature.MergeRefs;
                propertyGridFeatureProperties.Refresh();

                if (e.Node.Checked)
                {


                    if (!mergeRefs.Keys.Contains(merge.Id))
                    {
                        IsWiXMergeRef mergeRef = new IsWiXMergeRef();
                        mergeRef.Id = merge.Id;
                        mergeRefs.Add(mergeRef.Id, mergeRef);
                        mergeRefs.Save();
                    }
                }
                else
                {
                    if (feature.MergeRefs.Keys.Contains(merge.Id))
                    {
                        mergeRefs.Remove(merge.Id);
                        mergeRefs.Save();
                    }
                }
            }
        }

        private void treeViewFeatures_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Label))
                {
                    e.CancelEdit = true;
                }
                else
                {
                    IsWiXFeature iswixFeature = treeViewFeatures.SelectedNode.Tag as IsWiXFeature;
                    iswixFeature.Id = e.Label;
                    feature.Id = iswixFeature.Id;
                }

                propertyGridFeatureProperties.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                e.CancelEdit = true;
            }
        }

        private void toolStripMenuItemRename_Click(object sender, EventArgs e)
        {
            treeViewFeatures.SelectedNode.BeginEdit();
        }

        private void toolStripMenuItemNewFeature_Click(object sender, EventArgs e)
        {
            if (treeViewFeatures.SelectedNode == null)//|| treeViewFeatures.SelectedNode.Parent == null)
            {
                IsWiXFeatures features = new IsWiXFeatures();
                string featureName = IsWiXFeatures.SuggestNextFeatureName();
                IsWiXFeature feature = features.Create(featureName);
                TreeNode node = treeViewFeatures.Nodes.Add(feature.Id);
                node.Tag = feature;
                _refresh = true;
                treeViewFeatures.SelectedNode = node;
            }
            else
            {
                IsWiXFeatures features = new IsWiXFeatures();
                string featureName = IsWiXFeatures.SuggestNextFeatureName();
                var iswixFeature = treeViewFeatures.SelectedNode.Tag as IsWiXFeature;
                IsWiXFeature feature = features.Create(iswixFeature.Id, featureName);

                TreeNode node = treeViewFeatures.Nodes.Insert(treeViewFeatures.SelectedNode.Index + 1, feature.Id);
                node.Tag = feature;
                _refresh = true;
                treeViewFeatures.SelectedNode = node;

            }
            treeViewFeatures.SelectedNode.BeginEdit();
            
        }

   

        private void toolStripMenuItemNewSubFeature_Click(object sender, EventArgs e)
        {
            IsWiXFeatures features = new IsWiXFeatures();
            string featureName = IsWiXFeatures.SuggestNextFeatureName();
            var iswixFeature = treeViewFeatures.SelectedNode.Tag as IsWiXFeature;
            IsWiXFeature feature = features.CreateSubFeature(iswixFeature.Id, featureName);

            TreeNode node = treeViewFeatures.SelectedNode.Nodes.Add(featureName);
            node.Tag = feature;
            _refresh = true;
            treeViewFeatures.SelectedNode = node;
            treeViewFeatures.SelectedNode.BeginEdit();
            
        }

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemRename.Enabled = false;
            toolStripMenuItemNewSubFeature.Enabled = false;

            IsWiXFeature feature = treeViewFeatures.SelectedNode.Tag as IsWiXFeature;
            feature.Delete();
            treeViewFeatures.SelectedNode.Remove();

            if (treeViewFeatures.Nodes.Count > 0)
            {
                _refresh = true;
                treeViewFeatures.Select();
                treeViewFeatures.SelectedNode = treeViewFeatures.Nodes[0];
                treeViewFeatures.ExpandAll();

            }

        }

        private void toolStripMenuItemMoveUp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: Move Up (Developers Wanted)");
        }

        private void toolStripMenuItemMoveDown_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: Move Down (Developers Wanted)");
        }

        private void toolStripMenuItemMoveLeft_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: Move Left (Developers Wanted");
        }

        private void toolStripMenuItemMoveRight_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: Move Right (Developers Wanted)");
        }

        private void treeViewFeatures_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeViewFeatures.SelectedNode = e.Node;
            }

        }

        private void ClearPropertyGrid()
        {
            feature.Clear();
            propertyGridFeatureProperties.Enabled = false;
            propertyGridFeatureProperties.Refresh();
        }

        private void UpdatePropertyGrid()
        {
            propertyGridFeatureProperties.Enabled = true;
            propertyGridFeatureProperties.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridFeatureProperties_PropertyValueChanged);
            propertyGridFeatureProperties.Refresh();
        }

        private void UpdateMoveOptions()
        {
            if (treeViewFeatures.SelectedNode.Index > 0)
            {
                toolStripMenuItemMoveUp.Enabled = true;
            }
            else
            {
                toolStripMenuItemMoveUp.Enabled = false;
            }


            int siblings;
            if (treeViewFeatures.SelectedNode.Parent == null)
            {
                siblings = treeViewFeatures.Nodes.Count;
            }
            else
            {
                siblings = treeViewFeatures.SelectedNode.Parent.Nodes.Count;
            }

            if (treeViewFeatures.SelectedNode.Index < siblings - 1)
            {
                toolStripMenuItemMoveDown.Enabled = true;
            }
            else
            {
                toolStripMenuItemMoveDown.Enabled = false;
            }
        }

    }
}
