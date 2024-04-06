namespace ShortCutsDesigner
{
    partial class ComponentPicker
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentPicker));
            panel1 = new System.Windows.Forms.Panel();
            buttonCancel = new System.Windows.Forms.Button();
            buttonSelect = new System.Windows.Forms.Button();
            panel2 = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            treeView1 = new System.Windows.Forms.TreeView();
            label2 = new System.Windows.Forms.Label();
            textBoxIncludeFilter = new System.Windows.Forms.TextBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(buttonCancel);
            panel1.Controls.Add(buttonSelect);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 487);
            panel1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(888, 67);
            panel1.TabIndex = 2;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(743, 12);
            buttonCancel.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(125, 44);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSelect
            // 
            buttonSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonSelect.Enabled = false;
            buttonSelect.Location = new System.Drawing.Point(608, 12);
            buttonSelect.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            buttonSelect.Name = "buttonSelect";
            buttonSelect.Size = new System.Drawing.Size(125, 44);
            buttonSelect.TabIndex = 1;
            buttonSelect.Text = "&Select";
            buttonSelect.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.Controls.Add(textBoxIncludeFilter);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(label1);
            panel2.Dock = System.Windows.Forms.DockStyle.Top;
            panel2.Location = new System.Drawing.Point(0, 0);
            panel2.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(888, 63);
            panel2.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(20, 17);
            label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(427, 25);
            label1.TabIndex = 0;
            label1.Text = "Please select the file you wish to create a shortcut to.";
            label1.Click += label1_Click;
            // 
            // treeView1
            // 
            treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView1.Location = new System.Drawing.Point(0, 63);
            treeView1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            treeView1.Name = "treeView1";
            treeView1.Size = new System.Drawing.Size(888, 424);
            treeView1.TabIndex = 4;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(472, 17);
            label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 25);
            label2.TabIndex = 1;
            label2.Text = "Filter:";
            // 
            // textBoxIncludeFilter
            // 
            textBoxIncludeFilter.Location = new System.Drawing.Point(534, 14);
            textBoxIncludeFilter.Name = "textBoxIncludeFilter";
            textBoxIncludeFilter.Size = new System.Drawing.Size(158, 31);
            textBoxIncludeFilter.TabIndex = 8;
            textBoxIncludeFilter.KeyDown += textBoxIncludeFilter_KeyDown;
            // 
            // ComponentPicker
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(888, 554);
            Controls.Add(treeView1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            Name = "ComponentPicker";
            Text = "ComponentPicker - IsWiX";
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxIncludeFilter;
    }
}