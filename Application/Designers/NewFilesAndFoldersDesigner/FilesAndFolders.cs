using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiXAutomationInterface;
using NewFilesAndFoldersDesigner.Enums;
using static FireworksFramework.Types.Enums;

namespace Designers.NewFilesAndFolders
{

    public partial class FilesAndFolders : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        IsWiXComponentGroup _isWiXComponentGroup;
        string rootPath;

        private const string Seperator = ";";
        private const string Wildcard = "*";
        private const string BaseExcludePattern = "";
        private const string SourceDirVar = "$(var.SourceDir)";
        private string SourceStart;
        protected string fileFilterPattern;
        protected string fileExcludePattern;
        private string DragSourceName;
        private TreeNode HoverNode;
        private const string ListViewItemCollectionFormatIdentifier = "System.Windows.Forms.ListView+SelectedListViewItemCollection";


        public string FileFilterPattern
        {
            get { return fileFilterPattern; }
            set
            {
                fileFilterPattern = value + Wildcard;  // passing the wildcard automatically for the user
            }
        }

        public string FileExcludePattern
        {
            get { return fileExcludePattern; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    fileExcludePattern = BaseExcludePattern;
                }
                else
                {
                    var input = (value.EndsWith("*")) ? value : value + Wildcard;
                    fileExcludePattern = BaseExcludePattern + Seperator + input;
                }
            }
        }

        public FilesAndFolders()
        {
            InitializeComponent();
#if !DEBUG

            // create the column sorter and assign it to the correct control
            lvColumnSorter = new ListViewColumnSorter();
            lvSourceFiles.ListViewItemSorter = lvColumnSorter;

            lvDestinationColumnSorter = new ListViewColumnSorter();
            lvDestination.ListViewItemSorter = lvDestinationColumnSorter;
#endif
        }

        private void PopulateSource()
        {
            Cursor = Cursors.WaitCursor;
            tvSourceFiles.Nodes.Clear();
            var tnStart = tvSourceFiles.Nodes.Add(SourceStart);
            FileFilterPattern = textBoxIncludeFilter.Text;
            FileExcludePattern = textBoxExcludeFilter.Text;
            PopulateSourceTree(SourceStart, tnStart, PopulateMode.OneLevel);
            tnStart.Expand();
            tvSourceFiles.SelectedNode = tnStart;
            DisplaySourceListItemsForSelectedNode(tvSourceFiles.SelectedNode);
            Cursor = Cursors.Default;
        }

        public void LoadData()
        {

            if (String.IsNullOrEmpty(_documentManager.DocumentPath))
            {
                CallMessageBox("Please save your file before using Files and Folders.", "File Save Warning");
                return;
            }
            _isWiXComponentGroup = new IsWiXComponentGroup();
            rootPath = _isWiXComponentGroup.GetRootPath();
            tvSourceFiles.Nodes.Clear();  //Clear before loading

            if (_isWiXComponentGroup.GetComponentRules() == ComponentRules.OneToMany)
            {
                rbOneToMany.Checked = true;
            }
            else
            {
                rbOneToOne.Checked = true;
            }

            try
            {
                if (!rootPath.Contains(":"))
                {
                    var documentInfo = new FileInfo(_documentManager.DocumentPath);
                    rootPath = System.IO.Path.Combine(documentInfo.DirectoryName, rootPath);
                }

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }
                SourceStart = new DirectoryInfo(rootPath).FullName;
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("An error occurred trying to use '{0}' as the SourceDir.  Defaulting to document directory.", rootPath));
            }


            PopulateSource();
            LoadDocument();


        }

        private void LoadDocument()
        {
            Cursor = Cursors.WaitCursor;
            List<string> dirs = _isWiXComponentGroup.GetDirectories();
            AddDirectoryNodesToDestination(dirs);
            tvDestination.ExpandAll();
            bool found = false;
            foreach (TreeNode node in tvDestination.Nodes[0].Nodes)
            {
                if (node.Text == "[INSTALLLOCATION]")
                {
                    tvDestination.SelectedNode = node;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                tvDestination.SelectedNode = tvDestination.Nodes[0];
            }

            Cursor = Cursors.Default;
        }


        private void AddDirectoryNodesToDestination(List<string> dirs)
        {
            tvDestination.Nodes.Clear();
            var subTreeNode = tvDestination.Nodes.Add("Destination Computer");
            subTreeNode.ImageIndex = (int)ImageLibrary.Computer;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Computer;

            string pathSeparator = @"\";
            foreach (var dir in dirs)
            {
                string subPathAgg = null;
                TreeNode lastNode = null;
                foreach (string subPath in dir.Split(pathSeparator))
                {
                    subPathAgg += subPath + pathSeparator;
                    TreeNode[] nodes = subTreeNode.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                        {
                            lastNode = subTreeNode.Nodes.Add(subPathAgg, "[" + subPath + "]");
                            if (IdIsWithinSystemFolderPropertyEnum(subPath))
                            {
                                lastNode.ImageIndex = (int)ImageLibrary.BlueFolderClosed;
                                lastNode.SelectedImageIndex = (int)ImageLibrary.BlueFolderOpen;
                            }
                        }
                        else
                        {
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                        }
                    else
                    {
                        lastNode = nodes[0];
                    }
                }
            }
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


        private static void CallMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void PopulateSourceTree(string startingLocation, TreeNode currentNode, PopulateMode populateMode)
        {
            currentNode.Nodes.Clear();
            var diSourceFiles = new DirectoryInfo(startingLocation);
            try
            {
                var directoryList = diSourceFiles.GetDirectories();
                Array.Sort(directoryList, new Comparison<DirectoryInfo>((d1, d2) => string.Compare(d1.Name, d2.Name)));
                foreach (var diSourceDir in directoryList)
                {
                    var childNode = currentNode.Nodes.Add(diSourceDir.Name);

                    if (populateMode == PopulateMode.OneLevel)
                    {
                        PopulateSourceTree(diSourceDir.FullName, childNode, PopulateMode.NoMore);
                    }
                    else if (populateMode == PopulateMode.Recursive)
                    {
                        PopulateSourceTree(diSourceDir.FullName, childNode, PopulateMode.Recursive);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // when we can't go into a directory because of access permissions
                // we skip over it.
                return;
            }
        }

        private void tvSourceFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //if (e.Node.Nodes.Count == 0)
            //{
            //    e.Node.Nodes.Clear();
            //    PopulateSourceTree(e.Node.FullPath, e.Node);
            //}
            DisplaySourceListItemsForSelectedNode(e.Node);
            //e.Node.Expand();
        }

        private void DisplaySourceListItemsForSelectedNode(TreeNode treeNode)
        {
            if (treeNode == null) return;
            AddListItemsFromDirectory(treeNode.FullPath);
        }

        private void AddListItemsFromDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            //get values for exclude and filter pattern from parent control
            var fileInfos = GetFilteredFiles(directoryInfo, FileExcludePattern, FileFilterPattern);

            lvSourceFiles.Items.Clear();
            //if (fileInfos == null) return;
            List<ListViewItem> listItems = GetItemListFromSelectedDirectory(directoryPath);
            if (listItems == null) return;
            // wrap in the Begin/End and AddRange to improve performance of adding directories
            // with a large number of files
            lvSourceFiles.BeginUpdate();
            lvSourceFiles.Items.AddRange(listItems.ToArray());
            lvSourceFiles.EndUpdate();
        }

        private FileInfo[] GetFilteredFiles(DirectoryInfo directoryInfo, string FileExcludePatter, string FileFilterPattern)
        {
            FileInfo[] files = RemoveExcludedFiles(GetFiles(directoryInfo), FileExcludePatter);

            var includedFiles = new List<FileInfo>();

            foreach (var file in files)
            {
                bool include = false;
                foreach (var filter in FileFilterPattern.Split(Seperator.ToCharArray()))
                {
                    if (FileNameContainsPattern(file.Name, filter))
                    {
                        include = true;
                    }
                }
                if (include)
                {
                    includedFiles.Add(file);
                }
            }
            return includedFiles.ToArray();
        }
        private List<ListViewItem> GetItemListFromSelectedDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            // get values for exclude and filter pattern from parent control
            var fileInfos = GetFilteredFiles(directoryInfo, FileExcludePattern, FileFilterPattern);

            if (fileInfos == null) return null;
            List<ListViewItem> listItems = CreateItemList(fileInfos);
            return listItems;
        }

        private static FileInfo[] GetFiles(DirectoryInfo directoryInfo)
        {
            try
            {
                return directoryInfo.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        private List<ListViewItem> CreateItemList(FileInfo[] fileInfos)
        {
            var listItems = new List<ListViewItem>();
            imageListFileIcons.Images.Clear();

            foreach (var info in fileInfos)
            {


                if (!imageListFileIcons.Images.ContainsKey(info.Extension.ToLower()))
                {
                    imageListFileIcons.Images.Add(info.Extension.ToLower(), ShellIcon.GetSmallBitmap(info.FullName));
                }

                var listViewItem = new ListViewItem(info.Name);
                listViewItem.SubItems.Add(info.Extension);
                listViewItem.SubItems.Add(String.Format(new FileSizeFormatter(), "{0}", info.Length));
                //listViewItem.SubItems.Add(info.Length.ToString("N"));
                listViewItem.SubItems.Add(info.LastWriteTime.ToString("f"));
                //Add another column (possibly hidden) that contains the full path to the file on the file system
                // column width is set to 0 (in the designer) to hide the column from view
                listViewItem.ImageKey = info.Extension;
                listViewItem.SubItems.Add(info.FullName);
                listItems.Add(listViewItem);
            }
            return listItems;
        }

        private static FileInfo[] RemoveExcludedFiles(FileInfo[] files, string fileExclusionPattern)
        {
            if (files != null)
            {
                var updatedFileList = new List<FileInfo>(files);
                var patterns = fileExclusionPattern.Split(Seperator.ToCharArray());
                foreach (var pattern in patterns)
                {
                    if (String.IsNullOrEmpty(pattern)) continue;
                    foreach (var file in files)
                    {
                        if (FileNameContainsPattern(file.Name, pattern) && updatedFileList.Contains(file))
                            updatedFileList.Remove(file);
                    }
                }
                return updatedFileList.ToArray();
            }
            else
            {
                return new List<FileInfo>().ToArray();
            }
        }

        private static bool FileNameContainsPattern(string fileName, string pattern)
        {
            var patternText = pattern.Replace(Wildcard, String.Empty);
            var patternType = DetermineTypeFromPattern(pattern);
            switch (patternType)
            {
                case PatternSearchType.AtBeginning:
                    return fileName.StartsWith(patternText, StringComparison.CurrentCultureIgnoreCase);
                case PatternSearchType.InMiddle:
                    // because String.Contains(pattern) does a case sensitive search with no way to 
                    // ignore case, we have to use String.IndexOf
                    // the = 0 case is included here to allow for the patternText being at the beginning
                    // if the pattern text is String.Empty String.IndexOf would return 0 so a check
                    // for IsNullOrEmpty(pattern) is done to ensure the code does not get to here with
                    // an empty pattern
                    return fileName.IndexOf(patternText, StringComparison.CurrentCultureIgnoreCase) >= 0;
                case PatternSearchType.AtEnd:
                    return fileName.EndsWith(patternText, StringComparison.CurrentCultureIgnoreCase);
                case PatternSearchType.Unknown:
                    return fileName.Equals(patternText);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines the type of search pattern based on the position of the wildcard character * (asterisk)
        /// If the pattern starts with the wildcard, the user expects the pattern to exist at the end of the
        /// string. If the pattern ends with the wildcard, the user expects that pattern to exist at the
        /// beginning of the string.
        /// </summary>
        /// <param name="pattern">Search string in the format *.txt, text*, or *text*</param>
        /// <returns>The Pattern Search Type (AtBeginning, InMiddle, AtEnd, or Unknown)</returns>
        private static PatternSearchType DetermineTypeFromPattern(string pattern)
        {
            if (pattern.StartsWith(Wildcard))
            {
                return pattern.EndsWith(Wildcard) ? PatternSearchType.InMiddle : PatternSearchType.AtEnd;
            }
            return pattern.EndsWith(Wildcard) ? PatternSearchType.AtBeginning : PatternSearchType.Unknown;
        }

        private static FileInfo[] GetFiles(DirectoryInfo directoryInfo, string fileFilterPattern)
        {
            var files = new List<FileInfo>();
            var patterns = fileFilterPattern.Split(Seperator.ToCharArray());
            foreach (var pattern in patterns)
            {
                files.AddRange(directoryInfo.GetFiles(pattern));
                //files.Sort();
            }
            return files.ToArray();
        }

        private void lvSourceFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if the column clicked has been clicked before
            if (e.Column == lvColumnSorter.SortColumn)
            {
                // reverse sort direction
                lvColumnSorter.Order = (lvColumnSorter.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // set column to be sorted, default order to ascending
                lvColumnSorter.Order = SortOrder.Ascending;
                lvColumnSorter.SortColumn = e.Column;
            }
            lvSourceFiles.Sort();
        }

        private void refreshSourceFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopulateSource();
        }


        private void tvSourceFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F5) return;
            PopulateSource();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplaySourceListItemsForSelectedNode(tvSourceFiles.SelectedNode);
        }

        private void lvSourceFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                DisplaySourceListItemsForSelectedNode(tvSourceFiles.SelectedNode);
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in lvSourceFiles.Items)
                {
                    item.Selected = true;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                foreach (ListViewItem item in lvSourceFiles.Items)
                {
                    item.Selected = false;
                }
            }
        }

        private void tvSourceFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            PopulateSourceTree(e.Node.FullPath, e.Node, PopulateMode.OneLevel);
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {
            return IsWiXComponentGroup.IsValidDocument();
        }

        public System.Drawing.Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("NewFilesAndFoldersDesigner.FilesAndFolders.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NewFilesAndFoldersDesigner.License.txt")).ReadToEnd();
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginName
        {
            get { return "File and Folders"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group3_filesandfolders"; }
        }


        #endregion

        private void textBoxExcludeFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                UpdateFilter();
            }
        }

        private void textBoxIncludeFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                UpdateFilter();
            }
        }

        private void UpdateFilter()
        {
            FileFilterPattern = textBoxIncludeFilter.Text;
            FileExcludePattern = textBoxExcludeFilter.Text;
            DisplaySourceListItemsForSelectedNode(tvSourceFiles.SelectedNode);
        }

        private void rbOneToMany_Click(object sender, EventArgs e)
        {
            SetComponentRulesXPI();
        }

        private void rbOneToOne_Click(object sender, EventArgs e)
        {
            SetComponentRulesXPI();
        }


        private void SetComponentRulesXPI()
        {
            ComponentRules componentRules = ComponentRules.OneToMany;
            if (rbOneToOne.Checked)
            {
                componentRules = ComponentRules.OneToOne;
            }
            _isWiXComponentGroup.SetComponentRules(componentRules);
        }

        private void tvDestination_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level.Equals(0))
            {
                lvDestination.Items.Clear();
                lvDestination.Enabled = false;
            }
            else
            {
                lvDestination.Enabled = true;
                DisplayDestinationItemsFromSelectedNode(e.Node);
            }
        }
        private void DisplayDestinationItemsFromSelectedNode(TreeNode treeNode)
        {
            List<ListViewItem> listItems = new List<ListViewItem>();
            lvDestination.Items.Clear();
            //tvDestination.SelectedNode.Text  To Display the Selected Node in the ListView
            if (treeNode == null) return;

            string directoryPath = treeNode.FullPath.Replace(@"Destination Computer\", "");
            List<Tuple<string, bool>> files = _isWiXComponentGroup.GetFiles(directoryPath);
            foreach (var file in files)
            {
                listItems.Add(CreateDestinationItemFromFile(file.Item1, file.Item2));
            }
            if (listItems == null) return;
            lvDestination.BeginUpdate();
            lvDestination.Items.AddRange(listItems.ToArray());
            lvDestination.EndUpdate();


        }
        private ListViewItem CreateDestinationItemFromFile(string source, bool keyPath)
        {
            string temp = source.Replace(SourceDirVar + @"\", "");
            var realSourcePath = Path.Combine(SourceStart, temp);
            var sourceFileInfo = new FileInfo(realSourcePath);

            var filename = sourceFileInfo.Name;// source.Substring(source.LastIndexOf("\\")+1);
            var extension = sourceFileInfo.Extension; // source.Substring(source.LastIndexOf("\\"));
            var item = new ListViewItem(filename);
            item.SubItems.Add(extension.ToLower());
            item.SubItems.Add(source);

            if (!imageListFileIconsDst.Images.ContainsKey(extension.ToLower()))
            {
                if (sourceFileInfo.Exists)
                {
                    imageListFileIconsDst.Images.Add(extension.ToLower(), ShellIcon.GetSmallBitmap(realSourcePath));
                }
            }

            try
            {
                if (keyPath)
                {
                    item.SubItems.Add("Key File");
                }
            }
            catch (Exception)
            {
                item.SubItems.Add("");
            }
            if (!imageListFileIconsDst.Images.ContainsKey(extension.ToLower()))
            {
                if (!string.IsNullOrEmpty(extension))
                {
                    item.ImageKey = "stockdeletedimage";
                    item.ForeColor = Color.Red;
                }
            }
            else
            {
                item.ImageKey = extension.ToLower();
            }
            return item;
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
                        cmsDestinationTreeDefault.Show(tvDestination, p);
                        cmsDestinationTreeDefault.Items[0].Visible = true;
                        cmsDestinationTreeDefault.Items[1].Visible = false;
                        cmsDestinationTreeDefault.Items[2].Visible = true;
                        if (node.Nodes.Count == 0)
                        {
                            cmsDestinationTreeDefault.Items[4].Visible = false;
                            cmsDestinationTreeDefault.Items[5].Visible = false;
                        }
                        else
                        {
                            cmsDestinationTreeDefault.Items[4].Visible = true;
                            cmsDestinationTreeDefault.Items[5].Visible = true;
                        }
                    }
                    else
                    {
                        cmsDestinationTreeDefault.Show(tvDestination, p);
                        cmsDestinationTreeDefault.Items[0].Visible = true;
                        cmsDestinationTreeDefault.Items[1].Visible = true;
                        cmsDestinationTreeDefault.Items[2].Visible = true;
                    }
                }
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
            List<string> dirs = _isWiXComponentGroup.GetDirectories();
            foreach (ToolStripItem toolStripItem in cmsDestinationRoot.Items)
            {
                toolStripItem.Visible = true;
            }

            // foreach item on the special folders list, add a menu item for the ones we dont ignore
            // if it already exists in the document make the item disabled.
            foreach (var s in Enum.GetNames(typeof(SystemFolderProperty)))
            {
                if (IsASpecialDirectoryToIgnore(s) || dirs.Contains(s))
                {
                    // pass over the item
                }
                else
                {
                    if (s == "GlobalAssemblyCache") continue;
                    var toolStripMenuItem = new ToolStripMenuItem(s);
                    toolStripMenuItem.Name = s;
                    toolStripMenuItem.Click += toolStripMenuItem_Click;
                    cmsDestinationRoot.Items.Add(toolStripMenuItem);
                }
            }
        }

        void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            AddSpecialFolder(toolStripMenuItem.Name);
        }

        bool IsASpecialDirectoryToIgnore(string elementId)
        {
            if (IdIsWithinSystemFolderPropertyEnum(elementId))
            {
                var systemFolderProperty = (SystemFolderProperty)Enum.Parse(typeof(SystemFolderProperty), elementId, true);
                switch (systemFolderProperty)
                {
                    case SystemFolderProperty.AdminToolsFolder:
                    case SystemFolderProperty.AppDataFolder:
                    case SystemFolderProperty.CommonFiles64Folder:
                    case SystemFolderProperty.DesktopFolder:
                    case SystemFolderProperty.FavoritesFolder:
                    case SystemFolderProperty.FontsFolder:
                    case SystemFolderProperty.LocalAppDataFolder:
                    case SystemFolderProperty.MyPicturesFolder:
                    case SystemFolderProperty.PersonalFolder:
                    case SystemFolderProperty.ProgramFiles64Folder:
                    case SystemFolderProperty.ProgramMenuFolder:
                    case SystemFolderProperty.SendToFolder:
                    case SystemFolderProperty.StartupFolder:
                    case SystemFolderProperty.StartMenuFolder:
                    case SystemFolderProperty.System16Folder:
                    case SystemFolderProperty.System64Folder:
                    case SystemFolderProperty.SystemFolder:
                    case SystemFolderProperty.TempFolder:
                    case SystemFolderProperty.TemplateFolder:
                    case SystemFolderProperty.CommonFilesFolder:
                    case SystemFolderProperty.ProgramFilesFolder:
                        return true;
                    case SystemFolderProperty.ProgramFiles6432Folder:
                    case SystemFolderProperty.CommonFiles6432Folder:
                    case SystemFolderProperty.INSTALLLOCATION:
                    case SystemFolderProperty.GlobalAssemblyCache:
                    case SystemFolderProperty.System6432Folder:
                    case SystemFolderProperty.CommonAppDataFolder:
                    case SystemFolderProperty.WindowsFolder:
                    case SystemFolderProperty.WindowsVolume:
                        return false;
                    default:
                        return false;
                }
            }
            return false;
        }

        private void AddSpecialFolder(string folderId)
        {
            _isWiXComponentGroup.GetOrCreateDirectoryComponent(folderId);
            LoadDocument();
        }

        private void lvSourceFiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragSourceName = "lvSourceFiles";
            lvSourceFiles.DoDragDrop(lvSourceFiles.SelectedItems, DragDropEffects.Copy);

        }
        private void lvDestination_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection))) || (e.Data.GetDataPresent(typeof(TreeNode))))
            {
                if (tvDestination.SelectedNode != null)
                {
                    //The data from the drag source is moved to the target.	
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    // Notify the user in some fashion that they cannot drop files without selecting a node
                    //CallMessageBox("Please select a destination location.", "Drag and Drop Error");
                }
            }

        }


        private void lvDestination_DragDrop(object sender, DragEventArgs e)
        {
            //_documentManager.DisableChangeWatcher();
            if (e.Data.GetDataPresent(ListViewItemCollectionFormatIdentifier, false))
            {
                var itemCollection = (ListView.SelectedListViewItemCollection)e.Data.GetData(ListViewItemCollectionFormatIdentifier);
                List<FileMeta> files = new List<FileMeta>();
                string sourcePath = tvSourceFiles.SelectedNode.FullPath;
                string destinationPath = tvDestination.SelectedNode.FullPath.Replace(@"Destination Computer\", "");
                if (tvDestination.SelectedNode.Parent.Text == "Destination Computer")
                {
                    destinationPath = destinationPath.Replace("[", "").Replace("]", "");
                }
                foreach (ListViewItem item in itemCollection)
                {
                    FileMeta fileMeta = new FileMeta();
                    fileMeta.Source = Path.Combine(sourcePath, item.Text);
                    fileMeta.Destination = destinationPath;
                    files.Add(fileMeta);
                }
                _isWiXComponentGroup.AddFiles(files);
                DisplayDestinationItemsFromSelectedNode(tvDestination.SelectedNode);
            }

            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                if (e.Data.GetDataPresent(ListViewItemCollectionFormatIdentifier, false))
                {

                    var point = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                    TreeNode dropNode = ((TreeView)sender).GetNodeAt(point);
                    if (dropNode != null && dropNode.Text != "Destination Computer")
                    {
                        var sourceFolder = (TreeNode)e.Data.GetData(typeof(TreeNode));
                        if (sourceFolder == tvSourceFiles.Nodes[0])
                        {
                            CallMessageBox("Copying the root node of the source tree is not allowed. Please choose a different folder to add to the project.", "Copy Directory Warning");
                        }
                        else
                        {
                            // Refresh the directory structure incase of delayed load or changes since loading
                            sourceFolder.Nodes.Clear();
                            PopulateSourceTree(sourceFolder.FullPath, sourceFolder, PopulateMode.Recursive);
                            List<FileMeta> files = new List<FileMeta>();
                            string directoryPath = dropNode.FullPath;
                            GetFilesRecursive(sourceFolder, ref files, directoryPath, string.Empty);
                            _isWiXComponentGroup.AddFiles(files);
                            _isWiXComponentGroup.PruneDirectory(directoryPath);
                            LoadDocument();
                        }
                    }
                    else
                    {
                        CallMessageBox("Drop the items onto a folder", "Drop Warning");
                    }
                }
            }
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode dropNode = tvSourceFiles.SelectedNode;
                if (dropNode != null)
                {
                    var sourceFolder = (TreeNode)e.Data.GetData(typeof(TreeNode));
                    if (sourceFolder == tvSourceFiles.Nodes[0])
                    {
                        CallMessageBox("Copying the root node of the source tree is not allowed. Please choose a different folder to add to the project.", "Copy Directory Warning");
                    }
                    else
                    {
                        // Refresh the directory structure incase of delayed load or changes since loading
                        sourceFolder.Nodes.Clear();
                        PopulateSourceTree(sourceFolder.FullPath, sourceFolder, PopulateMode.Recursive);
                        List<FileMeta> files = new List<FileMeta>();
                        string directoryPath = tvDestination.SelectedNode.FullPath;
                        GetFilesRecursive(sourceFolder, ref files, directoryPath, string.Empty);
                        _isWiXComponentGroup.AddFiles(files);
                        _isWiXComponentGroup.PruneDirectory(directoryPath);
                        LoadDocument();
                    }
                }
                else
                {
                    CallMessageBox("Drop the items onto a folder", "Drop Warning");
                }

            }
            _isWiXComponentGroup.SortComponents();
            //_documentManager.EnableChangeWatcher();
        }

        private void tvSourceFiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragSourceName = "tvSourceFiles";
            tvSourceFiles.DoDragDrop(e.Item, DragDropEffects.Copy);
            //LoadDocument();
        }

        private void lvDestination_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragSourceName = "lvDestination";
            lvDestination.DoDragDrop(lvDestination.SelectedItems, DragDropEffects.Move);

        }

        private void tvDestination_DragDrop(object sender, DragEventArgs e)
        {
            Cursor = Cursors.WaitCursor;
           // _documentManager.DisableChangeWatcher();

            if (e.Data.GetDataPresent(ListViewItemCollectionFormatIdentifier, false))
            {
                var point = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode dropNode = ((TreeView)sender).GetNodeAt(point);
                if (dropNode != null && dropNode.Text != "Destination Computer")
                {
                    List<FileMeta> fileMetas = new List<FileMeta>();
                    var itemCollection = (ListView.SelectedListViewItemCollection)e.Data.GetData(ListViewItemCollectionFormatIdentifier);
                    foreach (ListViewItem item in itemCollection)
                    {
                        FileMeta fileMeta = new FileMeta();
                        fileMeta.Source = item.SubItems[4].Text;
                        fileMeta.Destination = dropNode.FullPath;
                        fileMetas.Add(fileMeta);
                    }
                    _isWiXComponentGroup.AddFiles(fileMetas);
                    LoadDocument();
                    DisplayDestinationItemsFromSelectedNode(tvDestination.SelectedNode);
                }
                else
                {
                    CallMessageBox("Drop the items onto a folder", "Drop Warning");
                }
            }

            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                var point = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode dropNode = ((TreeView)sender).GetNodeAt(point);
                if (dropNode != null && dropNode.Text != "Destination Computer")
                {
                    var sourceFolder = (TreeNode)e.Data.GetData(typeof(TreeNode));
                    if (sourceFolder == tvSourceFiles.Nodes[0])
                    {
                        CallMessageBox("Copying the root node of the source tree is not allowed. Please choose a different folder to add to the project.", "Copy Directory Warning");
                    }
                    else
                    {
                        // Refresh the directory structure incase of delayed load or changes since loading
                        sourceFolder.Nodes.Clear();
                        PopulateSourceTree(sourceFolder.FullPath, sourceFolder, PopulateMode.Recursive);
                        List<FileMeta> files = new List<FileMeta>();
                        string directoryPath = dropNode.FullPath;
                        GetFilesRecursive(sourceFolder, ref files, directoryPath, string.Empty);
                        _isWiXComponentGroup.AddFiles(files);
                        _isWiXComponentGroup.PruneDirectory(directoryPath);
                        LoadDocument();
                    }
                }
                else
                {
                    CallMessageBox("Drop the items onto a folder", "Drop Warning");
                }
            }
            _isWiXComponentGroup.SortComponents();
            HoverNode.BackColor = Color.White;
            HoverNode.ForeColor = Color.Black;
         //   _documentManager.EnableChangeWatcher();
            Cursor = Cursors.Arrow;
        }

        private void GetFilesRecursive(TreeNode sourceFolder, ref List<FileMeta> files, string destinationDirectoryPath, string parent)
        {
            parent = Path.Combine(parent, sourceFolder.Text);
            string rootPath = _isWiXComponentGroup.GetRootPath();
            var directoryInfo = new DirectoryInfo(sourceFolder.FullPath);
            var fileInfos = GetFilteredFiles(directoryInfo, FileExcludePattern, FileFilterPattern);
            if (files.Count == 0)
            {
                if (sourceFolder.Nodes.Count == 0)
                {
                    string subDirectory = (directoryInfo.FullName.Replace(rootPath + "\\", ""));
                    FileMeta fileMeta = new FileMeta();
                    fileMeta.Source = ".";
                    fileMeta.Destination = Path.Combine(destinationDirectoryPath, parent);
                    files.Add(fileMeta);
                }
            }
            foreach (var fileInfo in fileInfos)
            {
                string subDirectory = (fileInfo.DirectoryName.Replace(rootPath + "\\", ""));
                FileMeta fileMeta = new FileMeta();
                fileMeta.Source = fileInfo.FullName;
                fileMeta.Destination = Path.Combine(destinationDirectoryPath, parent);
                files.Add(fileMeta);
            }
            foreach (TreeNode subNode in sourceFolder.Nodes)
            {
                GetFilesRecursive(subNode, ref files, destinationDirectoryPath, parent);
            }
        }

        private void tvDestination_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))
            {
                switch (DragSourceName)
                {
                    case "lvDestination":
                        e.Effect = DragDropEffects.Move;
                        break;
                    case "lvSourceFiles":
                        e.Effect = DragDropEffects.Copy;
                        break;
                    default:
                        break;
                }
            }
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void tvDestination_DragOver(object sender, DragEventArgs e)
        {
            var mouseLocation = tvDestination.PointToClient(new Point(e.X, e.Y));
            var node = tvDestination.GetNodeAt(mouseLocation);
            if (node != null)
            {
                if (HoverNode == null)
                {
                    node.BackColor = Color.SteelBlue;
                    node.ForeColor = Color.White;
                    HoverNode = node;
                }
                else if (HoverNode != node)
                {
                    HoverNode.BackColor = Color.White;
                    HoverNode.ForeColor = Color.Black;
                    node.BackColor = Color.SteelBlue;
                    node.ForeColor = Color.White;
                    HoverNode = node;
                }
            }
        }

        private void lvDestination_MouseUp(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);
                cmsDestinationFiles.Show(lvDestination, p);
            }

        }

        private void removeFileFromProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvDestination.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show("Do you really want to delete the selected file(s)?", "File Deletion Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (DialogResult.OK != result) return;
                List<FileMeta> files = new List<FileMeta>();
                string destination = tvDestination.SelectedNode.FullPath.Replace("Destination Computer\\", "");
                if (tvDestination.SelectedNode.Parent.Text == "Destination Computer")
                {
                    destination = destination.Replace("[", "").Replace("]", "");
                }
                foreach (ListViewItem item in lvDestination.SelectedItems)
                {
                    files.Add(new FileMeta() { Destination = destination, Source = item.SubItems[2].Text });
                }
                _isWiXComponentGroup.DeleteFiles(files);
                foreach (ListViewItem item in lvDestination.SelectedItems)
                {
                    item.Remove();
                }
            }
        }

        private void lvDestination_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (lvDestination.SelectedItems.Count > 0)
                {
                    var result = MessageBox.Show("Do you really want to delete the selected file(s)?", "File Deletion Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (DialogResult.OK != result) return;
                    List<FileMeta> files = new List<FileMeta>();
                    string destination = tvDestination.SelectedNode.FullPath.Replace("Destination Computer\\", "");
                    if (tvDestination.SelectedNode.Parent.Text == "Destination Computer")
                    {
                        destination = destination.Replace("[", "").Replace("]", "");
                    }
                    foreach (ListViewItem item in lvDestination.SelectedItems)
                    {
                        files.Add(new FileMeta() { Destination = destination, Source = item.SubItems[2].Text });
                    }
                    _isWiXComponentGroup.DeleteFiles(files);
                    foreach (ListViewItem item in lvDestination.SelectedItems)
                    {
                        item.Remove();
                    }
                }
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in lvDestination.Items)
                {
                    item.Selected = true;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                foreach (ListViewItem item in lvDestination.Items)
                {
                    item.Selected = false;
                }
            }

        }

        private void expandAllToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();
        }

        private void collapseAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.Collapse();
        }

        private void expandAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();

        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.Collapse();
        }

        private void createNewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newFolderName = _isWiXComponentGroup.GetNextSubDirectoryName(tvDestination.SelectedNode.FullPath.Replace(@"Destination Computer\", ""));
            string directoryPath = Path.Combine(tvDestination.SelectedNode.FullPath.Replace(@"Destination Computer\", ""), newFolderName);
            var test = _isWiXComponentGroup.GetOrCreateDirectoryComponent(directoryPath);
            _isWiXComponentGroup.PruneDirectory(tvDestination.SelectedNode.FullPath);
            var nodeToEdit = tvDestination.SelectedNode.Nodes.Add(newFolderName, newFolderName);
            nodeToEdit.Tag = newFolderName;
            tvDestination.SelectedNode = nodeToEdit;
            tvDestination.LabelEdit = true;
            tvDestination.SelectedNode.BeginEdit();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _isWiXComponentGroup.DeleteDirectory(tvDestination.SelectedNode.FullPath, tvDestination.SelectedNode.Parent.FullPath);
            tvDestination.SelectedNode.Remove();
        }

        private void lvDestination_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tvDestination_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {

                if (tvDestination.SelectedNode.Parent == null)
                {
                    var message = string.Format("Can't delete {0} folder", tvDestination.SelectedNode.Text);
                    CallMessageBox(message, "Delete Folder Warning");
                }
                else
                {
                    var message = string.Format("Do you really want to delete the {0} folder?", tvDestination.SelectedNode.Text);
                    var result = MessageBox.Show(message, "Folder Deletion Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (DialogResult.OK != result)
                    {
                        return;
                    }
                    _isWiXComponentGroup.DeleteDirectory(tvDestination.SelectedNode.FullPath, tvDestination.SelectedNode.Parent.FullPath);
                    tvDestination.SelectedNode.Remove();
                }
               // _documentManager.DisableChangeWatcher();
                _isWiXComponentGroup.SortComponents();
               // _documentManager.EnableChangeWatcher();
            }

            if (e.KeyCode == Keys.Insert)
            {
                // make sure there is a selected directory
                if (tvDestination.SelectedNode != null)
                {
                    //check if selected node is not Destination Computer
                    if (tvDestination.SelectedNode.Parent != null)
                    {
                        // add new directory
                        string newFolderName = _isWiXComponentGroup.GetNextSubDirectoryName(tvDestination.SelectedNode.FullPath.Replace(@"Destination Computer\", ""));
                        string directoryPath = Path.Combine(tvDestination.SelectedNode.FullPath.Replace(@"Destination Computer\", ""), newFolderName);
                        _isWiXComponentGroup.GetOrCreateDirectoryComponent(Path.Combine(directoryPath));
                        var nodeToEdit = tvDestination.SelectedNode.Nodes.Add(newFolderName, newFolderName);
                        nodeToEdit.Tag = newFolderName;
                        tvDestination.SelectedNode = nodeToEdit;
                        tvDestination.LabelEdit = true;
                        // tvDestination.SelectedNode.BeginEdit();
                    }
                    else
                    {
                        CallMessageBox("You cannot add a folder at this location", "Insert Warning");
                    }
                }
                else
                {
                    CallMessageBox("Please select a destination directory before choosing to insert a new directory.", "Insert Warning");
                }
            }

            if (e.KeyCode == Keys.F2)
            {
                // make sure there is a selected directory
                if (tvDestination.SelectedNode != null)
                {
                    //check if selected node is not Destination Computer
                    if (tvDestination.SelectedNode.Parent != null && tvDestination.SelectedNode.Parent.Parent != null)
                    {
                        tvDestination.LabelEdit = true;
                        tvDestination.SelectedNode.BeginEdit();
                    }
                    else
                    {
                        CallMessageBox("You cannot rename this folder!", "Rename Warning");
                    }
                }
                else
                {
                    CallMessageBox("Please select a directory before attempting to rename it.", "Rename Warning");
                }
            }
        }

        private void tvDestination_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
           // _documentManager.DisableChangeWatcher();
            if (e.Label == null)
            {
                e.CancelEdit = true;
            }
            else if (!_isWiXComponentGroup.RenameDirectory(tvDestination.SelectedNode.FullPath, e.Label))
            {
                e.CancelEdit = true;
            }
            _isWiXComponentGroup.SortComponents();
            //_documentManager.EnableChangeWatcher();
        }

        private void renameFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.LabelEdit = true;
            tvDestination.SelectedNode.BeginEdit();

        }

        private void lvDestination_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if the column clicked has been clicked before
            if (e.Column == lvDestinationColumnSorter.SortColumn)
            {
                // reverse sort direction
                //lvColumnSorter.Order = (lvColumnSorter.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
                lvDestinationColumnSorter.Order = (lvDestinationColumnSorter.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // set column to be sorted, default order to ascending
                lvDestinationColumnSorter.Order = SortOrder.Ascending;
                lvDestinationColumnSorter.SortColumn = e.Column;
            }
            lvDestination.Sort();
        }

        private void lvSourceFiles_ColumnClick_1(object sender, ColumnClickEventArgs e)
        {
            // Determine if the column clicked has been clicked before
            if (e.Column == lvColumnSorter.SortColumn)
            {
                // reverse sort direction
                lvColumnSorter.Order = (lvColumnSorter.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // set column to be sorted, default order to ascending
                lvColumnSorter.Order = SortOrder.Ascending;
                lvColumnSorter.SortColumn = e.Column;
            }
            lvSourceFiles.Sort();
        }
    }
}