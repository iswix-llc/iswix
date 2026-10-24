using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.ComponentModel;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiX.WiXModel.Core;
using static FireworksFramework.Types.Enums;
using Wxs = IsWiX.WiXModel;

namespace ShortCutsDesigner
{
    public partial class ShortCuts : UserControl, IFireworksDesigner
    {
        private XNamespace WixNamespace;
        private const string DestinationPathPrefix = "Destination Computer\\[MergeRedirectFolder]\\";
        private const string MrfHashStringPrefix = "MergeRedirectFolder\\";
        List<Wxs.Shortcut> _shortcuts;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
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
        public ShortCuts()
        {
            InitializeComponent();
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
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("ShortCutsDesigner.ShortCuts.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ShortCutsDesigner.License.txt")).ReadToEnd();
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
            WixNamespace = WixNamespaces.Wxs;

            _shortcuts = ShortcutDocument.Load(_documentManager.Document);
            Cursor = Cursors.WaitCursor;
            tvDestination.Nodes.Clear();
            //Ensure the XML file has any Directories.
            try
            {
                TreeNode treeNode = AddDestinationComputerToDestination(null);
                AddFirstLevelFolder(treeNode, "DesktopFolder");
                AddFirstLevelFolder(treeNode, "ProgramMenuFolder");
                AddFirstLevelFolder(treeNode, "SendToFolder");
                //AddFirstLevelFolder(treeNode, "StartMenuFolder");  Old School
                AddFirstLevelFolder(treeNode, "StartupFolder");
            }
            catch (Exception exception)
            {
                throw exception;
            }
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
                TreeNode parentNode = null;
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
            foreach (var part in path.Split("\\"))
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

        private TreeNode FindNodeRecursiveByText(TreeNode node, string text, string tag)
        {
            if ((node.Text == text) && node.Tag.ToString() == tag)
            {
                return node;
            }
            foreach (TreeNode treeNode in node.Nodes)
            {
                TreeNode foundNode = FindNodeRecursiveByText(treeNode, text, tag);
                if (foundNode != null) return foundNode;
            }
            return null;
        }

        private TreeNode AddDestinationComputerToDestination(XElement element)
        {
            var nodeName = CreateNodeName(element);
            var subTreeNode = tvDestination.Nodes.Add(nodeName);
            subTreeNode.ImageIndex = (int)ImageLibrary.Computer;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Computer;
            if (element != null)
            {
                subTreeNode.Tag = GetElementId(element);
            }
            return subTreeNode;
        }

        private string CreateNodeName(XElement element)
        {
            if (element == null)
            {
                return "Destination Computer";
            }
            else
            {
                return String.Format("{0}", GetElementName(element));
            }
        }

        private bool SubElementIsDirectory(XElement directoryElement)
        {
            XElement subElement = directoryElement.Element(WixNamespace + "Directory");
            return subElement != null;
        }

        private string GetElementName(XElement element)
        {
            var elementId = GetElementId(element);
            var dirName = (IdIsWithinSystemFolderPropertyEnum(elementId)) ? "[" + elementId + "]" : element.Attribute("Name").Value;
            //Rename SourceDir to Destination Computer, if needed
            dirName = (dirName != "SourceDir") ? dirName : "Destination Computer";
            return dirName;
        }

        private string GetElementId(XElement element)
        {
            var id = (element.Attribute("Id") != null) ? element.Attribute("Id").Value : "No ID";
            return id;
        }
        private bool DirectoryIsNotEmpty(XElement directoryElement)
        {
            if (directoryElement.HasElements)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsASpecialDirectoryToIgnore(XElement element)
        {
            var elementId = GetElementId(element);
            return IsASpecialDirectoryToIgnore(elementId);
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
            // foreach item on the special folders list, add a menu item for the ones we dont ignore
            // if it already exists in the document make the item disabled.
            foreach (var s in Enum.GetNames(typeof(SystemFolderProperty)))
            {
                if (IsASpecialDirectoryToIgnore(s))
                {
                    // pass over the item
                }
                else
                {
                    if (s == "GlobalAssemblyCache") continue;
                    var toolStripMenuItem = new ToolStripMenuItem(s);
                    toolStripMenuItem.Name = s;
                    if (ExistsInTree(s))
                    {
                        toolStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        // add event handler
                        toolStripMenuItem.Click += (toolStripMenuItem_Click);
                    }
                    cmsDestinationRoot.Items.Add(toolStripMenuItem);
                }
            }
        }
        private bool ExistsInTree(string tag)
        {
            foreach (TreeNode node in tvDestination.Nodes[0].Nodes)
            {
                if (node.Tag == null || node.Tag.ToString() == tag)
                {
                    return true;
                }
            }
            return false;
        }
        void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            switch (toolStripMenuItem.Name)
            {
                case "DesktopFolder":
                    AddSpecialFolder("DesktopFolder");
                    break;
                case "ProgramMenuFolder":
                    AddSpecialFolder("ProgramMenuFolder");
                    break;
                case "SendToFolder":
                    AddSpecialFolder("SendToFolder");
                    break;
                case "StartMenuFolder":
                    AddSpecialFolder("StartMenuFolder");
                    break;
                case "StartupFolder":
                    AddSpecialFolder("StartupFolder");
                    break;
                default:
                    break;
            }
        }
        private void AddSpecialFolder(string folderId)
        {
            //find DestinationRoot
            XElement destinationComputer = FindDirectoryElement("SourceDir", "TARGETDIR");

            //add folder with folderId as the Id attribute value
            XElement specialDirectory = new XElement(WixNamespace + "Directory", new XAttribute("Id", folderId));
            destinationComputer.Add(specialDirectory);

            //reload the document so the new folder can be seen
            //SortData();
            LoadDocument();
        }

        private void AddSpecialFolder(string folderId, string folderName)
        {
            //find DestinationRoot
            XElement destinationComputer = FindDirectoryElement("SourceDir", "TARGETDIR");

            //add folder with folderId as the Id attribute value
            XElement specialDirectory = new XElement(WixNamespace + "Directory", new XAttribute("Id", folderId), new XAttribute("Name", folderName));
            destinationComputer.Add(specialDirectory);

            //reload the document so the new folder can be seen
            //SortData();
            LoadDocument();
        }

        private XElement FindDirectoryElement(TreeNode treeNode)
        {
            string tag = string.Empty;
            if (treeNode.Tag != null)
            {
                tag = treeNode.Tag.ToString();
            }


            var selectedDirectory = FindDirectoryElement(_documentManager.Document, treeNode.Text, tag);
            return selectedDirectory;
        }

        private XElement FindDirectoryElement(string text, string tag)
        {
            var selectedDirectory = FindDirectoryElement(_documentManager.Document, text, tag);
            return selectedDirectory;
        }

        private XElement FindDirectoryElement(XDocument document, string text, string tag)
        {
            XElement selectedDirectory;
            try
            {
                switch (text)
                {
                    case "[DesktopFolder]":

                        selectedDirectory = (from item in document.Descendants(WixNamespace + "Directory")
                                             where
                                                 item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == "DesktopFolder"
                                             select item).First();
                        break;

                    case "[ProgramMenuFolder]":
                        selectedDirectory = (from item in document.Descendants(WixNamespace + "Directory")
                                             where
                                                 item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == "ProgramMenuFolder"
                                             select item).First();
                        break;

                    case "[SendToFolder]":

                        selectedDirectory = (from item in document.Descendants(WixNamespace + "Directory")
                                             where
                                                 item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == "SendToFolder"
                                             select item).First();
                        break;

                    case "[StartMenuFolder]":

                        selectedDirectory = (from item in document.Descendants(WixNamespace + "Directory")
                                             where
                                                 item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == "StartMenuFolder"
                                             select item).First();
                        break;

                    case "[StartupFolder]":

                        selectedDirectory = (from item in document.Descendants(WixNamespace + "Directory")
                                             where
                                                 item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == "StartupFolder"
                                             select item).First();
                        break;

                    default:
                        selectedDirectory = (from item in document.Descendants(WixNamespace + "Directory")
                                             where
                                                 item.Attribute("Name") != null &&
                                                 item.Attribute("Name").Value == text
                                                 && item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == tag
                                             select item).First();
                        break;

                }
            }
            catch (Exception)
            {
                selectedDirectory = null;
            }

            return selectedDirectory;
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
            var previousName = e.Node.Text;
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

        private bool CanRenameFragmentStyle(string previousName, string newName)
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
                string oldDirectory = old.Split("\\").First();
                string oldSubDirectory = old.Substring(oldDirectory.Length + 1, old.Length - oldDirectory.Length - 1);
                string after = tvDestination.SelectedNode.FullPath.Replace("Destination Computer\\", "").Replace($"[", "").Replace("]", "");
                string afterDirectory = after.Split("\\").First();
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
        private static string CreateDirectoryStringForHash(string path, string newDirectory)
        {
            var newPath = String.Format("{0}\\{1}", path, newDirectory);
            return newPath.Replace(DestinationPathPrefix, MrfHashStringPrefix);
        }

        private static string CreateDirectoryStringForHash(string path)
        {
            return path.Replace(DestinationPathPrefix, MrfHashStringPrefix);
        }

        private static string CreateFilePathForHash(string fullPath, string fileName)
        {
            var newPath = String.Format("{0}\\{1}", fullPath, fileName);
            return newPath.Replace(DestinationPathPrefix, MrfHashStringPrefix);
        }
        internal static string GetMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input.ToUpper()));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpper();
        }
        private void ReCalculateChildrenHashes(XElement directoryElement, string newDirectoryPath)
        {
            // recalculate hash for directory
            string newIdHash = GetMd5Hash(newDirectoryPath);

            UpdateShortcutDirectory(directoryElement.Attribute("Id").Value, "scd" + newIdHash);
            directoryElement.Attribute("Id").Value = "scd" + newIdHash;

            // repeat the task for each subdirectory of directoryElement
            var directoryList = directoryElement.Elements(WixNamespace + "Directory");
            foreach (XElement subDirectory in directoryList)
            {
                var newSubDirectoryPath = String.Format("{0}\\{1}", newDirectoryPath, subDirectory.Attribute("Name").Value);
                ReCalculateChildrenHashes(subDirectory, newSubDirectoryPath);
            }
        }
        private static void CallMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void AddDirectoryToDestination(TreeNode destinationFolder, TreeNode sourceFolder)
        {
            if (!DestinationContainsFolder(destinationFolder, sourceFolder))
            {
                // create new folder in destination
                var directory = CreateNewDirectory(destinationFolder, sourceFolder);
                var parentDirectory = FindDirectoryElement(destinationFolder);
                parentDirectory.Add(directory);

                // need to reload data so that the newly added directory can be found in the tree view
                //SortData();
                LoadDocument();

                // the following code choses the newly added child folder as the place in which
                // to add files.
                TreeNode foundNode = FindNodeRecursiveByText(tvDestination.Nodes[0], sourceFolder.Text, GetElementId(directory));
                if (foundNode != null)
                {
                    tvDestination.SelectedNode = foundNode;
                    foundNode.EnsureVisible();
                }

            }
            else
            {
                CallMessageBox(String.Format("Directory [{0}] already exists in destination location.", sourceFolder.Text), "Copy Directory Warning");
                sourceFolder.Remove();
            }
        }
        private bool DestinationContainsFolder(TreeNode destinationFolder, TreeNode sourceFolder)
        {
            var directory = FindDirectoryElement(destinationFolder);
            var subDirectories = directory.Elements(WixNamespace + "Directory");

            foreach (var subDirectory in subDirectories)
            {
                if (subDirectory.Attribute("Name") != null)
                {
                    if (subDirectory.Attribute("Name").Value.ToLower().Equals(sourceFolder.Text.ToLower())) return true;
                }
            }
            return false;
        }
        private XElement CreateNewDirectory(TreeNode treeNode, TreeNode sourceFolder)
        {
            var nodeInTree = FindNodeRecursiveByText(tvDestination.Nodes[0], treeNode.Text, treeNode.Tag.ToString());
            var path = nodeInTree.FullPath;
            var newDirectory = sourceFolder.Text;
            var input = CreateDirectoryStringForHash(path, newDirectory);

            var directory = new XElement(WixNamespace + "Directory",
                                    new XAttribute("Id", "scd" + GetMd5Hash(input)),
                                    new XAttribute("Name", sourceFolder.Text));

            return directory;
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

        private bool ParentDirectoryNowEmpty(TreeNode directoryNode)
        {
            var directoryElement = FindDirectoryElement(directoryNode);
            return !directoryElement.HasElements;
        }
        private static bool IsRootDirectory(TreeNode node)
        {
            if (IdIsWithinSystemFolderPropertyEnum(node.Tag.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CreateShortCut();
        }

        private void toolStripMenuItemCreateShortCut_Click(object sender, EventArgs e)
        {
            CreateShortCut();
        }

        private void CreateShortCut()
        {
            ComponentPicker picker = new ComponentPicker(ShortcutDocument.GetShortCutCandidates(_documentManager.Document));
            picker.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = picker.ShowDialog();
            if (dr != DialogResult.Cancel)
            {
                if (!string.IsNullOrEmpty(picker.FileKey) || picker.FileElement != null)
                {
                    string scDirectory = string.Empty;
                    string scSubdirectory = string.Empty;

                    string fullPath = tvDestination.SelectedNode.FullPath.Replace("Destination Computer\\", "").Replace("[", "").Replace("]", "");
                    List<string> parts = fullPath.Split("\\").ToList();
                    scDirectory = parts[0];
                    parts.RemoveAt(0);
                    scSubdirectory = string.Join("\\", parts.ToArray());

                    string prefix = picker.FileName;
                    int index = 0;
                    bool added = false;
                    do
                    {
                        index++;
                        bool exists = false;
                        if (_shortcuts != null)
                        {
                            foreach (var shortcut in _shortcuts)
                            {
                                string name = string.Format("{0}{1}", prefix, index);
                                if (IsInFolderWithName(shortcut, scDirectory, scSubdirectory, name))
                                {
                                    exists = true;
                                    break;
                                }
                            }

                        }

                        if (exists == false)
                        {
                            string name = string.Format("{0}{1}", prefix, index);

                            if (index == 1)
                            {
                                foreach (var shortcut in _shortcuts)
                                {
                                    if (IsInFolderWithName(shortcut, scDirectory, scSubdirectory, prefix))
                                    {
                                        exists = true;
                                        break;
                                    }
                                }
                                if (exists == false)
                                {
                                    name = prefix;
                                }

                            }

                            Wxs.Shortcut shortCut = ShortcutDocument.Create(name, scDirectory, scSubdirectory, picker.FileElement);
                            _shortcuts.Add(shortCut);
                            AddShortcutNode(shortCut);
                            added = true;
                        }
                    }
                    while (added == false);
                }
            }
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

        private void UpdateShortcutDirectory(string oldId, string newId)
        {
            foreach (var shortcut in _shortcuts)
            {
                if (shortcut.Directory == oldId)
                {
                    ShortcutDocument.SetDirectory(shortcut, newId);
                }
            }
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
                shortcut1.Read((Wxs.Shortcut)e.Node.Tag);
                propertyGridShortCut.SelectedObject = shortcut1;
            }
            else
            {
                propertyGridShortCut.SelectedObject = null;
            }
        }

        private void propertyGridShortCut_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            shortcut1.Write(e.ChangedItem.Label);
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
