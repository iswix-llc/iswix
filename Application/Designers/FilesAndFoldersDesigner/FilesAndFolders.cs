using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiXAutomationInterface;
using static FireworksFramework.Types.Enums;

namespace Designers.FilesAndFolders
{

    public partial class FilesAndFolders : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        #region (------ Global Variables and Constants -----)
        // DirectoryObject directoryObject = new DirectoryObject();
        private const string Seperator = ";";
        private const string Wildcard = "*";
        private const string BaseExcludePattern = ""; // "*unittest*;*.pdb" <- We used to default this
        private const string DestinationPathPrefix = "Destination Computer\\[MergeRedirectFolder]\\";
        private const string DestinationFolderName = "Destination Computer";
        private const string MergeRedirectFolderName = "[MergeRedirectFolder]";
        private const string MrfHashStringPrefix = "MergeRedirectFolder\\";
        private const string ListViewItemCollectionFormatIdentifier = "System.Windows.Forms.ListView+SelectedListViewItemCollection";
        private const string SourceDirVar = "$(var.SourceDir)";
        private string SourceStart;
        private const bool DoNotProcessPath = false;
        private const bool ProcessPath = true;
        private bool Merge64;
        private bool SkipSort;
        XNamespace ns;

        //private TreeNode OldSelectNode;
        private string OldSelectedNodeText;
        private string OldSelectedNodeTagText;

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
            ns = _documentManager.Document.GetWiXNameSpace();

            if (_documentManager.Document.GetWiXVersion() == WiXVersion.v3)
            {
                Merge64 = Is64BitMergeModule();
            }
            else
            {
                // No longer needed in WiX v4
                Merge64 = false;
            }

            if (String.IsNullOrEmpty(_documentManager.DocumentPath))
            {
                CallMessageBox("Please save your file before using Files and Folders.", "File Save Warning");
                return;
            }

            if (!FilePassesOurTest())
            {
                _documentManager.Document.Descendants(ns + "Wix").First().AddFirst(new XProcessingInstruction("define", "SourceDir=\".\""));
            }

            IsWixUpgradeFixer.Fix();
            tvSourceFiles.Nodes.Clear();  //Clear before loading

            PopulateSource();
            LoadDocument();
        }

        private void LoadDocument()
        {
            Cursor = Cursors.WaitCursor;
            //SortData();
            if (tvDestination.SelectedNode != null)
            {
                OldSelectedNodeText = tvDestination.SelectedNode.Text;
                OldSelectedNodeTagText = tvDestination.SelectedNode.Tag.ToString();
            }
            tvDestination.Nodes.Clear();
            //Ensure the XML file has any Directories.
            try
            {
                var firstDirectory = _documentManager.Document.Descendants(ns + "Directory").First();
                AddDirectoryNodesToDestination(firstDirectory, null);

                if (!string.IsNullOrEmpty(OldSelectedNodeText) && !string.IsNullOrEmpty(OldSelectedNodeTagText))
                {
                    TreeNode foundNode = FindNodeRecursiveByText(tvDestination.Nodes[0], OldSelectedNodeText, OldSelectedNodeTagText);
                    if (foundNode != null)
                    {
                        tvDestination.SelectedNode = foundNode;
                        tvDestination.SelectedNode.Expand();
                    }
                }
                else
                {
                    tvDestination.Nodes[0].ExpandAll();
                }
                
                if (tvDestination.Nodes.Count > 0 && tvDestination.Nodes[0].Nodes.Count > 0 && String.IsNullOrEmpty(OldSelectedNodeText))
                {
                    // Select Merge Redirect Folder by Default
                    tvDestination.SelectedNode = tvDestination.Nodes[0].Nodes[0];
                    DisplayDestinationItemsFromSelectedNode(tvDestination.SelectedNode);
                }
                
            }
            catch (Exception exception)
            {
                throw exception;
                //CallMessageBox(exception.Message, "Load Document Exception");
            }
            Cursor = Cursors.Default;
        }

        private bool FilePassesOurTest()
        {
            var rootPath = string.Empty;
            var sourceDirXpiExists = false;
            var componentRulesExists = false;
            var sourceDirExistsOnFileSystem = false;

            rbOneToMany.Checked = true;
            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("ComponentRules"))
                    {
                        componentRulesExists = true;
                        if (fields[1].Replace("\"", "").Trim().ToLower().Equals("onetoone"))
                        {
                            rbOneToOne.Checked = true;
                        }
                    }
                }
                if (componentRulesExists)
                {
                    goto FirstTest;
                }
            }

            FirstTest:
            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("SourceDir"))
                    {
                        sourceDirXpiExists = true;
                        rootPath = fields[1].Replace("\"", "").Trim();
                    }
                }
                if (sourceDirXpiExists)
                {
                    goto SecondTest;
                }
            }
            // set label here for breaking out of first for loop
            SecondTest:
            // figure out if sourceDirValue exists or can be created
            try
            {
                if (!rootPath.Contains(":"))
                {
                    var documentInfo = new FileInfo(_documentManager.DocumentPath);
                    rootPath = Path.Combine(documentInfo.DirectoryName, rootPath);
                }

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }
                sourceDirExistsOnFileSystem = true;
                SourceStart = new DirectoryInfo(rootPath).FullName;
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format("An error occurred trying to use '{0}' as the SourceDir.  Defaulting to document directory.", rootPath));
            }
            return sourceDirXpiExists &&  sourceDirExistsOnFileSystem;
        }

        public void SortData()
        {
            if (!SkipSort)
            {
                var tempDocument = XDocument.Parse(_documentManager.Document.ToString());
                var tempStartElement = FindDirectoryElement(tempDocument, "SourceDir", "TARGETDIR");
                var tempDirectoryList = (from item in tempStartElement.Elements(ns + "Directory")
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

            if (HasComponents(originalStartingDirectory))
            {
                // sort the components containing program executable files by selecting them from
                // the temp document, removing the items from the original then inserting the sorted
                // items into the original
                var sortedPEComponents =
                    (from a in tempStartingDirectory.Elements(ns + "Component")
                     where a.Element(ns + "File").Attribute("KeyPath") != null
                           && a.Element(ns + "File").Attribute("KeyPath").Value == "yes"
                     orderby a.Element(ns + "File").Attribute("Source").Value, a.Attribute("Id").Value ascending
                     select a);

                var currentPEComponents =
                    (from a in originalStartingDirectory.Elements(ns + "Component")
                     where a.Element(ns + "File").Attribute("KeyPath") != null
                           && a.Element(ns + "File").Attribute("KeyPath").Value == "yes"
                     select a);

                currentPEComponents.Remove();
                originalStartingDirectory.Add(sortedPEComponents);

                try
                {
                    var originalNonPEComponentList = (from item in originalStartingDirectory.Elements(ns + "Component")
                                              where item.Element(ns + "File").Attribute("KeyPath") == null
                                              select item);

                    var tempNonPEComponentList = (from item in tempStartingDirectory.Elements(ns + "Component")
                                         where item.Element(ns + "File").Attribute("KeyPath") == null
                                         select item);

                    // tempNonPEComponent.Elements().Remove();

                    if (originalNonPEComponentList.Count() > 0)
                    {
                        var originalNonPEComponent = originalNonPEComponentList.First();
                        var tempNonPEComponent = tempNonPEComponentList.First();

                        // sort files in the temporary non PE component
                        var sortedFiles = from f in tempNonPEComponent.Elements(ns + "File")
                                          orderby f.Attribute("Source").Value ascending
                                          select f;

                        var originalFiles = from f in originalNonPEComponent.Elements(ns + "File")
                                          select f;
                        
                        originalFiles.Remove();
                        originalNonPEComponent.Add(sortedFiles);
                        // need to remove component and place after PE components to retain proper sequencing
                        var siblings = originalNonPEComponent.ElementsAfterSelf();
                        if (siblings.Any())
                        {
                            siblings.Last().AddAfterSelf(originalNonPEComponent);
                            originalNonPEComponent.Remove();
                        }
                    }

                }
                catch (InvalidOperationException)
                {
                    // do nothing as the original non PE component either doesn't exist or has no file
                    // but it remains unchanged
                }
                catch (NullReferenceException)
                {
                    // do nothing as the original non PE component either doesn't exist or has no file
                    // but it remains unchanged
                }
            }

            if (HasSubDirectories(originalStartingDirectory))
            {
                IEnumerable<XElement> originalSubDirectories;
                IOrderedEnumerable<XElement> sortedSubDirectories;

                // sorts the directories 
                if (originalStartingDirectory.Attribute("Id") != null && originalStartingDirectory.Attribute("Id").Value== "TARGETDIR")
                {
                    originalSubDirectories = (from dir in originalStartingDirectory.Elements(ns + "Directory")
                                              select dir);

                    sortedSubDirectories = (from dir in tempStartingDirectory.Elements(ns + "Directory")
                                              orderby dir.Attribute("Id").Value ascending
                                              select dir);
                }
                else
                {
                    originalSubDirectories = (from dir in originalStartingDirectory.Elements(ns + "Directory")
                                              where dir.Attribute("Name") != null
                                              select dir);
                    
                    sortedSubDirectories = (from dir in tempStartingDirectory.Elements(ns + "Directory")
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
                            
                            if (DirectoryIsNotEmpty(subElement))
                            {
                                if (IsASpecialDirectoryToIgnore(subElement))
                                {
                                    // ignore
                                }
                                else
                                {
                                    //Add Directory to treeNode
                                    var newNode = treeNode.Nodes.Add(CreateNodeName(subElement));
                                    var newNodeTag = GetElementId(subElement);
                                    newNode.Tag = newNodeTag;

                                    if (IdIsWithinSystemFolderPropertyEnum(newNodeTag))
                                    {
                                        newNode.ImageIndex = (int)ImageLibrary.BlueFolderClosed;
                                        newNode.SelectedImageIndex = (int)ImageLibrary.BlueFolderOpen;
                                    }

                                    AddDirectoryNodesToDestination(subElement, newNode);
                                }
                            }
                            else // Directory Is Empty
                            {
                                if (IsASpecialDirectoryToIgnore(subElement))
                                {
                                    // ignore
                                }
                                else
                                {
                                    var newNodeTag = GetElementId(subElement);
                                    if (IdIsWithinSystemFolderPropertyEnum(newNodeTag))
                                    {
                                        var newNode = treeNode.Nodes.Add(CreateNodeName(subElement));
                                        newNode.Tag = newNodeTag;
                                        newNode.ImageIndex = (int)ImageLibrary.BlueFolderClosed;
                                        newNode.SelectedImageIndex = (int)ImageLibrary.BlueFolderOpen;
                                    }
                                }
                            }
                            break;
                        default:
                            //pass over the element
                            break;
                    }
                }
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
                    case SystemFolderProperty.TempFolder:
                    case SystemFolderProperty.TemplateFolder:
                    case SystemFolderProperty.WindowsFolder:
                    case SystemFolderProperty.WindowsVolume:
                        return true;
                    case SystemFolderProperty.ProgramFilesFolder:
                    case SystemFolderProperty.CommonAppDataFolder:
                    case SystemFolderProperty.CommonFilesFolder:
                    case SystemFolderProperty.MergeRedirectFolder:
                    case SystemFolderProperty.GlobalAssemblyCache:
                    case SystemFolderProperty.SystemFolder:
                    case SystemFolderProperty.System64Folder:
                        return false;
                    default:
                        return false;
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

        private bool DirectoryIsNotEmpty(XElement directoryElement)
        {
            if (directoryElement.HasElements)
            {
                // check for type of elements
                return DirectoryElementContainsProperSubElements(directoryElement);
            }
            else
            {
                return false;
            }
        }

        private bool DirectoryElementContainsProperSubElements(XElement directoryElement)
        {
            return ( SubElementIsDirectory(directoryElement) || SubElementIsGoodComponent(directoryElement));
        }

        private bool SubElementIsGoodComponent(XElement directoryElement)
        {
            XElement subElement = directoryElement.Element(ns + "Component");
            if (subElement != null)
            {
                // check if file or create folder
                XElement childElementFile = subElement.Element(ns + "File");
                XElement childElementCreateFolder = subElement.Element(ns + "CreateFolder");

                return (childElementFile != null) || (childElementCreateFolder != null);
            }
            else
            {
                return false;
            }
        }

        private bool SubElementIsDirectory(XElement directoryElement)
        {
            XElement subElement = directoryElement.Element(ns + "Directory");
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

        private TreeNode AddDestinationComputerToDestination(XElement element)
        {
            var nodeName = CreateNodeName(element);
            var subTreeNode = tvDestination.Nodes.Add(nodeName);
            subTreeNode.ImageIndex =  (int)ImageLibrary.Computer;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Computer;
            subTreeNode.Tag = GetElementId(element);
            return subTreeNode;
        }

        private string CreateNodeName(XElement element)
        {
            //return String.Format("[{0}] [{1}]", GetElementId(element), GetElementName(element));
            return String.Format("{0}", GetElementName(element));
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

        private void GetDirectories(XElement element, TreeNode treeNode)
        {
            //ToDo: Rememeber to check if the Element xNamespace is not null also

            // IEnumerable<XElement> directories = element.Descendants(xNamespace + "Directory");

            try
            {
                var dir = element.Descendants(ns + "Directory").First();
                var Id = (dir.Attribute("Id") != null) ? dir.Attribute("Id").Value : "foo";
                var dirName = (dir.Attribute("Name") != null) ? dir.Attribute("Name").Value : "bar";
                var subTreeNode = (treeNode == null) ? tvDestination.Nodes.Add(Id + " " + dirName) : treeNode.Nodes.Add(Id + " " + dirName);

                GetDirectories(dir, subTreeNode);
            }
            catch
            {
                return;
            }             
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
            lvDestination.Items.Clear();
            //tvDestination.SelectedNode.Text  To Display the Selected Node in the ListView
            if (treeNode == null) return;

            // find the directory element based on the information from the selected treenode
            List<ListViewItem> listItems = GetItemListFromSelectedDestinationDirectory(treeNode);
            if (listItems == null) return;
            lvDestination.BeginUpdate();
            lvDestination.Items.AddRange(listItems.ToArray());
            lvDestination.EndUpdate();
        }

        private List<ListViewItem> GetItemListFromSelectedDestinationDirectory(TreeNode treeNode)
        {
            imageListFileIconsDst.Images.Clear();
            imageListFileIconsDst.Images.Add("stockdeletedimage", ilImageLibrary.Images["delete_16x16.gif"]);

            var itemList = new List<ListViewItem>();
            try
            {
                var selectedDirectory = FindDirectoryElement(treeNode);
                // find all the file elements for the directory element
                var directoryComponets = selectedDirectory.Elements(ns + "Component");
                // loop through them displaying the source name and display context sensitive icon
                foreach (var component in directoryComponets)
                {
                    var files = component.Elements(ns + "File");
                    foreach (var file in files)
                    {
                        itemList.Add(CreateDestinationItemFromFile(file));
                    }
                }
            }
            catch(Exception)
            {
            }
            return itemList;
        }

        private ListViewItem CreateDestinationItemFromFile(XElement file)
        {
            var source = file.Attribute("Source").Value;
            string temp = source.Replace(SourceDirVar + @"\", "");
            var realSourcePath = Path.Combine(SourceStart, temp );
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
                if (file.Attribute("KeyPath").Value.ToLower().Equals("yes"))
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

        private void lvSourceFiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragSourceName = "lvSourceFiles";
            lvSourceFiles.DoDragDrop(lvSourceFiles.SelectedItems, DragDropEffects.Copy);
        }

        private void lvDestination_DragDrop(object sender, DragEventArgs e)
        {
            _documentManager.DisableChangeWatcher();
            if (e.Data.GetDataPresent(ListViewItemCollectionFormatIdentifier, false))
            {
                var itemCollection = (ListView.SelectedListViewItemCollection)e.Data.GetData(ListViewItemCollectionFormatIdentifier);
                foreach (ListViewItem item in itemCollection)
                {
                    if (CanBeMoved(item, tvDestination.SelectedNode))
                    {
                        AddItemToDocument(tvDestination.SelectedNode, item);
                    }
                    else
                    {
                        CallMessageBox(String.Format("File [{0}] Exists in Destination", item.Text), "Drop Warning");
                    }
                }
                SortData();
                LoadDocument();
                DisplayDestinationItemsFromSelectedNode(tvDestination.SelectedNode);
            }

            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                var sourceFolder = (TreeNode) e.Data.GetData(typeof (TreeNode));
                if (!DestinationContainsFolder(tvDestination.SelectedNode, sourceFolder))
                {
                    AddAllDirectoriesToDestination(tvDestination.SelectedNode, sourceFolder);
                    SortData();
                    LoadDocument();
                }
                else
                {
                    CallMessageBox(String.Format("Directory [{0}] already exists in destination location.", sourceFolder.Text), "Copy Directory Warning");
                }
            }
            _documentManager.EnableChangeWatcher();

        }

        private void AddAllDirectoriesToDestination(TreeNode destinationNode, TreeNode sourceFolder)
        {
            AddDirectoryToDestination(destinationNode, sourceFolder);
            TreeNode newDestination = tvDestination.SelectedNode;
            foreach (TreeNode childFolder in sourceFolder.Nodes)
            {
                AddAllDirectoriesToDestination(newDestination, childFolder);
            }
        }

        private void AddDirectoryToDestination(TreeNode destinationFolder, TreeNode sourceFolder)
        {
            if (!DestinationContainsFolder(destinationFolder, sourceFolder))
            {
                // create new folder in destination
                var directory = CreateNewDirectory(destinationFolder, sourceFolder);
                var parentDirectory = FindDirectoryElement(destinationFolder);
                var containsCreateFolderElement = ContainsCreateFolder(parentDirectory);
                if (containsCreateFolderElement)
                {
                    RemoveCreateFolderComponentFromDirectory(parentDirectory);
                }
                parentDirectory.Add(directory);

                // need to reload data so that the newly added directory can be found in the tree view
                SortData();
                LoadDocument();

                // the following code choses the newly added child folder as the place in which
                // to add files.
                TreeNode foundNode = FindNodeRecursiveByText(tvDestination.Nodes[0], sourceFolder.Text, GetElementId(directory));
                if (foundNode != null)
                {
                    tvDestination.SelectedNode = foundNode; 
                    foundNode.EnsureVisible();
                }

                // find the items in the source folder and
                List<ListViewItem> itemCollection;
                try
                {
                    itemCollection = GetItemListFromSelectedDirectory(sourceFolder.FullPath);
                }
                catch (InvalidOperationException)
                {
                    itemCollection = null;
                }
                if (itemCollection != null && itemCollection.Count != 0)
                {
                    // add item to the new destination folder
                    foreach (ListViewItem item in itemCollection)
                    {
                        // need to get treenode representation of directory
                        if (CanBeMoved(item, tvDestination.SelectedNode))
                        {
                            AddItemToDocument(tvDestination.SelectedNode, item);
                        }
                        else
                        {
                            CallMessageBox(String.Format("File [{0}] Exists in Destination", item.Text),
                                           "Drop Warning");
                        }
                    }
                }
                DisplayDestinationItemsFromSelectedNode(tvDestination.SelectedNode); // Refreshes destination
            }
            else
            {
                CallMessageBox(String.Format("Directory [{0}] already exists in destination location.", sourceFolder.Text), "Copy Directory Warning");
                sourceFolder.Remove();
            }
        }
        
        private TreeNode FindNodeRecursiveByText(TreeNode node, string text, string tag)
        {
            if ( (node.Text == text) && node.Tag.ToString()==tag)
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

        private XElement CreateNewDirectory(TreeNode treeNode, TreeNode sourceFolder)
        {
            var nodeInTree = FindNodeRecursiveByText(tvDestination.Nodes[0], treeNode.Text, treeNode.Tag.ToString());
            var path = nodeInTree.FullPath;
            var newDirectory = sourceFolder.Text;
            var input = CreateDirectoryStringForHash(path, newDirectory);

            var directory = new XElement(ns + "Directory", 
                                    new XAttribute("Id", "owd" + GetMd5Hash(input)),
                                    new XAttribute("Name", sourceFolder.Text));
            
            var createFolderComponent = CreateNewComponentWithCreateFolder(input, DoNotProcessPath);
            
            directory.Add(createFolderComponent);

            return directory;
        }

        private bool DestinationContainsFolder(TreeNode destinationFolder, TreeNode sourceFolder)
        {
            var directory = FindDirectoryElement(destinationFolder);
            var subDirectories = directory.Elements(ns + "Directory");

            foreach (var subDirectory in subDirectories)
            {
                if (subDirectory.Attribute("Name") != null)
                {
                    if (subDirectory.Attribute("Name").Value.ToLower().Equals(sourceFolder.Text.ToLower())) return true;
                }
            }
            return false;
        }

        private void AddItemToDocument(TreeNode treeNode, ListViewItem item)
        {
            var parentDirectory = FindDirectoryElement(treeNode);
            var containsCreateFolderElement = ContainsCreateFolder(parentDirectory);

            if (IsProgramExecutable(item.Text))
            {
                AddProgramExecutableFileToDirectory(treeNode, item, parentDirectory, containsCreateFolderElement);
            }
            else
            {
                AddNonProgramExecutableFileToDirectory(treeNode, item, parentDirectory, containsCreateFolderElement);
            }
        }

        private void AddNonProgramExecutableFileToDirectory(TreeNode treeNode, ListViewItem item, XElement directory, bool containsCreateFolderElement)
        {
            // the item being added doesn not represent a program executable 
            // see if there is an existing component with a create folder
            if (containsCreateFolderElement)
            {
                RemoveCreateFolderComponentFromDirectory(directory);
                AddNewComponetToDirectory(treeNode, item, directory, WithoutKeyPath);
            }
            else
            {
                AddNonProgramExecutableFileToCorrectComponent(treeNode, item, directory);
            }
        }

        private void AddNonProgramExecutableFileToCorrectComponent(TreeNode treeNode, ListViewItem item, XElement directory)
        {
            var directoryComponents = directory.Elements(ns + "Component");

            var directoryComponentCount = directoryComponents.Count();
            if (directoryComponentCount == 0)
            {
                // create a new component without KeyPath and add it
                AddNewComponetToDirectory(treeNode, item, directory, WithoutKeyPath);
            }
            if (directoryComponentCount == 1)
            {
                AddNonProgramExecutableFileToNewOrExistingComponent(treeNode, item, directory, directoryComponents);
            }
            if (directoryComponentCount > 1)
            {
                AddNonProgramExecutableFileToFilesComponent(treeNode, item, directory, directoryComponents);
            }
        }

        private void AddNonProgramExecutableFileToNewOrExistingComponent(TreeNode treeNode, ListViewItem item, XElement directory, IEnumerable<XElement> componentElements)
        {
            // find number files 
            var componentFiles = componentElements.First().Elements(ns + "File");
            var componentFileCount = componentFiles.Count();
            if (componentFileCount == 0) return; // no file elements, user created bad xml
            if (componentFileCount == 1)
            {
                // check file contents
                var existingFile = componentElements.First().Elements(ns + "File").First();
                if (existingFile.Attribute("Source") != null)
                {
                    if (IsProgramExecutable(existingFile.Attribute("Source").Value))
                    {
                        AddNewComponetToDirectory(treeNode, item, directory, WithoutKeyPath);
                    }
                    else
                    {
                        AddNewFileToComponent(treeNode, item, componentElements.First());
                    }
                }
            }
            else
            {
                AddNewFileToComponent(treeNode, item, componentElements.First());
            }
        }

        private void AddNonProgramExecutableFileToFilesComponent(TreeNode treeNode, ListViewItem item, XElement directory, IEnumerable<XElement> componentElements)
        {
            var foundNonProgramExecuteableFileBlock = false;
            foreach (var component in componentElements)
            {
                /* This is the old logic that found the component based on file extensions.
                var componentFiles = component.Elements(ns + "File");
                if (componentFiles.First().Attribute("Source") != null)
                {
                    if (!IsProgramExecutable(componentFiles.First().Attribute("Source").Value))
                    {
                        AddNewFileToComponent(treeNode, item, component);
                        foundNonProgramExecuteableFileBlock = true;
                    }
                }
                */

                //  This is the new logic that finds the component based on the KeyPath attribute.  
                //  We need to clean this up to not throw and swallow exceptions.
                try
                {
                    if (component.Attribute("KeyPath").Value.ToLower().Equals("yes"))
                    {
                        AddNewFileToComponent(treeNode, item, component);
                        foundNonProgramExecuteableFileBlock = true;
                    }
                }
                catch( Exception)
                {
                }

            }
            if (!foundNonProgramExecuteableFileBlock)
            {
                AddNewComponetToDirectory(treeNode, item, directory, WithoutKeyPath);
            }
        }

        private void AddProgramExecutableFileToDirectory(TreeNode treeNode, ListViewItem item, XElement directory, bool containsCreateFolderElement)
        {
            if (containsCreateFolderElement)
            {
                RemoveCreateFolderComponentFromDirectory(directory);
            }
            AddNewComponetToDirectory(treeNode, item, directory, WithKeyPath);
        }

        private void RemoveCreateFolderComponentFromDirectory(XElement directory)
        {
            var oldComponent = directory.Element(ns + "Component");
            if (oldComponent != null) oldComponent.Remove();
        }

        private void AddNewFileToComponent(TreeNode treeNode, ListViewItem item, XElement component)
        {
            var newFile = CreateNewFileElement(treeNode, item);
            component.Add(newFile);
        }

        private void AddNewComponetToDirectory(TreeNode treeNode, ListViewItem item, XElement directory, bool withKeyPath)
        {
            var newComponent = CreateNewComponentWithFile(treeNode, item, withKeyPath);
            directory.Add(newComponent);
        }

        private bool ContainsCreateFolder(XElement directory)
        {
            XElement createFolder = null;
            if (directory.Element(ns + "Component") != null)
            {
                createFolder = (from item in directory.Elements(ns + "Component")
                                    select item).First().Element(ns + "CreateFolder");
            }

            return createFolder != null;
        }

        private bool IsProgramExecutable(string fileName)
        {

            return
                fileName.ToLower().EndsWith(".dll") ||
                fileName.ToLower().EndsWith(".exe") ||
                fileName.ToLower().EndsWith(".ocx") ||
                fileName.ToLower().EndsWith(".chm") ||
                fileName.ToLower().EndsWith(".hlp") ||
                rbOneToOne.Checked;
                
        }

        private XElement CreateNewComponentWithFile(TreeNode treeNode, ListViewItem item, bool keyPath)
        {
            var sourcePath = CreateSourcePath(item);

            string dirHashResult;
            Guid componentGuid;
            var sourceForHash = CreateFilePathForHash(treeNode.FullPath, item.Text);
            var hashResult = GetMd5Hash(sourceForHash);
            if (keyPath)
            {
                dirHashResult = hashResult;
                // deterministically unique guid for program executable files
                componentGuid = CreateGuidForComponent(sourceForHash);
            }
            else
            {
                var sourceForDirectoryHash = CreateDirectoryStringForHash(treeNode.FullPath);
                dirHashResult = GetMd5Hash(sourceForDirectoryHash);
                // arbitrarily unique guid for non program executable files
                componentGuid = Guid.NewGuid();
            }

            var component = new XElement(ns + "Component", new XAttribute("Id", "owc" + dirHashResult),
                                         new XAttribute("Guid", componentGuid));

            if (Merge64)
            {
                component.Add(new XAttribute("Win64", "yes"));
            }


            var file = new XElement(ns + "File", new XAttribute("Id", "owf" + hashResult),
                                    new XAttribute("Source", sourcePath));

            if (keyPath)
            {
               file.Add(new XAttribute("KeyPath", "yes"));
            }
            else
            {
                component.Add(new XAttribute("KeyPath", "yes"));
            }
            component.Add(file);
            return component;
        }

        private Guid CreateGuidForComponent(string input)
        {
            var cryptoServiceProvider = new MD5CryptoServiceProvider();
            var inputBytes = Encoding.Default.GetBytes(input);
            var hashBytes = cryptoServiceProvider.ComputeHash(inputBytes);
            var componentGuid = new Guid(hashBytes);
            return componentGuid;
        }

        private XElement CreateNewComponentWithCreateFolder(string path, bool processPath)
        {
            var newPath = (processPath) ? CreateDirectoryStringForHash(path): path;
            var component = new XElement(ns + "Component", new XAttribute("Id", "owc" + GetMd5Hash(newPath)),
                                         new XAttribute("Guid", Guid.NewGuid()));

            if(Merge64)
            {
                component.Add(new XAttribute("Win64", "yes"));
            }

            var createFolder = new XElement(ns + "CreateFolder");

            component.Add(createFolder);
            return component;
        }

        private XElement CreateNewFileElement(TreeNode treeNode, ListViewItem item)
        {
            var sourcePath = CreateSourcePath(item);
            // var source = CreateSourcePath(treeNode.FullPath, item.Text);
            var filePathForHash = CreateFilePathForHash(treeNode.FullPath, item.Text);

            var file = new XElement(ns + "File", new XAttribute("Id", "owf" + GetMd5Hash(filePathForHash)),
                                    new XAttribute("Source", sourcePath));
            return file;
        }

        private string CreateSourcePath(ListViewItem item)
        {
            var newFileSourcePath = String.Empty;
            if (DragSourceName == "lvDestination")
            {
                newFileSourcePath = item.SubItems[2].Text;
            }
            if ((DragSourceName == "lvSourceFiles") || (DragSourceName == "tvSourceFiles"))
            {
                //newFileSourcePath = item.SubItems[4].Text.Replace(SourceStart + "\\", SourcePathPrefix);
                newFileSourcePath = item.SubItems[4].Text;
            }
            var replaceString = (SourceStart.EndsWith("\\")) ? SourceStart.TrimEnd("\\".ToCharArray()): SourceStart;
            newFileSourcePath = newFileSourcePath.Replace(replaceString, SourceDirVar);
            return newFileSourcePath;
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
                    case "[MergeRedirectFolder]":

                        selectedDirectory = (from item in document.Descendants(ns + "Directory")
                                             where
                                                 item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == "MergeRedirectFolder"
                                             select item).First(); 
                                             break;

                    case "[CommonFilesFolder]":
                                             selectedDirectory = (from item in document.Descendants(ns + "Directory")
                                                                  where
                                                                      item.Attribute("Id") != null &&
                                                                      item.Attribute("Id").Value == "CommonFilesFolder"
                                                                  select item).First();
                                             break;
                    case "[CommonAppDataFolder]":
                                             selectedDirectory = (from item in document.Descendants(ns + "Directory")
                                                                  where
                                                                      item.Attribute("Id") != null &&
                                                                      item.Attribute("Id").Value == "CommonAppDataFolder"
                                                                  select item).First();
                                             break;
                    case "[ProgramFilesFolder]":
                        selectedDirectory = (from item in document.Descendants(ns + "Directory")
                                             where
                                                 item.Attribute("Id") != null &&
                                                 item.Attribute("Id").Value == "ProgramFilesFolder"
                                             select item).First();
                        break;
                    case "[SystemFolder]":
                                             selectedDirectory = (from item in document.Descendants(ns + "Directory")
                                                                  where
                                                                      item.Attribute("Id") != null &&
                                                                      item.Attribute("Id").Value == "SystemFolder"
                                                                  select item).First();
                                             break;
                    case "[System64Folder]":
                                             selectedDirectory = (from item in document.Descendants(ns + "Directory")
                                                                  where
                                                                      item.Attribute("Id") != null &&
                                                                      item.Attribute("Id").Value == "System64Folder"
                                                                  select item).First();
                                             break;
                    default:
                                             selectedDirectory = (from item in document.Descendants(ns + "Directory")
                                                                  where
                                                                      item.Attribute("Name") != null &&
                                                                      item.Attribute("Name").Value == text
                                                                      && item.Attribute("Id") != null &&
                                                                      item.Attribute("Id").Value == tag
                                                                  select item).First();
                        break;

                }
            }
            catch(Exception)
            {
                selectedDirectory = null;
            }

            return selectedDirectory;
        }

        private XElement FindFileElement(TreeNode treeNode)
        {
            var selectedFile = (from item in _documentManager.Document.Descendants(ns + "Directory")
                                     where
                                         item.Attribute("Name") != null &&
                                         item.Attribute("Name").Value == treeNode.Text
                                         && item.Attribute("Id") != null &&
                                         item.Attribute("Id").Value == treeNode.Tag.ToString()
                                     select item).First();

            return selectedFile;
        }

        private void lvDestination_DragEnter(object sender, DragEventArgs e)
        {
            if  ( (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection))) || (e.Data.GetDataPresent(typeof(TreeNode))))
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

        private void lvDestination_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragSourceName = "lvDestination";
            lvDestination.DoDragDrop(lvDestination.SelectedItems, DragDropEffects.Move);
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
            if ( e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void tvDestination_DragDrop(object sender, DragEventArgs e)
        {
            _documentManager.DisableChangeWatcher();
            if (e.Data.GetDataPresent(ListViewItemCollectionFormatIdentifier, false))
            {
                var point = ((TreeView) sender).PointToClient(new Point(e.X, e.Y));
                TreeNode dropNode = ((TreeView) sender).GetNodeAt(point);
                if (dropNode != null)
                {
                    var itemCollection = (ListView.SelectedListViewItemCollection)e.Data.GetData(ListViewItemCollectionFormatIdentifier);
                    foreach (ListViewItem item in itemCollection)
                    {
                        if (CanBeMoved(item, dropNode))
                        {
                            AddItemToDocument(dropNode, item);
                            if (e.Effect == DragDropEffects.Move)
                            {
                                RemoveItemFromDocument(tvDestination.SelectedNode, item);
                            }
                        }
                        else
                        {
                            CallMessageBox(String.Format("File [{0}] Exists in Destination", item.Text), "Drop Warning");
                        }
                    }
                    SortData();
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
                if (dropNode != null)
                {
                    var sourceFolder = (TreeNode)e.Data.GetData(typeof(TreeNode));
                    if (sourceFolder == tvSourceFiles.Nodes[0])
                    {
                        CallMessageBox("Copying the root node of the source tree is not allowed. Please choose a different folder to add to the project.", "Copy Directory Warning");
                    }
                    else
                    {
                        if (!DestinationContainsFolder(dropNode, sourceFolder))
                        {
                            // Refresh the directory structure incase of delayed load or changes since loading
                            sourceFolder.Nodes.Clear();
                            PopulateSourceTree(sourceFolder.FullPath, sourceFolder, PopulateMode.Recursive);
                            AddAllDirectoriesToDestination(dropNode, sourceFolder);
                            SortData();
                            LoadDocument();
                        }
                        else
                        {
                            CallMessageBox(String.Format("Directory [{0}] already exists in destination location.", sourceFolder.Text), "Copy Directory Warning");
                        }
                    }
                }
                else
                {
                    CallMessageBox("Drop the items onto a folder", "Drop Warning");
                }
            }
            HoverNode.BackColor = Color.White;
            HoverNode.ForeColor = Color.Black;
            _documentManager.EnableChangeWatcher();
        }

        private void RemoveItemFromDocument(TreeNode treeNode, ListViewItem item)
        {
            var directory = FindDirectoryElement(treeNode);
            var directoryComponets = directory.Elements(ns + "Component");

            var componentCount = directoryComponets.Count();
            foreach (var component in directoryComponets)
            {
                var componentIsCandidateForDelete = false;
                var files = component.Elements(ns + "File");
                var fileCount = files.Count();
                foreach (var file in files)
                {
                    if (file.Attribute("Source") == null) continue;
                    if (!file.Attribute("Source").Value.EndsWith(item.Text)) continue;
                    file.Remove();
                    componentIsCandidateForDelete = true;
                }
                if (fileCount > 1) continue;
                //check to see if the component needs to be removed or filled
                if ( (componentCount > 1) && componentIsCandidateForDelete)
                {
                    // remove component
                    component.Remove();
                }
                
                if ( (componentCount == 1) && componentIsCandidateForDelete)
                {
                    if (HasSubDirectories(directory))
                    {
                        component.Remove();
                    }
                    else
                    {
                        // fill component with <CreateFolder />
                        var createFolder = new XElement(ns + "CreateFolder");
                        component.Add(createFolder);
                    }
                }
            }
        }

        private bool HasSubDirectories(XElement directory)
        {
            return directory.Elements(ns + "Directory").Count() > 0;
        }

        private bool HasComponents(XElement directory)
        {
            return directory.Elements(ns + "Component").Elements(ns + "File").Count() > 0;
        }

        private bool CanBeMoved(ListViewItem item, TreeNode dropNode)
        {
            if (IsInDirectory(item, dropNode))
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }

        private bool IsInDirectory(ListViewItem item, TreeNode dropNode)
        {
            var directory = FindDirectoryElement(dropNode);
            var directoryComponets = directory.Elements(ns + "Component");

            foreach (var component in directoryComponets)
            {
                var files = component.Elements(ns + "File");
                foreach (var file in files)
                {
                    var newFileSourcePath = CreateSourcePath(item);
                    // var newFileSourcePath = CreateSourcePath(dropNode.FullPath, fileName);
                    if (file.Attribute("Source") != null)
                    {
                        var filepath = file.Attribute("Source").Value;
                        var filename = filepath.Substring(filepath.LastIndexOf("\\") + 1).ToUpper();

                        var newFilename = newFileSourcePath.Substring(newFileSourcePath.LastIndexOf("\\") + 1).ToUpper();

                        if (filename == newFilename) return true;
                    }
                }
            }
            return false;
        }

        #region (------ Utility Methods ------)
        private static void CallMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion

        private void tvSourceFiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragSourceName = "tvSourceFiles";
            SkipSort = true;
            tvSourceFiles.DoDragDrop(e.Item, DragDropEffects.Copy);
            SkipSort = false;
            SortData();
            LoadDocument();
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

                    // Find the appropriate ContextMenu depending on the selected node.
                    switch (Convert.ToString(node.Text))
                    {
                        case "[CommonAppDataFolder]":
                        case "[CommonFilesFolder]":
                        case "[GlobalAssemblyCache]":
                        case "[ProgramFilesFolder]":
                        case "[SystemFolder]":
                        case "[System64Folder]":
                        case "[MergeRedirectFolder]":
                            cmsMergeRedirectFolder.Show(tvDestination, p);
                            break;
                        case "Destination Computer":
                            ClearOldDestinationRootMenuItems();
                            AddItemsToDestinationRootMenu();
                            cmsDestinationRoot.Show(tvDestination,p);
                            break;
                        default:
                            cmsDestinationTreeDefault.Show(tvDestination, p);
                            break;
                    }
                }
            }
        }

        private void ClearOldDestinationRootMenuItems()
        {
            var itemCount = cmsDestinationRoot.Items.Count;
            if (itemCount <= 3) return;
            for (var i = (itemCount-1); i >= 3; i-- )
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

        void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem) sender;
            switch (toolStripMenuItem.Name)
            {
                case "CommonAppDataFolder":
                    AddSpecialFolder("CommonAppDataFolder", "CommonAppData");
                    break;
                case "ProgramFilesFolder":
                    AddSpecialFolder("ProgramFilesFolder", "ProgramFilesFolder");
                    break;
                case "SystemFolder":
                    AddSpecialFolder("SystemFolder", "SystemFolder");
                    break;
                case "System64Folder":
                    AddSpecialFolder("System64Folder", "System64Folder");
                    break;
                case "CommonFilesFolder":
                    AddSpecialFolder("CommonFilesFolder");
                    break;
                case "MergeRedirectFolder":
                    AddSpecialFolder("MergeRedirectFolder");
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
            XElement specialDirectory = new XElement(ns + "Directory", new XAttribute("Id", folderId));
            destinationComputer.Add(specialDirectory);

            //reload the document so the new folder can be seen
            SortData();
            LoadDocument();
        }

        private void AddSpecialFolder(string folderId, string folderName)
        {
            //find DestinationRoot
            XElement destinationComputer = FindDirectoryElement("SourceDir", "TARGETDIR");

            //add folder with folderId as the Id attribute value
            XElement specialDirectory = new XElement(ns + "Directory", new XAttribute("Id", folderId), new XAttribute("Name", folderName));
            destinationComputer.Add(specialDirectory);

            //reload the document so the new folder can be seen
            SortData();
            LoadDocument();
        }


        private bool ExistsInTree(string tag)
        {
            foreach (TreeNode node in tvDestination.Nodes[0].Nodes)
            {
                if (node.Tag.ToString() == tag)
                {
                    return true;
                }
            }
            return false;
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
            if (ParentDirectoryNowEmpty(tempNode))
            {
                if (!IsRootDirectory(tempNode))
                {
                    //Add Create Folder Component
                    var newComponent = CreateNewComponentWithCreateFolder(tempNode.FullPath, ProcessPath);
                    var parentDirectory = FindDirectoryElement(tempNode);
                    parentDirectory.Add(newComponent);
                }
            }
            SortData();
            LoadDocument();
            var parent = FindNodeRecursiveByText(tvDestination.Nodes[0], tempNode.Text, tempNode.Tag.ToString());
            if (parent != null)
            {
                tvDestination.SelectedNode = parent;
                tvDestination.SelectedNode.Expand();
                DisplayDestinationItemsFromSelectedNode(parent);
            }
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

        private bool ParentDirectoryNowEmpty(TreeNode directoryNode)
        {
            var directoryElement = FindDirectoryElement(directoryNode);
            return !directoryElement.HasElements;
        }

        private void createNewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNodeToDestinationTree();
        }

        private void AddNodeToDestinationTree()
        {
            // Check for existance of directories that contain the text 'New Folder'
            var directoryElement = FindDirectoryElement(tvDestination.SelectedNode);
            var newFolderList = (from s in directoryElement.Elements(ns + "Directory")
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

        private void expandAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.Collapse();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();
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

            // determine if the node with the previous name exists 
            var directoryElement = FindDirectoryElement(previousName, e.Node.Tag.ToString());
            if (directoryElement != null)
            {
                // find out if a node with the new label exists within document
                string nodePath = tvDestination.SelectedNode.FullPath;
                string newDirectoryPath = CreateDirectoryStringForHash(nodePath);
                var directoryId = "owd" + GetMd5Hash(newDirectoryPath);
                var directoryElementWithSameNameAsLabel = FindDirectoryElement(e.Label, directoryId);
                if (directoryElementWithSameNameAsLabel == null)
                {
                    // rename the node
                    directoryElement.Attribute("Name").Value = e.Node.Text;
                    ReCalculateChildrenHashes(directoryElement, newDirectoryPath);
                    // set the selected node's tag to the new directory id
                    // so that we can find it again after we load the document
                    tvDestination.SelectedNode.Tag = directoryId;
                    SortData();
                    LoadDocument();
                }
                else
                {
                    CallMessageBox(String.Format("A folder with the name [{0}] already exists.", e.Node.Text), "Folder Rename Warning");
                    tvDestination.LabelEdit = true;
                    tvDestination.SelectedNode.BeginEdit();
                }
            }
            else
            {
                // need to take information from node and create directory in XML
                AddDirectoryToDestination(e.Node.Parent, e.Node);
                e.CancelEdit = true;
            }
        }

        private void ReCalculateChildrenHashes(XElement directoryElement, string newDirectoryPath)
        {
            // recalculate hash for directory
            string newIdHash = GetMd5Hash(newDirectoryPath);
            directoryElement.Attribute("Id").Value = "owd" + newIdHash;

            // recalculate hash for components
            var componentList = directoryElement.Elements(ns + "Component");
            foreach (XElement componentElement in componentList)
            {
                var isPEComponent = false;
                var filePath = String.Empty;
                var fileList = componentElement.Elements(ns + "File");
                foreach (XElement fileElement in fileList)
                {
                    // build new hash based on new path and filename
                    var fileName = fileElement.Attribute("Source").Value.Substring(fileElement.Attribute("Source").Value.LastIndexOf("\\") + 1);
                    filePath = CreateFilePathForHash(newDirectoryPath, fileName);
                    var newFileHash = GetMd5Hash(filePath);
                    fileElement.Attribute("Id").Value = "owf" + newFileHash;
                    // check for program executable file
                    if (IsProgramExecutable(fileElement.Attribute("Source").Value))
                    {
                        isPEComponent = true;
                    }
                }
                if (isPEComponent)
                {
                    componentElement.Attribute("Id").Value = "owc" + GetMd5Hash(filePath);
                }
                else
                {
                    componentElement.Attribute("Id").Value = "owc" + GetMd5Hash(newDirectoryPath);
                }
            }

            // repeat the task for each subdirectory of directoryElement
            var directoryList = directoryElement.Elements(ns + "Directory");
            foreach (XElement subDirectory in directoryList)
            {
                var newSubDirectoryPath = String.Format("{0}\\{1}", newDirectoryPath ,subDirectory.Attribute("Name").Value);
                ReCalculateChildrenHashes(subDirectory, newSubDirectoryPath);
            }
        }

        /// <summary>
        /// Hash an input string and return the hash as a 32 character hexadecimal string.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>A 32 character hexadecimal string (UPPERCASE).</returns>
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

        private void lvDestination_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (lvDestination.SelectedItems.Count > 0)
                {
                    var result = MessageBox.Show("Do you really want to delete the selected file(s)?", "File Deletion Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (DialogResult.OK != result) return;
                    foreach (ListViewItem item in lvDestination.SelectedItems)
                    {
                        RemoveItemFromDirectory(tvDestination.SelectedNode, item);
                    }
                    SortData();
                    LoadDocument();
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

        private void RemoveItemFromDirectory(TreeNode directory, ListViewItem file)
        {
            var directoryElement = FindDirectoryElement(directory);
            var componentList = directoryElement.Elements(ns + "Component");
            if (componentList.Count() == 1)
            {
                // check files and possibly remove component
                var fileList = componentList.First().Elements(ns + "File");
                if (fileList.Count() == 1)
                {
                    // remove the component and add component with CreateFolder if it is not a Root Directory
                    componentList.First().Remove();
                    if (HasSubDirectories(directoryElement))
                    {
                        // do nothing more
                    }
                    else
                    {
                        if (!IsRootDirectory(directory))
                        {
                            var newComponent = CreateNewComponentWithCreateFolder(directory.FullPath, ProcessPath);
                            directoryElement.Add(newComponent);
                        }
                    }
                }
                if (fileList.Count() > 1)
                {
                    // remove the file that matches item
                    var fileToBeRemoved = (from fileItem in fileList
                                          where fileItem.Attribute("Source").Value.EndsWith("\\" + file.Text) 
                                          select fileItem).First();
                    fileToBeRemoved.Remove();
                }
            }
            if (componentList.Count() > 1)
            {
                // check file count and if file is PE or non PE
                var foundFile = (
                    from fileItem in componentList.Descendants(ns + "File")
                    where fileItem.Attribute("Source").Value.EndsWith("\\" + file.Text) 
                    //the componentItem has a file that has an attribute that ends with the file.txt;
                    select fileItem).First();
                
                if (foundFile.Parent.Elements(ns + "File").Count() > 1)
                {
                    foundFile.Remove();
                }
                else
                {
                    foundFile.Parent.Remove();
                }
                
            }
        }

        private void tvDestination_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if ((tvDestination.SelectedNode.Text == MergeRedirectFolderName) || (tvDestination.SelectedNode.Text == DestinationFolderName))
                {
                    var message = string.Format("Can't delete {0} folder", tvDestination.SelectedNode.Text);
                    CallMessageBox(message, "Delete Folder Warning");
                }
                else
                {
                    var message = string.Format("Do you really want to delete the {0} folder?", tvDestination.SelectedNode.Text);
                    var result = MessageBox.Show(message, "Folder Deletion Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (DialogResult.OK != result) return;
                    RemoveDirectoryFromDocument(tvDestination.SelectedNode);
                }
            }

            if (e.KeyCode == Keys.Insert)
            {
                // make sure there is a selected directory
                if (tvDestination.SelectedNode != null)
                {
                    //check if selected node is not Destination Computer
                    if (tvDestination.SelectedNode.Tag.ToString() != "TARGETDIR")
                    {
                        // add new directory
                        AddNodeToDestinationTree();
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
                    if (tvDestination.SelectedNode.Tag.ToString() != "TARGETDIR" && !IsRootDirectory(tvDestination.SelectedNode))
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

        private void renameFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvDestination.LabelEdit = true;
            tvDestination.SelectedNode.BeginEdit();
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

        private void removeFileFromProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // call remove function
            if (lvDestination.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show("Do you really want to delete the selected file(s)?", "File Deletion Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (DialogResult.OK != result) return;
                foreach (ListViewItem item in lvDestination.SelectedItems)
                {
                    RemoveItemFromDirectory(tvDestination.SelectedNode, item);
                }
                SortData();
                LoadDocument();
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

        private void expandAllToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.ExpandAll();
        }

        private void collapseAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tvDestination.SelectedNode.Collapse();
        }



        private void tvSourceFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            PopulateSourceTree( e.Node.FullPath, e.Node, PopulateMode.OneLevel);
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {
            if( _documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public System.Drawing.Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("FilesAndFoldersDesigner.FilesAndFolders.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("FilesAndFoldersDesigner.License.txt")).ReadToEnd();
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
            SetComponentRulesXPI();
        }

        private void rbOneToOne_Click(object sender, EventArgs e)
        {
            SetComponentRulesXPI();
        }


        private void SetComponentRulesXPI()
        {
            bool componentRulesExists = false;
            string componentRules = "\"OneToMany\"";

            if (rbOneToOne.Checked)
            {
                componentRules = "\"OneToOne\"";
            }

            foreach (var node in _documentManager.Document.DescendantNodes())
            {
                var xpi = node as XProcessingInstruction;
                if (xpi != null && xpi.Target == "define")
                {
                    var fields = new List<string>(xpi.Data.Split(new char[] { '=' }));

                    if (fields[0].Trim().Equals("ComponentRules"))
                    {
                        componentRulesExists = true;
                        xpi.Data = string.Format("ComponentRules={0}", componentRules);
                    }
                }
            }
            
            if (!componentRulesExists)
            {
                _documentManager.Document.Descendants(ns + "Wix").First().AddFirst(
                    new XProcessingInstruction("define", string.Format("ComponentRules={0}", componentRules)));
            }
        }

        private bool Is64BitMergeModule()
        {
            bool is64Bit = false;
            if(_documentManager.Document.Descendants(ns + "Package").First().GetOptionalAttribute("Platform").Contains("64"))
            {
                is64Bit = true;
            }

            return is64Bit;
        }
    }
}
