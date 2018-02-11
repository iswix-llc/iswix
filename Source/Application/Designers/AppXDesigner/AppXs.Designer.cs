namespace AppXDesigner
{
    partial class AppXs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppXs));
            this.panelTop = new System.Windows.Forms.Panel();
            this.linkLabelRequirements = new System.Windows.Forms.LinkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.appX1 = new AppXDesigner.AppX(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.treeViewAppXs = new System.Windows.Forms.TreeView();
            this.contextMenuStripAppX = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemNewFeature = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.iImageLibrary = new System.Windows.Forms.ImageList(this.components);
            this.panelTop.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.contextMenuStripAppX.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.linkLabelRequirements);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(670, 30);
            this.panelTop.TabIndex = 0;
            // 
            // linkLabelRequirements
            // 
            this.linkLabelRequirements.AutoSize = true;
            this.linkLabelRequirements.Location = new System.Drawing.Point(3, 9);
            this.linkLabelRequirements.Name = "linkLabelRequirements";
            this.linkLabelRequirements.Size = new System.Drawing.Size(591, 13);
            this.linkLabelRequirements.TabIndex = 0;
            this.linkLabelRequirements.TabStop = true;
            this.linkLabelRequirements.Text = "The AppX designer requires Fire Giant WiX Expansion PackWiX3.10.300.13704  (or ne" +
    "wer).  Click here for more information.";
            this.linkLabelRequirements.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelRequirements_LinkClicked);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.propertyGrid1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(670, 273);
            this.panel1.TabIndex = 1;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Enabled = false;
            this.propertyGrid1.Location = new System.Drawing.Point(299, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid1.SelectedObject = this.appX1;
            this.propertyGrid1.Size = new System.Drawing.Size(371, 273);
            this.propertyGrid1.TabIndex = 1;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // appX1
            // 
            this.appX1.Description = null;
            this.appX1.DisplayName = null;
            this.appX1.LogoFile = null;
            this.appX1.MainPackage = null;
            this.appX1.Manufacturer = null;
            this.appX1.Publisher = null;
            this.appX1.Target = IsWiXAutomationInterface.TargetType.desktop;
            this.appX1.Version = null;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.treeViewAppXs);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(299, 273);
            this.panel2.TabIndex = 0;
            // 
            // treeViewAppXs
            // 
            this.treeViewAppXs.ContextMenuStrip = this.contextMenuStripAppX;
            this.treeViewAppXs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewAppXs.ImageIndex = 0;
            this.treeViewAppXs.ImageList = this.iImageLibrary;
            this.treeViewAppXs.LabelEdit = true;
            this.treeViewAppXs.Location = new System.Drawing.Point(0, 0);
            this.treeViewAppXs.Name = "treeViewAppXs";
            this.treeViewAppXs.SelectedImageIndex = 0;
            this.treeViewAppXs.ShowNodeToolTips = true;
            this.treeViewAppXs.Size = new System.Drawing.Size(299, 273);
            this.treeViewAppXs.TabIndex = 1;
            this.treeViewAppXs.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewAppXs_AfterLabelEdit);
            this.treeViewAppXs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAppXs_AfterSelect);
            // 
            // contextMenuStripAppX
            // 
            this.contextMenuStripAppX.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNewFeature,
            this.toolStripSeparator1,
            this.toolStripMenuItemRename,
            this.toolStripMenuItemDelete,
            this.toolStripSeparator2});
            this.contextMenuStripAppX.Name = "contextMenuStripFeatures";
            this.contextMenuStripAppX.Size = new System.Drawing.Size(118, 82);
            // 
            // toolStripMenuItemNewFeature
            // 
            this.toolStripMenuItemNewFeature.Name = "toolStripMenuItemNewFeature";
            this.toolStripMenuItemNewFeature.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItemNewFeature.Text = "New ";
            this.toolStripMenuItemNewFeature.Click += new System.EventHandler(this.toolStripMenuItemNewFeature_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(114, 6);
            // 
            // toolStripMenuItemRename
            // 
            this.toolStripMenuItemRename.Enabled = false;
            this.toolStripMenuItemRename.Name = "toolStripMenuItemRename";
            this.toolStripMenuItemRename.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItemRename.Text = "Rename";
            this.toolStripMenuItemRename.Click += new System.EventHandler(this.toolStripMenuItemRename_Click);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Enabled = false;
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItemDelete.Text = "Delete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(114, 6);
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
            this.iImageLibrary.Images.SetKeyName(7, "AppX.ico");
            this.iImageLibrary.Images.SetKeyName(8, "smXML.gif");
            this.iImageLibrary.Images.SetKeyName(9, "blue_folder_closed");
            this.iImageLibrary.Images.SetKeyName(10, "delete_16x16.gif");
            this.iImageLibrary.Images.SetKeyName(11, "blue_folder_open.PNG");
            // 
            // AppXs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelTop);
            this.Name = "AppXs";
            this.Size = new System.Drawing.Size(670, 303);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.contextMenuStripAppX.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.LinkLabel linkLabelRequirements;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TreeView treeViewAppXs;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private AppX appX1;
        private System.Windows.Forms.ImageList iImageLibrary;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAppX;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewFeature;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRename;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}
