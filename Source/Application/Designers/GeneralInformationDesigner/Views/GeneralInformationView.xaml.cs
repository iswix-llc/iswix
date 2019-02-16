using Designers.GeneralInformation.Models;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiXAutomationInterface;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using static FireworksFramework.Types.Enums;

namespace GeneralInformationDesigner.Views
{
    /// <summary>
    /// Interaction logic for GeneralInformationView.xaml
    /// </summary>
    public partial class GeneralInformationView : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        public GeneralInformationView()
        {
            InitializeComponent();
        }

        public System.Drawing.Image PluginImage
        {
            get
            {
                return System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("GeneralInformationDesigner.GeneralInformation.ico"));
            }
        }

        public PluginType PluginType { get { return PluginType.Designer; } }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GeneralInformationDesigner.License.txt")).ReadToEnd();
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
            IsWiXDocumentType docType = _documentManager.Document.GetDocumentType();

            if (docType == IsWiXDocumentType.Product || docType == IsWiXDocumentType.Module)
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
            viewModel.Load();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.AddDependency();
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
                viewModel.RemoveDependency();
        }

        private void DataGridDependencies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.ProcessSelectionChanged();
        }
    }
}