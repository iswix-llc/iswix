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
using Microsoft.Deployment.WindowsInstaller;
using IsWiXAutomationInterface;
using static FireworksFramework.Types.Enums;

namespace Designers.GeneralInformation
{
    public partial class GeneralInformation : UserControl, IFireworksDesigner
    {
        XNamespace ns;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;


        public GeneralInformation()
        {
            InitializeComponent();
        }

        public void LoadData()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            switch (_documentManager.Document.GetDocumentType())
            {
                case IsWiXDocumentType.Product:
                    dependency.Enabled = false;
                    dependency.Visible = false;
                    propertyGridModule.Enabled = false;
                    propertyGridModule.Visible = false;
                    LoadProductData();
                    LoadPackageData();

                    break;

                case IsWiXDocumentType.Module:
                    propertyGridProduct.Enabled = false;
                    propertyGridProduct.Visible = false;
                    LoadModuleData();
                    LoadPackageData();
                    LoadDependencies();
                    break;

                default:
                    // We shouldn't ever get here
                    break;
            }
        }

        public void LoadProductData()
        {
            propertyGridProduct.PropertyValueChanged -= new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridProduct_PropertyValueChanged);
            product.Read( new IsWiXProduct());
            propertyGridProduct.Refresh();
            propertyGridProduct.Enabled = true;
            propertyGridProduct.Visible = true;
            propertyGridProduct.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridProduct_PropertyValueChanged);
        }

        public void LoadPackageData()
        {
            propertyGridPackage.PropertyValueChanged -= new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridPackage_PropertyValueChanged);
            package.Read(new IsWiXPackage());
            propertyGridPackage.Refresh();
            propertyGridPackage.Enabled = true;
            propertyGridPackage.Visible = true;
            propertyGridPackage.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridPackage_PropertyValueChanged);
        }

        public void LoadModuleData()
        {
            propertyGridModule.PropertyValueChanged -= new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridModule_PropertyValueChanged);
            module.Read( new IsWiXModule() );
            propertyGridModule.Refresh();
            propertyGridModule.Enabled = true;
            propertyGridModule.Visible = true;
            propertyGridModule.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridModule_PropertyValueChanged);
        }

        private void LoadDependencies()
        {
            dependency.Read();
            dependency.Refresh();
            dependency.Enabled = true;
            dependency.Visible = true;
        }

        private void propertyGridModule_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            module.Write(e.ChangedItem.Label);
        }

        private void propertyGridPackage_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            package.Write(e.ChangedItem.Label);
        }

        private void propertyGridProduct_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            product.Write(e.ChangedItem.Label);
        }

        public System.Drawing.Image PluginImage
        {
            get
            {
                return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("GeneralInformationDesigner.GeneralInformation.ico"));  
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
            get { return "General Information";  }
        }

        public string PluginOrder
        {
            get { return "iswix_group1_general_information"; }
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

    }
}
