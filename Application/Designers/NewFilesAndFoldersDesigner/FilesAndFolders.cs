﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiXAutomationInterface;
using static FireworksFramework.Types.Enums;

namespace Designers.NewFilesAndFolders
{

    public partial class FilesAndFolders : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        IsWiXComponentGroup _isWiXComponentGroup;

        #region (------ Global Variables and Constants -----)
        // DirectoryObject directoryObject = new DirectoryObject();
        private const string Seperator = ";";
        private const string Wildcard = "*";
        private const string BaseExcludePattern = ""; // "*unittest*;*.pdb" <- We used to default this
        private const string SourceDirVar = "$(var.SourceDir)";
        private string SourceStart;

        private TreeNode HoverNode;
        
        private string DragSourceName;
        protected string fileFilterPattern;
        protected string fileExcludePattern;
        
        private enum PopulateMode
        {
            OneLevel,
            Recursive,
            NoMore
        }

        private enum PatternSearchType
        {
            AtBeginning,
            AtEnd,
            InMiddle,
            Unknown
        }
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

        private const bool WithKeyPath = true;
        private const bool WithoutKeyPath = false;
        #endregion

        #region (------ Public Methods -----)

        #region (------ Constructor -----)
        public FilesAndFolders()
        {
            InitializeComponent();

            // create the column sorter and assign it to the correct control
            lvColumnSorter = new ListViewColumnSorter();
            lvSourceFiles.ListViewItemSorter = lvColumnSorter;

            lvDestinationColumnSorter = new ListViewColumnSorter();
            lvDestination.ListViewItemSorter = lvDestinationColumnSorter;
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
        #endregion

        public void LoadData()
        {

            if (String.IsNullOrEmpty(_documentManager.DocumentPath))
            {
                CallMessageBox("Please save your file before using Files and Folders.", "File Save Warning");
                return;
            }
            _isWiXComponentGroup = new IsWiXComponentGroup();
            SourceStart = _isWiXComponentGroup.GetRootPath();
            tvSourceFiles.Nodes.Clear();  //Clear before loading
            PopulateSource();
        }

        #region (------ Utility Methods ------)
        private static void CallMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion

        #endregion

        #region (------ Public Properties -----)
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
        #endregion

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

        private FileInfo[] GetFilteredFiles( DirectoryInfo directoryInfo, string FileExcludePatter, string FileFilterPattern )
        {
            FileInfo[] files = RemoveExcludedFiles( GetFiles(directoryInfo), FileExcludePatter);

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

        private void tvSourceFiles_MouseUp(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);
                cmsSourceTree.Show(tvSourceFiles, p);
            }
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

        private void lvSourceFiles_MouseUp(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);
                cmsSourceFiles.Show(lvSourceFiles, p);
            }
        }

        private void lvSourceFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.KeyCode == Keys.F5 )
            {
                DisplaySourceListItemsForSelectedNode(tvSourceFiles.SelectedNode);
            }
            else if( e.KeyCode == Keys.A && e.Control )
            {
                foreach (ListViewItem item in lvSourceFiles.Items)
                {
                    item.Selected = true;
                }
            }
            else if( e.KeyCode == Keys.Escape)
            {
                foreach (ListViewItem item in lvSourceFiles.Items)
                {
                    item.Selected = false;
                }
            }
        }

        private void tvSourceFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            PopulateSourceTree( e.Node.FullPath, e.Node, PopulateMode.OneLevel);
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
            if( e.KeyCode == Keys.Enter )
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
        }

        private void rbOneToOne_Click(object sender, EventArgs e)
        {
        }


        

    }
}
