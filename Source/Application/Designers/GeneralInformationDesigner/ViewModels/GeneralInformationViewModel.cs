using System.Xml.Linq;
using System.Windows;
using IsWiXAutomationInterface;
using Designers.GeneralInformation.Models;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using System;
using System.ComponentModel;

namespace GeneralInformationDesigner.ViewModels
{

    class GeneralInformationViewModel : ObservableObject
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        XNamespace ns;

        ProductModel _product;
        ModuleModel _module;
        PackageModel _package;
        IsWiXProduct _iswixProduct;
        IsWiXModule _iswixModule;
        IsWiXPackage _iswixPackage;

        Visibility _productPropertyGridVisible = Visibility.Hidden;
        Visibility _modulePropertyGridVisible = Visibility.Hidden;
        Visibility _packagePropertyGridVisible = Visibility.Hidden;
        public ProductModel Product { get { return _product; } set { _product = value; RaisePropertyChangedEvent("Product"); } }
        public ModuleModel Module
        {
            get
            {
                return _module;
            }
            set
            {
                _module = value;
               RaisePropertyChangedEvent("Module");
            }
        }
        public PackageModel Package { get { return _package; } set { _package = value; RaisePropertyChangedEvent("Package"); } }
        public Visibility ProductPropertyGridVisible { get { return _productPropertyGridVisible; } set { _productPropertyGridVisible = value; RaisePropertyChangedEvent("ProductPropertyGridVisible"); } }
        public Visibility ModulePropertyGridVisible
        {
            get
            {
                return _modulePropertyGridVisible;
            }
            set
            {
                _modulePropertyGridVisible = value;
                RaisePropertyChangedEvent("ModulePropertyGridVisible");
            }
        }
        public Visibility PackagePropertyGridVisible { get { return _packagePropertyGridVisible; } set { _packagePropertyGridVisible = value; RaisePropertyChangedEvent("PackagePropertyGridVisible"); } }

        public void Load()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            switch (_documentManager.Document.GetDocumentType())
            {
                case IsWiXDocumentType.Product:
                    ProductPropertyGridVisible = Visibility.Visible;
                    ModulePropertyGridVisible = Visibility.Hidden;
                    Product = LoadProduct();
                    break;

                case IsWiXDocumentType.Module:
                    ProductPropertyGridVisible = Visibility.Hidden;
                    ModulePropertyGridVisible = Visibility.Visible;
                    Module = LoadModule();
                    break;

                default:
                    break;
            }

            PackageModel package = new PackageModel();
            Package = LoadPackage();
            PackagePropertyGridVisible = Visibility.Visible;
        }

        ModuleModel LoadModule()
        {
            ModuleModel module = new ModuleModel();
            _iswixModule = new IsWiXModule();
            module.Id = _iswixModule.Id;
            module.Language = _iswixModule.Language;
            module.Codepage = _iswixModule.Codepage;
            module.Version = _iswixModule.Version;
            module.PropertyChanged += Module_PropertyChanged;
            return module;
        }

        PackageModel LoadPackage()
        {
            PackageModel package = new PackageModel();
            _iswixPackage = new IsWiXPackage();
            package.AdminImage = _iswixPackage.AdminImage;
            package.Comments = _iswixPackage.Comments;
            package.Compressed = _iswixPackage.Compressed;
            package.Description = _iswixPackage.Description;
            package.Id = _iswixPackage.Id;
            package.InstallerVersion = _iswixPackage.InstallerVersion;
            package.InstallPrivileges = _iswixPackage.InstallPrivileges;
            package.InstallScope = _iswixPackage.InstallScope;
            package.Keywords = _iswixPackage.Keywords;
            package.Languages = _iswixPackage.Languages;
            package.Manufacturer = _iswixPackage.Manufacturer;
            package.Platform = _iswixPackage.Platform;
            package.ReadOnly = _iswixPackage.ReadOnly;
            package.ShortNames = _iswixPackage.ShortNames;
            package.SummaryCodepage = _iswixPackage.SummaryCodepage;
            package.PropertyChanged += Package_PropertyChanged;
            return package;
        }

        ProductModel LoadProduct()
        {
            ProductModel product = new ProductModel();
            _iswixProduct = new IsWiXProduct();
            product.Codepage = _iswixProduct.Codepage;
            product.Id = _iswixProduct.Id;
            product.Language = _iswixProduct.Language;
            product.Manufacturer = _iswixProduct.Manufacturer;
            product.Name = _iswixProduct.Name;
            product.UpgradeCode = _iswixProduct.UpgradeCode;
            product.Version = _iswixProduct.Version;
            product.PropertyChanged += Product_PropertyChanged;
            return product;
        }

        private void Module_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Codepage":
                    _iswixModule.Codepage = Module.Codepage;
                    break;

                case "Id":
                    _iswixModule.Id = Module.Id;
                    break;

                case "Language":
                    _iswixModule.Language = Module.Language;
                    break;

                case "Version":
                    _iswixModule.Version = Module.Version;
                    break;

            }

        }
        private void Package_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AdminImage":
                    _iswixPackage.AdminImage = Package.AdminImage;
                    break;

                case "Comments":
                    _iswixPackage.Comments = Package.Comments;
                    break;

                case "Compressed":
                    if (Package.Compressed != null && _documentManager.Document.GetDocumentType() == IsWiXDocumentType.Module)
                    {
                        MessageBox.Show("The compressed attribute cannot be set in a Merge Module.");
                        Package.Compressed = null;
                    }
                    else
                    {
                        _iswixPackage.Compressed = Package.Compressed;
                    }
                    break;

                case "Description":
                    _iswixPackage.Description = Package.Description;
                    break;

                case "InstallPrivileges":
                    _iswixPackage.InstallPrivileges = Package.InstallPrivileges;
                    break;

                case "ReadOnly":
                    _iswixPackage.ReadOnly = Package.ReadOnly;
                    break;

                case "Keywords":
                    _iswixPackage.Keywords = Package.Keywords;
                    break;

                case "Languages":
                    _iswixPackage.Languages = Package.Languages;
                    break;

                case "InstallScope":
                    _iswixPackage.InstallScope = Package.InstallScope;
                    break;

                case "Platform":
                    _iswixPackage.Platform = Package.Platform;
                    break;

                case "ShortNames":

                    _iswixPackage.ShortNames = Package.ShortNames;
                    break;

                case "SummaryCodepage":
                    _iswixPackage.SummaryCodepage = Package.SummaryCodepage;
                    break;

                case "Manufacturer":
                    _iswixPackage.Manufacturer = Package.Manufacturer;
                    break;

                case "Id":
                    _iswixPackage.Id = Package.Id;
                    break;

                case "InstallerVersion":
                    _iswixPackage.InstallerVersion = Package.InstallerVersion;
                    break;
            }
        }
        private void Product_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Id":
                    _iswixProduct.Id = Product.Id;
                    break;
                case "Codepage":
                    _iswixProduct.Codepage = Product.Codepage;
                    break;
                case "Language":
                    _iswixProduct.Language = Product.Language;
                    break;
                case "Manufacturer":
                    _iswixProduct.Manufacturer = Product.Manufacturer;
                    break;
                case "Name":
                    _iswixProduct.Name = Product.Name;
                    break;
                case "UpgradeCode":
                    _iswixProduct.UpgradeCode = Product.UpgradeCode;
                    break;
                case "Version":
                    _iswixProduct.Version = Product.Version;
                    break;
            }
        }
    }
}
