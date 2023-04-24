namespace Designers.NewFilesAndFolders
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilesAndFolders));
            scMain = new System.Windows.Forms.SplitContainer();
            gbSourceFiles = new System.Windows.Forms.GroupBox();
            scSourceFiles = new System.Windows.Forms.SplitContainer();
            tvSourceFiles = new System.Windows.Forms.TreeView();
            ilImageLibrary = new System.Windows.Forms.ImageList(components);
            lvSourceFiles = new System.Windows.Forms.ListView();
            chFileName = new System.Windows.Forms.ColumnHeader();
            chFileExtn = new System.Windows.Forms.ColumnHeader();
            chFileSize = new System.Windows.Forms.ColumnHeader();
            chModification = new System.Windows.Forms.ColumnHeader();
            chSourcePath = new System.Windows.Forms.ColumnHeader();
            imageListFileIcons = new System.Windows.Forms.ImageList(components);
            gbDestination = new System.Windows.Forms.GroupBox();
            scDestination = new System.Windows.Forms.SplitContainer();
            tvDestination = new System.Windows.Forms.TreeView();
            lvDestination = new System.Windows.Forms.ListView();
            chDestinationFileName = new System.Windows.Forms.ColumnHeader();
            chExtension = new System.Windows.Forms.ColumnHeader();
            chFilePath = new System.Windows.Forms.ColumnHeader();
            chKeyPath = new System.Windows.Forms.ColumnHeader();
            imageListFileIconsDst = new System.Windows.Forms.ImageList(components);
            cmsDestinationTreeDefault = new System.Windows.Forms.ContextMenuStrip(components);
            createNewFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            renameFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            expandAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmsINSTALLLOCATION = new System.Windows.Forms.ContextMenuStrip(components);
            createNewFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmsSourceTree = new System.Windows.Forms.ContextMenuStrip(components);
            refreshSourceFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmsSourceFiles = new System.Windows.Forms.ContextMenuStrip(components);
            refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmsDestinationFiles = new System.Windows.Forms.ContextMenuStrip(components);
            removeFileFromProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmsDestinationRoot = new System.Windows.Forms.ContextMenuStrip(components);
            expandAllToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            collapseAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            textBoxExcludeFilter = new System.Windows.Forms.TextBox();
            labelExcludeFilter = new System.Windows.Forms.Label();
            labelIncludeFilter = new System.Windows.Forms.Label();
            textBoxIncludeFilter = new System.Windows.Forms.TextBox();
            lblComponentRules = new System.Windows.Forms.Label();
            rbOneToOne = new System.Windows.Forms.RadioButton();
            rbOneToMany = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)scMain).BeginInit();
            scMain.Panel1.SuspendLayout();
            scMain.Panel2.SuspendLayout();
            scMain.SuspendLayout();
            gbSourceFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scSourceFiles).BeginInit();
            scSourceFiles.Panel1.SuspendLayout();
            scSourceFiles.Panel2.SuspendLayout();
            scSourceFiles.SuspendLayout();
            gbDestination.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scDestination).BeginInit();
            scDestination.Panel1.SuspendLayout();
            scDestination.Panel2.SuspendLayout();
            scDestination.SuspendLayout();
            cmsDestinationTreeDefault.SuspendLayout();
            cmsINSTALLLOCATION.SuspendLayout();
            cmsSourceTree.SuspendLayout();
            cmsSourceFiles.SuspendLayout();
            cmsDestinationFiles.SuspendLayout();
            cmsDestinationRoot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // scMain
            // 
            scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            scMain.Location = new System.Drawing.Point(0, 0);
            scMain.Name = "scMain";
            scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMain.Panel1
            // 
            scMain.Panel1.Controls.Add(gbSourceFiles);
            // 
            // scMain.Panel2
            // 
            scMain.Panel2.Controls.Add(gbDestination);
            scMain.Size = new System.Drawing.Size(1200, 666);
            scMain.SplitterDistance = 364;
            scMain.TabIndex = 0;
            // 
            // gbSourceFiles
            // 
            gbSourceFiles.Controls.Add(scSourceFiles);
            gbSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            gbSourceFiles.Location = new System.Drawing.Point(0, 0);
            gbSourceFiles.Name = "gbSourceFiles";
            gbSourceFiles.Size = new System.Drawing.Size(1200, 364);
            gbSourceFiles.TabIndex = 0;
            gbSourceFiles.TabStop = false;
            gbSourceFiles.Text = "Source Files";
            // 
            // scSourceFiles
            // 
            scSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            scSourceFiles.Location = new System.Drawing.Point(3, 19);
            scSourceFiles.Name = "scSourceFiles";
            // 
            // scSourceFiles.Panel1
            // 
            scSourceFiles.Panel1.Controls.Add(tvSourceFiles);
            // 
            // scSourceFiles.Panel2
            // 
            scSourceFiles.Panel2.Controls.Add(lvSourceFiles);
            scSourceFiles.Size = new System.Drawing.Size(1194, 342);
            scSourceFiles.SplitterDistance = 263;
            scSourceFiles.TabIndex = 0;
            // 
            // tvSourceFiles
            // 
            tvSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            tvSourceFiles.ImageIndex = 1;
            tvSourceFiles.ImageList = ilImageLibrary;
            tvSourceFiles.Location = new System.Drawing.Point(0, 0);
            tvSourceFiles.Name = "tvSourceFiles";
            tvSourceFiles.SelectedImageIndex = 0;
            tvSourceFiles.Size = new System.Drawing.Size(263, 342);
            tvSourceFiles.TabIndex = 0;
            tvSourceFiles.BeforeExpand += tvSourceFiles_BeforeExpand;
            tvSourceFiles.ItemDrag += tvSourceFiles_ItemDrag;
            tvSourceFiles.AfterSelect += tvSourceFiles_AfterSelect;
            tvSourceFiles.KeyDown += tvSourceFiles_KeyDown;
            // 
            // ilImageLibrary
            // 
            ilImageLibrary.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            ilImageLibrary.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("ilImageLibrary.ImageStream");
            ilImageLibrary.TransparentColor = System.Drawing.Color.Transparent;
            ilImageLibrary.Images.SetKeyName(0, "folder_open_16x16.gif");
            ilImageLibrary.Images.SetKeyName(1, "folder_closed_16x16.gif");
            ilImageLibrary.Images.SetKeyName(2, "computer1_16x16.gif");
            ilImageLibrary.Images.SetKeyName(3, "SmDll.gif");
            ilImageLibrary.Images.SetKeyName(4, "file_16x16.gif");
            ilImageLibrary.Images.SetKeyName(5, "WEBFILE.ICO");
            ilImageLibrary.Images.SetKeyName(6, "mdf_ndf_dbfiles.ico");
            ilImageLibrary.Images.SetKeyName(7, "nt_service.ico");
            ilImageLibrary.Images.SetKeyName(8, "smXML.gif");
            ilImageLibrary.Images.SetKeyName(9, "blue_folder_closed.PNG");
            ilImageLibrary.Images.SetKeyName(10, "delete_16x16.gif");
            ilImageLibrary.Images.SetKeyName(11, "blue_folder_open.PNG");
            // 
            // lvSourceFiles
            // 
            lvSourceFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { chFileName, chFileExtn, chFileSize, chModification, chSourcePath });
            lvSourceFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            lvSourceFiles.Location = new System.Drawing.Point(0, 0);
            lvSourceFiles.Name = "lvSourceFiles";
            lvSourceFiles.Size = new System.Drawing.Size(927, 342);
            lvSourceFiles.SmallImageList = imageListFileIcons;
            lvSourceFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lvSourceFiles.TabIndex = 0;
            lvSourceFiles.UseCompatibleStateImageBehavior = false;
            lvSourceFiles.View = System.Windows.Forms.View.Details;
            lvSourceFiles.ItemDrag += lvSourceFiles_ItemDrag;
            lvSourceFiles.KeyDown += lvSourceFiles_KeyDown;
            // 
            // chFileName
            // 
            chFileName.Text = "File Name";
            chFileName.Width = 200;
            // 
            // chFileExtn
            // 
            chFileExtn.Text = "Extension";
            chFileExtn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            chFileExtn.Width = 70;
            // 
            // chFileSize
            // 
            chFileSize.Text = "Size";
            chFileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            chFileSize.Width = 75;
            // 
            // chModification
            // 
            chModification.Text = "Last Modified";
            chModification.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            chModification.Width = 200;
            // 
            // chSourcePath
            // 
            chSourcePath.Text = "Source Path ";
            chSourcePath.Width = 500;
            // 
            // imageListFileIcons
            // 
            imageListFileIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageListFileIcons.ImageSize = new System.Drawing.Size(16, 16);
            imageListFileIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // gbDestination
            // 
            gbDestination.Controls.Add(scDestination);
            gbDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            gbDestination.Location = new System.Drawing.Point(0, 0);
            gbDestination.Name = "gbDestination";
            gbDestination.Size = new System.Drawing.Size(1200, 298);
            gbDestination.TabIndex = 0;
            gbDestination.TabStop = false;
            gbDestination.Text = "Destination Files";
            // 
            // scDestination
            // 
            scDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            scDestination.Location = new System.Drawing.Point(3, 19);
            scDestination.Name = "scDestination";
            // 
            // scDestination.Panel1
            // 
            scDestination.Panel1.Controls.Add(tvDestination);
            // 
            // scDestination.Panel2
            // 
            scDestination.Panel2.Controls.Add(lvDestination);
            scDestination.Size = new System.Drawing.Size(1194, 276);
            scDestination.SplitterDistance = 264;
            scDestination.TabIndex = 0;
            // 
            // tvDestination
            // 
            tvDestination.AllowDrop = true;
            tvDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            tvDestination.HotTracking = true;
            tvDestination.ImageIndex = 1;
            tvDestination.ImageList = ilImageLibrary;
            tvDestination.Indent = 19;
            tvDestination.Location = new System.Drawing.Point(0, 0);
            tvDestination.Name = "tvDestination";
            tvDestination.SelectedImageIndex = 0;
            tvDestination.Size = new System.Drawing.Size(264, 276);
            tvDestination.TabIndex = 0;
            tvDestination.AfterSelect += tvDestination_AfterSelect;
            tvDestination.DragDrop += tvDestination_DragDrop;
            tvDestination.DragEnter += tvDestination_DragEnter;
            tvDestination.DragOver += tvDestination_DragOver;
            tvDestination.MouseUp += tvDestination_MouseUp;
            // 
            // lvDestination
            // 
            lvDestination.AllowDrop = true;
            lvDestination.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { chDestinationFileName, chExtension, chFilePath, chKeyPath });
            lvDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            lvDestination.Location = new System.Drawing.Point(0, 0);
            lvDestination.Name = "lvDestination";
            lvDestination.Size = new System.Drawing.Size(926, 276);
            lvDestination.SmallImageList = imageListFileIconsDst;
            lvDestination.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lvDestination.TabIndex = 0;
            lvDestination.UseCompatibleStateImageBehavior = false;
            lvDestination.View = System.Windows.Forms.View.Details;
            lvDestination.ItemDrag += lvDestination_ItemDrag;
            lvDestination.DragDrop += lvDestination_DragDrop;
            lvDestination.DragEnter += lvDestination_DragEnter;
            // 
            // chDestinationFileName
            // 
            chDestinationFileName.Text = "File Name";
            chDestinationFileName.Width = 187;
            // 
            // chExtension
            // 
            chExtension.Text = "Extension";
            chExtension.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            chExtension.Width = 146;
            // 
            // chFilePath
            // 
            chFilePath.Text = "File Path";
            chFilePath.Width = 184;
            // 
            // chKeyPath
            // 
            chKeyPath.Text = "Key";
            // 
            // imageListFileIconsDst
            // 
            imageListFileIconsDst.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageListFileIconsDst.ImageSize = new System.Drawing.Size(16, 16);
            imageListFileIconsDst.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // cmsDestinationTreeDefault
            // 
            cmsDestinationTreeDefault.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmsDestinationTreeDefault.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { createNewFolderToolStripMenuItem, renameFolderToolStripMenuItem, deleteToolStripMenuItem, toolStripSeparator1, expandAllToolStripMenuItem1, collapseAllToolStripMenuItem });
            cmsDestinationTreeDefault.Name = "cmsDestinationTreeDefault";
            cmsDestinationTreeDefault.Size = new System.Drawing.Size(221, 160);
            // 
            // createNewFolderToolStripMenuItem
            // 
            createNewFolderToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("createNewFolderToolStripMenuItem.Image");
            createNewFolderToolStripMenuItem.Name = "createNewFolderToolStripMenuItem";
            createNewFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            createNewFolderToolStripMenuItem.Size = new System.Drawing.Size(220, 30);
            createNewFolderToolStripMenuItem.Text = "Create New Folder";
            // 
            // renameFolderToolStripMenuItem
            // 
            renameFolderToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("renameFolderToolStripMenuItem.Image");
            renameFolderToolStripMenuItem.Name = "renameFolderToolStripMenuItem";
            renameFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            renameFolderToolStripMenuItem.Size = new System.Drawing.Size(220, 30);
            renameFolderToolStripMenuItem.Text = "Rename Folder";
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("deleteToolStripMenuItem.Image");
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            deleteToolStripMenuItem.Size = new System.Drawing.Size(220, 30);
            deleteToolStripMenuItem.Text = "Remove From Project";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(217, 6);
            // 
            // expandAllToolStripMenuItem1
            // 
            expandAllToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("expandAllToolStripMenuItem1.Image");
            expandAllToolStripMenuItem1.Name = "expandAllToolStripMenuItem1";
            expandAllToolStripMenuItem1.Size = new System.Drawing.Size(220, 30);
            expandAllToolStripMenuItem1.Text = "Expand All";
            // 
            // collapseAllToolStripMenuItem
            // 
            collapseAllToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("collapseAllToolStripMenuItem.Image");
            collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            collapseAllToolStripMenuItem.Size = new System.Drawing.Size(220, 30);
            collapseAllToolStripMenuItem.Text = "Collapse All";
            // 
            // cmsINSTALLLOCATION
            // 
            cmsINSTALLLOCATION.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmsINSTALLLOCATION.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { createNewFolderToolStripMenuItem1, toolStripSeparator2, expandAllToolStripMenuItem });
            cmsINSTALLLOCATION.Name = "cmsINSTALLLOCATION";
            cmsINSTALLLOCATION.Size = new System.Drawing.Size(202, 70);
            // 
            // createNewFolderToolStripMenuItem1
            // 
            createNewFolderToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("createNewFolderToolStripMenuItem1.Image");
            createNewFolderToolStripMenuItem1.Name = "createNewFolderToolStripMenuItem1";
            createNewFolderToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            createNewFolderToolStripMenuItem1.Size = new System.Drawing.Size(201, 30);
            createNewFolderToolStripMenuItem1.Text = "Create New Folder";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(198, 6);
            // 
            // expandAllToolStripMenuItem
            // 
            expandAllToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("expandAllToolStripMenuItem.Image");
            expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            expandAllToolStripMenuItem.Size = new System.Drawing.Size(201, 30);
            expandAllToolStripMenuItem.Text = "Expand All";
            // 
            // cmsSourceTree
            // 
            cmsSourceTree.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmsSourceTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { refreshSourceFilesToolStripMenuItem });
            cmsSourceTree.Name = "cmsSourceTree";
            cmsSourceTree.Size = new System.Drawing.Size(141, 34);
            // 
            // refreshSourceFilesToolStripMenuItem
            // 
            refreshSourceFilesToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("refreshSourceFilesToolStripMenuItem.Image");
            refreshSourceFilesToolStripMenuItem.Name = "refreshSourceFilesToolStripMenuItem";
            refreshSourceFilesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            refreshSourceFilesToolStripMenuItem.Size = new System.Drawing.Size(140, 30);
            refreshSourceFilesToolStripMenuItem.Text = "Refresh";
            refreshSourceFilesToolStripMenuItem.Click += refreshSourceFilesToolStripMenuItem_Click;
            // 
            // cmsSourceFiles
            // 
            cmsSourceFiles.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmsSourceFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { refreshToolStripMenuItem });
            cmsSourceFiles.Name = "cmsSourceFiles";
            cmsSourceFiles.Size = new System.Drawing.Size(141, 34);
            // 
            // refreshToolStripMenuItem
            // 
            refreshToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("refreshToolStripMenuItem.Image");
            refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            refreshToolStripMenuItem.Size = new System.Drawing.Size(140, 30);
            refreshToolStripMenuItem.Text = "Refresh";
            refreshToolStripMenuItem.Click += refreshToolStripMenuItem_Click;
            // 
            // cmsDestinationFiles
            // 
            cmsDestinationFiles.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmsDestinationFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { removeFileFromProjectToolStripMenuItem });
            cmsDestinationFiles.Name = "cmsDestinationFiles";
            cmsDestinationFiles.Size = new System.Drawing.Size(221, 34);
            // 
            // removeFileFromProjectToolStripMenuItem
            // 
            removeFileFromProjectToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("removeFileFromProjectToolStripMenuItem.Image");
            removeFileFromProjectToolStripMenuItem.Name = "removeFileFromProjectToolStripMenuItem";
            removeFileFromProjectToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            removeFileFromProjectToolStripMenuItem.Size = new System.Drawing.Size(220, 30);
            removeFileFromProjectToolStripMenuItem.Text = "Remove From Project";
            // 
            // cmsDestinationRoot
            // 
            cmsDestinationRoot.ImageScalingSize = new System.Drawing.Size(24, 24);
            cmsDestinationRoot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { expandAllToolStripMenuItem2, collapseAllToolStripMenuItem1, toolStripSeparator3 });
            cmsDestinationRoot.Name = "cmsDestinationRoot";
            cmsDestinationRoot.Size = new System.Drawing.Size(145, 70);
            // 
            // expandAllToolStripMenuItem2
            // 
            expandAllToolStripMenuItem2.Image = (System.Drawing.Image)resources.GetObject("expandAllToolStripMenuItem2.Image");
            expandAllToolStripMenuItem2.Name = "expandAllToolStripMenuItem2";
            expandAllToolStripMenuItem2.Size = new System.Drawing.Size(144, 30);
            expandAllToolStripMenuItem2.Text = "Expand All";
            // 
            // collapseAllToolStripMenuItem1
            // 
            collapseAllToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("collapseAllToolStripMenuItem1.Image");
            collapseAllToolStripMenuItem1.Name = "collapseAllToolStripMenuItem1";
            collapseAllToolStripMenuItem1.Size = new System.Drawing.Size(144, 30);
            collapseAllToolStripMenuItem1.Text = "Collapse All";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(141, 6);
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(textBoxExcludeFilter);
            splitContainer1.Panel1.Controls.Add(labelExcludeFilter);
            splitContainer1.Panel1.Controls.Add(labelIncludeFilter);
            splitContainer1.Panel1.Controls.Add(textBoxIncludeFilter);
            splitContainer1.Panel1.Controls.Add(lblComponentRules);
            splitContainer1.Panel1.Controls.Add(rbOneToOne);
            splitContainer1.Panel1.Controls.Add(rbOneToMany);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(scMain);
            splitContainer1.Size = new System.Drawing.Size(1200, 700);
            splitContainer1.SplitterDistance = 30;
            splitContainer1.TabIndex = 6;
            // 
            // textBoxExcludeFilter
            // 
            textBoxExcludeFilter.Location = new System.Drawing.Point(313, -1);
            textBoxExcludeFilter.Name = "textBoxExcludeFilter";
            textBoxExcludeFilter.Size = new System.Drawing.Size(158, 23);
            textBoxExcludeFilter.TabIndex = 7;
            textBoxExcludeFilter.KeyDown += textBoxExcludeFilter_KeyDown;
            // 
            // labelExcludeFilter
            // 
            labelExcludeFilter.AutoSize = true;
            labelExcludeFilter.Location = new System.Drawing.Point(238, 2);
            labelExcludeFilter.Name = "labelExcludeFilter";
            labelExcludeFilter.Size = new System.Drawing.Size(51, 15);
            labelExcludeFilter.TabIndex = 6;
            labelExcludeFilter.Text = "Exclude:";
            // 
            // labelIncludeFilter
            // 
            labelIncludeFilter.AutoSize = true;
            labelIncludeFilter.Location = new System.Drawing.Point(3, 2);
            labelIncludeFilter.Name = "labelIncludeFilter";
            labelIncludeFilter.Size = new System.Drawing.Size(49, 15);
            labelIncludeFilter.TabIndex = 4;
            labelIncludeFilter.Text = "Include:";
            // 
            // textBoxIncludeFilter
            // 
            textBoxIncludeFilter.Location = new System.Drawing.Point(74, -1);
            textBoxIncludeFilter.Name = "textBoxIncludeFilter";
            textBoxIncludeFilter.Size = new System.Drawing.Size(158, 23);
            textBoxIncludeFilter.TabIndex = 3;
            textBoxIncludeFilter.KeyDown += textBoxIncludeFilter_KeyDown;
            // 
            // lblComponentRules
            // 
            lblComponentRules.AutoSize = true;
            lblComponentRules.Location = new System.Drawing.Point(477, 2);
            lblComponentRules.Name = "lblComponentRules";
            lblComponentRules.Size = new System.Drawing.Size(105, 15);
            lblComponentRules.TabIndex = 2;
            lblComponentRules.Text = "Component Rules:";
            // 
            // rbOneToOne
            // 
            rbOneToOne.AutoSize = true;
            rbOneToOne.Location = new System.Drawing.Point(632, 0);
            rbOneToOne.Name = "rbOneToOne";
            rbOneToOne.Size = new System.Drawing.Size(87, 19);
            rbOneToOne.TabIndex = 1;
            rbOneToOne.TabStop = true;
            rbOneToOne.Text = "One To One";
            rbOneToOne.UseVisualStyleBackColor = true;
            rbOneToOne.Click += rbOneToOne_Click;
            // 
            // rbOneToMany
            // 
            rbOneToMany.AutoSize = true;
            rbOneToMany.Checked = true;
            rbOneToMany.Location = new System.Drawing.Point(762, 0);
            rbOneToMany.Name = "rbOneToMany";
            rbOneToMany.Size = new System.Drawing.Size(95, 19);
            rbOneToMany.TabIndex = 0;
            rbOneToMany.TabStop = true;
            rbOneToMany.Text = "One To Many";
            rbOneToMany.UseVisualStyleBackColor = true;
            rbOneToMany.Click += rbOneToMany_Click;
            // 
            // FilesAndFolders
            // 
            Controls.Add(splitContainer1);
            Name = "FilesAndFolders";
            Size = new System.Drawing.Size(1200, 700);
            scMain.Panel1.ResumeLayout(false);
            scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scMain).EndInit();
            scMain.ResumeLayout(false);
            gbSourceFiles.ResumeLayout(false);
            scSourceFiles.Panel1.ResumeLayout(false);
            scSourceFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scSourceFiles).EndInit();
            scSourceFiles.ResumeLayout(false);
            gbDestination.ResumeLayout(false);
            scDestination.Panel1.ResumeLayout(false);
            scDestination.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scDestination).EndInit();
            scDestination.ResumeLayout(false);
            cmsDestinationTreeDefault.ResumeLayout(false);
            cmsINSTALLLOCATION.ResumeLayout(false);
            cmsSourceTree.ResumeLayout(false);
            cmsSourceFiles.ResumeLayout(false);
            cmsDestinationFiles.ResumeLayout(false);
            cmsDestinationRoot.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.ContextMenuStrip cmsINSTALLLOCATION;
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
