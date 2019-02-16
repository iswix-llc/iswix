using Designers.GeneralInformation;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IsWiXAutomationInterface;
using System.Windows;

namespace GeneralInformationDesigner.ViewModels
{

    class GeneralInformationViewModel : ObservableObject
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        XNamespace ns;

        Product _product;
        Module _module;
        Package _package;
        Visibility _productPropertyGridVisible = Visibility.Hidden;
        Visibility _modulePropertyGridVisible = Visibility.Hidden;
        Visibility _packagePropertyGridVisible = Visibility.Hidden;
        public Product Product { get { return _product; } set { _product = value; RaisePropertyChangedEvent("Product"); } }
        public Module Module { get { return _module; } set { _module = value; RaisePropertyChangedEvent("Module"); } }
        public Package Package { get { return _package; } set { _package = value; RaisePropertyChangedEvent("Package"); } }
        public Visibility ProductPropertyGridVisible { get { return _productPropertyGridVisible; } set { _productPropertyGridVisible = value; RaisePropertyChangedEvent("ProductPropertyGridVisible"); } }
        public Visibility ModulePropertyGridVisible { get { return _modulePropertyGridVisible; } set { _modulePropertyGridVisible = value; RaisePropertyChangedEvent("ModulePropertyGridVisible"); } }
        public Visibility PackagePropertyGridVisible { get { return _packagePropertyGridVisible; } set { _packagePropertyGridVisible = value; RaisePropertyChangedEvent("PackagePropertyGridVisible"); } }

        public void Load()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            switch (_documentManager.Document.GetDocumentType())
            {
                case IsWiXDocumentType.Product:
                    ProductPropertyGridVisible = Visibility.Visible;
                    ModulePropertyGridVisible = Visibility.Hidden;
                    Product product = new Product();
                    product.Read(new IsWiXProduct());
                    Product = product;
                    break;

                case IsWiXDocumentType.Module:
                    ProductPropertyGridVisible = Visibility.Hidden;
                    ModulePropertyGridVisible = Visibility.Visible;
                    Module module = new Module();
                    module.Read(new IsWiXModule());
                    Module = module;
                    break;
                    

                default:
                    break;
            }

            Package package = new Package();
            package.Read(new IsWiXPackage());
            Package = package;
            PackagePropertyGridVisible = Visibility.Visible;
        }
    }
}
