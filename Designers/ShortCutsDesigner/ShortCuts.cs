using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Services;
using static FireworksFramework.Types.Enums;
using Wxs = IsWiX.WiXModel;

namespace ShortCutsDesigner
{
    public partial class ShortCuts : UserControl, IFireworksDesigner
    {
        List<Wxs.Shortcut> _shortcuts = new List<Wxs.Shortcut>();
        readonly DocumentManager _documentManager;
        string _oldPath = string.Empty;
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
            BlueFolderOpen,
            Shortcut
        }

        private enum SystemFolderProperty
        {
            AdminToolsFolder,
            AppDataFolder,
            CommonAppDataFolder,
            CommonFiles64Folder,
            CommonFilesFolder,
            DesktopFolder,
            FavoritesFolder,
            FontsFolder,
            GlobalAssemblyCache,
            LocalAppDataFolder,
            MergeRedirectFolder,
            MyPicturesFolder,
            PersonalFolder,
            ProgramFiles64Folder,
            ProgramFilesFolder,
            ProgramMenuFolder,
            SendToFolder,
            StartMenuFolder,
            StartupFolder,
            System16Folder,
            System64Folder,
            SystemFolder,
            TempFolder,
            TemplateFolder,
            WindowsFolder,
            WindowsVolume
        }
        public ShortCuts(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            InitializeComponent();
            // Images come from FireworksFramework rather than the .resx, so the designer also runs on .NET Framework.
            expandAllToolStripMenuItem2.Image = DesignerImages.MenuImage("expand_all");
            collapseAllToolStripMenuItem1.Image = DesignerImages.MenuImage("collapse_all");
            createNewFolderToolStripMenuItem.Image = DesignerImages.MenuImage("new_folder");
            renameFolderToolStripMenuItem.Image = DesignerImages.MenuImage("rename");
            deleteToolStripMenuItem.Image = DesignerImages.MenuImage("delete");
            toolStripMenuItemCreateSC.Image = DesignerImages.MenuImage("new_shortcut");
            expandAllToolStripMenuItem1.Image = DesignerImages.MenuImage("expand_all");
            collapseAllToolStripMenuItem.Image = DesignerImages.MenuImage("collapse_all");
            createNewFolderToolStripMenuItem1.Image = DesignerImages.MenuImage("new_folder");
            toolStripMenuItemCreateShortCut.Image = DesignerImages.MenuImage("new_shortcut");
            expandAllToolStripMenuItem.Image = DesignerImages.MenuImage("expand_all");
            toolStripMenuItem3.Image = DesignerImages.MenuImage("delete");
            DesignerImages.AddLibrary(iImageLibrary);
            DesignerImages.Add(iImageLibrary, typeof(ShortCuts).Assembly, "ShortCutsDesigner.Images.", "shortcut");
            AddToolbar();
        }

        ToolStripButton _newFolderButton = null!;
        ToolStripDropDownButton _newShortcutButton = null!;
        ToolStripButton _renameButton = null!;
        ToolStripButton _deleteButton = null!;

        // The tree's commands as buttons above it too: nothing else shows that right-clicking a folder adds a shortcut.
        private void AddToolbar()
        {
            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            ToolStripButton Button(string text, Action command)
            {
                // No tooltip: it would only repeat the button's text.
                var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, AutoToolTip = false, Enabled = false };
                button.Click += (s, e) =>
                {
                    // A button doesn't take the focus, so a folder's name can still be being edited (New Folder leaves
                    // it so); that edit is finished first.
                    if (tvDestination.SelectedNode is { IsEditing: true } editing)
                    {
                        editing.EndEdit(false);
                    }

                    command();
                };
                toolbar.Items.Add(button);
                return button;
            }

            _newFolderButton = Button("New Folder", AddNodeToDestinationTreeFragmentStyle);

            // New Shortcut lists the files a shortcut can be made for, by folder, here and in the tree's menus.
            _newShortcutButton = new ToolStripDropDownButton("New Shortcut") { DisplayStyle = ToolStripItemDisplayStyle.Text, AutoToolTip = false, Enabled = false };
            FillNewShortcutMenu(_newShortcutButton.DropDownItems);
            _newShortcutButton.DropDownOpening += (s, e) =>
            {
                if (tvDestination.SelectedNode is { IsEditing: true } editing)
                {
                    editing.EndEdit(false);
                }

                FillNewShortcutMenu(_newShortcutButton.DropDownItems);
            };
            toolbar.Items.Add(_newShortcutButton);
            foreach (var (menu, item) in new[] { (cmsDestinationTreeDefault, toolStripMenuItemCreateSC), (cmsMergeRedirectFolder, toolStripMenuItemCreateShortCut) })
            {
                FillNewShortcutMenu(item.DropDownItems);
                menu.Opening += (s, e) => FillNewShortcutMenu(item.DropDownItems);
            }

            _renameButton = Button("Rename", () =>
            {
                tvDestination.LabelEdit = true;
                tvDestination.SelectedNode.BeginEdit();
            });
            _deleteButton = Button("Delete", () =>
            {
                RemoveDirectoryFromDocumentFragmentStyle(tvDestination.SelectedNode);
                UpdateToolbar();
            });
            splitContainer1.Panel1.Controls.Add(toolbar);
        }

        // As the tree's menus: shortcut locations (Desktop, Program Menu...) take new folders and shortcuts, the folders
        // below them can also be renamed and deleted, and shortcuts can only be deleted.
        private void UpdateToolbar()
        {
            var node = tvDestination.SelectedNode;
            var shortcut = node != null && IsShortcutNode();
            var folder = node != null && node.Level >= 1 && !shortcut;
            _newFolderButton.Enabled = folder;
            _newShortcutButton.Enabled = folder;
            _renameButton.Enabled = folder && node!.Level >= 2;
            _deleteButton.Enabled = shortcut || (folder && node!.Level >= 2);
        }

        #region IFireworksDesigner Members


        public bool IsValidContext()
        {
            return ShortcutDocument.IsValidDocument(_documentManager.Document);
        }

        public void LoadData()
        {
            tvDestination.Nodes.Clear();
            propertyGridShortCut.SelectedObject = null;
            LoadDocument();
            tvDestination.ExpandAll();
            tvDestination.SelectedNode = null;
            UpdateToolbar();
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("ShortCutsDesigner.ShortCuts.ico")!);
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ShortCutsDesigner.License.txt")!).ReadToEnd();
            }
        }

        public string PluginName
        {
            get { return "Shortcuts"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group3_shortcuts"; }
        }

        public PluginType PluginType
        {
            get { return PluginType.Designer; }
        }

        #endregion
        private void LoadDocument()
        {
            _shortcuts = ShortcutDocument.Load(_documentManager.Document);
            Cursor = Cursors.WaitCursor;
            tvDestination.Nodes.Clear();
            // The folders are each shortcut's Directory and Subdirectory, under the four shortcut locations.
            TreeNode treeNode = AddDestinationComputerToDestination();
            AddFirstLevelFolder(treeNode, "DesktopFolder");
            AddFirstLevelFolder(treeNode, "ProgramMenuFolder");
            AddFirstLevelFolder(treeNode, "SendToFolder");
            //AddFirstLevelFolder(treeNode, "StartMenuFolder");  Old School
            AddFirstLevelFolder(treeNode, "StartupFolder");
            tvDestination.ExpandAll();

            Cursor = Cursors.Default;
        }

        private void AddFirstLevelFolder(TreeNode treeNode, string folderName)
        {
            TreeNode directoryNode = treeNode.Nodes.Add($"[{folderName}]");
            directoryNode.ImageIndex = (int)ImageLibrary.BlueFolderClosed;
            directoryNode.SelectedImageIndex = (int)ImageLibrary.BlueFolderOpen;
            foreach (var shortcut in _shortcuts)
            {
                if (shortcut.Directory == folderName)
                {
                    if (string.IsNullOrEmpty(shortcut.Subdirectory))
                    {
                        TreeNode newNode = directoryNode.Nodes.Add(CalculateSelectedNodeText(shortcut));
                        newNode.ImageIndex = (int)ImageLibrary.Shortcut;
                        newNode.SelectedImageIndex = (int)ImageLibrary.Shortcut;
                        newNode.Tag = shortcut;
                    }
                    else
                    {
                        TreeNode subDirectoryNode = GetOrCreateTreeNode(directoryNode, shortcut.Subdirectory);
                        TreeNode newNode = subDirectoryNode.Nodes.Add(CalculateSelectedNodeText(shortcut));
                        newNode.ImageIndex = (int)ImageLibrary.Shortcut;
                        newNode.SelectedImageIndex = (int)ImageLibrary.Shortcut;
                        newNode.Tag = shortcut;
                    }
                }
            }
        }

        private TreeNode GetOrCreateTreeNode(TreeNode directoryNode, string path)
        {
            TreeNode searchNode = directoryNode;
            foreach (var part in path.Split('\\'))
            {
                bool found = false;
                foreach (TreeNode subNode in searchNode.Nodes)
                {
                    if (subNode.Text == part)
                    {
                        searchNode = subNode;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    searchNode = searchNode.Nodes.Add($"{part}");
                    searchNode.ImageIndex = (int)ImageLibrary.FolderClosed;
                    searchNode.SelectedImageIndex = (int)ImageLibrary.FolderOpen;
                }
            }
            return searchNode;
        }

        private TreeNode AddDestinationComputerToDestination()
        {
            var subTreeNode = tvDestination.Nodes.Add("Destination Computer");
            subTreeNode.ImageIndex = (int)ImageLibrary.Computer;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Computer;
            return subTreeNode;
        }

        private bool IsASpecialDirectoryToIgnore(string elementId)
        {
            if (IdIsWithinSystemFolderPropertyEnum(elementId))
            {
                var systemFolderProperty = (SystemFolderProperty)Enum.Parse(typeof(SystemFolderProperty), elementId, true);
                switch (systemFolderProperty)
                {
                    case SystemFolderProperty.AdminToolsFolder:
                    case SystemFolderProperty.AppDataFolder:
                    case SystemFolderProperty.CommonFiles64Folder:
                    case SystemFolderProperty.FavoritesFolder:
                    case SystemFolderProperty.FontsFolder:
                    case SystemFolderProperty.LocalAppDataFolder:
                    case SystemFolderProperty.MyPicturesFolder:
                    case SystemFolderProperty.PersonalFolder:
                    case SystemFolderProperty.ProgramFiles64Folder:
                    case SystemFolderProperty.ProgramFilesFolder:
                    case SystemFolderProperty.System16Folder:
                    case SystemFolderProperty.System64Folder:
                    case SystemFolderProperty.SystemFolder:
                    case SystemFolderProperty.TempFolder:
                    case SystemFolderProperty.TemplateFolder:
                    case SystemFolderProperty.WindowsFolder:
                    case SystemFolderProperty.WindowsVolume:
                    case SystemFolderProperty.CommonAppDataFolder:
                    case SystemFolderProperty.CommonFilesFolder:
                    case SystemFolderProperty.MergeRedirectFolder:
                    case SystemFolderProperty.GlobalAssemblyCache:
                        return true;
                    case SystemFolderProperty.DesktopFolder:
                    case SystemFolderProperty.ProgramMenuFolder:
                    case SystemFolderProperty.SendToFolder:
                    case SystemFolderProperty.StartupFolder:
                    case SystemFolderProperty.StartMenuFolder:
                        return false;
                    default:
                        return true;
                }
            }
            return false;
        }
        private static bool IdIsWithinSystemFolderPropertyEnum(string elementId)
        {
            foreach (var s in Enum.GetNames(typeof(SystemFolderProperty)))
            {
                if (string.Compare(s, elementId, true) == 0)
                    return true;
            }
            return false;
        }

        private void tvDestination_MouseUp(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                TreeNode node = tvDestination.GetNodeAt(p);
                if (node != null)
                {
                    // Select the node the user has clicked.
                    // The node appears selected once the menu is displayed on the screen.
                    tvDestination.SelectedNode = node;
                    foreach (ToolStripItem toolStripItem in cmsDestinationRoot.Items)
                    {
                        toolStripItem.Visible = false;
                    }

                    // Find the appropriate ContextMenu depending on the selected node.
                    if (node.Parent == null)
                    {
                        ClearOldDestinationRootMenuItems();
                        AddItemsToDestinationRootMenu();
                        cmsDestinationRoot.Show(tvDestination, p);
                        if (node.Nodes.Count == 0)
                        {
                            cmsDestinationRoot.Items[0].Visible = false;
                            cmsDestinationRoot.Items[1].Visible = false;
                        }
                        else
                        {
                            cmsDestinationRoot.Items[0].Visible = true;
                            cmsDestinationRoot.Items[1].Visible = true;
                        }

                    }
                    else if (node.Parent.Text == "Destination Computer")
                    {
                        cmsMergeRedirectFolder.Show(tvDestination, p);

                    }
                    else
                    {
                        if (node.ImageIndex.Equals(12))
                        {
                            cmsShortcut.Show(tvDestination, p);
                        }
                        else
                        {
                            cmsDestinationTreeDefault.Show(tvDestination, p);
                        }
                    }
                }
            }
        }

        private bool IsShortcutNode()
        {
            if (tvDestination.SelectedNode.ImageIndex.Equals(12))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ClearOldDestinationRootMenuItems()
        {
            var itemCount = cmsDestinationRoot.Items.Count;
            if (itemCount <= 3) return;
            for (var i = (itemCount - 1); i >= 3; i--)
            {
                cmsDestinationRoot.Items.RemoveAt(i);
            }
        }
        private void AddItemsToDestinationRootMenu()
        {
            // The shortcut locations, listed for reference. The tree always shows them, so they are never added and
            // their items stay disabled.
            foreach (var s in Enum.GetNames(typeof(SystemFolderProperty)))
            {
                if (IsASpecialDirectoryToIgnore(s) || s == "GlobalAssemblyCache")
                {
                    continue;
                }

                var toolStripMenuItem = new ToolStripMenuItem(s);
                toolStripMenuItem.Name = s;
                toolStripMenuItem.Enabled = false;
                cmsDestinationRoot.Items.Add(toolStripMenuItem);
            }
        }

        private void createNewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNodeToDestinationTreeFragmentStyle();
        }

        private void AddNodeToDestinationTreeFragmentStyle()
        {
            // Check for existance of directories that contain the text 'New Folder'
            List<string> folderNames = new List<string>();
            foreach (TreeNode node in tvDestination.SelectedNode.Nodes)
            {
                if (node.Text.StartsWith("New Folder"))
                {
                    folderNames.Add(node.Text);
                }
            }
            var newFolderName = "New Folder";
            if (folderNames.Count() > 0)
            {
                newFolderName += String.Format(" ({0})", folderNames.Count());
            }
            //create a node
            var nodeToEdit = tvDestination.SelectedNode.Nodes.Add(newFolderName, newFolderName);
            nodeToEdit.Tag = newFolderName;
            tvDestination.SelectedNode = nodeToEdit;
            tvDestination.LabelEdit = true;
            tvDestination.SelectedNode.BeginEdit();
        }


        private void tvDestination_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            tvDestination.LabelEdit = false;
            var previousName = e.Node!.Text;
            if (String.IsNullOrEmpty(e.Label))
            {
                e.CancelEdit = true;
            }
            else
            {
                e.Node.Text = e.Label;
            }

            bool canRename = CanRenameFragmentStyle(previousName, e.Label);
            if (!canRename)
            {
                CallMessageBox(String.Format("A folder with the name [{0}] already exists.", e.Node.Text), "Folder Rename Warning");
                e.Node.Text = previousName;
                tvDestination.LabelEdit = true;
                tvDestination.SelectedNode.BeginEdit();
                e.CancelEdit = true;
            }

        }

        private bool CanRenameFragmentStyle(string previousName, string? newName)
        {
            int count = 0;
            foreach (TreeNode node in tvDestination.SelectedNode.Parent.Nodes)
            {
                if (node.Text == newName)
                {
                    count++;
                }
            }
            if (count < 2)
            {
                string old = _oldPath.Replace("Destination Computer\\", "").Replace($"[", "").Replace("]", "");
                string oldDirectory = old.Split('\\').First();
                string oldSubDirectory = old.Substring(oldDirectory.Length + 1, old.Length - oldDirectory.Length - 1);
                string after = tvDestination.SelectedNode.FullPath.Replace("Destination Computer\\", "").Replace($"[", "").Replace("]", "");
                string afterDirectory = after.Split('\\').First();
                string afterSubDirectory = after.Substring(afterDirectory.Length + 1, after.Length - afterDirectory.Length - 1);

                foreach (Wxs.Shortcut shortcut in _shortcuts)
                {
                    string subdirectory = ShortcutDocument.Subdirectory(shortcut);
                    if (shortcut.Directory == oldDirectory &&
                        (subdirectory.Equals(oldSubDirectory) ||
                        subdirectory.StartsWith(oldSubDirectory + "\\")))
                    {
                        string subString = subdirectory.Substring(oldSubDirectory.Length, subdirectory.Length - oldSubDirectory.Length);
                        ShortcutDocument.SetSubdirectory(shortcut, afterSubDirectory + subString);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private static void CallMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void renameFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.LabelEdit = true;
            tvDestination.SelectedNode.BeginEdit();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveDirectoryFromDocumentFragmentStyle(tvDestination.SelectedNode);
        }
        private void RemoveDirectoryFromDocumentFragmentStyle(TreeNode treeNode)
        {
            bool isShortcut = (treeNode.ImageIndex == 12);

            if (isShortcut)
            {
                Wxs.Shortcut selectedShortcut = (Wxs.Shortcut)treeNode.Tag;
                ShortcutDocument.Delete(selectedShortcut);
                _shortcuts.Remove(selectedShortcut);
            }
            else
            {
                // The folder's shortcuts, including those in its subfolders (and only in this top-level folder).
                var (directory, subdirectory) = SplitFolderPath(treeNode.FullPath);
                var shortcuts = _shortcuts.Where(shortcut =>
                    shortcut.Directory == directory &&
                    (ShortcutDocument.Subdirectory(shortcut).Equals(subdirectory) ||
                    ShortcutDocument.Subdirectory(shortcut).StartsWith(subdirectory + @"\"))).ToList();
                foreach (var shortcut in shortcuts)
                {
                    ShortcutDocument.Delete(shortcut);
                    _shortcuts.Remove(shortcut);
                }
            }
            treeNode.Remove();

        }

        // "Destination Computer\[ProgramMenuFolder]\Acme\Tools" is Directory ProgramMenuFolder, Subdirectory Acme\Tools.
        private static (string Directory, string Subdirectory) SplitFolderPath(string fullPath)
        {
            string[] parts = fullPath.Replace(@"Destination Computer\", "").Split('\\');
            return (parts[0].Replace("[", "").Replace("]", ""), Path.Combine(parts.Skip(1).ToArray()));
        }

        private void createNewFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AddNodeToDestinationTreeFragmentStyle();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();
        }

        private void expandAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.Collapse();
        }

        private void expandAllToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();
        }

        private void collapseAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.Collapse();
        }

        // A shortcut can be made for any file but a .dll or .pdb.
        private void FillNewShortcutMenu(ToolStripItemCollection items)
        {
            var candidates = _documentManager.Document is null
                ? new List<ShortCutCandidate>()
                : ShortcutDocument.GetShortCutCandidates(_documentManager.Document);
            CandidateMenu.Fill(items, candidates, c => c.DestinationFilePath, CreateShortCut,
                "No files to make a shortcut for", byFolder: true);
        }

        // In the selected folder, named for the file, numbered if a shortcut in the folder already has that name.
        private void CreateShortCut(ShortCutCandidate candidate)
        {
            List<string> parts = tvDestination.SelectedNode.FullPath.Replace("Destination Computer\\", "").Replace("[", "").Replace("]", "").Split('\\').ToList();
            string scDirectory = parts[0];
            string scSubdirectory = string.Join("\\", parts.Skip(1));

            string name = candidate.FileName;
            for (int index = 1; _shortcuts.Any(s => IsInFolderWithName(s, scDirectory, scSubdirectory, name)); index++)
            {
                name = candidate.FileName + index;
            }

            Wxs.Shortcut shortCut = ShortcutDocument.Create(name, scDirectory, scSubdirectory, candidate.FileElement);
            _shortcuts.Add(shortCut);
            AddShortcutNode(shortCut);
        }


        private static bool IsInFolderWithName(Wxs.Shortcut shortcut, string directory, string subdirectory, string name) =>
            string.Equals(shortcut.Directory, directory, StringComparison.InvariantCultureIgnoreCase) &&
            string.Equals(ShortcutDocument.Subdirectory(shortcut), subdirectory, StringComparison.InvariantCultureIgnoreCase) &&
            string.Equals(shortcut.Name, name, StringComparison.InvariantCultureIgnoreCase);

        private void AddShortcutNode(Wxs.Shortcut iswixShortcut)
        {
            var subTreeNode = tvDestination.SelectedNode.Nodes.Add(CalculateSelectedNodeText(iswixShortcut));
            subTreeNode.ImageIndex = (int)ImageLibrary.Shortcut;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Shortcut;
            subTreeNode.Tag = iswixShortcut;
            tvDestination.ExpandAll();
            var parentNode = tvDestination.Nodes[0];
            tvDestination.SelectedNode = subTreeNode;
            tvDestination.Select();
            UpdatedSelectedNodeText();
        }


        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Wxs.Shortcut shortcut = (Wxs.Shortcut)tvDestination.SelectedNode.Tag;
            ShortcutDocument.Delete(shortcut);
            _shortcuts.Remove(shortcut);
            tvDestination.SelectedNode.Remove();
            tvDestination.SelectedNode = tvDestination.Nodes[0];

        }

        private void tvDestination_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (IsShortcutNode())
            {
                shortcut1.Read((Wxs.Shortcut)e.Node!.Tag);
                propertyGridShortCut.SelectedObject = shortcut1;
            }
            else
            {
                propertyGridShortCut.SelectedObject = null;
            }

            UpdateToolbar();
        }

        private void propertyGridShortCut_PropertyValueChanged(object? s, PropertyValueChangedEventArgs e)
        {
            shortcut1.Write(e.ChangedItem?.Label ?? string.Empty);
            propertyGridShortCut.Refresh();
            UpdatedSelectedNodeText();
        }

        private void UpdatedSelectedNodeText()
        {
            if (tvDestination.SelectedNode.Tag is Wxs.Shortcut shortcut)
            {
                tvDestination.SelectedNode.Text = CalculateSelectedNodeText(shortcut);
            }
        }

        private static string CalculateSelectedNodeText(Wxs.Shortcut shortcut)
        {
            return string.Format("{0} ({1})", shortcut.Name, ShortcutDocument.DestinationFilePath(shortcut));
        }

        private void tvDestination_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            _oldPath = tvDestination.SelectedNode.FullPath;
        }



        private void tvDestination_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {

                if (tvDestination.SelectedNode.Level < 2)
                {
                    var message = string.Format("Can't delete {0} folder", tvDestination.SelectedNode.Text);
                    CallMessageBox(message, "Delete Folder Warning");
                }
                else
                {
                    string objectType = string.Empty;
                    if(tvDestination.SelectedNode.ImageIndex == 12)
                    {
                        objectType = "shortcut";
                    }
                    else
                    {
                        objectType = "folder";
                    }
                    var message = string.Format("Do you really want to delete the {0} {1}?", tvDestination.SelectedNode.Text, objectType);
                    var result = MessageBox.Show(message, "Deletion Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (DialogResult.OK == result)
                    {
                        RemoveDirectoryFromDocumentFragmentStyle(tvDestination.SelectedNode);
                    }
                }
            }

            if (e.KeyCode == Keys.F2)
            {
                if (tvDestination.SelectedNode.Level < 2)
                {
                    var message = string.Format("Can't delete {0} folder", tvDestination.SelectedNode.Text);
                    CallMessageBox(message, "Delete Folder Warning");
                }
                else
                {
                    tvDestination.LabelEdit = true;
                    tvDestination.SelectedNode.BeginEdit();
                }
            }
        }
    }
}
