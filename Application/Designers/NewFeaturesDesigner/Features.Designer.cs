namespace NewFeaturesDesigner
{
    partial class Features
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Features));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewFeatures = new System.Windows.Forms.TreeView();
            this.contextMenuStripFeatures = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemNewFeature = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewSubFeature = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMoveLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMoveRight = new System.Windows.Forms.ToolStripMenuItem();
            this.imageLibrary = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.treeViewMerges = new System.Windows.Forms.TreeView();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.propertyGridFeatureProperties = new System.Windows.Forms.PropertyGrid();
            this.feature = new NewFeaturesDesigner.Feature(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStripFeatures.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.treeViewFeatures);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(516, 218);
            this.splitContainer1.SplitterDistance = 172;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeViewFeatures
            // 
            this.treeViewFeatures.ContextMenuStrip = this.contextMenuStripFeatures;
            this.treeViewFeatures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewFeatures.ImageIndex = 0;
            this.treeViewFeatures.ImageList = this.imageLibrary;
            this.treeViewFeatures.LabelEdit = true;
            this.treeViewFeatures.Location = new System.Drawing.Point(0, 0);
            this.treeViewFeatures.Name = "treeViewFeatures";
            this.treeViewFeatures.SelectedImageIndex = 0;
            this.treeViewFeatures.Size = new System.Drawing.Size(172, 218);
            this.treeViewFeatures.TabIndex = 2;
            this.treeViewFeatures.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewFeatures_AfterLabelEdit);
            this.treeViewFeatures.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewFeatures_AfterSelect);
            this.treeViewFeatures.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewFeatures_NodeMouseClick);
            // 
            // contextMenuStripFeatures
            // 
            this.contextMenuStripFeatures.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNewFeature,
            this.toolStripMenuItemNewSubFeature,
            this.toolStripSeparator1,
            this.toolStripMenuItemRename,
            this.toolStripMenuItemDelete,
            this.toolStripSeparator2,
            this.toolStripMenuItemMoveUp,
            this.toolStripMenuItemMoveDown,
            this.toolStripMenuItemMoveLeft,
            this.toolStripMenuItemMoveRight});
            this.contextMenuStripFeatures.Name = "contextMenuStripFeatures";
            this.contextMenuStripFeatures.Size = new System.Drawing.Size(164, 192);
            // 
            // toolStripMenuItemNewFeature
            // 
            this.toolStripMenuItemNewFeature.Name = "toolStripMenuItemNewFeature";
            this.toolStripMenuItemNewFeature.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemNewFeature.Text = "New Feature";
            this.toolStripMenuItemNewFeature.Click += new System.EventHandler(this.toolStripMenuItemNewFeature_Click);
            // 
            // toolStripMenuItemNewSubFeature
            // 
            this.toolStripMenuItemNewSubFeature.Name = "toolStripMenuItemNewSubFeature";
            this.toolStripMenuItemNewSubFeature.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemNewSubFeature.Text = "New Sub Feature";
            this.toolStripMenuItemNewSubFeature.Click += new System.EventHandler(this.toolStripMenuItemNewSubFeature_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(160, 6);
            // 
            // toolStripMenuItemRename
            // 
            this.toolStripMenuItemRename.Enabled = false;
            this.toolStripMenuItemRename.Name = "toolStripMenuItemRename";
            this.toolStripMenuItemRename.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemRename.Text = "Rename";
            this.toolStripMenuItemRename.Click += new System.EventHandler(this.toolStripMenuItemRename_Click);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Enabled = false;
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemDelete.Text = "Delete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(160, 6);
            // 
            // toolStripMenuItemMoveUp
            // 
            this.toolStripMenuItemMoveUp.Enabled = false;
            this.toolStripMenuItemMoveUp.Name = "toolStripMenuItemMoveUp";
            this.toolStripMenuItemMoveUp.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemMoveUp.Text = "Move Up";
            this.toolStripMenuItemMoveUp.Click += new System.EventHandler(this.toolStripMenuItemMoveUp_Click);
            // 
            // toolStripMenuItemMoveDown
            // 
            this.toolStripMenuItemMoveDown.Enabled = false;
            this.toolStripMenuItemMoveDown.Name = "toolStripMenuItemMoveDown";
            this.toolStripMenuItemMoveDown.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemMoveDown.Text = "Move Down";
            this.toolStripMenuItemMoveDown.Click += new System.EventHandler(this.toolStripMenuItemMoveDown_Click);
            // 
            // toolStripMenuItemMoveLeft
            // 
            this.toolStripMenuItemMoveLeft.Enabled = false;
            this.toolStripMenuItemMoveLeft.Name = "toolStripMenuItemMoveLeft";
            this.toolStripMenuItemMoveLeft.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemMoveLeft.Text = "Menu Left";
            this.toolStripMenuItemMoveLeft.Click += new System.EventHandler(this.toolStripMenuItemMoveLeft_Click);
            // 
            // toolStripMenuItemMoveRight
            // 
            this.toolStripMenuItemMoveRight.Enabled = false;
            this.toolStripMenuItemMoveRight.Name = "toolStripMenuItemMoveRight";
            this.toolStripMenuItemMoveRight.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItemMoveRight.Text = "Move Right";
            this.toolStripMenuItemMoveRight.Click += new System.EventHandler(this.toolStripMenuItemMoveRight_Click);
            // 
            // imageLibrary
            // 
            this.imageLibrary.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageLibrary.ImageStream")));
            this.imageLibrary.TransparentColor = System.Drawing.Color.Transparent;
            this.imageLibrary.Images.SetKeyName(0, "Features.ico");
            this.imageLibrary.Images.SetKeyName(1, "database.png");
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.propertyGridFeatureProperties);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(340, 218);
            this.splitContainer2.SplitterDistance = 113;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.treeViewMerges);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.textBox1);
            this.splitContainer3.Size = new System.Drawing.Size(340, 101);
            this.splitContainer3.SplitterDistance = 72;
            this.splitContainer3.TabIndex = 0;
            // 
            // treeViewMerges
            // 
            this.treeViewMerges.CausesValidation = false;
            this.treeViewMerges.CheckBoxes = true;
            this.treeViewMerges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewMerges.Enabled = false;
            this.treeViewMerges.ImageKey = "database.png";
            this.treeViewMerges.ImageList = this.imageLibrary;
            this.treeViewMerges.Location = new System.Drawing.Point(0, 0);
            this.treeViewMerges.Name = "treeViewMerges";
            this.treeViewMerges.SelectedImageKey = "database.png";
            this.treeViewMerges.ShowLines = false;
            this.treeViewMerges.ShowPlusMinus = false;
            this.treeViewMerges.ShowRootLines = false;
            this.treeViewMerges.Size = new System.Drawing.Size(338, 70);
            this.treeViewMerges.TabIndex = 1;
            this.treeViewMerges.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMerges_AfterCheck);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(338, 23);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "\t";
            // 
            // propertyGridFeatureProperties
            // 
            this.propertyGridFeatureProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridFeatureProperties.Enabled = false;
            this.propertyGridFeatureProperties.Location = new System.Drawing.Point(0, 0);
            this.propertyGridFeatureProperties.Name = "propertyGridFeatureProperties";
            this.propertyGridFeatureProperties.SelectedObject = this.feature;
            this.propertyGridFeatureProperties.Size = new System.Drawing.Size(340, 113);
            this.propertyGridFeatureProperties.TabIndex = 0;
            this.propertyGridFeatureProperties.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridFeatureProperties_PropertyValueChanged);
            // 
            // feature
            // 
            this.feature.AllowAbsent = null;
            this.feature.AllowAdvertise = null;
            this.feature.ConfigurableDirectory = null;
            this.feature.Description = null;
            this.feature.Display = null;
            this.feature.InstallDefault = null;
            this.feature.Level = null;
            this.feature.Title = null;
            this.feature.TypicalDefault = null;
            // 
            // Features
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "Features";
            this.Size = new System.Drawing.Size(516, 218);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStripFeatures.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.PropertyGrid propertyGridFeatureProperties;
        private System.Windows.Forms.TreeView treeViewFeatures;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFeatures;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewFeature;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewSubFeature;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRename;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMoveUp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMoveDown;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMoveLeft;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMoveRight;
        private System.Windows.Forms.ImageList imageLibrary;
        private Feature feature;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TreeView treeViewMerges;
        private System.Windows.Forms.TextBox textBox1;
    }
}
