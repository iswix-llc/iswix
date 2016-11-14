namespace ShortCutsDesigner
{
    partial class ShortCuts
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortCuts));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvDestination = new System.Windows.Forms.TreeView();
            this.iImageLibrary = new System.Windows.Forms.ImageList(this.components);
            this.propertyGridShortCut = new System.Windows.Forms.PropertyGrid();
            this.cmsDestinationRoot = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandAllToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cmsDestinationTreeDefault = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createNewFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCreateSC = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMergeRedirectFolder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createNewFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCreateShortCut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsShortcut = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.shortcut1 = new ShortCutsDesigner.Shortcut(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.cmsDestinationRoot.SuspendLayout();
            this.cmsDestinationTreeDefault.SuspendLayout();
            this.cmsMergeRedirectFolder.SuspendLayout();
            this.cmsShortcut.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvDestination);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGridShortCut);
            this.splitContainer1.Size = new System.Drawing.Size(573, 213);
            this.splitContainer1.SplitterDistance = 315;
            this.splitContainer1.TabIndex = 0;
            // 
            // tvDestination
            // 
            this.tvDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDestination.HotTracking = true;
            this.tvDestination.ImageIndex = 0;
            this.tvDestination.ImageList = this.iImageLibrary;
            this.tvDestination.Location = new System.Drawing.Point(0, 0);
            this.tvDestination.Name = "tvDestination";
            this.tvDestination.SelectedImageIndex = 0;
            this.tvDestination.Size = new System.Drawing.Size(315, 213);
            this.tvDestination.TabIndex = 0;
            this.tvDestination.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvDestination_AfterLabelEdit);
            this.tvDestination.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvDestination_AfterSelect);
            this.tvDestination.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tvDestination_MouseUp);
            // 
            // iImageLibrary
            // 
            this.iImageLibrary.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iImageLibrary.ImageStream")));
            this.iImageLibrary.TransparentColor = System.Drawing.Color.Transparent;
            this.iImageLibrary.Images.SetKeyName(0, "folder_open_16x16.gif");
            this.iImageLibrary.Images.SetKeyName(1, "folder_closed_16x16.gif");
            this.iImageLibrary.Images.SetKeyName(2, "computer1_16x16.gif");
            this.iImageLibrary.Images.SetKeyName(3, "SmDll.gif");
            this.iImageLibrary.Images.SetKeyName(4, "file_16x16.gif");
            this.iImageLibrary.Images.SetKeyName(5, "WEBFILE.ICO");
            this.iImageLibrary.Images.SetKeyName(6, "mdf_ndf_dbfiles.ico");
            this.iImageLibrary.Images.SetKeyName(7, "nt_service.ico");
            this.iImageLibrary.Images.SetKeyName(8, "smXML.gif");
            this.iImageLibrary.Images.SetKeyName(9, "blue_folder_closed");
            this.iImageLibrary.Images.SetKeyName(10, "delete_16x16.gif");
            this.iImageLibrary.Images.SetKeyName(11, "blue_folder_open.PNG");
            this.iImageLibrary.Images.SetKeyName(12, "ShortCuts.ico");
            // 
            // propertyGridShortCut
            // 
            this.propertyGridShortCut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridShortCut.Location = new System.Drawing.Point(0, 0);
            this.propertyGridShortCut.Name = "propertyGridShortCut";
            this.propertyGridShortCut.Size = new System.Drawing.Size(254, 213);
            this.propertyGridShortCut.TabIndex = 0;
            this.propertyGridShortCut.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridShortCut_PropertyValueChanged);
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
            // cmsDestinationTreeDefault
            // 
            this.cmsDestinationTreeDefault.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewFolderToolStripMenuItem,
            this.renameFolderToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItemCreateSC,
            this.toolStripSeparator1,
            this.expandAllToolStripMenuItem1,
            this.collapseAllToolStripMenuItem});
            this.cmsDestinationTreeDefault.Name = "cmsDestinationTreeDefault";
            this.cmsDestinationTreeDefault.Size = new System.Drawing.Size(213, 142);
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
            // toolStripMenuItemCreateSC
            // 
            this.toolStripMenuItemCreateSC.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemCreateSC.Image")));
            this.toolStripMenuItemCreateSC.Name = "toolStripMenuItemCreateSC";
            this.toolStripMenuItemCreateSC.Size = new System.Drawing.Size(212, 22);
            this.toolStripMenuItemCreateSC.Text = "Create ShortCut";
            this.toolStripMenuItemCreateSC.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
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
            this.toolStripMenuItemCreateShortCut,
            this.toolStripSeparator2,
            this.expandAllToolStripMenuItem});
            this.cmsMergeRedirectFolder.Name = "cmsMergeRedirectFolder";
            this.cmsMergeRedirectFolder.Size = new System.Drawing.Size(194, 98);
            // 
            // createNewFolderToolStripMenuItem1
            // 
            this.createNewFolderToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("createNewFolderToolStripMenuItem1.Image")));
            this.createNewFolderToolStripMenuItem1.Name = "createNewFolderToolStripMenuItem1";
            this.createNewFolderToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.createNewFolderToolStripMenuItem1.Size = new System.Drawing.Size(193, 22);
            this.createNewFolderToolStripMenuItem1.Text = "Create New Folder";
            this.createNewFolderToolStripMenuItem1.Click += new System.EventHandler(this.createNewFolderToolStripMenuItem1_Click);
            // 
            // toolStripMenuItemCreateShortCut
            // 
            this.toolStripMenuItemCreateShortCut.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemCreateShortCut.Image")));
            this.toolStripMenuItemCreateShortCut.Name = "toolStripMenuItemCreateShortCut";
            this.toolStripMenuItemCreateShortCut.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItemCreateShortCut.Text = "Create ShortCut";
            this.toolStripMenuItemCreateShortCut.Click += new System.EventHandler(this.toolStripMenuItemCreateShortCut_Click);
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
            // cmsShortcut
            // 
            this.cmsShortcut.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3});
            this.cmsShortcut.Name = "cmsDestinationTreeDefault";
            this.cmsShortcut.Size = new System.Drawing.Size(213, 26);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItem3.Image")));
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.toolStripMenuItem3.Size = new System.Drawing.Size(212, 22);
            this.toolStripMenuItem3.Text = "Remove From Project";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // shortcut1
            // 
            this.shortcut1.Arguments = null;
            this.shortcut1.Description = null;
            this.shortcut1.Name = null;
            this.shortcut1.Show = null;
            this.shortcut1.WorkingDirectory = null;
            // 
            // ShortCuts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ShortCuts";
            this.Size = new System.Drawing.Size(573, 213);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.cmsDestinationRoot.ResumeLayout(false);
            this.cmsDestinationTreeDefault.ResumeLayout(false);
            this.cmsMergeRedirectFolder.ResumeLayout(false);
            this.cmsShortcut.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvDestination;
        private System.Windows.Forms.PropertyGrid propertyGridShortCut;
        private System.Windows.Forms.ImageList iImageLibrary;
        private System.Windows.Forms.ContextMenuStrip cmsDestinationRoot;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ContextMenuStrip cmsDestinationTreeDefault;
        private System.Windows.Forms.ToolStripMenuItem createNewFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsMergeRedirectFolder;
        private System.Windows.Forms.ToolStripMenuItem createNewFolderToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCreateShortCut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCreateSC;
        private System.Windows.Forms.ContextMenuStrip cmsShortcut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private Shortcut shortcut1;
    }
}
