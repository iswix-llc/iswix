using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Linq;  
using System.Windows;
using Microsoft.Win32;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using IsWiXAutomationInterface;
using Designers.GeneralInformation.Models;

namespace GeneralInformationDesigner.ViewModels
{

    class GeneralInformationViewModel : ObservableObject
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        XNamespace ns;

        ProductModel _product;
        ModuleModel _module;
        Module4Model _module4;
        PackageModel _package;
        Package4Model _package4;
        SummaryInformation4Model summaryInformation4Model;
        IsWiXProduct _iswixProduct;
        IsWiXPackage4 _iswixPackage4;
        IsWiXModule _iswixModule;
        IsWiXModule4 _iswixModule4;
        IsWiXPackage _iswixPackage;
        IsWiXDependencies _iswixDependencies;
        IsWiXSummaryInformation4 _iswixSummaryInformation4;

        bool _removeEnabled;
        DependencyModel _selectedDependency;
        public bool RemoveEnabled { get { return _removeEnabled; } set { _removeEnabled = value; RaisePropertyChangedEvent("RemoveEnabled"); } }
        public DependencyModel SelectedDependency { get { return _selectedDependency; } set { _selectedDependency = value; RaisePropertyChangedEvent("SelectedDependency"); } }
        ObservableCollection<DependencyModel> _dependencies = new ObservableCollection<DependencyModel>();
        public ObservableCollection<DependencyModel> Dependencies { get { return _dependencies; }set { _dependencies = value; RaisePropertyChangedEvent("Dependencies"); } }
        Visibility _productPropertyGridVisible = Visibility.Hidden;
        Visibility _modulePropertyGridVisible = Visibility.Hidden;
        Visibility _module4PropertyGridVisible = Visibility.Hidden;
        Visibility _productProperty4GridVisible = Visibility.Hidden;
        Visibility _summaryInformation4PropertyGridVisible = Visibility.Hidden;
        Visibility _packagePropertyGridVisible = Visibility.Hidden;
        Visibility _package4PropertyGridVisible = Visibility.Hidden;
        public ProductModel Product { get { return _product; } set { _product = value; RaisePropertyChangedEvent("Product"); } }
        public Package4Model Package4 { get { return _package4; } set { _package4 = value; RaisePropertyChangedEvent("Package4"); } }
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
        public Module4Model Module4
        {
            get
            {
                return _module4;
            }
            set
            {
                _module4 = value;
                RaisePropertyChangedEvent("Module4");
            }
        }


        public PackageModel Package { get { return _package; } set { _package = value; RaisePropertyChangedEvent("Package"); } }
        public SummaryInformation4Model SummaryInformation4 { get { return summaryInformation4Model; } set { summaryInformation4Model = value; RaisePropertyChangedEvent("SummaryInformation4"); } }
        public Visibility ProductPropertyGridVisible { get { return _productPropertyGridVisible; } set { _productPropertyGridVisible = value; RaisePropertyChangedEvent("ProductPropertyGridVisible"); } }
        public Visibility SummaryInformation4PropertyGridVisible { get { return _summaryInformation4PropertyGridVisible; } set { _summaryInformation4PropertyGridVisible = value; RaisePropertyChangedEvent("SummaryInformation4PropertyGridVisible"); } }
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
        public Visibility Module4PropertyGridVisible
        {
            get
            {
                return _module4PropertyGridVisible;
            }
            set
            {
                _module4PropertyGridVisible = value;
                RaisePropertyChangedEvent("Module4PropertyGridVisible");
            }
        }
        public Visibility PackagePropertyGridVisible { get { return _packagePropertyGridVisible; } set { _packagePropertyGridVisible = value; RaisePropertyChangedEvent("PackagePropertyGridVisible"); } }
        public Visibility Package4PropertyGridVisible { get { return _package4PropertyGridVisible; } set { _package4PropertyGridVisible = value; RaisePropertyChangedEvent("PackagePropertyGridVisible"); } }

        public void Load()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            if (_documentManager.Document.GetWiXVersion() == WiXVersion.v3)
            {
                switch (_documentManager.Document.GetDocumentType())
                {
                    case IsWiXDocumentType.Product:
                        ProductPropertyGridVisible = Visibility.Visible;
                        ModulePropertyGridVisible = Visibility.Hidden;
                        Package4PropertyGridVisible = Visibility.Hidden;
                        Module4PropertyGridVisible = Visibility.Hidden;
                        Product = LoadProduct();
                        break;

                    case IsWiXDocumentType.Module:
                        ProductPropertyGridVisible = Visibility.Hidden;
                        ModulePropertyGridVisible = Visibility.Visible;
                        Package4PropertyGridVisible = Visibility.Hidden;
                        Module4PropertyGridVisible = Visibility.Hidden;
                        Module = LoadModule();
                        break;

                    default:
                        break;
                }

                PackageModel package = new PackageModel();
                Package = LoadPackage();
                PackagePropertyGridVisible = Visibility.Visible;
                SummaryInformation4PropertyGridVisible = Visibility.Hidden;
            }
            else
            {
                switch (_documentManager.Document.GetDocumentType())
                {
                    case IsWiXDocumentType.Product:
                        ProductPropertyGridVisible = Visibility.Hidden;
                        ModulePropertyGridVisible = Visibility.Hidden;
                        Package4PropertyGridVisible = Visibility.Visible;
                        Module4PropertyGridVisible = Visibility.Hidden;

                        Package4  = LoadPackage4();
                        break;

                    case IsWiXDocumentType.Module:
                        ProductPropertyGridVisible = Visibility.Hidden;
                        ModulePropertyGridVisible = Visibility.Hidden;
                        Package4PropertyGridVisible = Visibility.Hidden;
                        Module4PropertyGridVisible = Visibility.Visible;

                        Module4 = LoadModule4();
                        break;

                    default:
                        break;
                }
                PackagePropertyGridVisible = Visibility.Hidden;
                SummaryInformation4 = LoadSummaryInformation4();
                SummaryInformation4PropertyGridVisible = Visibility.Visible; 
            }
            Dependencies = LoadDependencies();
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

        Module4Model LoadModule4()
        {
            Module4Model module = new Module4Model();
            _iswixModule4 = new IsWiXModule4();
            module.Codepage = _iswixModule4.Codepage;
            module.Id = _iswixModule4.Id;
            module.Version = _iswixModule4.Version;
            module.Guid = _iswixModule4.Guid;
            module.InstallerVersion = _iswixModule4.InstallerVersion;
            module.Language = _iswixModule4.Language;
            module.PropertyChanged += Module4_PropertyChanged;
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

        SummaryInformation4Model LoadSummaryInformation4()
        {
            SummaryInformation4Model summaryInformation4Model = new SummaryInformation4Model();
            _iswixSummaryInformation4 = new IsWiXSummaryInformation4();
            summaryInformation4Model.Codepage = _iswixSummaryInformation4.Codepage;
            summaryInformation4Model.Description = _iswixSummaryInformation4.Description;
            summaryInformation4Model.Keywords = _iswixSummaryInformation4.Keywords;
            summaryInformation4Model.Manufacturer = _iswixSummaryInformation4.Manufacturer;
            return summaryInformation4Model;
        }

        ObservableCollection<DependencyModel> LoadDependencies()
        {
            RemoveEnabled = false;
            ObservableCollection<DependencyModel> dependencies = new ObservableCollection<DependencyModel>();
            _iswixDependencies = new IsWiXDependencies();

            foreach (var iswixDependency in _iswixDependencies)
            {
                DependencyModel dependency = new DependencyModel();
                dependency.RequiredId = iswixDependency.RequiredId;
                dependency.RequiredLanguage = iswixDependency.RequiredLanguage;
                dependency.RequiredVersion = iswixDependency.RequiredVersion;
                dependencies.Add(dependency);
            }
            return dependencies;
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

        Package4Model LoadPackage4()
        {
            Package4Model package = new Package4Model();
            _iswixPackage4 = new IsWiXPackage4();
            package.Codepage = _iswixPackage4.Codepage;
            package.ProductCode = _iswixPackage4.ProductCode;
            package.UpgradeCode = _iswixPackage4.UpgradeCode;
            package.Compressed = _iswixPackage4.Compressed;
            package.Version = _iswixPackage4.Version;
            package.InstallerVersion = _iswixPackage4.InstallerVersion;
            package.Language = _iswixPackage4.Language;
            package.Manufacturer = _iswixPackage4.Manufacturer;
            package.Name = _iswixPackage4.Name;
            package.ProductCode = _iswixPackage4.ProductCode;
            package.Scope = _iswixPackage4.Scope;
            package.ShortNames = _iswixPackage4.ShortNames;
            return package;
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

        private void Module4_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "InstallerVersion":
                    _iswixModule4.InstallerVersion = Module4.InstallerVersion;
                    break;

                case "Language":
                    _iswixModule4.Language =Module4.Language;
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
                    _iswixPackage.Compressed = Package.Compressed;
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

        public void AddDependency()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            fileDialog.FileName = "";
            fileDialog.RestoreDirectory = true;
            fileDialog.Filter = "MSI Merge Modules (*.msm)|*.msm";
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog()==true)
            {
                foreach (var fileName in fileDialog.FileNames)
                {
                    try
                    {
                        var iswixDependency = new IsWiXDependency(fileName);
                        DependencyModel dependency = new DependencyModel();
                        dependency.RequiredId = iswixDependency.RequiredId;
                        dependency.RequiredLanguage = iswixDependency.RequiredLanguage;
                        dependency.RequiredVersion = iswixDependency.RequiredVersion;
                        _iswixDependencies.Add(iswixDependency);
                        if (!Dependencies.Contains(dependency))
                        {
                            Dependencies.Add(dependency);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        public void RemoveDependency()
        {
            _iswixDependencies.Remove(new IsWiXDependency(SelectedDependency.RequiredId, SelectedDependency.RequiredLanguage, SelectedDependency.RequiredVersion));
            Dependencies.Remove(SelectedDependency);
            if(Dependencies.Count==0)
            {
                RemoveEnabled = false;
            }
        }

        public void ProcessSelectionChanged()
        {
            if(SelectedDependency!=null)
            {
                RemoveEnabled = true;
            }
        }
    }
}
