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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortCuts));
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tvDestination = new System.Windows.Forms.TreeView();
            iImageLibrary = new System.Windows.Forms.ImageList(components);
            propertyGridShortCut = new System.Windows.Forms.PropertyGrid();
            cmsDestinationRoot = new System.Windows.Forms.ContextMenuStrip(components);
            expandAllToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            collapseAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            cmsDestinationTreeDefault = new System.Windows.Forms.ContextMenuStrip(components);
            createNewFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            renameFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemCreateSC = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            expandAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmsMergeRedirectFolder = new System.Windows.Forms.ContextMenuStrip(components);
            createNewFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemCreateShortCut = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmsShortcut = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            shortcut1 = new Shortcut(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            cmsDestinationRoot.SuspendLayout();
            cmsDestinationTreeDefault.SuspendLayout();
            cmsMergeRedirectFolder.SuspendLayout();
            cmsShortcut.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tvDestination);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(propertyGridShortCut);
            splitContainer1.Size = new System.Drawing.Size(668, 246);
            splitContainer1.SplitterDistance = 367;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // tvDestination
            // 
            tvDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            tvDestination.HotTracking = true;
            tvDestination.ImageIndex = 0;
            tvDestination.ImageList = iImageLibrary;
            tvDestination.Location = new System.Drawing.Point(0, 0);
            tvDestination.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tvDestination.Name = "tvDestination";
            tvDestination.SelectedImageIndex = 0;
            tvDestination.Size = new System.Drawing.Size(367, 246);
            tvDestination.TabIndex = 0;
            tvDestination.AfterLabelEdit += tvDestination_AfterLabelEdit;
            tvDestination.AfterSelect += tvDestination_AfterSelect;
            tvDestination.MouseUp += tvDestination_MouseUp;
            // 
            // iImageLibrary
            // 
            iImageLibrary.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            iImageLibrary.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("iImageLibrary.ImageStream");
            iImageLibrary.TransparentColor = System.Drawing.Color.Transparent;
            iImageLibrary.Images.SetKeyName(0, "folder_open_16x16.gif");
            iImageLibrary.Images.SetKeyName(1, "folder_closed_16x16.gif");
            iImageLibrary.Images.SetKeyName(2, "computer1_16x16.gif");
            iImageLibrary.Images.SetKeyName(3, "SmDll.gif");
            iImageLibrary.Images.SetKeyName(4, "file_16x16.gif");
            iImageLibrary.Images.SetKeyName(5, "WEBFILE.ICO");
            iImageLibrary.Images.SetKeyName(6, "mdf_ndf_dbfiles.ico");
            iImageLibrary.Images.SetKeyName(7, "nt_service.ico");
            iImageLibrary.Images.SetKeyName(8, "smXML.gif");
            iImageLibrary.Images.SetKeyName(9, "blue_folder_closed");
            iImageLibrary.Images.SetKeyName(10, "delete_16x16.gif");
            iImageLibrary.Images.SetKeyName(11, "blue_folder_open.PNG");
            iImageLibrary.Images.SetKeyName(12, "ShortCuts.ico");
            // 
            // propertyGridShortCut
            // 
            propertyGridShortCut.Dock = System.Windows.Forms.DockStyle.Fill;
            propertyGridShortCut.Location = new System.Drawing.Point(0, 0);
            propertyGridShortCut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            propertyGridShortCut.Name = "propertyGridShortCut";
            propertyGridShortCut.Size = new System.Drawing.Size(296, 246);
            propertyGridShortCut.TabIndex = 0;
            propertyGridShortCut.PropertyValueChanged += propertyGridShortCut_PropertyValueChanged;
            // 
            // cmsDestinationRoot
            // 
            cmsDestinationRoot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { expandAllToolStripMenuItem2, collapseAllToolStripMenuItem1, toolStripSeparator3 });
            cmsDestinationRoot.Name = "cmsDestinationRoot";
            cmsDestinationRoot.Size = new System.Drawing.Size(137, 54);
            // 
            // expandAllToolStripMenuItem2
            // 
            expandAllToolStripMenuItem2.Image = (System.Drawing.Image)resources.GetObject("expandAllToolStripMenuItem2.Image");
            expandAllToolStripMenuItem2.Name = "expandAllToolStripMenuItem2";
            expandAllToolStripMenuItem2.Size = new System.Drawing.Size(136, 22);
            expandAllToolStripMenuItem2.Text = "Expand All";
            expandAllToolStripMenuItem2.Click += expandAllToolStripMenuItem2_Click;
            // 
            // collapseAllToolStripMenuItem1
            // 
            collapseAllToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("collapseAllToolStripMenuItem1.Image");
            collapseAllToolStripMenuItem1.Name = "collapseAllToolStripMenuItem1";
            collapseAllToolStripMenuItem1.Size = new System.Drawing.Size(136, 22);
            collapseAllToolStripMenuItem1.Text = "Collapse All";
            collapseAllToolStripMenuItem1.Click += collapseAllToolStripMenuItem1_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(133, 6);
            // 
            // cmsDestinationTreeDefault
            // 
            cmsDestinationTreeDefault.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { createNewFolderToolStripMenuItem, renameFolderToolStripMenuItem, deleteToolStripMenuItem, toolStripMenuItemCreateSC, toolStripSeparator1, expandAllToolStripMenuItem1, collapseAllToolStripMenuItem });
            cmsDestinationTreeDefault.Name = "cmsDestinationTreeDefault";
            cmsDestinationTreeDefault.Size = new System.Drawing.Size(213, 164);
            // 
            // createNewFolderToolStripMenuItem
            // 
            createNewFolderToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("createNewFolderToolStripMenuItem.Image");
            createNewFolderToolStripMenuItem.Name = "createNewFolderToolStripMenuItem";
            createNewFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            createNewFolderToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            createNewFolderToolStripMenuItem.Text = "Create New Folder";
            createNewFolderToolStripMenuItem.Click += createNewFolderToolStripMenuItem_Click;
            // 
            // renameFolderToolStripMenuItem
            // 
            renameFolderToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("renameFolderToolStripMenuItem.Image");
            renameFolderToolStripMenuItem.Name = "renameFolderToolStripMenuItem";
            renameFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            renameFolderToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            renameFolderToolStripMenuItem.Text = "Rename Folder";
            renameFolderToolStripMenuItem.Click += renameFolderToolStripMenuItem_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("deleteToolStripMenuItem.Image");
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            deleteToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            deleteToolStripMenuItem.Text = "Remove From Project";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // toolStripMenuItemCreateSC
            // 
            toolStripMenuItemCreateSC.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItemCreateSC.Image");
            toolStripMenuItemCreateSC.Name = "toolStripMenuItemCreateSC";
            toolStripMenuItemCreateSC.Size = new System.Drawing.Size(212, 22);
            toolStripMenuItemCreateSC.Text = "Create ShortCut";
            toolStripMenuItemCreateSC.Click += toolStripMenuItem1_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(209, 6);
            // 
            // expandAllToolStripMenuItem1
            // 
            expandAllToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("expandAllToolStripMenuItem1.Image");
            expandAllToolStripMenuItem1.Name = "expandAllToolStripMenuItem1";
            expandAllToolStripMenuItem1.Size = new System.Drawing.Size(212, 22);
            expandAllToolStripMenuItem1.Text = "Expand All";
            expandAllToolStripMenuItem1.Click += expandAllToolStripMenuItem1_Click;
            // 
            // collapseAllToolStripMenuItem
            // 
            collapseAllToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("collapseAllToolStripMenuItem.Image");
            collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            collapseAllToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            collapseAllToolStripMenuItem.Text = "Collapse All";
            collapseAllToolStripMenuItem.Click += collapseAllToolStripMenuItem_Click;
            // 
            // cmsMergeRedirectFolder
            // 
            cmsMergeRedirectFolder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { createNewFolderToolStripMenuItem1, toolStripMenuItemCreateShortCut, toolStripSeparator2, expandAllToolStripMenuItem });
            cmsMergeRedirectFolder.Name = "cmsMergeRedirectFolder";
            cmsMergeRedirectFolder.Size = new System.Drawing.Size(194, 76);
            // 
            // createNewFolderToolStripMenuItem1
            // 
            createNewFolderToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("createNewFolderToolStripMenuItem1.Image");
            createNewFolderToolStripMenuItem1.Name = "createNewFolderToolStripMenuItem1";
            createNewFolderToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            createNewFolderToolStripMenuItem1.Size = new System.Drawing.Size(193, 22);
            createNewFolderToolStripMenuItem1.Text = "Create New Folder";
            createNewFolderToolStripMenuItem1.Click += createNewFolderToolStripMenuItem1_Click;
            // 
            // toolStripMenuItemCreateShortCut
            // 
            toolStripMenuItemCreateShortCut.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItemCreateShortCut.Image");
            toolStripMenuItemCreateShortCut.Name = "toolStripMenuItemCreateShortCut";
            toolStripMenuItemCreateShortCut.Size = new System.Drawing.Size(193, 22);
            toolStripMenuItemCreateShortCut.Text = "Create ShortCut";
            toolStripMenuItemCreateShortCut.Click += toolStripMenuItemCreateShortCut_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // expandAllToolStripMenuItem
            // 
            expandAllToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("expandAllToolStripMenuItem.Image");
            expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            expandAllToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            expandAllToolStripMenuItem.Text = "Expand All";
            expandAllToolStripMenuItem.Click += expandAllToolStripMenuItem_Click;
            // 
            // cmsShortcut
            // 
            cmsShortcut.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem3 });
            cmsShortcut.Name = "cmsDestinationTreeDefault";
            cmsShortcut.Size = new System.Drawing.Size(213, 26);
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem3.Image");
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            toolStripMenuItem3.Size = new System.Drawing.Size(212, 22);
            toolStripMenuItem3.Text = "Remove From Project";
            toolStripMenuItem3.Click += toolStripMenuItem3_Click;
            // 
            // shortcut1
            // 
            shortcut1.Arguments = null;
            shortcut1.Description = null;
            shortcut1.Name = null;
            shortcut1.Show = null;
            shortcut1.WorkingDirectory = null;
            // 
            // ShortCuts
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ShortCuts";
            Size = new System.Drawing.Size(668, 246);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            cmsDestinationRoot.ResumeLayout(false);
            cmsDestinationTreeDefault.ResumeLayout(false);
            cmsMergeRedirectFolder.ResumeLayout(false);
            cmsShortcut.ResumeLayout(false);
            ResumeLayout(false);
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
