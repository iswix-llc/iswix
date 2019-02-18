using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MSIXDesigner
{
    public partial class FireGiantWiXMessage : Form
    {
        public FireGiantWiXMessage()
        {
            InitializeComponent();
        }

        private void linkLabelRequirements_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.firegiant.com/products/wix-expansion-pack/MSIX/");
        }
    }
}
