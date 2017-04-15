namespace ProtocolHandlersDesigner
{
    partial class ProtocolHandlers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProtocolHandlers));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewProtocolHandlers = new System.Windows.Forms.TreeView();
            this.propertyGridServiceInstall = new System.Windows.Forms.PropertyGrid();
            this.cmsComputer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createNewFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmsService = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.renameFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.iImageLibrary = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.cmsComputer.SuspendLayout();
            this.cmsService.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.treeViewProtocolHandlers);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGridServiceInstall);
            this.splitContainer1.Size = new System.Drawing.Size(533, 372);
            this.splitContainer1.SplitterDistance = 229;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeViewProtocolHandlers
            // 
            this.treeViewProtocolHandlers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewProtocolHandlers.Location = new System.Drawing.Point(0, 0);
            this.treeViewProtocolHandlers.Name = "treeViewProtocolHandlers";
            this.treeViewProtocolHandlers.ShowNodeToolTips = true;
            this.treeViewProtocolHandlers.Size = new System.Drawing.Size(229, 372);
            this.treeViewProtocolHandlers.TabIndex = 0;
            this.treeViewProtocolHandlers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewServices_AfterSelect);
            // 
            // propertyGridServiceInstall
            // 
            this.propertyGridServiceInstall.Location = new System.Drawing.Point(0, 0);
            this.propertyGridServiceInstall.Name = "propertyGridServiceInstall";
            this.propertyGridServiceInstall.Size = new System.Drawing.Size(130, 130);
            this.propertyGridServiceInstall.TabIndex = 0;
            // 
            // cmsComputer
            // 
            this.cmsComputer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewFolderToolStripMenuItem1,
            this.toolStripSeparator2});
            this.cmsComputer.Name = "cmsMergeRedirectFolder";
            this.cmsComputer.Size = new System.Drawing.Size(198, 32);
            // 
            // createNewFolderToolStripMenuItem1
            // 
            this.createNewFolderToolStripMenuItem1.Name = "createNewFolderToolStripMenuItem1";
            this.createNewFolderToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.createNewFolderToolStripMenuItem1.Size = new System.Drawing.Size(197, 22);
            this.createNewFolderToolStripMenuItem1.Text = "Create New Protocol Handler";
            this.createNewFolderToolStripMenuItem1.Click += new System.EventHandler(this.createNewFolderToolStripMenuItem1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(194, 6);
            // 
            // cmsService
            // 
            this.cmsService.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameFolderToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator1});
            this.cmsService.Name = "cmsDestinationTreeDefault";
            this.cmsService.Size = new System.Drawing.Size(182, 54);
            // 
            // renameFolderToolStripMenuItem
            // 
            this.renameFolderToolStripMenuItem.Name = "renameFolderToolStripMenuItem";
            this.renameFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.renameFolderToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.renameFolderToolStripMenuItem.Text = "Rename Protocol Handler";
            this.renameFolderToolStripMenuItem.Visible = false;
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.deleteToolStripMenuItem.Text = "Remove Protocol Handler";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(178, 6);
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
            // 
            // ProtocolHandlers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ProtocolHandlers";
            this.Size = new System.Drawing.Size(533, 372);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.cmsComputer.ResumeLayout(false);
            this.cmsService.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeViewProtocolHandlers;
        private System.Windows.Forms.PropertyGrid propertyGridServiceInstall;
        private System.Windows.Forms.ContextMenuStrip cmsComputer;
        private System.Windows.Forms.ToolStripMenuItem createNewFolderToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ContextMenuStrip cmsService;
        private System.Windows.Forms.ToolStripMenuItem renameFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ImageList iImageLibrary;
    }
}
