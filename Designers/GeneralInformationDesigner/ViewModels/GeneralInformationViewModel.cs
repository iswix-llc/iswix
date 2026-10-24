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

        Module4Model _module4;
        Package4Model _package4;
        SummaryInformation4Model summaryInformation4Model;
        IsWiXPackage4 _iswixPackage4;
        IsWiXModule4 _iswixModule4;
        IsWiXDependencies _iswixDependencies;
        IsWiXSummaryInformation4 _iswixSummaryInformation4;

        bool _removeEnabled;
        DependencyModel _selectedDependency;
        public bool RemoveEnabled { get { return _removeEnabled; } set { _removeEnabled = value; RaisePropertyChangedEvent("RemoveEnabled"); } }
        public DependencyModel SelectedDependency { get { return _selectedDependency; } set { _selectedDependency = value; RaisePropertyChangedEvent("SelectedDependency"); } }
        ObservableCollection<DependencyModel> _dependencies = new ObservableCollection<DependencyModel>();
        public ObservableCollection<DependencyModel> Dependencies { get { return _dependencies; }set { _dependencies = value; RaisePropertyChangedEvent("Dependencies"); } }
        Visibility _module4PropertyGridVisible = Visibility.Hidden;
        Visibility _productProperty4GridVisible = Visibility.Hidden;
        Visibility _summaryInformation4PropertyGridVisible = Visibility.Hidden;
        Visibility _package4PropertyGridVisible = Visibility.Hidden;
        public Package4Model Package4 { get { return _package4; } set { _package4 = value; RaisePropertyChangedEvent("Package4"); } }
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
        public SummaryInformation4Model SummaryInformation4 { get { return summaryInformation4Model; } set { summaryInformation4Model = value; RaisePropertyChangedEvent("SummaryInformation4"); } }
        public Visibility SummaryInformation4PropertyGridVisible { get { return _summaryInformation4PropertyGridVisible; } set { _summaryInformation4PropertyGridVisible = value; RaisePropertyChangedEvent("SummaryInformation4PropertyGridVisible"); } }
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
        public Visibility Package4PropertyGridVisible { get { return _package4PropertyGridVisible; } set { _package4PropertyGridVisible = value; RaisePropertyChangedEvent("Package4PropertyGridVisible"); } }

        public void Load()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            switch (_documentManager.Document.GetDocumentType())
            {
                case IsWiXDocumentType.Product:
                    Package4PropertyGridVisible = Visibility.Visible;
                    Module4PropertyGridVisible = Visibility.Hidden;

                    Package4  = LoadPackage4();
                    break;

                case IsWiXDocumentType.Module:
                    Package4PropertyGridVisible = Visibility.Hidden;
                    Module4PropertyGridVisible = Visibility.Visible;

                    Module4 = LoadModule4();
                    break;

                default:
                    break;
            }
            SummaryInformation4 = LoadSummaryInformation4();
            SummaryInformation4PropertyGridVisible = Visibility.Visible;
            Dependencies = LoadDependencies();
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

        SummaryInformation4Model LoadSummaryInformation4()
        {
            SummaryInformation4Model summaryInformation4Model = new SummaryInformation4Model();
            _iswixSummaryInformation4 = new IsWiXSummaryInformation4();
            summaryInformation4Model.Codepage = _iswixSummaryInformation4.Codepage;
            summaryInformation4Model.Description = _iswixSummaryInformation4.Description;
            summaryInformation4Model.Keywords = _iswixSummaryInformation4.Keywords;
            summaryInformation4Model.Manufacturer = _iswixSummaryInformation4.Manufacturer;
            summaryInformation4Model.PropertyChanged += SummaryInformation_PropertyChanged;
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
            package.PropertyChanged += Package4_PropertyChanged;
            return package;
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
                case "Version":
                    _iswixModule4.Version = Module4.Version;
                    break;
                case "Codepage":
                    _iswixModule4.Codepage = Module4.Codepage;
                    break;
                case "Id":
                    _iswixModule4.Id = Module4.Id;
                    break;
                case "Guid":
                    _iswixModule4.Guid = Module4.Guid;
                    break;
            }

        }

        private void Package4_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Scope":
                    _iswixPackage4.Scope = Package4.Scope;
                    break;
                case "Manufacturer":
                    _iswixPackage4.Manufacturer = Package4.Manufacturer;
                    break;
                case "Version":
                    _iswixPackage4.Version = Package4.Version;
                    break;
                case "InstallerVersion":
                    _iswixPackage4.InstallerVersion = Package4.InstallerVersion;
                    break;
                case "Language":
                    _iswixPackage4.Language = Package4.Language;
                    break;
                case "Codepage":
                    _iswixPackage4.Codepage = Package4.Codepage;
                    break;
                case "Compressed":
                    _iswixPackage4.Compressed = Package4.Compressed;
                    break;
                case "ShortNames":
                    _iswixPackage4.ShortNames = Package4.ShortNames;
                    break;
                case "Name":
                    _iswixPackage4.Name = Package4.Name;
                    break;
                case "ProductCode":
                    _iswixPackage4.ProductCode = Package4.ProductCode;
                    break;
                case "UpgradeCode":
                    _iswixPackage4.UpgradeCode = Package4.UpgradeCode;
                    break;
            }
        }

        private void SummaryInformation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Manufacturer":
                    _iswixSummaryInformation4.Manufacturer = SummaryInformation4.Manufacturer;
                    break;
                case "Description":
                    _iswixSummaryInformation4.Description = SummaryInformation4.Description;
                    break;
                case "Codepage":
                    _iswixSummaryInformation4.Codepage = SummaryInformation4.Codepage;
                    break;
                case "Keywords":
                    _iswixSummaryInformation4.Keywords = SummaryInformation4.Keywords;
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
