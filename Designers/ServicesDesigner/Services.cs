using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.ComponentModel;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Services;
using static FireworksFramework.Types.Enums;

namespace ServicesDesigner
{
    public partial class Services : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        private enum ImageLibrary
        {
            FolderOpen,
            FolderClosed,
            Computer,
            Dll,
            File,
            Web,
            Sql,
            Services,
            Xml,
            BlueFolderClosed,
            FileNotPresent,
            BlueFolderOpen
        }

        List<ServiceItem> _services = new List<ServiceItem>();

        public Services()
        {
            InitializeComponent();
            // Images come from FireworksFramework rather than the .resx, so the designer also runs on .NET Framework.
            createNewFolderToolStripMenuItem1.Image = DesignerImages.MenuImage("new_folder");
            renameFolderToolStripMenuItem.Image = DesignerImages.MenuImage("rename");
            deleteToolStripMenuItem.Image = DesignerImages.MenuImage("delete");
            DesignerImages.AddLibrary(iImageLibrary);
        }

        #region IFireworksDesigner Members

        public bool IsValidContext()
        {
            return ServiceDocument.IsValidDocument(_documentManager.Document);
        }

        public void LoadData()
        {
            _services = ServiceDocument.Load(_documentManager.Document);

            treeViewServices.Nodes.Clear();
            AddComputerDestinationNode();

            foreach (var iswixService in _services)
            {
                AddServiceNode(iswixService);
            }


        }

        private void AddComputerDestinationNode()
        {
            var subTreeNode = treeViewServices.Nodes.Add("Destination Computer");
            subTreeNode.ImageIndex = (int)ImageLibrary.Computer;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Computer;
            treeViewServices.SelectedNode = subTreeNode;
        }

        private void AddServiceNode(ServiceItem iswixService)
        {
            var subTreeNode = treeViewServices.Nodes[0].Nodes.Add(CalculateSelectedNodeText(iswixService));
            subTreeNode.ImageIndex = (int)ImageLibrary.Services;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Services;
            subTreeNode.Tag = iswixService;
            //subTreeNode.ToolTipText = iswixService.ServiceInstall.ParentComponentXML.ToString();
            treeViewServices.ExpandAll();
            var parentNode = treeViewServices.Nodes[0];
            if (parentNode.Nodes.Count > 0)
            {
                treeViewServices.SelectedNode = parentNode.Nodes[0];
            }
            else
            {
                treeViewServices.SelectedNode = parentNode;
            }
            treeViewServices.Select();
            UpdatedSelectedNodeText();
        }

        private void UpdatedSelectedNodeText()
        {
            if (treeViewServices.SelectedNode.Tag is ServiceItem service)
            {
                treeViewServices.SelectedNode.Text = CalculateSelectedNodeText(service);
            }
        }

        private string CalculateSelectedNodeText(ServiceItem service)
        {
            return string.Format("{0} ({1})", service.Install.Name, ServiceDocument.DestinationFilePath(service));
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("ServicesDesigner.Services.ico")!);
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ServicesDesigner.License.txt")!).ReadToEnd();
            }
        }

        public string PluginName
        {
            get { return "Services"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group3_services"; }
        }

        public PluginType PluginType
        {
            get { return PluginType.Designer; }
        }

        #endregion

        private void treeViewServices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node!.Level.Equals(0))
            {
                treeViewServices.ContextMenuStrip = cmsComputer;
                propertyGridServiceInstall.Enabled = false;
                propertyGridServiceInstall.SelectedObject = null;
            }
            else
            {
                treeViewServices.ContextMenuStrip = cmsService;
                propertyGridServiceInstall.Enabled = true;
                propertyGridServiceInstall.SelectedObject = serviceInstall;
                serviceInstall.Read((ServiceItem)e.Node.Tag);

            }
        }

        private void createNewFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ComponentPicker picker = new ComponentPicker(ServiceDocument.GetServiceCandidates(_documentManager.Document));
            picker.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = picker.ShowDialog();
            if (dr != DialogResult.Cancel)
            {

                if (!string.IsNullOrEmpty(picker.FileKey) || picker.FileElement != null)
                {
                    string prefix = picker.FileName;
                    int index = 0;
                    bool added = false;
                    do
                    {
                        index++;
                        bool exists = false;
                        foreach (var existingService in _services)
                        {
                            string name = string.Format("{0}{1}", prefix, index);
                            if (string.Equals(existingService.Install.Name, name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                exists = true;
                                break;
                            }
                        }


                        if (exists == false)
                        {
                            string name = string.Format("{0}{1}", prefix, index);

                            if (index == 1)
                            {
                                foreach (var existingService in _services)
                                {
                                    if (string.Equals(existingService.Install.Name, prefix, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        exists = true;
                                        break;
                                    }
                                }
                                if (exists == false)
                                {
                                    name = prefix;
                                }

                            }

                            ServiceItem service = ServiceDocument.Create(_documentManager.Document, name, picker.FileKey, picker.FileElement);
                            _services.Add(service);
                            AddServiceNode(service);
                            added = true;
                        }
                    }
                    while (added == false);

                }
            }
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServiceItem service = (ServiceItem)treeViewServices.SelectedNode.Tag;
            ServiceDocument.Delete(service);
            _services.Remove(service);
            treeViewServices.SelectedNode.Remove();
            treeViewServices.SelectedNode = treeViewServices.Nodes[0];
        }

        private void renameFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Rename service");
        }

        private void treeViewServices_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void propertyGridServiceInstall_PropertyValueChanged(object? s, PropertyValueChangedEventArgs e)
        {
            serviceInstall.Write(e.ChangedItem?.Label ?? string.Empty);
            propertyGridServiceInstall.Refresh();
            UpdatedSelectedNodeText();
        }
    }
}