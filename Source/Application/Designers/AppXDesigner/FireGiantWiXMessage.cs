using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace AppXDesigner
{
    public partial class FireGiantWiXMessage : Form
    {
        public FireGiantWiXMessage()
        {
            InitializeComponent();

            if (Properties.Settings.Default.SuppressFireGiantMessage.Equals("true", StringComparison.InvariantCulture))
            {
                checkBox1.Checked = true;
            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {

            if (checkBox1.Checked)
            {
                Properties.Settings.Default.SuppressFireGiantMessage = "true";
            }
            else
            {
                Properties.Settings.Default.SuppressFireGiantMessage = "false";
            }
            Properties.Settings.Default.Save();
        }

        private void linkLabelRequirements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.firegiant.com/products/wix-expansion-pack/appx/");
        }
    }
}
