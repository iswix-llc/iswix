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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MSIXs));
            panelTop = new System.Windows.Forms.Panel();
            linkLabelRequirements = new System.Windows.Forms.LinkLabel();
            panel1 = new System.Windows.Forms.Panel();
            propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            panel2 = new System.Windows.Forms.Panel();
            treeViewMSIXs = new System.Windows.Forms.TreeView();
            contextMenuStripMSIX = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItemNewFeature = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripMenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            iImageLibrary = new System.Windows.Forms.ImageList(components);
            panelTop.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            contextMenuStripMSIX.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.Controls.Add(linkLabelRequirements);
            panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelTop.Location = new System.Drawing.Point(0, 0);
            panelTop.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            panelTop.Name = "panelTop";
            panelTop.Size = new System.Drawing.Size(1116, 58);
            panelTop.TabIndex = 0;
            // 
            // linkLabelRequirements
            // 
            linkLabelRequirements.AutoSize = true;
            linkLabelRequirements.Dock = System.Windows.Forms.DockStyle.Bottom;
            linkLabelRequirements.Location = new System.Drawing.Point(0, 33);
            linkLabelRequirements.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            linkLabelRequirements.Name = "linkLabelRequirements";
            linkLabelRequirements.Size = new System.Drawing.Size(760, 25);
            linkLabelRequirements.TabIndex = 0;
            linkLabelRequirements.TabStop = true;
            linkLabelRequirements.Text = "The MSIX designer requires the Fire Giant WiX Expansion Pack. Click here for more information.";
            linkLabelRequirements.LinkClicked += linkLabelRequirements_LinkClicked;
            // 
            // panel1
            // 
            panel1.Controls.Add(propertyGrid1);
            panel1.Controls.Add(panel2);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 58);
            panel1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1116, 525);
            panel1.TabIndex = 1;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            propertyGrid1.Enabled = false;
            propertyGrid1.Location = new System.Drawing.Point(499, 0);
            propertyGrid1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propertyGrid1.Size = new System.Drawing.Size(617, 525);
            propertyGrid1.TabIndex = 1;
            propertyGrid1.PropertyValueChanged += propertyGrid1_PropertyValueChanged;
            // 
            // panel2
            // 
            panel2.Controls.Add(treeViewMSIXs);
            panel2.Dock = System.Windows.Forms.DockStyle.Left;
            panel2.Location = new System.Drawing.Point(0, 0);
            panel2.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(499, 525);
            panel2.TabIndex = 0;
            // 
            // treeViewMSIXs
            // 
            treeViewMSIXs.ContextMenuStrip = contextMenuStripMSIX;
            treeViewMSIXs.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewMSIXs.ImageIndex = 0;
            treeViewMSIXs.ImageList = iImageLibrary;
            treeViewMSIXs.LabelEdit = true;
            treeViewMSIXs.Location = new System.Drawing.Point(0, 0);
            treeViewMSIXs.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            treeViewMSIXs.Name = "treeViewMSIXs";
            treeViewMSIXs.SelectedImageIndex = 0;
            treeViewMSIXs.ShowNodeToolTips = true;
            treeViewMSIXs.Size = new System.Drawing.Size(499, 525);
            treeViewMSIXs.TabIndex = 1;
            treeViewMSIXs.AfterLabelEdit += treeViewMSIXs_AfterLabelEdit;
            treeViewMSIXs.AfterSelect += treeViewMSIXs_AfterSelect;
            // 
            // contextMenuStripMSIX
            // 
            contextMenuStripMSIX.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStripMSIX.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItemNewFeature, toolStripSeparator1, toolStripMenuItemRename, toolStripMenuItemDelete, toolStripSeparator2 });
            contextMenuStripMSIX.Name = "contextMenuStripFeatures";
            contextMenuStripMSIX.Size = new System.Drawing.Size(148, 112);
            // 
            // toolStripMenuItemNewFeature
            // 
            toolStripMenuItemNewFeature.Name = "toolStripMenuItemNewFeature";
            toolStripMenuItemNewFeature.Size = new System.Drawing.Size(147, 32);
            toolStripMenuItemNewFeature.Text = "New ";
            toolStripMenuItemNewFeature.Click += toolStripMenuItemNewFeature_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(144, 6);
            // 
            // toolStripMenuItemRename
            // 
            toolStripMenuItemRename.Enabled = false;
            toolStripMenuItemRename.Name = "toolStripMenuItemRename";
            toolStripMenuItemRename.Size = new System.Drawing.Size(147, 32);
            toolStripMenuItemRename.Text = "Rename";
            toolStripMenuItemRename.Click += toolStripMenuItemRename_Click;
            // 
            // toolStripMenuItemDelete
            // 
            toolStripMenuItemDelete.Enabled = false;
            toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            toolStripMenuItemDelete.Size = new System.Drawing.Size(147, 32);
            toolStripMenuItemDelete.Text = "Delete";
            toolStripMenuItemDelete.Click += toolStripMenuItemDelete_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(144, 6);
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
            iImageLibrary.Images.SetKeyName(7, "MSIX.ico");
            iImageLibrary.Images.SetKeyName(8, "smXML.gif");
            iImageLibrary.Images.SetKeyName(9, "blue_folder_closed");
            iImageLibrary.Images.SetKeyName(10, "delete_16x16.gif");
            iImageLibrary.Images.SetKeyName(11, "blue_folder_open.PNG");
            // 
            // MSIXs
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(panelTop);
            Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            Name = "MSIXs";
            Size = new System.Drawing.Size(1116, 583);
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            contextMenuStripMSIX.ResumeLayout(false);
            ResumeLayout(false);
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
