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
using static FireworksFramework.Types.Enums;

namespace NamespacesDesigner.Views
{
    /// <summary>
    /// Interaction logic for NamespacesView.xaml
    /// </summary>
    public partial class NamespacesView : UserControl, IFireworksDesigner
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
    }
}
