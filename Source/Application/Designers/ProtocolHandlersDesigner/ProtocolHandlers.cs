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

namespace ProtocolHandlersDesigner
{
    public partial class ProtocolHandlers : UserControl, IFireworksDesigner
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
        IsWiXProtocolHandlers _protocolHandlers;

        public ProtocolHandlers()
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
            _protocolHandlers = new IsWiXProtocolHandlers(_mgr.DocumentManager.Document);

            treeViewProtocolHandlers.Nodes.Clear();
            AddComputerDestinationNode();

        }

        private void AddComputerDestinationNode()
        {
            var subTreeNode = treeViewProtocolHandlers.Nodes.Add("Destination Computer");
            subTreeNode.ImageIndex = (int)ImageLibrary.Computer;
            subTreeNode.SelectedImageIndex = (int)ImageLibrary.Computer;
            treeViewProtocolHandlers.SelectedNode = subTreeNode;
        }

        private string CalculateSelectedNodeText(IsWiXProtocolHandler protocolHandler)
        {
            return "";
           // return string.Format("{0} ({1})", service.ServiceInstall.Name, service.ServiceInstall.DestinationFilePath);
        }

        public Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtocolHandlersDesigner.ProtocolHandlers.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtocolHandlersDesigner.MS-PL.txt")).ReadToEnd();
            }
        }

        public string PluginName
        {
            get { return "Protocol Handlers"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group3_protocolhandlers"; }
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
                treeViewProtocolHandlers.ContextMenuStrip = cmsComputer;
                propertyGridServiceInstall.Enabled = false;
                propertyGridServiceInstall.SelectedObject = null;
            }
            else
            {
                treeViewProtocolHandlers.ContextMenuStrip = cmsService;
                propertyGridServiceInstall.Enabled = true;

            }
        }

        private void createNewFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ComponentPicker picker = new ComponentPicker(_protocolHandlers);
            picker.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = picker.ShowDialog();
            if (dr != DialogResult.Cancel)
            {
            }
        }


    }
}
