using NamespacesDesigner;

namespace CustomTablesDesigner
{
    partial class Namespaces
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
            this.dataGridViewNamespaces = new System.Windows.Forms.DataGridView();
            this.selectedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.xmlnsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.uriDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataTableNamespacesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataSetNamespaces = new DataSetNamespaces();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewNamespaces)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTableNamespacesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetNamespaces)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewNamespaces
            // 
            this.dataGridViewNamespaces.AllowUserToAddRows = false;
            this.dataGridViewNamespaces.AllowUserToDeleteRows = false;
            this.dataGridViewNamespaces.AllowUserToResizeColumns = false;
            this.dataGridViewNamespaces.AllowUserToResizeRows = false;
            this.dataGridViewNamespaces.AutoGenerateColumns = false;
            this.dataGridViewNamespaces.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewNamespaces.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.selectedDataGridViewCheckBoxColumn,
            this.xmlnsDataGridViewTextBoxColumn,
            this.uriDataGridViewTextBoxColumn});
            this.dataGridViewNamespaces.DataSource = this.dataTableNamespacesBindingSource;
            this.dataGridViewNamespaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewNamespaces.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewNamespaces.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewNamespaces.Name = "dataGridViewNamespaces";
            this.dataGridViewNamespaces.Size = new System.Drawing.Size(471, 240);
            this.dataGridViewNamespaces.TabIndex = 0;
            // 
            // selectedDataGridViewCheckBoxColumn
            // 
            this.selectedDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.selectedDataGridViewCheckBoxColumn.DataPropertyName = "selected";
            this.selectedDataGridViewCheckBoxColumn.HeaderText = "Selected";
            this.selectedDataGridViewCheckBoxColumn.Name = "selectedDataGridViewCheckBoxColumn";
            this.selectedDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.selectedDataGridViewCheckBoxColumn.Width = 55;
            // 
            // xmlnsDataGridViewTextBoxColumn
            // 
            this.xmlnsDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.xmlnsDataGridViewTextBoxColumn.DataPropertyName = "xmlns";
            this.xmlnsDataGridViewTextBoxColumn.FillWeight = 33F;
            this.xmlnsDataGridViewTextBoxColumn.HeaderText = "Extension Name";
            this.xmlnsDataGridViewTextBoxColumn.Name = "xmlnsDataGridViewTextBoxColumn";
            this.xmlnsDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // uriDataGridViewTextBoxColumn
            // 
            this.uriDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.uriDataGridViewTextBoxColumn.DataPropertyName = "uri";
            this.uriDataGridViewTextBoxColumn.FillWeight = 67F;
            this.uriDataGridViewTextBoxColumn.HeaderText = "Extension URI";
            this.uriDataGridViewTextBoxColumn.Name = "uriDataGridViewTextBoxColumn";
            this.uriDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dataTableNamespacesBindingSource
            // 
            this.dataTableNamespacesBindingSource.DataMember = "DataTableNamespaces";
            this.dataTableNamespacesBindingSource.DataSource = this.dataSetNamespaces;
            // 
            // dataSetNamespaces
            // 
            this.dataSetNamespaces.DataSetName = "DataSetNamespaces";
            this.dataSetNamespaces.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // Namespaces
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridViewNamespaces);
            this.Name = "Namespaces";
            this.Size = new System.Drawing.Size(471, 240);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewNamespaces)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTableNamespacesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetNamespaces)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewNamespaces;
        private System.Windows.Forms.BindingSource dataTableNamespacesBindingSource;
        private DataSetNamespaces dataSetNamespaces;
        private System.Windows.Forms.DataGridViewCheckBoxColumn selectedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn xmlnsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn uriDataGridViewTextBoxColumn;

    }
}
