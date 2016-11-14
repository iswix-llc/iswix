///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Schema;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Properties;
using FireworksFramework.Types;
//using ICSharpCode.XmlEditor;

namespace FireworksFramework
{
    partial class MainForm : Form, IPublishable, IDesignerManager
    {
        IFireworksDesigner selectedDesigner;
        FireworksManager _fireworksManager;

        #region Constructor
        public MainForm(FireworksManager fireworksManager )
        {
            _fireworksManager = fireworksManager;

            InitializeComponent();

            if (Settings.Default.WindowLocation != null)
            {
                this.Location = Settings.Default.WindowLocation;
            }

            // Set window size
            if (Settings.Default.WindowSize != null)
            {
                this.Size = Settings.Default.WindowSize;
            }


            PopulateMenuStrip();
            Text = Application.ProductName;

            // Pass object reference to document manager
            documentManager.Subscribe(this);

            // Open the document passed by command line
            if(!string.IsNullOrEmpty(fireworksManager.FilePath))
            {
                documentManager.OpenFilePath(fireworksManager.FilePath);
                this.WindowState = FormWindowState.Maximized;
            }

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        }
        #endregion

        private void PopulateMenuStrip()
        {
            DirectoryInfo di = new DirectoryInfo("Templates");
            if (di.Exists)
            {
                foreach (var dir in di.GetDirectories())
                {
                    newToolStripMenuItem.Enabled = true;
                    ToolStripMenuItem toolstripitem = (ToolStripMenuItem)newToolStripMenuItem.DropDownItems.Add(dir.Name);
                    ToolStripDropDownButton newButton = toolStripCommands.Items[0] as ToolStripDropDownButton;
                    ToolStripDropDownButton subButton = new ToolStripDropDownButton(dir.Name);
                    newButton.Enabled = true;
                    newButton.DropDownItems.Add(subButton);

                    foreach (var file in dir.GetFiles())
                    {
                        var templateToolStripItem = toolstripitem.DropDownItems.Add(Path.GetFileNameWithoutExtension(file.Name));
                        var test = subButton.DropDownItems.Add(Path.GetFileNameWithoutExtension(file.Name));

                        templateToolStripItem.Tag = Path.Combine(dir.Name, file.Name);
                        templateToolStripItem.Click += new System.EventHandler(this.CreateFromTemplateToolStripMenuItem_Click);

                        test.Tag = Path.Combine(dir.Name, file.Name);
                        test.Click += new System.EventHandler(this.CreateFromTemplateToolStripMenuItem_Click);

                    }
                }
            }
        }

        #region Subscriptions

        void IPublishable.PublishDocumentChange()
        {
            tabControlMain.SelectedIndex = 0;
        }

        void IPublishable.Publish(DocumentStates DocumentState)
        {
            switch (DocumentState)
            {
                case DocumentStates.Closed:
                    tabControlMain.Visible = false;
                    closeToolStripMenuItem.Enabled = false;
                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    toolStripButtonSave.Enabled = false;
                    tabControlMain.SelectedIndex = 0;
                    break;

                case DocumentStates.Dirty:
                    toolStripButtonSave.Enabled = true;
                    break;

                case DocumentStates.Clean:
                    tabControlMain.Visible = true;
                    closeToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    saveToolStripMenuItem.Enabled = true;
                    toolStripButtonSave.Enabled = false;
                    break;
 
                default:
                    MessageBox.Show("Unkown Documentstate recieved");
                    break;
            }
        }
        void IPublishable.Publish(string DocumentPath)
        {
            if( String.IsNullOrEmpty(DocumentPath))
            {
                this.Text = Application.ProductName;
            }
            else
            {
                this.Text = DocumentPath + " - " + Application.ProductName;
            }
        }

        void IPublishable.Publish(XNamespace Namespace)
        {
            ImageList imageListDesigners = new ImageList();
            imageListDesigners.ColorDepth = ColorDepth.Depth32Bit;
            listViewDesigners.SmallImageList = imageListDesigners;
            
            listViewDesigners.Clear();
            tabControlDesigners.TabPages.Clear();
            foreach (var designer in pluginManager.Designers.Values)
            {
                designer.DesignerManager = this;
                if ( designer.PluginType == PluginType.Designer && designer.IsValidContext())
                {
                    ListViewItem pluginItem = listViewDesigners.Items.Add(designer.PluginName);
                    pluginItem.Tag = designer;
                    imageListDesigners.Images.Add( designer.PluginName, designer.PluginImage);
                    pluginItem.ImageKey = designer.PluginName;
                }
            }

            if (listViewDesigners.Items.Count > 0)
            {
                listViewDesigners.Items[0].Selected = true;
            }
        }
        #endregion

        #region Menu Item Actions

        private void CreateFromTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            documentManager.CreateFromTemplate(item.Tag.ToString());
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            documentManager.Open();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            documentManager.Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            documentManager.SaveAs( new FileInfo( documentManager.DocumentPath).DirectoryName);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            documentManager.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox(pluginManager, _fireworksManager.BrandingBitMap).ShowDialog();
        }
        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (documentManager.AskClose() == false )
            {
                e.Cancel = true;
            }
            // Copy window location to app settings
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowLocation = this.Location;
                Settings.Default.WindowSize = this.Size;
                Settings.Default.Save();
            }
            
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void productToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            documentManager.CreateFromTemplate(item.Tag.ToString());
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            productToolStripMenuItem_Click(sender, e);
        }


        #region IFireworks Members

        public IDocumentManager DocumentManager
        {
            get { return documentManager; }
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                if (toolStripButtonSave.Enabled.Equals(true))
                {
                    documentManager.Save();
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void listViewDesigners_SelectedIndexChanged(object sender, EventArgs e)
        {

            if( listViewDesigners.SelectedItems.Count > 0 )
            {
                #region DisablePreviousDesigner
            foreach (TabPage item in tabControlDesigners.TabPages)
            {
                tabControlDesigners.TabPages.Remove(item);
            }
            selectedDesigner = null;
            #endregion

                #region EnableSelectedDesigner
            // Take previously constructed designer and make it visible on the screen
            tabControlDesigners.TabPages.Add(tabPagePlugin);
            TabPage pluginTabPage = tabControlDesigners.TabPages[0];
            selectedDesigner = listViewDesigners.SelectedItems[0].Tag as IFireworksDesigner;

            var selectedControl = selectedDesigner as Control;

            SuspendLayout();
            pluginTabPage.Text = selectedDesigner.PluginName;
            pluginTabPage.Controls.Clear();
            pluginTabPage.Location = new System.Drawing.Point(4, 22);
            pluginTabPage.Name = "pluginTabPage";
            pluginTabPage.Size = new System.Drawing.Size(648, 280);
            pluginTabPage.TabIndex = 2;


            if (selectedControl != null)
            {
                pluginTabPage.Controls.Add(selectedControl);
                selectedControl.Dock = System.Windows.Forms.DockStyle.Fill;
                selectedControl.Location = new System.Drawing.Point(0, 0);
                selectedControl.Name = "selectedDesigner";
                selectedControl.Size = new System.Drawing.Size(648, 280);
                selectedControl.TabIndex = 0;
            }

            ResumeLayout(true);
            // Ensure designer is in proper state
            selectedDesigner.DesignerManager = this;
            selectedDesigner.LoadData();
            #endregion
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }

}
