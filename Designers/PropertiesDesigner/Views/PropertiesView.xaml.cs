
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using PropertiesDesigner.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Xml.Linq;
using static FireworksFramework.Types.Enums;

namespace PropertiesDesigner.Views
{
    /// <summary>
    /// Interaction logic for PropertiesView.xaml
    /// </summary>
    public partial class PropertiesView : UserControl, IFireworksDesigner, ISelectionLinked
    {
        readonly DocumentManager _documentManager;
        readonly ViewModels.PropertiesViewModel viewModel;

        public PropertiesView(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            // Created here rather than in the XAML, which can't pass it the document.
            viewModel = new ViewModels.PropertiesViewModel(documentManager);
            DataContext = viewModel;
            InitializeComponent();
        }

        public System.Drawing.Image PluginImage
        {
            get
            {
                return System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("PropertiesDesigner.Properties.ico")!);
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("PropertiesDesigner.License.txt")!).ReadToEnd();
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginName
        {
            get { return "Properties"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group2_properties"; }
        }
        public bool IsValidContext()
        {
            return ViewModels.PropertiesViewModel.IsValidDocument(_documentManager.Document);
        }

        public void LoadData()
        {
            viewModel.Load();
        }

        private void DataGridDependencies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.EnableRemove = dataGridDependencies.SelectedItem != null;
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? SelectedElementChanged;

        public XElement? SelectedElement => (dataGridDependencies.SelectedItem as PropertyModel)?.Element.Xml;

        PropertyModel? ModelFor(XElement element) =>
            viewModel.Properties.FirstOrDefault(p => element.AncestorsAndSelf().Contains(p.Element.Xml));

        // From the XML rather than the rows, which are only loaded once the designer has been shown.
        public bool Handles(XElement element)
        {
            var container = element.Document?.Root?.Elements().FirstOrDefault();
            return element.AncestorsAndSelf().Any(e => e.Name == IsWiX.WiXModel.Property.ElementName && e.Parent == container);
        }

        public void SelectElement(XElement element)
        {
            var model = ModelFor(element);
            if (model != null && !ReferenceEquals(dataGridDependencies.SelectedItem, model))
            {
                dataGridDependencies.SelectedItem = model;
                dataGridDependencies.ScrollIntoView(model);
            }
        }

        private void NewProperty_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.Add();
        }

        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.Remove(dataGridDependencies.SelectedItem as PropertyModel);
        }

    }
}
