namespace PropertiesDesigner
{
    partial class Properties
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
            this.dataGridViewProperties = new System.Windows.Forms.DataGridView();
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.adminDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.hiddenDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.secureDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.suppressModularizationDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.contextMenuStripProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataSetPropertiesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataSetProperties = new PropertiesDesigner.DataSetProperties();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProperties)).BeginInit();
            this.contextMenuStripProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetPropertiesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetProperties)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewProperties
            // 
            this.dataGridViewProperties.AllowUserToAddRows = false;
            this.dataGridViewProperties.AllowUserToDeleteRows = false;
            this.dataGridViewProperties.AllowUserToResizeColumns = false;
            this.dataGridViewProperties.AllowUserToResizeRows = false;
            this.dataGridViewProperties.AutoGenerateColumns = false;
            this.dataGridViewProperties.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idDataGridViewTextBoxColumn,
            this.valueDataGridViewTextBoxColumn,
            this.adminDataGridViewCheckBoxColumn,
            this.hiddenDataGridViewCheckBoxColumn,
            this.secureDataGridViewCheckBoxColumn,
            this.suppressModularizationDataGridViewCheckBoxColumn});
            this.dataGridViewProperties.ContextMenuStrip = this.contextMenuStripProperties;
            this.dataGridViewProperties.DataMember = "Property";
            this.dataGridViewProperties.DataSource = this.dataSetPropertiesBindingSource;
            this.dataGridViewProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewProperties.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewProperties.Name = "dataGridViewProperties";
            this.dataGridViewProperties.Size = new System.Drawing.Size(865, 367);
            this.dataGridViewProperties.TabIndex = 1;
            this.dataGridViewProperties.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridViewProperties_CellBeginEdit);
            this.dataGridViewProperties.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridViewProperties_CellValidating);
            this.dataGridViewProperties.SelectionChanged += new System.EventHandler(this.dataGridViewProperties_SelectionChanged);
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            this.idDataGridViewTextBoxColumn.HeaderText = "Id";
            this.idDataGridViewTextBoxColumn.MinimumWidth = 100;
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            // 
            // valueDataGridViewTextBoxColumn
            // 
            this.valueDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.valueDataGridViewTextBoxColumn.DataPropertyName = "Value";
            this.valueDataGridViewTextBoxColumn.HeaderText = "Value";
            this.valueDataGridViewTextBoxColumn.Name = "valueDataGridViewTextBoxColumn";
            // 
            // adminDataGridViewCheckBoxColumn
            // 
            this.adminDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.adminDataGridViewCheckBoxColumn.DataPropertyName = "Admin";
            this.adminDataGridViewCheckBoxColumn.HeaderText = "Admin";
            this.adminDataGridViewCheckBoxColumn.Name = "adminDataGridViewCheckBoxColumn";
            this.adminDataGridViewCheckBoxColumn.Width = 42;
            // 
            // hiddenDataGridViewCheckBoxColumn
            // 
            this.hiddenDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.hiddenDataGridViewCheckBoxColumn.DataPropertyName = "Hidden";
            this.hiddenDataGridViewCheckBoxColumn.HeaderText = "Hidden";
            this.hiddenDataGridViewCheckBoxColumn.Name = "hiddenDataGridViewCheckBoxColumn";
            this.hiddenDataGridViewCheckBoxColumn.Width = 47;
            // 
            // secureDataGridViewCheckBoxColumn
            // 
            this.secureDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.secureDataGridViewCheckBoxColumn.DataPropertyName = "Secure";
            this.secureDataGridViewCheckBoxColumn.HeaderText = "Secure";
            this.secureDataGridViewCheckBoxColumn.Name = "secureDataGridViewCheckBoxColumn";
            this.secureDataGridViewCheckBoxColumn.Width = 47;
            // 
            // suppressModularizationDataGridViewCheckBoxColumn
            // 
            this.suppressModularizationDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.suppressModularizationDataGridViewCheckBoxColumn.DataPropertyName = "SuppressModularization";
            this.suppressModularizationDataGridViewCheckBoxColumn.HeaderText = "SuppressModularization";
            this.suppressModularizationDataGridViewCheckBoxColumn.Name = "suppressModularizationDataGridViewCheckBoxColumn";
            this.suppressModularizationDataGridViewCheckBoxColumn.Width = 125;
            // 
            // contextMenuStripProperties
            // 
            this.contextMenuStripProperties.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.contextMenuStripProperties.Name = "contextMenuStripProperties";
            this.contextMenuStripProperties.Size = new System.Drawing.Size(118, 48);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.addToolStripMenuItem.Text = "&Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "&Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // dataSetPropertiesBindingSource
            // 
            this.dataSetPropertiesBindingSource.DataSource = this.dataSetProperties;
            this.dataSetPropertiesBindingSource.Position = 0;
            // 
            // dataSetProperties
            // 
            this.dataSetProperties.DataSetName = "DataSetProperties";
            this.dataSetProperties.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // Properties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridViewProperties);
            this.Name = "Properties";
            this.Size = new System.Drawing.Size(865, 367);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProperties)).EndInit();
            this.contextMenuStripProperties.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataSetPropertiesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetProperties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.BindingSource dataSetPropertiesBindingSource;
        private DataSetProperties dataSetProperties;
        private System.Windows.Forms.DataGridView dataGridViewProperties;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProperties;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn adminDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn hiddenDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn secureDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn suppressModularizationDataGridViewCheckBoxColumn;
    }
}
