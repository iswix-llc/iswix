namespace CustomTablesDesigner
{
    partial class CustomTablesDesigner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomTablesDesigner));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listBoxTables = new System.Windows.Forms.ListBox();
            this.contextMenuStripTables = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemTableCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRenameTable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemTableDrop = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxColumnSchema = new System.Windows.Forms.GroupBox();
            this.checkBoxNullable = new System.Windows.Forms.CheckBox();
            this.textBoxLength = new System.Windows.Forms.TextBox();
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelLength = new System.Windows.Forms.Label();
            this.labelType = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.listViewColumns = new System.Windows.Forms.ListView();
            this.contextMenuStripColumns = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertAboveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertBelowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListIcons = new System.Windows.Forms.ImageList(this.components);
            this.labelColumns = new System.Windows.Forms.Label();
            this.dataGridViewRows = new System.Windows.Forms.DataGridView();
            this.contextMenuStripRows = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemRowAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRowRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenuStripTables.SuspendLayout();
            this.groupBoxColumnSchema.SuspendLayout();
            this.contextMenuStripColumns.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRows)).BeginInit();
            this.contextMenuStripRows.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewRows);
            this.splitContainer1.Size = new System.Drawing.Size(687, 340);
            this.splitContainer1.SplitterDistance = 331;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.listBoxTables);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBoxColumnSchema);
            this.splitContainer2.Panel2.Controls.Add(this.listViewColumns);
            this.splitContainer2.Panel2.Controls.Add(this.labelColumns);
            this.splitContainer2.Size = new System.Drawing.Size(331, 340);
            this.splitContainer2.SplitterDistance = 125;
            this.splitContainer2.TabIndex = 1;
            // 
            // listBoxTables
            // 
            this.listBoxTables.ContextMenuStrip = this.contextMenuStripTables;
            this.listBoxTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxTables.FormattingEnabled = true;
            this.listBoxTables.Location = new System.Drawing.Point(0, 0);
            this.listBoxTables.Name = "listBoxTables";
            this.listBoxTables.Size = new System.Drawing.Size(331, 121);
            this.listBoxTables.Sorted = true;
            this.listBoxTables.TabIndex = 1;
            this.listBoxTables.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // contextMenuStripTables
            // 
            this.contextMenuStripTables.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemTableCreate,
            this.toolStripMenuItemRenameTable,
            this.toolStripMenuItemTableDrop});
            this.contextMenuStripTables.Name = "contextMenuStripTables";
            this.contextMenuStripTables.Size = new System.Drawing.Size(153, 92);
            // 
            // toolStripMenuItemTableCreate
            // 
            this.toolStripMenuItemTableCreate.Name = "toolStripMenuItemTableCreate";
            this.toolStripMenuItemTableCreate.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItemTableCreate.Text = "Create";
            this.toolStripMenuItemTableCreate.Click += new System.EventHandler(this.toolStripMenuItemTableCreate_Click);
            // 
            // toolStripMenuItemRenameTable
            // 
            this.toolStripMenuItemRenameTable.Enabled = false;
            this.toolStripMenuItemRenameTable.Name = "toolStripMenuItemRenameTable";
            this.toolStripMenuItemRenameTable.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItemRenameTable.Text = "Rename";
            this.toolStripMenuItemRenameTable.Click += new System.EventHandler(this.toolStripMenuItemRenameTable_Click);
            // 
            // toolStripMenuItemTableDrop
            // 
            this.toolStripMenuItemTableDrop.Enabled = false;
            this.toolStripMenuItemTableDrop.Name = "toolStripMenuItemTableDrop";
            this.toolStripMenuItemTableDrop.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItemTableDrop.Text = "Drop";
            this.toolStripMenuItemTableDrop.Click += new System.EventHandler(this.toolStripMenuItemTableDrop_Click);
            // 
            // groupBoxColumnSchema
            // 
            this.groupBoxColumnSchema.Controls.Add(this.checkBoxNullable);
            this.groupBoxColumnSchema.Controls.Add(this.textBoxLength);
            this.groupBoxColumnSchema.Controls.Add(this.comboBoxType);
            this.groupBoxColumnSchema.Controls.Add(this.textBoxName);
            this.groupBoxColumnSchema.Controls.Add(this.labelLength);
            this.groupBoxColumnSchema.Controls.Add(this.labelType);
            this.groupBoxColumnSchema.Controls.Add(this.labelName);
            this.groupBoxColumnSchema.Location = new System.Drawing.Point(138, 9);
            this.groupBoxColumnSchema.Name = "groupBoxColumnSchema";
            this.groupBoxColumnSchema.Size = new System.Drawing.Size(190, 189);
            this.groupBoxColumnSchema.TabIndex = 23;
            this.groupBoxColumnSchema.TabStop = false;
            this.groupBoxColumnSchema.Text = "Column Schema: (Not Completed)";
            // 
            // checkBoxNullable
            // 
            this.checkBoxNullable.AutoSize = true;
            this.checkBoxNullable.Enabled = false;
            this.checkBoxNullable.Location = new System.Drawing.Point(83, 147);
            this.checkBoxNullable.Name = "checkBoxNullable";
            this.checkBoxNullable.Size = new System.Drawing.Size(64, 17);
            this.checkBoxNullable.TabIndex = 6;
            this.checkBoxNullable.Text = "Nullable";
            this.checkBoxNullable.UseVisualStyleBackColor = true;
            // 
            // textBoxLength
            // 
            this.textBoxLength.Enabled = false;
            this.textBoxLength.Location = new System.Drawing.Point(70, 109);
            this.textBoxLength.Name = "textBoxLength";
            this.textBoxLength.Size = new System.Drawing.Size(100, 20);
            this.textBoxLength.TabIndex = 5;
            // 
            // comboBoxType
            // 
            this.comboBoxType.Enabled = false;
            this.comboBoxType.FormattingEnabled = true;
            this.comboBoxType.Items.AddRange(new object[] {
            "binary",
            "int",
            "string"});
            this.comboBoxType.Location = new System.Drawing.Point(70, 67);
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(100, 21);
            this.comboBoxType.TabIndex = 4;
            // 
            // textBoxName
            // 
            this.textBoxName.Enabled = false;
            this.textBoxName.Location = new System.Drawing.Point(70, 27);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(100, 20);
            this.textBoxName.TabIndex = 3;
            // 
            // labelLength
            // 
            this.labelLength.AutoSize = true;
            this.labelLength.Location = new System.Drawing.Point(14, 112);
            this.labelLength.Name = "labelLength";
            this.labelLength.Size = new System.Drawing.Size(43, 13);
            this.labelLength.TabIndex = 2;
            this.labelLength.Text = "Length:";
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.Location = new System.Drawing.Point(14, 70);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(34, 13);
            this.labelType.TabIndex = 1;
            this.labelType.Text = "Type:";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(14, 30);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(38, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name:";
            // 
            // listViewColumns
            // 
            this.listViewColumns.ContextMenuStrip = this.contextMenuStripColumns;
            this.listViewColumns.FullRowSelect = true;
            this.listViewColumns.Location = new System.Drawing.Point(3, 25);
            this.listViewColumns.MultiSelect = false;
            this.listViewColumns.Name = "listViewColumns";
            this.listViewColumns.Size = new System.Drawing.Size(130, 173);
            this.listViewColumns.SmallImageList = this.imageListIcons;
            this.listViewColumns.TabIndex = 24;
            this.listViewColumns.UseCompatibleStateImageBehavior = false;
            this.listViewColumns.View = System.Windows.Forms.View.List;
            this.listViewColumns.SelectedIndexChanged += new System.EventHandler(this.listViewColumns_SelectedIndexChanged);
            // 
            // contextMenuStripColumns
            // 
            this.contextMenuStripColumns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setKeyToolStripMenuItem,
            this.clearKeyToolStripMenuItem,
            this.insertAboveToolStripMenuItem,
            this.insertBelowToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem});
            this.contextMenuStripColumns.Name = "contextMenuStripColumns";
            this.contextMenuStripColumns.Size = new System.Drawing.Size(141, 180);
            // 
            // setKeyToolStripMenuItem
            // 
            this.setKeyToolStripMenuItem.Enabled = false;
            this.setKeyToolStripMenuItem.Name = "setKeyToolStripMenuItem";
            this.setKeyToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.setKeyToolStripMenuItem.Text = "Set Key";
            // 
            // clearKeyToolStripMenuItem
            // 
            this.clearKeyToolStripMenuItem.Enabled = false;
            this.clearKeyToolStripMenuItem.Name = "clearKeyToolStripMenuItem";
            this.clearKeyToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.clearKeyToolStripMenuItem.Text = "Clear Key";
            // 
            // insertAboveToolStripMenuItem
            // 
            this.insertAboveToolStripMenuItem.Enabled = false;
            this.insertAboveToolStripMenuItem.Name = "insertAboveToolStripMenuItem";
            this.insertAboveToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.insertAboveToolStripMenuItem.Text = "Insert Above";
            // 
            // insertBelowToolStripMenuItem
            // 
            this.insertBelowToolStripMenuItem.Enabled = false;
            this.insertBelowToolStripMenuItem.Name = "insertBelowToolStripMenuItem";
            this.insertBelowToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.insertBelowToolStripMenuItem.Text = "Insert Below";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Enabled = false;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Enabled = false;
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Enabled = false;
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.moveUpToolStripMenuItem.Text = "Move Up";
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Enabled = false;
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.moveDownToolStripMenuItem.Text = "Move Down";
            // 
            // imageListIcons
            // 
            this.imageListIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIcons.ImageStream")));
            this.imageListIcons.TransparentColor = System.Drawing.Color.Teal;
            this.imageListIcons.Images.SetKeyName(0, "PrimaryKey");
            // 
            // labelColumns
            // 
            this.labelColumns.AutoSize = true;
            this.labelColumns.Location = new System.Drawing.Point(2, 9);
            this.labelColumns.Name = "labelColumns";
            this.labelColumns.Size = new System.Drawing.Size(129, 13);
            this.labelColumns.TabIndex = 18;
            this.labelColumns.Text = "Columns: (Not Completed)";
            // 
            // dataGridViewRows
            // 
            this.dataGridViewRows.AllowUserToAddRows = false;
            this.dataGridViewRows.AllowUserToDeleteRows = false;
            this.dataGridViewRows.AllowUserToResizeRows = false;
            this.dataGridViewRows.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewRows.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewRows.ContextMenuStrip = this.contextMenuStripRows;
            this.dataGridViewRows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewRows.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewRows.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewRows.Name = "dataGridViewRows";
            this.dataGridViewRows.Size = new System.Drawing.Size(352, 340);
            this.dataGridViewRows.TabIndex = 0;
            this.dataGridViewRows.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewRows_DataError);
            this.dataGridViewRows.SelectionChanged += new System.EventHandler(this.dataGridViewRows_SelectionChanged);
            // 
            // contextMenuStripRows
            // 
            this.contextMenuStripRows.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRowAdd,
            this.toolStripMenuItemRowRemove});
            this.contextMenuStripRows.Name = "contextMenuStripRows";
            this.contextMenuStripRows.Size = new System.Drawing.Size(118, 48);
            // 
            // toolStripMenuItemRowAdd
            // 
            this.toolStripMenuItemRowAdd.Name = "toolStripMenuItemRowAdd";
            this.toolStripMenuItemRowAdd.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItemRowAdd.Text = "Add";
            this.toolStripMenuItemRowAdd.Click += new System.EventHandler(this.toolStripMenuItemRowAdd_Click);
            // 
            // toolStripMenuItemRowRemove
            // 
            this.toolStripMenuItemRowRemove.Name = "toolStripMenuItemRowRemove";
            this.toolStripMenuItemRowRemove.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItemRowRemove.Text = "Remove";
            this.toolStripMenuItemRowRemove.Click += new System.EventHandler(this.toolStripMenuItemRowRemove_Click);
            // 
            // CustomTablesDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "CustomTablesDesigner";
            this.Size = new System.Drawing.Size(687, 340);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.contextMenuStripTables.ResumeLayout(false);
            this.groupBoxColumnSchema.ResumeLayout(false);
            this.groupBoxColumnSchema.PerformLayout();
            this.contextMenuStripColumns.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRows)).EndInit();
            this.contextMenuStripRows.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridViewRows;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTables;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTableCreate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTableDrop;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRows;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRowAdd;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRowRemove;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox listBoxTables;
        private System.Windows.Forms.GroupBox groupBoxColumnSchema;
        private System.Windows.Forms.CheckBox checkBoxNullable;
        private System.Windows.Forms.TextBox textBoxLength;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelLength;
        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.ListView listViewColumns;
        private System.Windows.Forms.ImageList imageListIcons;
        private System.Windows.Forms.Label labelColumns;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripColumns;
        private System.Windows.Forms.ToolStripMenuItem setKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertAboveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertBelowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRenameTable;
    }
}
