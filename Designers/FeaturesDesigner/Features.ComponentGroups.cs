using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FireworksFramework.Services;
using Wxs = IsWiX.WiXModel;

namespace FeaturesDesigner
{
    // The component groups the selected feature installs: every component group the project defines, in this file or in
    // another of its .wxs files, with a check box for each. Checking one adds a ComponentGroupRef to the feature, and
    // unchecking removes it. A group can be opened where it is defined.
    public partial class Features
    {
        TreeView treeViewComponentGroups = null!;
        ContextMenuStrip _groupMenu = null!;
        ToolStripMenuItem _goToDefinitionItem = null!;

        // The groups the open file defines, as it is in memory, and the other files' groups, read from disk away from the
        // user interface and in place when they are in.
        List<ComponentGroupInfo> _currentGroups = new List<ComponentGroupInfo>();
        List<ComponentGroupInfo> _otherGroups = new List<ComponentGroupInfo>();
        int _scanVersion;
        bool _loadingGroupRefs;
        bool _groupSplitSet;

        private void AddComponentGroupList()
        {
            treeViewComponentGroups = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = true,
                HideSelection = false,
                ShowNodeToolTips = true,
                CausesValidation = false,
                Enabled = false,
            };

            _goToDefinitionItem = new ToolStripMenuItem("Go to Definition", null, (s, e) => GoToDefinition(treeViewComponentGroups.SelectedNode)) { ShortcutKeyDisplayString = "F12" };
            _groupMenu = new ContextMenuStrip();
            _groupMenu.Items.Add(_goToDefinitionItem);
            _groupMenu.Items.Add(new ToolStripMenuItem("Refresh", null, (s, e) => ReloadComponentGroups()));
            _groupMenu.Opening += (s, e) => _goToDefinitionItem.Enabled = treeViewComponentGroups.SelectedNode?.Tag is ComponentGroupInfo;
            treeViewComponentGroups.ContextMenuStrip = _groupMenu;

            // The merge modules' list makes room for this one beside it, each with a label to tell them apart.
            var groupsLabel = new Label { Text = "Component Groups", Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleLeft };
            var mergesLabel = new Label { Text = "Merge Modules", Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleLeft };
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
            splitContainer3.Panel1.Controls.Remove(treeViewMerges);
            split.Panel1.Controls.Add(treeViewComponentGroups);
            split.Panel1.Controls.Add(groupsLabel);
            split.Panel2.Controls.Add(treeViewMerges);
            split.Panel2.Controls.Add(mergesLabel);
            splitContainer3.Panel1.Controls.Add(split);
            split.SizeChanged += (s, e) =>
            {
                // Half each, once the container has the size it will be shown at; the splitter can be moved after.
                if (!_groupSplitSet && split.Width > 120)
                {
                    _groupSplitSet = true;
                    split.SplitterDistance = split.Width / 2;
                }
            };

            treeViewFeatures.AfterSelect += (s, e) => PopulateComponentGroups();
            treeViewComponentGroups.AfterCheck += treeViewComponentGroups_AfterCheck;
            treeViewComponentGroups.NodeMouseClick += (s, e) =>
            {
                treeViewComponentGroups.SelectedNode = e.Node;
                ShowGroupInXml(e.Node);
            };
            treeViewComponentGroups.NodeMouseDoubleClick += (s, e) => GoToDefinition(e.Node);
            treeViewComponentGroups.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F12)
                {
                    GoToDefinition(treeViewComponentGroups.SelectedNode);
                    e.Handled = true;
                }
            };
        }

        // Reads the groups again: the open file's from memory now, the others' from disk in the background. Called when
        // the designer loads, so after any change to the text, and from the list's menu.
        private void ReloadComponentGroups()
        {
            var path = _documentManager.DocumentPath;
            _currentGroups = ProjectComponentGroups.FindInDocument(_documentManager.Document, path);
            _otherGroups = new List<ComponentGroupInfo>();
            PopulateComponentGroups();

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var version = ++_scanVersion;
            Task.Run(() => ProjectComponentGroups.FindInOtherFiles(path)).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled || version != _scanVersion || IsDisposed || !IsHandleCreated)
                {
                    return;
                }

                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (version == _scanVersion && !IsDisposed)
                        {
                            _otherGroups = task.Result;
                            PopulateComponentGroups();
                        }
                    }));
                }
                catch (InvalidOperationException)
                {
                    // The designer was closed in the meantime.
                }
            });
        }

        // The list for the selected feature: a node for every group the project defines, checked for those the feature
        // installs, and one for each group it installs that nothing defines.
        private void PopulateComponentGroups()
        {
            var feature = treeViewFeatures.SelectedNode?.Tag as Wxs.Feature;
            var selectedId = (treeViewComponentGroups.SelectedNode?.Tag as ComponentGroupInfo)?.Id;
            var own = _documentManager.DocumentPath;
            var referenced = feature == null
                ? new HashSet<string>()
                : new HashSet<string>(feature.ComponentGroupRefs.Select(r => r.Id).Where(id => !string.IsNullOrEmpty(id)).Select(id => id!));

            _loadingGroupRefs = true;
            treeViewComponentGroups.BeginUpdate();
            try
            {
                treeViewComponentGroups.Nodes.Clear();
                treeViewComponentGroups.Enabled = feature != null;

                var groups = _currentGroups.OrderBy(g => g.Id, StringComparer.OrdinalIgnoreCase)
                    .Concat(_otherGroups.OrderBy(g => Path.GetFileName(g.FilePath), StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Id, StringComparer.OrdinalIgnoreCase));
                foreach (var group in groups)
                {
                    // The file a group is in is named unless it is this one.
                    var elsewhere = !string.Equals(group.FilePath, own, StringComparison.OrdinalIgnoreCase);
                    var node = treeViewComponentGroups.Nodes.Add(elsewhere ? $"{group.Id}  ({Path.GetFileName(group.FilePath)})" : group.Id);
                    node.Tag = group;
                    node.ToolTipText = elsewhere ? group.FilePath : "Defined in this file";
                    node.Checked = referenced.Contains(group.Id);
                }

                var defined = new HashSet<string>(_currentGroups.Concat(_otherGroups).Select(g => g.Id));
                foreach (var id in referenced.Where(id => !defined.Contains(id)).OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
                {
                    var node = treeViewComponentGroups.Nodes.Add($"{id}  (not defined in this project)");
                    node.Tag = new ComponentGroupInfo(id, null);
                    node.ForeColor = Color.IndianRed;
                    node.ToolTipText = "No .wxs file of this project defines this component group.";
                    node.Checked = true;
                }

                if (selectedId != null)
                {
                    treeViewComponentGroups.SelectedNode = treeViewComponentGroups.Nodes.Cast<TreeNode>().FirstOrDefault(n => (n.Tag as ComponentGroupInfo)?.Id == selectedId);
                }
            }
            finally
            {
                treeViewComponentGroups.EndUpdate();
                _loadingGroupRefs = false;
            }
        }

        // Checking a group adds a reference to it, unchecking removes it (every one, if the feature somehow has several).
        private void treeViewComponentGroups_AfterCheck(object? sender, TreeViewEventArgs e)
        {
            if (_loadingGroupRefs || e.Action == TreeViewAction.Unknown || e.Node?.Tag is not ComponentGroupInfo group || treeViewFeatures.SelectedNode?.Tag is not Wxs.Feature feature)
            {
                return;
            }

            Wxs.ComponentGroupRef? reference = null;
            if (e.Node.Checked)
            {
                reference = feature.AddComponentGroupRef(group.Id);
            }
            else
            {
                foreach (var existing in feature.ComponentGroupRefs.Where(r => r.Id == group.Id).ToList())
                {
                    existing.RemoveFromParent();
                }

                // A group nothing defines has nothing left to show once it isn't referenced.
                if (group.FilePath == null)
                {
                    BeginInvoke(new Action(PopulateComponentGroups));
                }
            }

            // The XML shows the reference made, or the feature that lost it.
            _attribute = null;
            _other = reference?.Xml;
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
        }

        // Choosing a group shows its reference in the XML if the feature has one, else the feature.
        private void ShowGroupInXml(TreeNode node)
        {
            if (node.Tag is not ComponentGroupInfo group || treeViewFeatures.SelectedNode?.Tag is not Wxs.Feature feature)
            {
                return;
            }

            _attribute = null;
            _other = feature.ComponentGroupRefs.FirstOrDefault(r => r.Id == group.Id)?.Xml;
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
        }

        // Selects the group's node in the list, for a reference the XML's caret is in.
        private void SelectGroupNode(string? id)
        {
            var node = treeViewComponentGroups.Nodes.Cast<TreeNode>().FirstOrDefault(n => (n.Tag as ComponentGroupInfo)?.Id == id);
            if (node != null)
            {
                treeViewComponentGroups.SelectedNode = node;
            }
        }

        // Opens the file that defines the group, with the group selected there.
        private void GoToDefinition(TreeNode? node)
        {
            if (node?.Tag is not ComponentGroupInfo group)
            {
                return;
            }

            if (group.FilePath == null)
            {
                MessageBox.Show($"The component group '{group.Id}' isn't defined in any .wxs file of this project.", "Go to Definition", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (string.IsNullOrEmpty(group.FilePath))
            {
                MessageBox.Show("Save the file first.", "Go to Definition", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (DesignerNavigation.GoToDefinition is not { } goTo || !goTo(group.FilePath, "ComponentGroup", group.Id))
            {
                MessageBox.Show($"Couldn't open {group.FilePath}.", "Go to Definition", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
