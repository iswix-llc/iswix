namespace Designers.FilesAndFolders
{
    partial class FilesAndFolders
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilesAndFolders));
            this.scMain = new System.Windows.Forms.SplitContainer();
            this.gbSourceFiles = new System.Windows.Forms.GroupBox();
            this.scSourceFiles = new System.Windows.Forms.SplitContainer();
            this.tvSourceFiles = new System.Windows.Forms.TreeView();
            this.ilImageLibrary = new System.Windows.Forms.ImageList(this.components);
            this.lvSourceFiles = new System.Windows.Forms.ListView();
            this.chFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFileExtn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chModification = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSourcePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListFileIcons = new System.Windows.Forms.ImageList(this.components);
            this.gbDestination = new System.Windows.Forms.GroupBox();
            this.scDestination = new System.Windows.Forms.SplitContainer();
            this.tvDestination = new System.Windows.Forms.TreeView();
            this.lvDestination = new System.Windows.Forms.ListView();
            this.chDestinationFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chExtension = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFilePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chKeyPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListFileIconsDst = new System.Windows.Forms.ImageList(this.components);
            this.cmsDestinationTreeDefault = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createNewFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMergeRedirectFolder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createNewFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSourceTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshSourceFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsSourceFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDestinationFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeFileFromProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDestinationRoot = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandAllToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.textBoxExcludeFilter = new System.Windows.Forms.TextBox();
            this.labelExcludeFilter = new System.Windows.Forms.Label();
            this.labelIncludeFilter = new System.Windows.Forms.Label();
            this.textBoxIncludeFilter = new System.Windows.Forms.TextBox();
            this.lblComponentRules = new System.Windows.Forms.Label();
            this.rbOneToOne = new System.Windows.Forms.RadioButton();
            this.rbOneToMany = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel1.SuspendLayout();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            this.gbSourceFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scSourceFiles)).BeginInit();
            this.scSourceFiles.Panel1.SuspendLayout();
            this.scSourceFiles.Panel2.SuspendLayout();
            this.scSourceFiles.SuspendLayout();
            this.gbDestination.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scDestination)).BeginInit();
            this.scDestination.Panel1.SuspendLayout();
            this.scDestination.Panel2.SuspendLayout();
            this.scDestination.SuspendLayout();
            this.cmsDestinationTreeDefault.SuspendLayout();
            this.cmsMergeRedirectFolder.SuspendLayout();
            this.cmsSourceTree.SuspendLayout();
            this.cmsSourceFiles.SuspendLayout();
            this.cmsDestinationFiles.SuspendLayout();
            this.cmsDestinationRoot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // scMain
            // 
            this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMain.Location = new System.Drawing.Point(0, 0);
            this.scMain.Name = "scMain";
            this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMain.Panel1
            // 
            this.scMain.Panel1.Controls.Add(this.gbSourceFiles);
            // 
            // scMain.Panel2
            // 
            this.scMain.Panel2.Controls.Add(this.gbDestination);
            this.scMain.Size = new System.Drawing.Size(843, 637);
            this.scMain.SplitterDistance = 306;
            this.scMain.TabIndex = 0;
            // 
            // gbSourceFiles
            // 
            this.gbSourceFiles.Controls.Add(this.scSourceFiles);
            this.gbSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSourceFiles.Location = new System.Drawing.Point(0, 0);
            this.gbSourceFiles.Name = "gbSourceFiles";
            this.gbSourceFiles.Size = new System.Drawing.Size(843, 306);
            this.gbSourceFiles.TabIndex = 0;
            this.gbSourceFiles.TabStop = false;
            this.gbSourceFiles.Text = "Source Files";
            // 
            // scSourceFiles
            // 
            this.scSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scSourceFiles.Location = new System.Drawing.Point(3, 16);
            this.scSourceFiles.Name = "scSourceFiles";
            // 
            // scSourceFiles.Panel1
            // 
            this.scSourceFiles.Panel1.Controls.Add(this.tvSourceFiles);
            // 
            // scSourceFiles.Panel2
            // 
            this.scSourceFiles.Panel2.Controls.Add(this.lvSourceFiles);
            this.scSourceFiles.Size = new System.Drawing.Size(837, 287);
            this.scSourceFiles.SplitterDistance = 222;
            this.scSourceFiles.TabIndex = 0;
            // 
            // tvSourceFiles
            // 
            this.tvSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvSourceFiles.ImageIndex = 1;
            this.tvSourceFiles.ImageList = this.ilImageLibrary;
            this.tvSourceFiles.Location = new System.Drawing.Point(0, 0);
            this.tvSourceFiles.Name = "tvSourceFiles";
            this.tvSourceFiles.SelectedImageIndex = 0;
            this.tvSourceFiles.Size = new System.Drawing.Size(222, 287);
            this.tvSourceFiles.TabIndex = 0;
            this.tvSourceFiles.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvSourceFiles_BeforeExpand);
            this.tvSourceFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvSourceFiles_ItemDrag);
            this.tvSourceFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvSourceFiles_AfterSelect);
            this.tvSourceFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvSourceFiles_KeyDown);
            this.tvSourceFiles.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tvSourceFiles_MouseUp);
            // 
            // ilImageLibrary
            // 
            this.ilImageLibrary.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImageLibrary.ImageStream")));
            this.ilImageLibrary.TransparentColor = System.Drawing.Color.Transparent;
            this.ilImageLibrary.Images.SetKeyName(0, "folder_open_16x16.gif");
            this.ilImageLibrary.Images.SetKeyName(1, "folder_closed_16x16.gif");
            this.ilImageLibrary.Images.SetKeyName(2, "computer1_16x16.gif");
            this.ilImageLibrary.Images.SetKeyName(3, "SmDll.gif");
            this.ilImageLibrary.Images.SetKeyName(4, "file_16x16.gif");
            this.ilImageLibrary.Images.SetKeyName(5, "WEBFILE.ICO");
            this.ilImageLibrary.Images.SetKeyName(6, "mdf_ndf_dbfiles.ico");
            this.ilImageLibrary.Images.SetKeyName(7, "nt_service.ico");
            this.ilImageLibrary.Images.SetKeyName(8, "smXML.gif");
            this.ilImageLibrary.Images.SetKeyName(9, "blue_folder_closed.PNG");
            this.ilImageLibrary.Images.SetKeyName(10, "delete_16x16.gif");
            this.ilImageLibrary.Images.SetKeyName(11, "blue_folder_open.PNG");
            // 
            // lvSourceFiles
            // 
            this.lvSourceFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chFileName,
            this.chFileExtn,
            this.chFileSize,
            this.chModification,
            this.chSourcePath});
            this.lvSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSourceFiles.Location = new System.Drawing.Point(0, 0);
            this.lvSourceFiles.Name = "lvSourceFiles";
            this.lvSourceFiles.Size = new System.Drawing.Size(611, 287);
            this.lvSourceFiles.SmallImageList = this.imageListFileIcons;
            this.lvSourceFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvSourceFiles.TabIndex = 0;
            this.lvSourceFiles.UseCompatibleStateImageBehavior = false;
            this.lvSourceFiles.View = System.Windows.Forms.View.Details;
            this.lvSourceFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvSourceFiles_ColumnClick);
            this.lvSourceFiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.lvSourceFiles_ItemDrag);
            this.lvSourceFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvSourceFiles_KeyDown);
            this.lvSourceFiles.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lvSourceFiles_MouseUp);
            // 
            // chFileName
            // 
            this.chFileName.Text = "File Name";
            this.chFileName.Width = 200;
            // 
            // chFileExtn
            // 
            this.chFileExtn.Text = "Extension";
            this.chFileExtn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.chFileExtn.Width = 70;
            // 
            // chFileSize
            // 
            this.chFileSize.Text = "Size";
            this.chFileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chFileSize.Width = 75;
            // 
            // chModification
            // 
            this.chModification.Text = "Last Modified";
            this.chModification.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chModification.Width = 200;
            // 
            // chSourcePath
            // 
            this.chSourcePath.Text = "Source Path ";
            this.chSourcePath.Width = 500;
            // 
            // imageListFileIcons
            // 
            this.imageListFileIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListFileIcons.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListFileIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // gbDestination
            // 
            this.gbDestination.Controls.Add(this.scDestination);
            this.gbDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDestination.Location = new System.Drawing.Point(0, 0);
            this.gbDestination.Name = "gbDestination";
            this.gbDestination.Size = new System.Drawing.Size(843, 327);
            this.gbDestination.TabIndex = 0;
            this.gbDestination.TabStop = false;
            this.gbDestination.Text = "Destination Files";
            // 
            // scDestination
            // 
            this.scDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scDestination.Location = new System.Drawing.Point(3, 16);
            this.scDestination.Name = "scDestination";
            // 
            // scDestination.Panel1
            // 
            this.scDestination.Panel1.Controls.Add(this.tvDestination);
            // 
            // scDestination.Panel2
            // 
            this.scDestination.Panel2.Controls.Add(this.lvDestination);
            this.scDestination.Size = new System.Drawing.Size(837, 308);
            this.scDestination.SplitterDistance = 223;
            this.scDestination.TabIndex = 0;
            // 
            // tvDestination
            // 
            this.tvDestination.AllowDrop = true;
            this.tvDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDestination.HotTracking = true;
            this.tvDestination.ImageIndex = 1;
            this.tvDestination.ImageList = this.ilImageLibrary;
            this.tvDestination.Indent = 19;
            this.tvDestination.Location = new System.Drawing.Point(0, 0);
            this.tvDestination.Name = "tvDestination";
            this.tvDestination.SelectedImageIndex = 0;
            this.tvDestination.Size = new System.Drawing.Size(223, 308);
            this.tvDestination.TabIndex = 0;
            this.tvDestination.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvDestination_AfterLabelEdit);
            this.tvDestination.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvDestination_AfterSelect);
            this.tvDestination.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvDestination_DragDrop);
            this.tvDestination.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvDestination_DragEnter);
            this.tvDestination.DragOver += new System.Windows.Forms.DragEventHandler(this.tvDestination_DragOver);
            this.tvDestination.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvDestination_KeyDown);
            this.tvDestination.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tvDestination_MouseUp);
            // 
            // lvDestination
            // 
            this.lvDestination.AllowDrop = true;
            this.lvDestination.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chDestinationFileName,
            this.chExtension,
            this.chFilePath,
            this.chKeyPath});
            this.lvDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDestination.Location = new System.Drawing.Point(0, 0);
            this.lvDestination.Name = "lvDestination";
            this.lvDestination.Size = new System.Drawing.Size(610, 308);
            this.lvDestination.SmallImageList = this.imageListFileIconsDst;
            this.lvDestination.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvDestination.TabIndex = 0;
            this.lvDestination.UseCompatibleStateImageBehavior = false;
            this.lvDestination.View = System.Windows.Forms.View.Details;
            this.lvDestination.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvDestination_ColumnClick);
            this.lvDestination.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.lvDestination_ItemDrag);
            this.lvDestination.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvDestination_DragDrop);
            this.lvDestination.DragEnter += new System.Windows.Forms.DragEventHandler(this.lvDestination_DragEnter);
            this.lvDestination.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvDestination_KeyDown);
            this.lvDestination.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lvDestination_MouseUp);
            // 
            // chDestinationFileName
            // 
            this.chDestinationFileName.Text = "File Name";
            this.chDestinationFileName.Width = 187;
            // 
            // chExtension
            // 
            this.chExtension.Text = "Extension";
            this.chExtension.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.chExtension.Width = 146;
            // 
            // chFilePath
            // 
            this.chFilePath.Text = "File Path";
            this.chFilePath.Width = 184;
            // 
            // chKeyPath
            // 
            this.chKeyPath.Text = "Key";
            // 
            // imageListFileIconsDst
            // 
            this.imageListFileIconsDst.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListFileIconsDst.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListFileIconsDst.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // cmsDestinationTreeDefault
            // 
            this.cmsDestinationTreeDefault.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewFolderToolStripMenuItem,
            this.renameFolderToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator1,
            this.expandAllToolStripMenuItem1,
            this.collapseAllToolStripMenuItem});
            this.cmsDestinationTreeDefault.Name = "cmsDestinationTreeDefault";
            this.cmsDestinationTreeDefault.Size = new System.Drawing.Size(213, 120);
            // 
            // createNewFolderToolStripMenuItem
            // 
            this.createNewFolderToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("createNewFolderToolStripMenuItem.Image")));
            this.createNewFolderToolStripMenuItem.Name = "createNewFolderToolStripMenuItem";
            this.createNewFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.createNewFolderToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.createNewFolderToolStripMenuItem.Text = "Create New Folder";
            this.createNewFolderToolStripMenuItem.Click += new System.EventHandler(this.createNewFolderToolStripMenuItem_Click);
            // 
            // renameFolderToolStripMenuItem
            // 
            this.renameFolderToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("renameFolderToolStripMenuItem.Image")));
            this.renameFolderToolStripMenuItem.Name = "renameFolderToolStripMenuItem";
            this.renameFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.renameFolderToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.renameFolderToolStripMenuItem.Text = "Rename Folder";
            this.renameFolderToolStripMenuItem.Click += new System.EventHandler(this.renameFolderToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deleteToolStripMenuItem.Image")));
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.deleteToolStripMenuItem.Text = "Remove From Project";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(209, 6);
            // 
            // expandAllToolStripMenuItem1
            // 
            this.expandAllToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("expandAllToolStripMenuItem1.Image")));
            this.expandAllToolStripMenuItem1.Name = "expandAllToolStripMenuItem1";
            this.expandAllToolStripMenuItem1.Size = new System.Drawing.Size(212, 22);
            this.expandAllToolStripMenuItem1.Text = "Expand All";
            this.expandAllToolStripMenuItem1.Click += new System.EventHandler(this.expandAllToolStripMenuItem1_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("collapseAllToolStripMenuItem.Image")));
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // cmsMergeRedirectFolder
            // 
            this.cmsMergeRedirectFolder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewFolderToolStripMenuItem1,
            this.toolStripSeparator2,
            this.expandAllToolStripMenuItem});
            this.cmsMergeRedirectFolder.Name = "cmsMergeRedirectFolder";
            this.cmsMergeRedirectFolder.Size = new System.Drawing.Size(194, 54);
            // 
            // createNewFolderToolStripMenuItem1
            // 
            this.createNewFolderToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("createNewFolderToolStripMenuItem1.Image")));
            this.createNewFolderToolStripMenuItem1.Name = "createNewFolderToolStripMenuItem1";
            this.createNewFolderToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.createNewFolderToolStripMenuItem1.Size = new System.Drawing.Size(193, 22);
            this.createNewFolderToolStripMenuItem1.Text = "Create New Folder";
            this.createNewFolderToolStripMenuItem1.Click += new System.EventHandler(this.createNewFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("expandAllToolStripMenuItem.Image")));
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.expandAllToolStripMenuItem.Text = "Expand All";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
            // 
            // cmsSourceTree
            // 
            this.cmsSourceTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshSourceFilesToolStripMenuItem});
            this.cmsSourceTree.Name = "cmsSourceTree";
            this.cmsSourceTree.Size = new System.Drawing.Size(133, 26);
            // 
            // refreshSourceFilesToolStripMenuItem
            // 
            this.refreshSourceFilesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("refreshSourceFilesToolStripMenuItem.Image")));
            this.refreshSourceFilesToolStripMenuItem.Name = "refreshSourceFilesToolStripMenuItem";
            this.refreshSourceFilesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshSourceFilesToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.refreshSourceFilesToolStripMenuItem.Text = "Refresh";
            this.refreshSourceFilesToolStripMenuItem.Click += new System.EventHandler(this.refreshSourceFilesToolStripMenuItem_Click);
            // 
            // cmsSourceFiles
            // 
            this.cmsSourceFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem});
            this.cmsSourceFiles.Name = "cmsSourceFiles";
            this.cmsSourceFiles.Size = new System.Drawing.Size(133, 26);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("refreshToolStripMenuItem.Image")));
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // cmsDestinationFiles
            // 
            this.cmsDestinationFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeFileFromProjectToolStripMenuItem});
            this.cmsDestinationFiles.Name = "cmsDestinationFiles";
            this.cmsDestinationFiles.Size = new System.Drawing.Size(213, 26);
            // 
            // removeFileFromProjectToolStripMenuItem
            // 
            this.removeFileFromProjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("removeFileFromProjectToolStripMenuItem.Image")));
            this.removeFileFromProjectToolStripMenuItem.Name = "removeFileFromProjectToolStripMenuItem";
            this.removeFileFromProjectToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.removeFileFromProjectToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.removeFileFromProjectToolStripMenuItem.Text = "Remove From Project";
            this.removeFileFromProjectToolStripMenuItem.Click += new System.EventHandler(this.removeFileFromProjectToolStripMenuItem_Click);
            // 
            // cmsDestinationRoot
            // 
            this.cmsDestinationRoot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllToolStripMenuItem2,
            this.collapseAllToolStripMenuItem1,
            this.toolStripSeparator3});
            this.cmsDestinationRoot.Name = "cmsDestinationRoot";
            this.cmsDestinationRoot.Size = new System.Drawing.Size(137, 54);
            // 
            // expandAllToolStripMenuItem2
            // 
            this.expandAllToolStripMenuItem2.Image = ((System.Drawing.Image)(resources.GetObject("expandAllToolStripMenuItem2.Image")));
            this.expandAllToolStripMenuItem2.Name = "expandAllToolStripMenuItem2";
            this.expandAllToolStripMenuItem2.Size = new System.Drawing.Size(136, 22);
            this.expandAllToolStripMenuItem2.Text = "Expand All";
            this.expandAllToolStripMenuItem2.Click += new System.EventHandler(this.expandAllToolStripMenuItem2_Click);
            // 
            // collapseAllToolStripMenuItem1
            // 
            this.collapseAllToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("collapseAllToolStripMenuItem1.Image")));
            this.collapseAllToolStripMenuItem1.Name = "collapseAllToolStripMenuItem1";
            this.collapseAllToolStripMenuItem1.Size = new System.Drawing.Size(136, 22);
            this.collapseAllToolStripMenuItem1.Text = "Collapse All";
            this.collapseAllToolStripMenuItem1.Click += new System.EventHandler(this.collapseAllToolStripMenuItem1_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(133, 6);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textBoxExcludeFilter);
            this.splitContainer1.Panel1.Controls.Add(this.labelExcludeFilter);
            this.splitContainer1.Panel1.Controls.Add(this.labelIncludeFilter);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxIncludeFilter);
            this.splitContainer1.Panel1.Controls.Add(this.lblComponentRules);
            this.splitContainer1.Panel1.Controls.Add(this.rbOneToOne);
            this.splitContainer1.Panel1.Controls.Add(this.rbOneToMany);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.scMain);
            this.splitContainer1.Size = new System.Drawing.Size(843, 666);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 6;
            // 
            // textBoxExcludeFilter
            // 
            this.textBoxExcludeFilter.Location = new System.Drawing.Point(272, 2);
            this.textBoxExcludeFilter.Name = "textBoxExcludeFilter";
            this.textBoxExcludeFilter.Size = new System.Drawing.Size(158, 20);
            this.textBoxExcludeFilter.TabIndex = 7;
            this.textBoxExcludeFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxExcludeFilter_KeyDown);
            // 
            // labelExcludeFilter
            // 
            this.labelExcludeFilter.AutoSize = true;
            this.labelExcludeFilter.Location = new System.Drawing.Point(218, 5);
            this.labelExcludeFilter.Name = "labelExcludeFilter";
            this.labelExcludeFilter.Size = new System.Drawing.Size(48, 13);
            this.labelExcludeFilter.TabIndex = 6;
            this.labelExcludeFilter.Text = "Exclude:";
            // 
            // labelIncludeFilter
            // 
            this.labelIncludeFilter.AutoSize = true;
            this.labelIncludeFilter.Location = new System.Drawing.Point(3, 5);
            this.labelIncludeFilter.Name = "labelIncludeFilter";
            this.labelIncludeFilter.Size = new System.Drawing.Size(45, 13);
            this.labelIncludeFilter.TabIndex = 4;
            this.labelIncludeFilter.Text = "Include:";
            // 
            // textBoxIncludeFilter
            // 
            this.textBoxIncludeFilter.Location = new System.Drawing.Point(54, 2);
            this.textBoxIncludeFilter.Name = "textBoxIncludeFilter";
            this.textBoxIncludeFilter.Size = new System.Drawing.Size(158, 20);
            this.textBoxIncludeFilter.TabIndex = 3;
            this.textBoxIncludeFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxIncludeFilter_KeyDown);
            // 
            // lblComponentRules
            // 
            this.lblComponentRules.AutoSize = true;
            this.lblComponentRules.Location = new System.Drawing.Point(436, 5);
            this.lblComponentRules.Name = "lblComponentRules";
            this.lblComponentRules.Size = new System.Drawing.Size(94, 13);
            this.lblComponentRules.TabIndex = 2;
            this.lblComponentRules.Text = "Component Rules:";
            // 
            // rbOneToOne
            // 
            this.rbOneToOne.AutoSize = true;
            this.rbOneToOne.Location = new System.Drawing.Point(551, 5);
            this.rbOneToOne.Name = "rbOneToOne";
            this.rbOneToOne.Size = new System.Drawing.Size(84, 17);
            this.rbOneToOne.TabIndex = 1;
            this.rbOneToOne.TabStop = true;
            this.rbOneToOne.Text = "One To One";
            this.rbOneToOne.UseVisualStyleBackColor = true;
            this.rbOneToOne.Click += new System.EventHandler(this.rbOneToOne_Click);
            // 
            // rbOneToMany
            // 
            this.rbOneToMany.AutoSize = true;
            this.rbOneToMany.Checked = true;
            this.rbOneToMany.Location = new System.Drawing.Point(641, 5);
            this.rbOneToMany.Name = "rbOneToMany";
            this.rbOneToMany.Size = new System.Drawing.Size(90, 17);
            this.rbOneToMany.TabIndex = 0;
            this.rbOneToMany.TabStop = true;
            this.rbOneToMany.Text = "One To Many";
            this.rbOneToMany.UseVisualStyleBackColor = true;
            this.rbOneToMany.Click += new System.EventHandler(this.rbOneToMany_Click);
            // 
            // FilesAndFolders
            // 
            this.Controls.Add(this.splitContainer1);
            this.Name = "FilesAndFolders";
            this.Size = new System.Drawing.Size(843, 666);
            this.scMain.Panel1.ResumeLayout(false);
            this.scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            this.gbSourceFiles.ResumeLayout(false);
            this.scSourceFiles.Panel1.ResumeLayout(false);
            this.scSourceFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scSourceFiles)).EndInit();
            this.scSourceFiles.ResumeLayout(false);
            this.gbDestination.ResumeLayout(false);
            this.scDestination.Panel1.ResumeLayout(false);
            this.scDestination.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scDestination)).EndInit();
            this.scDestination.ResumeLayout(false);
            this.cmsDestinationTreeDefault.ResumeLayout(false);
            this.cmsMergeRedirectFolder.ResumeLayout(false);
            this.cmsSourceTree.ResumeLayout(false);
            this.cmsSourceFiles.ResumeLayout(false);
            this.cmsDestinationFiles.ResumeLayout(false);
            this.cmsDestinationRoot.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer scMain;
        private System.Windows.Forms.GroupBox gbSourceFiles;
        private System.Windows.Forms.GroupBox gbDestination;
        private System.Windows.Forms.SplitContainer scSourceFiles;
        private System.Windows.Forms.TreeView tvSourceFiles;
        private System.Windows.Forms.ListView lvSourceFiles;
        private System.Windows.Forms.SplitContainer scDestination;
        private System.Windows.Forms.ColumnHeader chFileName;
        private System.Windows.Forms.ColumnHeader chFileExtn;
        private System.Windows.Forms.ColumnHeader chFileSize;
        private System.Windows.Forms.ColumnHeader chModification;
        private ListViewColumnSorter lvColumnSorter;
        private ListViewColumnSorter lvDestinationColumnSorter;
        private System.Windows.Forms.TreeView tvDestination;
        private System.Windows.Forms.ListView lvDestination;
        private System.Windows.Forms.ImageList ilImageLibrary;
        private System.Windows.Forms.ColumnHeader chDestinationFileName;
        private System.Windows.Forms.ColumnHeader chExtension;
        private System.Windows.Forms.ColumnHeader chFilePath;
        private System.Windows.Forms.ContextMenuStrip cmsDestinationTreeDefault;
        private System.Windows.Forms.ToolStripMenuItem createNewFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsMergeRedirectFolder;
        private System.Windows.Forms.ToolStripMenuItem createNewFolderToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameFolderToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader chSourcePath;
        private System.Windows.Forms.ContextMenuStrip cmsSourceTree;
        private System.Windows.Forms.ToolStripMenuItem refreshSourceFilesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsSourceFiles;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsDestinationFiles;
        private System.Windows.Forms.ToolStripMenuItem removeFileFromProjectToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsDestinationRoot;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ColumnHeader chKeyPath;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RadioButton rbOneToOne;
        private System.Windows.Forms.RadioButton rbOneToMany;
        private System.Windows.Forms.Label lblComponentRules;
        private System.Windows.Forms.ImageList imageListFileIcons;
        private System.Windows.Forms.TextBox textBoxExcludeFilter;
        private System.Windows.Forms.Label labelExcludeFilter;
        private System.Windows.Forms.Label labelIncludeFilter;
        private System.Windows.Forms.TextBox textBoxIncludeFilter;
        private System.Windows.Forms.ImageList imageListFileIconsDst;


    }
}
