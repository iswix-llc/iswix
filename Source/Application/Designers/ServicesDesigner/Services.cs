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
using FireworksFramework.Types;
using IsWiXAutomationInterface;

namespace ServicesDesigner
{
    public partial class Services : UserControl, IFireworksDesigner
    {
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

        IDesignerManager _mgr;
        IsWiXServices _services;

        public Services()
        {
            InitializeComponent();
        }

        #region IFireworksDesigner Members

        public IDesignerManager DesignerManager
        {
            set
            {
                _mgr = value;
            }
        }

        public bool IsValidContext()
        {
            if (_mgr.DocumentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadData()
        {
            _services = new IsWiXServices(_mgr.DocumentManager.Document);

            treeViewServices.Nodes.Clear();
            AddComputerDestinationNode();
            
            foreach (var iswixService in _services)
            {
                AddServiceNode(iswixService);
            }


        }

        private void AddComputerDestinationNode()
        {
            var subTreeNode =  treeViewServices.Nodes.Add("Destination Computer");
            subTreeNode.ImageIndex = (int)ImageLibrary.Computer;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Computer;
            treeViewServices.SelectedNode = subTreeNode;
        }

        private void AddServiceNode(IsWiXService iswixService)
        {
            var subTreeNode = treeViewServices.Nodes[0].Nodes.Add(CalculateSelectedNodeText(iswixService));
            subTreeNode.ImageIndex = (int)ImageLibrary.Services;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Services;
            subTreeNode.Tag = iswixService;
            //subTreeNode.ToolTipText = iswixService.ServiceInstall.ParentComponentXML.ToString();
            treeViewServices.ExpandAll();
            var parentNode = treeViewServices.Nodes[0];
            if(parentNode.Nodes.Count > 0)
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
            IsWiXService service = treeViewServices.SelectedNode.Tag as IsWiXService;
            if (service != null)
            {
                treeViewServices.SelectedNode.Text = CalculateSelectedNodeText(service);
            }
        }

        private string CalculateSelectedNodeText( IsWiXService service)
        {
            return string.Format("{0} ({1})", service.ServiceInstall.Name, service.ServiceInstall.DestinationFilePath);
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("ServicesDesigner.Services.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ServicesDesigner.MS-PL.txt")).ReadToEnd();
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
            if (e.Node.Level.Equals(0))
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
                serviceInstall.Read(e.Node.Tag as IsWiXService);
                
            }
        }

        private void createNewFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ComponentPicker picker = new ComponentPicker(_services);
            picker.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = picker.ShowDialog();
            if (dr != DialogResult.Cancel)
            {
                string fileKey = picker.FileKey;

                if (!string.IsNullOrEmpty(fileKey))
                {
                    string prefix = "NewService";
                    int index = 0;
                    bool added = false;

                     do
                    {
                         index++;
                         bool exists = false;
                        foreach (var existingService in _services)
                        {
                            string name = string.Format("{0}{1}", prefix, index);
                            if (existingService.ServiceInstall.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                exists = true;
                                break;
                            }
                        }


                        if (exists == false)
                        {
                            IsWiXService service = _services.Create(string.Format("{0}{1}",prefix,index), fileKey);
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
            IsWiXService service = treeViewServices.SelectedNode.Tag as IsWiXService;
            service.ServiceControl.Delete();
            service.ServiceInstall.Delete();
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

        private void propertyGridServiceInstall_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            serviceInstall.Write(e.ChangedItem.Label);
            UpdatedSelectedNodeText();
        }
    }
}
