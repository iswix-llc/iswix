using Designers.GeneralInformation.Models;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid;
using PropertyItem = Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem;
using static FireworksFramework.Types.Enums;
using System.Linq;
using System.Drawing.Imaging;

namespace GeneralInformationDesigner.Views
{
    /// <summary>
    /// Interaction logic for GeneralInformationView.xaml
    /// </summary>
    public partial class GeneralInformationView : UserControl, IFireworksDesigner, ISelectionLinked
    {
        readonly DocumentManager _documentManager;
        readonly ViewModels.GeneralInformationViewModel viewModel;

        public GeneralInformationView(DocumentManager documentManager)
        {
            _documentManager = documentManager;
            // Created here rather than in the XAML, which can't pass it the document.
            viewModel = new ViewModels.GeneralInformationViewModel(documentManager);
            DataContext = viewModel;
            InitializeComponent();
        }

        public System.Drawing.Image PluginImage
        {
            get
            {
                return System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("GeneralInformationDesigner.GeneralInformation.ico")!);
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GeneralInformationDesigner.License.txt")!).ReadToEnd();
            }
        }

        public string PluginName
        {
            get { return "General Information"; }
        }

        public string PluginOrder
        {
            get { return "iswix_group1_general_information2"; }
        }

        public bool IsValidContext()
        {
            return ViewModels.GeneralInformationViewModel.IsValidDocument(_documentManager.Document);
        }

        public void LoadData()
        {
            viewModel.Load();
        }

        // The grid for the package's attributes or the one for its summary information, whichever was last in use or
        // matched to the XML editor's caret.
        bool _summaryInformationActive;
        bool _anyActive;

        public event EventHandler? SelectedElementChanged;

        public XElement? SelectedElement => !_anyActive ? null : _summaryInformationActive ? viewModel.SummaryInformationXml : viewModel.PackageXml;

        // From the XML rather than the grids, so it holds before the designer has been shown.
        public bool Handles(XElement element, string? attribute)
        {
            var package = element.Document?.Root?.Elements().FirstOrDefault();
            return package != null && package.Name == IsWiX.WiXModel.Package.ElementName &&
                (element == package || (element.Name == IsWiX.WiXModel.SummaryInformation.ElementName && element.Parent == package));
        }

        // The model's properties are named for the attributes they edit.
        public string? SelectedAttribute => !_anyActive ? null : ((_summaryInformationActive ? propertyGridSummaryInformation : propertyGridPackage4).SelectedPropertyItem as PropertyItem)?.PropertyDescriptor?.Name;

        public void SelectElement(XElement element, string? attribute)
        {
            if (!Handles(element, attribute))
            {
                return;
            }

            var summaryInformation = element.Name == IsWiX.WiXModel.SummaryInformation.ElementName;
            Activate(summaryInformation);
            var grid = summaryInformation ? propertyGridSummaryInformation : propertyGridPackage4;
            if (attribute != null && grid.Properties.OfType<PropertyItem>().FirstOrDefault(p => p.PropertyDescriptor?.Name == attribute) is { } item)
            {
                item.IsSelected = true;
            }
        }

        private void PropertyGrid_SelectedPropertyItemChanged(object sender, RoutedPropertyChangedEventArgs<PropertyItemBase> e)
        {
            Activate(ReferenceEquals(sender, propertyGridSummaryInformation));
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
        }

        void Activate(bool summaryInformation)
        {
            _anyActive = true;
            _summaryInformationActive = summaryInformation;
            borderPackage4.BorderBrush = summaryInformation ? System.Windows.Media.Brushes.Transparent : System.Windows.SystemColors.HighlightBrush;
            borderSummaryInformation.BorderBrush = summaryInformation ? System.Windows.SystemColors.HighlightBrush : System.Windows.Media.Brushes.Transparent;
        }

        private void PropertyGrid_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Activate(ReferenceEquals(sender, propertyGridSummaryInformation));
            SelectedElementChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}