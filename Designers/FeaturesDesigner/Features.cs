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
using FireworksFramework.Services;
using static FireworksFramework.Types.Enums;
using Wxs = IsWiX.WiXModel;

namespace FeaturesDesigner
{
    public partial class Features : UserControl, IFireworksDesigner
    {
        readonly DocumentManager _documentManager;

        bool _refresh = false;
        bool _loadingMergeRefs = false;
        public Features(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            InitializeComponent();
            DesignerImages.Add(imageLibrary, typeof(Features).Assembly, "FeaturesDesigner.Images.", "feature", "database");
            AddToolbar();
        }

        // The tree's commands as buttons above it too: with no features the tree is empty, and nothing shows that
        // right-clicking it adds one.
        private void AddToolbar()
        {
            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            foreach (var menuItem in new[] { toolStripMenuItemNewFeature, toolStripMenuItemNewSubFeature, toolStripMenuItemRename, toolStripMenuItemDelete })
            {
                var button = new ToolStripButton(menuItem.Text) { DisplayStyle = ToolStripItemDisplayStyle.Text, Enabled = menuItem.Enabled };
                button.Click += (s, e) => RunAfterRename(menuItem.PerformClick);
                menuItem.EnabledChanged += (s, e) => button.Enabled = menuItem.Enabled;
                toolbar.Items.Add(button);
            }

            splitContainer1.Panel1.Controls.Add(toolbar);
        }

        // A toolbar button doesn't take the focus, so a feature's name can still be being edited (New Feature leaves it
        // so) when one is pressed. The edit is finished first, and the command runs after the tree has reloaded for the
        // new name, which AfterLabelEdit queues.
        private void RunAfterRename(Action command)
        {
            var editing = FindEditing(treeViewFeatures.Nodes);
            if (editing is null)
            {
                command();
                return;
            }

            editing.EndEdit(false);
            BeginInvoke(command);
        }

        private static TreeNode? FindEditing(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsEditing)
                {
                    return node;
                }

                var found = FindEditing(node.Nodes);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {
            return FeatureDocument.IsValidDocument(_documentManager.Document);
        }



        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("FeaturesDesigner.Features.ico")!);
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("FeaturesDesigner.License.txt")!).ReadToEnd();
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
            var container = FeatureDocument.Container(_documentManager.Document);

            treeViewFeatures.Nodes.Clear();
            ClearPropertyGrid();
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemRename.Enabled = false;
            toolStripMenuItemNewSubFeature.Enabled = false;

            foreach (var feature in FeatureDocument.Features(container))
            {
                var treeNode = treeViewFeatures.Nodes.Add(feature.Id, string.Format("{0}", feature.Id));
                treeNode.Tag = feature;
                LoadDataRecurse(treeNode, feature);

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

        private void LoadDataRecurse(TreeNode parentNode, Wxs.Feature parent)
        {
            foreach (var feature in FeatureDocument.Features(parent))
            {
                var treeNode = parentNode.Nodes.Add(feature.Id, string.Format("{0}", feature.Id));
                treeNode.Tag = feature;
                LoadDataRecurse(treeNode, feature);

            }
        }

        // Reloads the tree (top-level features are in Id order) and selects a feature, optionally renaming it.
        private void ReloadAndSelect(Wxs.Feature feature, bool beginEdit)
        {
            LoadData2();
            var node = FindNode(treeViewFeatures.Nodes, feature);
            if (node != null)
            {
                _refresh = true;
                treeViewFeatures.SelectedNode = node;
                if (beginEdit)
                {
                    node.BeginEdit();
                }
            }
        }

        private static TreeNode? FindNode(TreeNodeCollection nodes, Wxs.Feature feature)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is Wxs.Feature tagged && tagged.Xml == feature.Xml)
                {
                    return node;
                }

                var found = FindNode(node.Nodes, feature);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
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
            var merges = FeatureDocument.Merges(_documentManager.Document);
            foreach (var merge in merges)
            {
                TreeNode mergeNode = treeViewMerges.Nodes.Add(merge.Id, FeatureDocument.Describe(merge), 1);
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
                Wxs.Feature iswixFeature = (Wxs.Feature)treeViewFeatures.SelectedNode.Tag;
                feature.Read(iswixFeature);

                UpdatePropertyGrid();
                UpdateMergeModules();

                // Checking nodes to show the feature's references must not write them back.
                _loadingMergeRefs = true;
                foreach (var mergeRef in iswixFeature.MergeRefs)
                {
                    var nodes = treeViewMerges.Nodes.Find(mergeRef.Id ?? string.Empty, true);
                    foreach (var node in nodes)
                    {
                        node.Checked = true;
                    }
                }
                _loadingMergeRefs = false;
                toolStripMenuItemNewSubFeature.Enabled = true;
                toolStripMenuItemRename.Enabled = true;
                toolStripMenuItemDelete.Enabled = true;

                UpdateMoveOptions();
                propertyGridFeatureProperties.ExpandAllGridItems();
            }
        }

        private void propertyGridFeatureProperties_PropertyValueChanged(object? s, PropertyValueChangedEventArgs e)
        {
            feature.Write(e.ChangedItem?.Label ?? string.Empty);
        }
        private void treeViewMerges_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (treeViewFeatures.SelectedNode != null && !_loadingMergeRefs)
            {
                Wxs.Feature feature = (Wxs.Feature)treeViewFeatures.SelectedNode.Tag;
                Wxs.Merge merge = (Wxs.Merge)e.Node!.Tag;
                propertyGridFeatureProperties.Refresh();
                FeatureDocument.SetReference(feature, merge.Id, e.Node.Checked);
            }
        }

        private void treeViewFeatures_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // A null label means the edit was cancelled.
            if (e.Label == null)
            {
                return;
            }

            Wxs.Feature iswixFeature = (Wxs.Feature)e.Node!.Tag;
            if (e.Label == iswixFeature.Id)
            {
                return;
            }

            string? error = FeatureDocument.ValidateId(_documentManager.Document, iswixFeature, e.Label);
            if (error != null)
            {
                MessageBox.Show(error);
                e.CancelEdit = true;
                return;
            }

            FeatureDocument.Rename(iswixFeature, e.Label);
            // The tree is rebuilt for the feature's new place, after this edit ends.
            e.CancelEdit = true;
            BeginInvoke(new Action(() => ReloadAndSelect(iswixFeature, false)));
        }

        private void toolStripMenuItemRename_Click(object sender, EventArgs e)
        {
            treeViewFeatures.SelectedNode.BeginEdit();
        }

        private void toolStripMenuItemNewFeature_Click(object sender, EventArgs e)
        {
            // A new top-level feature; it lands in Id order.
            string featureName = FeatureDocument.SuggestNextFeatureName(_documentManager.Document);
            var feature = FeatureDocument.Create(FeatureDocument.Container(_documentManager.Document), featureName);
            ReloadAndSelect(feature, true);
        }

   

        private void toolStripMenuItemNewSubFeature_Click(object sender, EventArgs e)
        {
            string featureName = FeatureDocument.SuggestNextFeatureName(_documentManager.Document);
            var parent = (Wxs.Feature)treeViewFeatures.SelectedNode.Tag;
            var feature = FeatureDocument.Create(parent, featureName);
            ReloadAndSelect(feature, true);
            
        }

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemRename.Enabled = false;
            toolStripMenuItemNewSubFeature.Enabled = false;

            Wxs.Feature feature = (Wxs.Feature)treeViewFeatures.SelectedNode.Tag;
            FeatureDocument.Delete(feature);
            treeViewFeatures.SelectedNode.Remove();

            if (treeViewFeatures.Nodes.Count > 0)
            {
                _refresh = true;
                treeViewFeatures.Select();
                treeViewFeatures.SelectedNode = treeViewFeatures.Nodes[0];
                treeViewFeatures.ExpandAll();

            }
            else
            {
                // The last feature is gone: nothing left to show properties for.
                ClearPropertyGrid();
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

        // With no feature selected (none in the file, or the last one deleted) the property grid is hidden rather than
        // showing an empty feature's properties.
        private void ClearPropertyGrid()
        {
            feature.Clear();
            propertyGridFeatureProperties.Visible = false;
        }

        // PropertyValueChanged is subscribed once, in InitializeComponent.
        private void UpdatePropertyGrid()
        {
            propertyGridFeatureProperties.Visible = true;
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
