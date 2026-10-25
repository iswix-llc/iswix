using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CustomTablesDesigner
{
    public partial class FormTableName : Form
    {
        private string _tableName;
        private List<string> _existingTables;

        public FormTableName( string TableName, List<string> ExistingTables )
        {
            _tableName = TableName;
            _existingTables = ExistingTables;
            InitializeComponent();
            textBoxTableName.Text = _tableName;
            textBoxTableName.SelectAll();
            ValidateName();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TableName
        {
            get
            {
                return _tableName;
            }

            set
            {
                _tableName = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public List<string> ExistingTables
        {
            get
            {
                return _existingTables;
            }
        }

        // Says why the name can't be used, under the box, and enables OK only for a name that can be.
        private void ValidateName()
        {
            var name = textBoxTableName.Text;
            var exists = name != _tableName && _existingTables.Contains(name);

            labelError.Text = exists ? "A table with that name already exists." : string.Empty;
            labelError.ForeColor = Color.Red;
            buttonOK.Enabled = name.Length > 0 && !exists;
        }

        private void textBoxTableName_TextChanged(object sender, EventArgs e)
        {
            ValidateName();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxTableName.Text == _tableName)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                TableName = textBoxTableName.Text;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
