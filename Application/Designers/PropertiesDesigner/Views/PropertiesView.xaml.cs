using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using IsWiXAutomationInterface;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using static FireworksFramework.Types.Enums;

namespace PropertiesDesigner.Views
{
    /// <summary>
    /// Interaction logic for PropertiesView.xaml
    /// </summary>
    public partial class PropertiesView : UserControl, IFireworksDesigner
    {

        public PropertiesView()
        {
            InitializeComponent();
        }

        public System.Drawing.Image PluginImage
        {
            get
            {
                return System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("PropertiesDesigner.Properties.ico"));
            }
        }

        public string PluginInformation
        {
            get
            {
                return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("PropertiesDesigner.License.txt")).ReadToEnd();
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

            if (DocumentManager.DocumentManagerInstance.Document.GetDocumentType() == IsWiXDocumentType.None)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void LoadData()
        {
            viewModel.Load();
        }
    }
}
