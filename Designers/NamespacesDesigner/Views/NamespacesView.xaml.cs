using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using IsWiX.WiXModel.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using NamespacesDesigner.Models;
using static FireworksFramework.Types.Enums;

namespace NamespacesDesigner.Views
{
    /// <summary>
    /// Interaction logic for NamespacesView.xaml
    /// </summary>
    public partial class NamespacesView : UserControl, IFireworksDesigner, ISelectionLinked
    {
        readonly DocumentManager _documentManager;
        readonly ViewModels.NamespacesViewModel viewModel;

        public NamespacesView(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            // Created here rather than in the XAML, which can't pass it the document.
            viewModel = new ViewModels.NamespacesViewModel(documentManager);
            DataContext = viewModel;
            InitializeComponent();
        }

        public string PluginName
        {
            get
            {
                return "Namespaces"; 
            }
        }
        public System.Drawing.Image PluginImage
        {
            get
            {
                return System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("NamespacesDesigner.Namespaces.ico")!);
            }
        }
        public PluginType PluginType { get { return PluginType.Designer; } }
        public string PluginOrder
        {
            get { return "iswix_group3_namespaces"; }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("NamespacesDesigner.License.txt")!).ReadToEnd();
            }
        }


        public bool IsValidContext()
        {
            return _documentManager.DefaultNamespace == WixNamespaces.Wxs;
        }

        public void LoadData()
        {
            viewModel.LoadData();
        }

        // The namespaces are all attributes of the root element (xmlns:prefix), so the XML shows that element's tag
        // whatever is selected, or if nothing is, and even when it declares none.
        const string DeclarationPrefix = "xmlns:";

        public event EventHandler? SelectedElementChanged;

        public XElement? SelectedElement => _documentManager.Document.Root;

        public string? SelectedAttribute => null;

        // The caret is in the root's tag. From the document rather than the rows, which are only loaded once the
        // designer has been shown.
        public bool Handles(XElement element, string? attribute) => element == element.Document?.Root;

        public void SelectElement(XElement element, string? attribute)
        {
            // On a namespace declaration, its row; elsewhere in the tag, the designer is just shown.
            if (!Handles(element, attribute) || attribute is null || !attribute.StartsWith(DeclarationPrefix, StringComparison.Ordinal))
            {
                return;
            }

            var prefix = attribute.Substring(DeclarationPrefix.Length);
            var model = viewModel.Namespaces.FirstOrDefault(n => n.Selected && n.Prefix == prefix);
            if (model != null)
            {
                dataGridDependencies.SelectedItem = model;
                dataGridDependencies.ScrollIntoView(model);
            }
        }

        void RaiseSelectedElementChanged() => SelectedElementChanged?.Invoke(this, EventArgs.Empty);

        private void DataGridDependencies_SelectionChanged(object sender, SelectionChangedEventArgs e) => RaiseSelectedElementChanged();

        // Ticking a namespace declares it, which puts the XML's declaration where the selection should be.
        private void DataGridDependencies_Click(object sender, RoutedEventArgs e) => RaiseSelectedElementChanged();
    }
}
