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
        }

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

        public string ExistingTables
        {
            get
            {
                return _tableName;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if ( textBoxTableName.Text == String.Empty)
            {
                MessageBox.Show("Please enter a table name.");
            }
            else
            {
                if (textBoxTableName.Text == _tableName)
                {
                    DialogResult = DialogResult.Cancel;
                }
                else
                {
                    if (_existingTables.Contains(textBoxTableName.Text))
                    {
                        MessageBox.Show("The specified table already exists.");
                    }
                    else
                    {
                        TableName = textBoxTableName.Text;
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
