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
using DocumentManagement.Interfaces;
using DocumentManagement.Types;
using DocumentManagement.Managers;
using DifferencesViewer.Views;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace FireworksFramework
{
    partial class MainForm : Form, IPublishable
    {
        IFireworksDesigner selectedDesigner;
        FireworksManager _fireworksManager;
        DocumentManager _documentManager;
        string _templateName = string.Empty;
        string _documentExtension = string.Empty;
        string _documentDirectoryMRU = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Differences _differnces;
        #region Constructor
        public MainForm(FireworksManager fireworksManager)
        {
            string settingsMRU = Properties.Settings.Default.DocumentDirectoryMRU;
            if (Directory.Exists(settingsMRU))
            {
                _documentDirectoryMRU = settingsMRU;
            }

            _fireworksManager = fireworksManager;
            _documentManager = DocumentManager.DocumentManagerInstance;
            //  this.documentManager.DocumentText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n\r\n";


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
            _documentManager.Subscribe("mainform", this);

            // Open the document passed by command line
            if (!string.IsNullOrEmpty(fireworksManager.FilePath))
            {
                TryLoad(fireworksManager.FilePath);
                _documentExtension = Path.GetExtension(fireworksManager.FilePath);
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
                    showDifferencesWindowToolStripMenuItem.Enabled = false;
                    break;

                case DocumentStates.Dirty:
                    tabControlMain.Visible = true;
                    closeToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    saveToolStripMenuItem.Enabled = true;
                    toolStripButtonSave.Enabled = true;
                    if (_documentManager.Subscribers.ContainsKey("differences"))
                    {
                        showDifferencesWindowToolStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        showDifferencesWindowToolStripMenuItem.Enabled = true;
                    }
                    break;

                case DocumentStates.Clean:
                    tabControlMain.Visible = true;
                    closeToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    saveToolStripMenuItem.Enabled = true;
                    toolStripButtonSave.Enabled = false;
                    if (_documentManager.Subscribers.ContainsKey("differences"))
                    {
                        showDifferencesWindowToolStripMenuItem.Enabled = false;
                    }
                    else
                    { 
                        showDifferencesWindowToolStripMenuItem.Enabled = true;
                    }
                    break;

                default:
                    MessageBox.Show("Unkown Documentstate recieved");
                    break;
            }
        }
        void IPublishable.Publish(string DocumentPath)
        {
            if (String.IsNullOrEmpty(DocumentPath))
            {
                if (_documentManager.DocumentState == DocumentStates.Closed)
                {
                    this.Text = Application.ProductName;
                }
                else
                {
                    this.Text = "Untitled - " + Application.ProductName;
                }
            }
            else
            {
                this.Text = DocumentPath + " - " + Application.ProductName;
            }
        }

        void IPublishable.Publish(XNamespace Namespace)
        {
            PublishNamespace();
        }
        #endregion

        void PublishNamespace()
        {
            ImageList imageListDesigners = new ImageList();
            imageListDesigners.ColorDepth = ColorDepth.Depth32Bit;
            listViewDesigners.SmallImageList = imageListDesigners;

            listViewDesigners.Clear();
            tabControlDesigners.TabPages.Clear();
            foreach (var designer in pluginManager.Designers.Values)
            {
                if (designer.PluginType == PluginType.Designer && designer.IsValidContext())
                {
                    ListViewItem pluginItem = listViewDesigners.Items.Add(designer.PluginName);
                    pluginItem.Tag = designer;
                    imageListDesigners.Images.Add(designer.PluginName, designer.PluginImage);
                    pluginItem.ImageKey = designer.PluginName;
                }
            }

            if (listViewDesigners.Items.Count > 0)
            {
                listViewDesigners.Items[0].Selected = true;
            }

        }
        #region Menu Item Actions

        private void CreateFromTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            string templateName = item.Tag.ToString();
            if (AskClose())
            {
                try
                {
                    string templatePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Templates\\" + templateName);
                    _templateName = templateName;
                    _documentExtension = Path.GetExtension(templatePath);
                    _documentManager.UnSubscribe("mainform");
                    _documentManager.Load(templatePath);
                    if (SaveAs(_documentDirectoryMRU) == false)
                    {
                        _documentManager.Close();
                        _documentManager.Subscribe("mainform", this);
                    }
                    else
                    {
                        _documentManager.Subscribe("mainform", this);
                        _documentManager.Refresh();
                        PublishNamespace();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }



        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AskClose())
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = "";
                openFileDialog.InitialDirectory = _documentDirectoryMRU;
                openFileDialog.Filter = "All files (*.*)|*.*";
                var result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string _documentPath = openFileDialog.FileName;
                    _documentManager.Close();
                    if (TryLoad(_documentPath))
                    {
                        _documentDirectoryMRU = new FileInfo(_documentPath).Directory.FullName;
                        Settings.Default.DocumentDirectoryMRU = _documentDirectoryMRU;
                        Settings.Default.Save();
                        _documentExtension = Path.GetExtension(_documentPath);
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (_documentManager.CanSave)
            {
                if (string.IsNullOrEmpty(_documentManager.DocumentPath))
                {
                    SaveAs(_documentDirectoryMRU);
                }
                else if (new FileInfo(_documentManager.DocumentPath).IsReadOnly == true)
                {
                    SaveAs(new FileInfo(_documentManager.DocumentPath).DirectoryName);
                }
                else
                {
                    try
                    {
                        _documentManager.Save(_documentManager.DocumentPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving. {ex.Message}");
                    }
                }
            }
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs(new FileInfo(_documentManager.DocumentPath).DirectoryName);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AskClose())
            {
                if(_differnces!=null)
                {
                    _differnces.Close();
                    _differnces = null;
                }
                _documentManager.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox(pluginManager, _fireworksManager.BrandingBitMap).ShowDialog();
        }
        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_differnces != null)
            {
                _differnces.Close();
                _differnces = null;
            }

            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AskClose() == false)
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
            if(_differnces != null)
            {
                _differnces.Close();
                _differnces = null;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                if (toolStripButtonSave.Enabled.Equals(true))
                {
                    Save();
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void listViewDesigners_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listViewDesigners.SelectedItems.Count > 0)
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
                selectedDesigner.LoadData();
                #endregion
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        public bool AskClose()
        {
            bool canContinue = true;
            if (_documentManager.CanSave)
            {
                if (_documentManager.DocumentState == DocumentStates.Dirty)
                {
                    var result = MessageBox.Show("Would you like to save your changes?", Application.ProductName, MessageBoxButtons.YesNoCancel);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            Save();
                            // do stuff
                            break;
                        case DialogResult.No:
                            // do stuff
                            break;
                        case DialogResult.Cancel:
                            canContinue = false;
                            // do stuff
                            break;
                    }
                }
            }
            else
            {
                canContinue = false;
                MessageBox.Show("The document is not ready to be saved.");
            }
            return canContinue;
        }

        private bool TryLoad(string filePath)
        {
            bool success = false;
            try
            {
                _documentManager.Load(filePath);
                success = true;

            }
            catch (Exception)
            {
                MessageBox.Show($"An error occurred loading '{filePath}'. Please make sure it's a valid XML file.");
            }

            return success;
        }

        public bool SaveAs(string initialDirectory)
        {
            bool saved = false;
            if (_documentManager.CanSave)
            {
                var saveDialog = new SaveFileDialog();

                saveDialog.InitialDirectory = initialDirectory;
                saveDialog.FileName = Path.GetFileName(_documentManager.DocumentPath);
                saveDialog.Filter = Path.GetFileNameWithoutExtension(_templateName) + " files (*" + _documentExtension + ")|*" + _documentExtension + "|All files (*.*)|*.*";


                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _documentManager.Save(saveDialog.FileName);
                        _documentDirectoryMRU = new FileInfo(saveDialog.FileName).Directory.FullName;
                        Settings.Default.DocumentDirectoryMRU = _documentDirectoryMRU;
                        Settings.Default.Save();

                        saved = true;
                        _documentManager.Subscribe("mainform", this);
                        _documentManager.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving. {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("The document is not ready to be saved.");
            }

            return saved;
        }

        private void showDifferencesWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {

             _differnces = new Differences();
            Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap =
                 Imaging.CreateBitmapSourceFromHBitmap(
                      hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());

            _differnces.Icon = wpfBitmap;

            _differnces.Title = "IsWiX - Differences";
            _differnces.Show();
            
        }

        public void DocumentUpdated()
        {
        }

        private void vVisitIsWiXcomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.iswix.com");
        }
    }
}
