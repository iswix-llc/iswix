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
    public partial class Features : UserControl, IFireworksDesigner, ISelectionLinked
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

            // So the selected feature still shows when the XML editor has the focus.
            treeViewFeatures.HideSelection = false;
            treeViewFeatures.AfterSelect += (s, e) =>
            {
                // Whoever selected it, the user or the designer filling the tree. The window ignores this while it reloads
                // the designers for a change made in the XML, so typing there doesn't move the XML's selection.
                _attribute = null;
                _other = null;
                SelectedElementChanged?.Invoke(this, EventArgs.Empty);
            };
            // Clicking the node that is already selected, such as the one the designer selects first, selects nothing.
            treeViewFeatures.NodeMouseClick += (s, e) =>
            {
                if (e.Node == treeViewFeatures.SelectedNode)
                {
                    _attribute = null;
                    _other = null;
                    SelectedElementChanged?.Invoke(this, EventArgs.Empty);
                }
            };
            // The merge modules the selected feature can reference: choosing one shows its Merge element, and checking
            // one shows the MergeRef that makes (unchecking, the feature that lost it).
            treeViewMerges.HideSelection = false;
            treeViewMerges.NodeMouseClick += (s, e) =>
            {
                if (e.Node.Tag is Wxs.Merge merge)
                {
                    _attribute = null;
                    _other = merge.Xml;
                    SelectedElementChanged?.Invoke(this, EventArgs.Empty);
                }
            };
            treeViewMerges.AfterCheck += (s, e) =>
            {
                // Run after the designer's own handler, which adds or removes the reference. Only the user's check: not
                // the boxes ticked to show the feature's references.
                if (_loadingMergeRefs || e.Action == TreeViewAction.Unknown || e.Node?.Tag is not Wxs.Merge merge || treeViewFeatures.SelectedNode?.Tag is not Wxs.Feature feature)
                {
                    return;
                }

                _attribute = null;
                _other = e.Node.Checked ? feature.MergeRefs.FirstOrDefault(r => r.Id == merge.Id)?.Xml : null;
                SelectedElementChanged?.Invoke(this, EventArgs.Empty);
            };
            propertyGridFeatureProperties.SelectedGridItemChanged += (s, e) =>
            {
                if (propertyGridFeatureProperties.ContainsFocus && e.NewSelection?.GridItemType == GridItemType.Property)
                {
                    _attribute = e.NewSelection.PropertyDescriptor?.Name;
                    SelectedElementChanged?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        #region ISelectionLinked Members

        // The feature's attribute chosen in the property grid, whose items are named for the attributes.
        string? _attribute;

        public event EventHandler? SelectedElementChanged;

        // What the user chose in the merge modules, a Merge or the MergeRef that refers to one, instead of the feature.
        XElement? _other;

        public XElement? SelectedElement => _other ?? (treeViewFeatures.SelectedNode?.Tag as Wxs.Feature)?.Xml;

        public string? SelectedAttribute => _attribute;

        // From the XML rather than the tree, which is only filled once the designer has been shown. Anything in a
        // feature, such as a reference to a component group, counts as the feature. A merge module is a feature's to
        // reference.
        public bool Handles(XElement element, string? attribute) =>
            element.AncestorsAndSelf().Any(e => e.Name == Wxs.Feature.ElementName || e.Name == Wxs.Merge.ElementName);

        public void SelectElement(XElement element, string? attribute)
        {
            var merge = element.AncestorsAndSelf().FirstOrDefault(e => e.Name == Wxs.Merge.ElementName);
            if (merge != null)
            {
                var mergeNode = treeViewMerges.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Tag is Wxs.Merge tagged && tagged.Xml == merge);
                if (mergeNode != null)
                {
                    treeViewMerges.SelectedNode = mergeNode;
                    _attribute = null;
                    _other = merge;
                }

                return;
            }

            var node = element.AncestorsAndSelf().Select(e => FindNode(treeViewFeatures.Nodes, e)).FirstOrDefault(n => n != null);
            if (node is null)
            {
                return;
            }

            _attribute = attribute;
            treeViewFeatures.SelectedNode = node;
            _other = element.AncestorsAndSelf().FirstOrDefault(e => e.Name == Wxs.MergeRef.ElementName);
            if (attribute != null && propertyGridFeatureProperties.SelectedGridItem is { } selected)
            {
                var root = selected;
                while (root.Parent != null)
                {
                    root = root.Parent;
                }

                var item = AllItems(root).FirstOrDefault(i => i.GridItemType == GridItemType.Property && i.PropertyDescriptor?.Name == attribute);
                item?.Select();
            }
        }

        private static IEnumerable<GridItem> AllItems(GridItem item)
        {
            yield return item;
            foreach (GridItem child in item.GridItems)
            {
                foreach (var descendant in AllItems(child))
                {
                    yield return descendant;
                }
            }
        }

        private static TreeNode? FindNode(TreeNodeCollection nodes, XElement element)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is Wxs.Feature tagged && tagged.Xml == element)
                {
                    return node;
                }

                var found = FindNode(node.Nodes, element);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        #endregion

        // The tree's commands as buttons above it too: with no features the tree is empty, and nothing shows that
        // right-clicking it adds one.
        private void AddToolbar()
        {
            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            foreach (var menuItem in new[] { toolStripMenuItemNewFeature, toolStripMenuItemNewSubFeature, toolStripMenuItemRename, toolStripMenuItemDelete })
            {
                // No tooltip: it would only repeat the button's text.
                var button = new ToolStripButton(menuItem.Text) { DisplayStyle = ToolStripItemDisplayStyle.Text, Enabled = menuItem.Enabled, AutoToolTip = false };
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
                ShowSelectionInXml();
                if (beginEdit)
                {
                    node.BeginEdit();
                }
            }
        }

        // For a command that selects a feature in code (a new one, say), which the XML editor then shows as it does
        // for a click.
        private void ShowSelectionInXml()
        {
            _attribute = null;
            _other = null;
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
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
                ShowSelectionInXml();
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
