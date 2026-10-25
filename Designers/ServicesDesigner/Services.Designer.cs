namespace ServicesDesigner
{
    partial class Services
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Services));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewServices = new System.Windows.Forms.TreeView();
            this.iImageLibrary = new System.Windows.Forms.ImageList(this.components);
            this.propertyGridServiceInstall = new System.Windows.Forms.PropertyGrid();
            this.serviceInstall = new ServicesDesigner.Service(this.components);
            this.cmsComputer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createNewFolderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmsService = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.renameFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
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
            this.splitContainer1.Panel1.Controls.Add(this.treeViewServices);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGridServiceInstall);
            this.splitContainer1.Size = new System.Drawing.Size(533, 372);
            this.splitContainer1.SplitterDistance = 229;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeViewServices
            // 
            this.treeViewServices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewServices.ImageIndex = 0;
            this.treeViewServices.ImageList = this.iImageLibrary;
            this.treeViewServices.Location = new System.Drawing.Point(0, 0);
            this.treeViewServices.Name = "treeViewServices";
            this.treeViewServices.SelectedImageIndex = 0;
            this.treeViewServices.ShowNodeToolTips = true;
            this.treeViewServices.Size = new System.Drawing.Size(229, 372);
            this.treeViewServices.TabIndex = 0;
            this.treeViewServices.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewServices_BeforeCollapse);
            this.treeViewServices.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewServices_AfterSelect);
            // 
            // iImageLibrary
            // 
            this.iImageLibrary.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // propertyGridServiceInstall
            // 
            this.propertyGridServiceInstall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridServiceInstall.Location = new System.Drawing.Point(0, 0);
            this.propertyGridServiceInstall.Name = "propertyGridServiceInstall";
            this.propertyGridServiceInstall.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGridServiceInstall.SelectedObject = this.serviceInstall;
            this.propertyGridServiceInstall.Size = new System.Drawing.Size(300, 372);
            this.propertyGridServiceInstall.TabIndex = 0;
            this.propertyGridServiceInstall.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridServiceInstall_PropertyValueChanged);
            // 
            // serviceInstall
            // 
            this.serviceInstall.Account = null;
            this.serviceInstall.Arguments = null;
            this.serviceInstall.Description = null;
            this.serviceInstall.DisplayName = null;
            this.serviceInstall.EraseDescription = null;
            this.serviceInstall.ErrorControl = null;
            this.serviceInstall.Interactive = null;
            this.serviceInstall.LoadOrderGroup = null;
            this.serviceInstall.Name = null;
            this.serviceInstall.Password = null;
            this.serviceInstall.Remove = null;
            this.serviceInstall.Start = null;
            this.serviceInstall.Startup = null;
            this.serviceInstall.Stop = null;
            this.serviceInstall.Type = null;
            this.serviceInstall.Vital = null;
            this.serviceInstall.Wait = null;
            // 
            // cmsComputer
            // 
            this.cmsComputer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewFolderToolStripMenuItem1});
            this.cmsComputer.Name = "cmsMergeRedirectFolder";
            this.cmsComputer.Size = new System.Drawing.Size(198, 32);
            // 
            // createNewFolderToolStripMenuItem1
            // 
            this.createNewFolderToolStripMenuItem1.Name = "createNewFolderToolStripMenuItem1";
            this.createNewFolderToolStripMenuItem1.Size = new System.Drawing.Size(197, 22);
            this.createNewFolderToolStripMenuItem1.Text = "New Service";
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
            this.deleteToolStripMenuItem});
            this.cmsService.Name = "cmsDestinationTreeDefault";
            this.cmsService.Size = new System.Drawing.Size(182, 54);
            // 
            // renameFolderToolStripMenuItem
            // 
            this.renameFolderToolStripMenuItem.Name = "renameFolderToolStripMenuItem";
            this.renameFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.renameFolderToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.renameFolderToolStripMenuItem.Text = "Rename Service";
            this.renameFolderToolStripMenuItem.Visible = false;
            this.renameFolderToolStripMenuItem.Click += new System.EventHandler(this.renameFolderToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(178, 6);
            // 
            // Services
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "Services";
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
        private System.Windows.Forms.TreeView treeViewServices;
        private System.Windows.Forms.PropertyGrid propertyGridServiceInstall;
        private System.Windows.Forms.ImageList iImageLibrary;
        private System.Windows.Forms.ContextMenuStrip cmsComputer;
        private System.Windows.Forms.ToolStripMenuItem createNewFolderToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ContextMenuStrip cmsService;
        private System.Windows.Forms.ToolStripMenuItem renameFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Service serviceInstall;
    }
}
