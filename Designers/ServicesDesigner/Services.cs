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
using Wxs = IsWiX.WiXModel;

namespace ServicesDesigner
{
    public partial class Services : UserControl, IFireworksDesigner, ISelectionLinked
    {
        readonly DocumentManager _documentManager;

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

        public Services(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            InitializeComponent();
            // Images come from FireworksFramework rather than the .resx, so the designer also runs on .NET Framework.
            createNewFolderToolStripMenuItem1.Image = DesignerImages.MenuImage("new_folder");
            renameFolderToolStripMenuItem.Image = DesignerImages.MenuImage("rename");
            deleteToolStripMenuItem.Image = DesignerImages.MenuImage("delete");
            DesignerImages.AddLibrary(iImageLibrary);
            AddToolbar();

            // So the selected service still shows when the XML editor has the focus.
            treeViewServices.HideSelection = false;
            treeViewServices.AfterSelect += (s, e) =>
            {
                // Whoever selected it, the user or the designer filling the tree. The window ignores this while it reloads
                // the designers for a change made in the XML, so typing there doesn't move the XML's selection.
                ShowSelectionInXml(null);
            };
            // Clicking the node that is already selected, such as the one the designer selects first, selects nothing.
            treeViewServices.NodeMouseClick += (s, e) =>
            {
                if (e.Node == treeViewServices.SelectedNode)
                {
                    ShowSelectionInXml(null);
                }
            };
            propertyGridServiceInstall.SelectedGridItemChanged += (s, e) =>
            {
                if (propertyGridServiceInstall.ContainsFocus && e.NewSelection?.GridItemType == GridItemType.Property)
                {
                    ShowSelectionInXml(e.NewSelection.PropertyDescriptor?.Name);
                }
            };
        }

        #region ISelectionLinked Members

        // The grid item the user chose, which is named for an attribute of the ServiceInstall, or of the ServiceControl
        // for the four that say what happens to the service.
        string? _property;

        static readonly string[] ControlProperties = { "Start", "Stop", "Remove", "Wait" };

        public event EventHandler? SelectedElementChanged;

        ServiceItem? SelectedService => treeViewServices.SelectedNode?.Tag as ServiceItem;

        public XElement? SelectedElement =>
            SelectedService is { } service ? (_property != null && ControlProperties.Contains(_property) && service.Control != null ? service.Control.Xml : service.Install.Xml) : null;

        // The grid calls the install's Start "Startup", as Start is the control's.
        // A control's attribute has no place in the XML until the service has a ServiceControl.
        public string? SelectedAttribute =>
            _property == "Startup" ? "Start" : _property != null && ControlProperties.Contains(_property) && SelectedService?.Control == null ? null : _property;

        // From the XML rather than the tree, which is only filled once the designer has been shown.
        public bool Handles(XElement element, string? attribute) =>
            element.AncestorsAndSelf().Any(e => e.Name == Wxs.ServiceInstall.ElementName || e.Name == Wxs.ServiceControl.ElementName);

        public void SelectElement(XElement element, string? attribute)
        {
            var node = treeViewServices.Nodes[0].Nodes.Cast<TreeNode>().FirstOrDefault(n =>
                n.Tag is ServiceItem s && element.AncestorsAndSelf().Any(e => e == s.Install.Xml || (s.Control != null && e == s.Control.Xml)));
            if (node is null)
            {
                return;
            }

            var isControl = element.AncestorsAndSelf().Any(e => e.Name == Wxs.ServiceControl.ElementName);
            _property = attribute == null ? null : !isControl && attribute == "Start" ? "Startup" : attribute;
            treeViewServices.SelectedNode = node;
            if (_property != null && propertyGridServiceInstall.SelectedGridItem is { } selected)
            {
                var root = selected;
                while (root.Parent != null)
                {
                    root = root.Parent;
                }

                AllItems(root).FirstOrDefault(i => i.GridItemType == GridItemType.Property && i.PropertyDescriptor?.Name == _property)?.Select();
            }
        }

        // For a selection made by the user, or by a command that selects in code.
        private void ShowSelectionInXml(string? property)
        {
            _property = property;
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
        }

        private static IEnumerable<GridItem> AllItems(GridItem item)
        {
            yield return item;
            foreach (GridItem child in item.GridItems)
            {
                foreach (var descendant in AllItems(child))
                {
                    yield return descendant;
                }
            }
        }

        #endregion

        ToolStripButton _deleteButton = null!;

        // The tree's commands as buttons above it too: nothing else shows that right-clicking it adds a service.
        private void AddToolbar()
        {
            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            // New Service lists the files a service can be made for, here and in the tree's menu.
            var newButton = new ToolStripDropDownButton("New Service") { DisplayStyle = ToolStripItemDisplayStyle.Text, AutoToolTip = false };
            FillNewServiceMenu(newButton.DropDownItems);
            newButton.DropDownOpening += (s, e) => FillNewServiceMenu(newButton.DropDownItems);
            FillNewServiceMenu(createNewFolderToolStripMenuItem1.DropDownItems);
            cmsComputer.Opening += (s, e) => FillNewServiceMenu(createNewFolderToolStripMenuItem1.DropDownItems);
            _deleteButton = new ToolStripButton("Delete") { DisplayStyle = ToolStripItemDisplayStyle.Text, AutoToolTip = false, Enabled = false };
            _deleteButton.Click += (s, e) => deleteToolStripMenuItem.PerformClick();
            toolbar.Items.Add(newButton);
            toolbar.Items.Add(_deleteButton);
            splitContainer1.Panel1.Controls.Add(toolbar);
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
            _deleteButton.Enabled = !e.Node!.Level.Equals(0);
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

        // A service can be made for an .exe that is its component's key file.
        private void FillNewServiceMenu(ToolStripItemCollection items)
        {
            var candidates = _documentManager.Document is null
                ? new List<ServiceCandidate>()
                : ServiceDocument.GetServiceCandidates(_documentManager.Document);
            CandidateMenu.Fill(items, candidates, c => c.DestinationFilePath, CreateService,
                "No .exe is the key file of its component", byFolder: false);
        }

        // Named for the file, numbered if a service already has that name.
        private void CreateService(ServiceCandidate candidate)
        {
            bool Taken(string name) => _services.Any(s => string.Equals(s.Install.Name, name, StringComparison.InvariantCultureIgnoreCase));
            string name = candidate.FileName;
            for (int index = 1; Taken(name); index++)
            {
                name = candidate.FileName + index;
            }

            ServiceItem service = ServiceDocument.Create(_documentManager.Document, name, candidate.Id, candidate.FileElement);
            _services.Add(service);
            AddServiceNode(service);

            // The new service, not the first one that AddServiceNode selects.
            treeViewServices.SelectedNode = treeViewServices.Nodes[0].Nodes.Cast<TreeNode>().First(n => n.Tag == service);
            ShowSelectionInXml(null);
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServiceItem service = (ServiceItem)treeViewServices.SelectedNode.Tag;
            ServiceDocument.Delete(service);
            _services.Remove(service);
            treeViewServices.SelectedNode.Remove();
            treeViewServices.SelectedNode = treeViewServices.Nodes[0];
            _deleteButton.Enabled = false;
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