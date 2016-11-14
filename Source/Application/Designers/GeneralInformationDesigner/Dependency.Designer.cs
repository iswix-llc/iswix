namespace GeneralInformationDesigner
{
    partial class Dependency
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
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dataGridViewDependencies = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelModuleDependencies = new System.Windows.Forms.Label();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.dependencies = new WixShield.Designers.GeneralInformation.Dependencies();
            this.dependenciesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.requiredIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.requiredLanguageDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.requiredVersionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDependencies)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dependencies)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dependenciesBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.panel2);
            this.splitContainer3.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.buttonRemove);
            this.splitContainer3.Panel2.Controls.Add(this.buttonAdd);
            this.splitContainer3.Size = new System.Drawing.Size(726, 271);
            this.splitContainer3.SplitterDistance = 678;
            this.splitContainer3.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dataGridViewDependencies);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 26);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(678, 245);
            this.panel2.TabIndex = 3;
            // 
            // dataGridViewDependencies
            // 
            this.dataGridViewDependencies.AllowUserToAddRows = false;
            this.dataGridViewDependencies.AllowUserToDeleteRows = false;
            this.dataGridViewDependencies.AllowUserToOrderColumns = true;
            this.dataGridViewDependencies.AllowUserToResizeColumns = false;
            this.dataGridViewDependencies.AllowUserToResizeRows = false;
            this.dataGridViewDependencies.AutoGenerateColumns = false;
            this.dataGridViewDependencies.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDependencies.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.requiredIdDataGridViewTextBoxColumn,
            this.requiredLanguageDataGridViewTextBoxColumn,
            this.requiredVersionDataGridViewTextBoxColumn});
            this.dataGridViewDependencies.DataSource = this.dependenciesBindingSource;
            this.dataGridViewDependencies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDependencies.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewDependencies.Name = "dataGridViewDependencies";
            this.dataGridViewDependencies.ReadOnly = true;
            this.dataGridViewDependencies.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridViewDependencies.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDependencies.Size = new System.Drawing.Size(678, 245);
            this.dataGridViewDependencies.TabIndex = 2;
            this.dataGridViewDependencies.SelectionChanged += new System.EventHandler(this.dataGridViewDependencies_SelectionChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labelModuleDependencies);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(678, 26);
            this.panel1.TabIndex = 2;
            // 
            // labelModuleDependencies
            // 
            this.labelModuleDependencies.AutoSize = true;
            this.labelModuleDependencies.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelModuleDependencies.Location = new System.Drawing.Point(13, 4);
            this.labelModuleDependencies.Name = "labelModuleDependencies";
            this.labelModuleDependencies.Size = new System.Drawing.Size(92, 13);
            this.labelModuleDependencies.TabIndex = 0;
            this.labelModuleDependencies.Text = "Dependencies:";
            // 
            // buttonRemove
            // 
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Location = new System.Drawing.Point(2, 60);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(26, 23);
            this.buttonRemove.TabIndex = 1;
            this.buttonRemove.Text = "-";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(2, 31);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(26, 23);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "+";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // dependencies
            // 
            this.dependencies.DataSetName = "Dependencies";
            this.dependencies.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // dependenciesBindingSource
            // 
            this.dependenciesBindingSource.DataMember = "Dependencies";
            this.dependenciesBindingSource.DataSource = this.dependencies;
            // 
            // requiredIdDataGridViewTextBoxColumn
            // 
            this.requiredIdDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.requiredIdDataGridViewTextBoxColumn.DataPropertyName = "RequiredId";
            this.requiredIdDataGridViewTextBoxColumn.HeaderText = "RequiredId";
            this.requiredIdDataGridViewTextBoxColumn.Name = "requiredIdDataGridViewTextBoxColumn";
            this.requiredIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // requiredLanguageDataGridViewTextBoxColumn
            // 
            this.requiredLanguageDataGridViewTextBoxColumn.DataPropertyName = "RequiredLanguage";
            this.requiredLanguageDataGridViewTextBoxColumn.HeaderText = "RequiredLanguage";
            this.requiredLanguageDataGridViewTextBoxColumn.Name = "requiredLanguageDataGridViewTextBoxColumn";
            this.requiredLanguageDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // requiredVersionDataGridViewTextBoxColumn
            // 
            this.requiredVersionDataGridViewTextBoxColumn.DataPropertyName = "RequiredVersion";
            this.requiredVersionDataGridViewTextBoxColumn.HeaderText = "RequiredVersion";
            this.requiredVersionDataGridViewTextBoxColumn.Name = "requiredVersionDataGridViewTextBoxColumn";
            this.requiredVersionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // Dependency
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer3);
            this.Name = "Dependency";
            this.Size = new System.Drawing.Size(726, 271);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDependencies)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dependencies)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dependenciesBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dataGridViewDependencies;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelModuleDependencies;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.BindingSource dependenciesBindingSource;
        private WixShield.Designers.GeneralInformation.Dependencies dependencies;
        private System.Windows.Forms.DataGridViewTextBoxColumn requiredIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn requiredLanguageDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn requiredVersionDataGridViewTextBoxColumn;

    }
}
