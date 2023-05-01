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
using IsWiXAutomationInterface;
using static FireworksFramework.Types.Enums;

namespace ShortCutsDesigner
{
    public partial class ShortCuts : UserControl, IFireworksDesigner
    {
        private XNamespace WixNamespace;
        private const string DestinationPathPrefix = "Destination Computer\\[MergeRedirectFolder]\\";
        private const string MrfHashStringPrefix = "MergeRedirectFolder\\";
        IsWiXShortCuts _shortcuts;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

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
            if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module ||
                (_documentManager.Document.GetWiXVersion() == WiXVersion.v4 && _documentManager.Document.GetDocumentType() == IsWiXDocumentType.Fragment))
            {
                return true;
            }
            else
            {
                return false;
            }
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
            WixNamespace = _documentManager.Document.GetWiXNameSpace();

            IsWixUpgradeFixer.Fix();

            _shortcuts = new IsWiXShortCuts();
            Cursor = Cursors.WaitCursor;
            SortData();
            tvDestination.Nodes.Clear();
            //Ensure the XML file has any Directories.
            try
            {
                if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
                {
                    var firstDirectory = _documentManager.Document.Descendants(WixNamespace + "Directory").First();
                    AddDirectoryNodesToDestination(firstDirectory, null);
                }
                else
                {
                    TreeNode treeNode = AddDestinationComputerToDestination(null);
                    AddFirstLevelFolder(treeNode, "DesktopFolder");
                    AddFirstLevelFolder(treeNode, "ProgramMenuFolder");
                    AddFirstLevelFolder(treeNode, "SendToFolder");
                    AddFirstLevelFolder(treeNode, "StartMenuFolder");
                    AddFirstLevelFolder(treeNode, "StartupFolder");
                }
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
                if (shortcut.Directory == folderName)
                {
                    if (!string.IsNullOrEmpty(shortcut.Subdirectory))
                    {
                        directoryNode = GetOrCreateTreeNode(directoryNode, shortcut.Subdirectory);
                    }
                    var newShortcutNode = directoryNode.Nodes.Add(string.Format("{0} ({1})", shortcut.Name, shortcut.DestinationFilePath));
                    newShortcutNode.ImageIndex = (int)ImageLibrary.Shortcut;
                    newShortcutNode.SelectedImageIndex = (int)ImageLibrary.Shortcut;
                    newShortcutNode.Tag = shortcut;
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
                if(!found)
                {
                    searchNode = searchNode.Nodes.Add($"{part}");
                    searchNode.ImageIndex = (int)ImageLibrary.FolderClosed;
                    searchNode.SelectedImageIndex = (int)ImageLibrary.FolderOpen;
                }
            }
            return searchNode;
        }

        private void AddDirectoryNodesToDestination(XElement element, TreeNode treeNode)
        {
            if (treeNode == null)
            {
                treeNode = AddDestinationComputerToDestination(element);
                AddDirectoryNodesToDestination(element, treeNode);
            }
            else
            {
                foreach (var subElement in element.Elements())
                {
                    switch (subElement.Name.LocalName)
                    {
                        case "Directory":
                            if (!IsASpecialDirectoryToIgnore(subElement.Attribute("Id").Value))
                            {
                                var newNode = treeNode.Nodes.Add(CreateNodeName(subElement));
                                var newNodeTag = GetElementId(subElement);
                                newNode.ImageIndex = (int)ImageLibrary.FolderOpen;
                                newNode.SelectedImageIndex = (int)ImageLibrary.FolderOpen;
                                newNode.Tag = newNodeTag;
                                AddDirectoryNodesToDestination(subElement, newNode);


                                var shortcuts = from s in _shortcuts
                                                where s.Directory.Equals(subElement.Attribute("Id").Value)
                                                select s;

                                foreach (var shortcut in shortcuts)
                                {
                                    var newShortcutNode = newNode.Nodes.Add(string.Format("{0} ({1})", shortcut.Name, shortcut.DestinationFilePath));
                                    newShortcutNode.ImageIndex = (int)ImageLibrary.Shortcut;
                                    newShortcutNode.SelectedImageIndex = (int)ImageLibrary.Shortcut;
                                    newShortcutNode.Tag = shortcut;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

            }


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
            var selectedDirectory = FindDirectoryElement(_documentManager.Document, treeNode.Text, treeNode.Tag.ToString());
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
            if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
            {
                AddNodeToDestinationTree();
            }
            else
            {
                AddNodeToDestinationTreeFragmentStyle();
            }
        }

        private void AddNodeToDestinationTree()
        {
            // Check for existance of directories that contain the text 'New Folder'
            var directoryElement = FindDirectoryElement(tvDestination.SelectedNode);
            var newFolderList = (from s in directoryElement.Elements(WixNamespace + "Directory")
                                 where s.Attribute("Name").Value.Contains("New Folder")
                                 select s);
            var newFolderName = "New Folder";
            if (newFolderList.Count() > 0)
            {
                newFolderName += String.Format(" ({0})", newFolderList.Count());
            }
            //create a node
            var nodeToEdit = tvDestination.SelectedNode.Nodes.Add(newFolderName, newFolderName);
            nodeToEdit.Tag = newFolderName;
            tvDestination.SelectedNode = nodeToEdit;
            tvDestination.LabelEdit = true;
            tvDestination.SelectedNode.BeginEdit();
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

            bool canRename = true;
            if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
            {
                canRename = CanRename(previousName, e.Label);
            }
            else
            {
                canRename = CanRenameFragmentStyle(previousName, e.Label);
            }
            if(!canRename)
            {
                CallMessageBox(String.Format("A folder with the name [{0}] already exists.", e.Node.Text), "Folder Rename Warning");
                tvDestination.LabelEdit = true;
                tvDestination.SelectedNode.BeginEdit();
                e.CancelEdit = true;
            }

        }

        private bool CanRename(string previousName, string newName)
        {
            bool canRename = true;
            // determine if the node with the previous name exists 
            var directoryElement = FindDirectoryElement(previousName, newName);
            if (directoryElement != null)
            {
                // find out if a node with the new label exists within document
                string nodePath = tvDestination.SelectedNode.FullPath;
                string newDirectoryPath = CreateDirectoryStringForHash(nodePath);
                var directoryId = "scd" + GetMd5Hash(newDirectoryPath);
                var directoryElementWithSameNameAsLabel = FindDirectoryElement(newName, directoryId);
                if (directoryElementWithSameNameAsLabel == null)
                {
                    // rename the node
                    directoryElement.Attribute("Name").Value = newName;
                    ReCalculateChildrenHashes(directoryElement, newDirectoryPath);
                    // set the selected node's tag to the new directory id
                    // so that we can find it again after we load the document
                    tvDestination.SelectedNode.Tag = directoryId;
                    //SortData();
                    LoadDocument();
                }
                else
                {
                    canRename = false;
                }
            }
            else
            {
                // need to take information from node and create directory in XML
                AddDirectoryToDestination(tvDestination.SelectedNode.Parent, tvDestination.SelectedNode);
            }
            return canRename;
        }

        private bool CanRenameFragmentStyle(string previousName, string newName)
        {
            int count = 0;
            foreach (TreeNode node in tvDestination.SelectedNode.Parent.Nodes)
            {
                if(node.Text == newName )
                {
                    count++; 
                }
            }
            if (count < 2)
            {
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
        static string GetMd5Hash(string input)
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
            RemoveDirectoryFromDocument(tvDestination.SelectedNode);
        }
        private void RemoveDirectoryFromDocument(TreeNode treeNode)
        {
            var directoryToRemove = FindDirectoryElement(treeNode);
            var tempNode = treeNode.Parent;
            directoryToRemove.Remove();
            //SortData();
            LoadDocument();
            var parent = FindNodeRecursiveByText(tvDestination.Nodes[0], tempNode.Text, tempNode.Tag.ToString());
            if (parent != null)
            {
                tvDestination.SelectedNode = parent;
                tvDestination.SelectedNode.Expand();
            }
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
            if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
            {
                AddNodeToDestinationTree();
            }
            else
            {
                AddNodeToDestinationTreeFragmentStyle();
            }
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

        public void SortData()
        {
            if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
            {

                var tempDocument = XDocument.Parse(_documentManager.Document.ToString());
                var tempStartElement = FindDirectoryElement(tempDocument, "SourceDir", "TARGETDIR");
                var tempDirectoryList = (from item in tempStartElement.Elements(WixNamespace + "Directory")
                                         orderby item.Attribute("Id").Value ascending
                                         select item);
                foreach (var directory in tempDirectoryList)
                {
                    if (IsASpecialDirectoryToIgnore(directory))
                    {
                        // ignore
                    }
                    else
                    {
                        SortDirectoriesAndChildren(directory);
                    }
                }
                // put order by list into original document
                var startingElement = FindDirectoryElement("SourceDir", "TARGETDIR");
                startingElement.Elements().Remove();
                startingElement.Add(tempDirectoryList);
            }

        }

        private void SortDirectoriesAndChildren(XElement originalStartingDirectory)
        {
            if (originalStartingDirectory == null) return;
            var elementName = (originalStartingDirectory.Attribute("Name") != null) ? originalStartingDirectory.Attribute("Name").Value : "[" + originalStartingDirectory.Attribute("Id").Value + "]";
            var elementId = (originalStartingDirectory.Attribute("Id") != null) ? originalStartingDirectory.Attribute("Id").Value : String.Empty;

            var tempDocument = XDocument.Parse(_documentManager.Document.ToString());
            var tempStartingDirectory = FindDirectoryElement(tempDocument, elementName, elementId);



            if (HasSubDirectories(originalStartingDirectory))
            {
                IEnumerable<XElement> originalSubDirectories;
                IOrderedEnumerable<XElement> sortedSubDirectories;

                // sorts the directories 
                if (originalStartingDirectory.Attribute("Id") != null && originalStartingDirectory.Attribute("Id").Value == "TARGETDIR")
                {
                    originalSubDirectories = (from dir in originalStartingDirectory.Elements(WixNamespace + "Directory")
                                              select dir);

                    sortedSubDirectories = (from dir in tempStartingDirectory.Elements(WixNamespace + "Directory")
                                            orderby dir.Attribute("Id").Value ascending
                                            select dir);
                }
                else
                {
                    originalSubDirectories = (from dir in originalStartingDirectory.Elements(WixNamespace + "Directory")
                                              where dir.Attribute("Name") != null
                                              select dir);

                    sortedSubDirectories = (from dir in tempStartingDirectory.Elements(WixNamespace + "Directory")
                                            where dir.Attribute("Name") != null
                                            orderby dir.Attribute("Name").Value ascending
                                            select dir);
                }

                foreach (var directory in sortedSubDirectories)
                {
                    SortDirectoriesAndChildren(directory);
                }
                originalSubDirectories.Remove();
                originalStartingDirectory.Add(sortedSubDirectories);
            }
        }
        private bool HasSubDirectories(XElement directory)
        {
            return directory.Elements(WixNamespace + "Directory").Count() > 0;
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
            ComponentPicker picker = new ComponentPicker(_shortcuts);
            picker.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = picker.ShowDialog();
            if (dr != DialogResult.Cancel)
            {
                if (!string.IsNullOrEmpty(picker.FileKey) || picker.FileElement != null )
                {
                    string scDirectory = string.Empty;
                    string scSubdirectory = string.Empty;

                    if(_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
                    {
                        scDirectory = tvDestination.SelectedNode.Tag as string;
                    }
                    else
                    {
                        string fullPath = tvDestination.SelectedNode.FullPath.Replace("Destination Computer\\", "").Replace("[", "").Replace("]", "");
                        scDirectory = fullPath.Split("\\").First();
                        scSubdirectory = fullPath.Substring(scDirectory.Length+1, fullPath.Length - scDirectory.Length-1);
                    }

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
                                if (shortcut.Directory.Equals(scDirectory, StringComparison.InvariantCultureIgnoreCase) && shortcut.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
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
                                    if (shortcut.Directory.Equals(scDirectory, StringComparison.InvariantCultureIgnoreCase) && shortcut.Name.Equals(prefix, StringComparison.InvariantCultureIgnoreCase))
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

                            IsWiXShortCut shortCut = _shortcuts.Create(name, scDirectory, scSubdirectory, picker.FileElement);
                            AddShortcutNode(shortCut);
                            added = true;
                        }
                    }
                    while (added == false);
                }
            }
        }


        private void AddShortcutNode(IsWiXShortCut iswixShortcut)
        {
            var subTreeNode = tvDestination.SelectedNode.Nodes.Add(string.Format("{0} ({1})", iswixShortcut.Name, iswixShortcut.DestinationFilePath));
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
                if (shortcut.Directory.Equals(oldId))
                {
                    shortcut.Directory = newId;
                }
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            IsWiXShortCut shortcut = tvDestination.SelectedNode.Tag as IsWiXShortCut;
            shortcut.Delete();
            tvDestination.SelectedNode.Remove();
            tvDestination.SelectedNode = tvDestination.Nodes[0];

        }

        private void tvDestination_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (IsShortcutNode())
            {
                shortcut1.Read(e.Node.Tag as IsWiXShortCut);
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
            UpdatedSelectedNodeText();
        }

        private void UpdatedSelectedNodeText()
        {
            IsWiXShortCut shortcut = tvDestination.SelectedNode.Tag as IsWiXShortCut;
            if (shortcut != null)
            {
                tvDestination.SelectedNode.Text = CalculateSelectedNodeText(shortcut);
            }
        }

        private string CalculateSelectedNodeText(IsWiXShortCut shortcut)
        {
            return string.Format("{0} ({1})", shortcut.Name, shortcut.DestinationFilePath);
        }

    }
}
