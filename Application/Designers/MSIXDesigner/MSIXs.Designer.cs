namespace MSIXDesigner
{
    partial class MSIXs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MSIXs));
            this.panelTop = new System.Windows.Forms.Panel();
            this.linkLabelRequirements = new System.Windows.Forms.LinkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.treeViewMSIXs = new System.Windows.Forms.TreeView();
            this.contextMenuStripMSIX = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemNewFeature = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.iImageLibrary = new System.Windows.Forms.ImageList(this.components);
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.msix1 = new MSIXDesigner.MSIX(this.components);
            this.panelTop.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.contextMenuStripMSIX.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.linkLabelRequirements);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(893, 37);
            this.panelTop.TabIndex = 0;
            // 
            // linkLabelRequirements
            // 
            this.linkLabelRequirements.AutoSize = true;
            this.linkLabelRequirements.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkLabelRequirements.Location = new System.Drawing.Point(0, 20);
            this.linkLabelRequirements.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelRequirements.Name = "linkLabelRequirements";
            this.linkLabelRequirements.Size = new System.Drawing.Size(612, 17);
            this.linkLabelRequirements.TabIndex = 0;
            this.linkLabelRequirements.TabStop = true;
            this.linkLabelRequirements.Text = "The MSIX designer requires the Fire Giant WiX Expansion Pack. Click here for more" +
    " information.";
            this.linkLabelRequirements.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRequirements_LinkClicked);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.propertyGrid1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 37);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(893, 336);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.treeViewMSIXs);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(399, 336);
            this.panel2.TabIndex = 0;
            // 
            // treeViewMSIXs
            // 
            this.treeViewMSIXs.ContextMenuStrip = this.contextMenuStripMSIX;
            this.treeViewMSIXs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewMSIXs.ImageIndex = 0;
            this.treeViewMSIXs.ImageList = this.iImageLibrary;
            this.treeViewMSIXs.LabelEdit = true;
            this.treeViewMSIXs.Location = new System.Drawing.Point(0, 0);
            this.treeViewMSIXs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.treeViewMSIXs.Name = "treeViewMSIXs";
            this.treeViewMSIXs.SelectedImageIndex = 0;
            this.treeViewMSIXs.ShowNodeToolTips = true;
            this.treeViewMSIXs.Size = new System.Drawing.Size(399, 336);
            this.treeViewMSIXs.TabIndex = 1;
            this.treeViewMSIXs.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewMSIXs_AfterLabelEdit);
            this.treeViewMSIXs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMSIXs_AfterSelect);
            // 
            // contextMenuStripMSIX
            // 
            this.contextMenuStripMSIX.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripMSIX.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNewFeature,
            this.toolStripSeparator1,
            this.toolStripMenuItemRename,
            this.toolStripMenuItemDelete,
            this.toolStripSeparator2});
            this.contextMenuStripMSIX.Name = "contextMenuStripFeatures";
            this.contextMenuStripMSIX.Size = new System.Drawing.Size(133, 88);
            // 
            // toolStripMenuItemNewFeature
            // 
            this.toolStripMenuItemNewFeature.Name = "toolStripMenuItemNewFeature";
            this.toolStripMenuItemNewFeature.Size = new System.Drawing.Size(132, 24);
            this.toolStripMenuItemNewFeature.Text = "New ";
            this.toolStripMenuItemNewFeature.Click += new System.EventHandler(this.toolStripMenuItemNewFeature_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(129, 6);
            // 
            // toolStripMenuItemRename
            // 
            this.toolStripMenuItemRename.Enabled = false;
            this.toolStripMenuItemRename.Name = "toolStripMenuItemRename";
            this.toolStripMenuItemRename.Size = new System.Drawing.Size(132, 24);
            this.toolStripMenuItemRename.Text = "Rename";
            this.toolStripMenuItemRename.Click += new System.EventHandler(this.toolStripMenuItemRename_Click);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Enabled = false;
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(132, 24);
            this.toolStripMenuItemDelete.Text = "Delete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(129, 6);
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
            this.iImageLibrary.Images.SetKeyName(7, "MSIX.ico");
            this.iImageLibrary.Images.SetKeyName(8, "smXML.gif");
            this.iImageLibrary.Images.SetKeyName(9, "blue_folder_closed");
            this.iImageLibrary.Images.SetKeyName(10, "delete_16x16.gif");
            this.iImageLibrary.Images.SetKeyName(11, "blue_folder_open.PNG");
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Enabled = false;
            this.propertyGrid1.Location = new System.Drawing.Point(399, 0);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid1.SelectedObject = this.msix1;
            this.propertyGrid1.Size = new System.Drawing.Size(494, 336);
            this.propertyGrid1.TabIndex = 1;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // msix1
            // 
            this.msix1.Capabilities = null;
            this.msix1.Description = null;
            this.msix1.DisplayName = null;
            this.msix1.LogoFile = null;
            this.msix1.MainPackage = null;
            this.msix1.Manufacturer = null;
            this.msix1.MaximumTestedOS = null;
            this.msix1.MinimumSupportedOS = null;
            this.msix1.Publisher = null;
            this.msix1.Target = IsWiXAutomationInterface.TargetType.desktop;
            this.msix1.Version = null;
            // 
            // MSIXs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelTop);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MSIXs";
            this.Size = new System.Drawing.Size(893, 373);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.contextMenuStripMSIX.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.LinkLabel linkLabelRequirements;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TreeView treeViewMSIXs;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private MSIX msix1;
        private System.Windows.Forms.ImageList iImageLibrary;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMSIX;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewFeature;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRename;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}
