using System;
using System.ComponentModel;
using System.Xml.Linq;
using System.Windows;
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

        Package4Model _package4;
        SummaryInformation4Model summaryInformation4Model;
        IsWiXPackage4 _iswixPackage4;
        IsWiXSummaryInformation4 _iswixSummaryInformation4;

        Visibility _productProperty4GridVisible = Visibility.Hidden;
        Visibility _summaryInformation4PropertyGridVisible = Visibility.Hidden;
        Visibility _package4PropertyGridVisible = Visibility.Hidden;
        public Package4Model Package4 { get { return _package4; } set { _package4 = value; RaisePropertyChangedEvent("Package4"); } }
        public SummaryInformation4Model SummaryInformation4 { get { return summaryInformation4Model; } set { summaryInformation4Model = value; RaisePropertyChangedEvent("SummaryInformation4"); } }
        public Visibility SummaryInformation4PropertyGridVisible { get { return _summaryInformation4PropertyGridVisible; } set { _summaryInformation4PropertyGridVisible = value; RaisePropertyChangedEvent("SummaryInformation4PropertyGridVisible"); } }
        public Visibility Package4PropertyGridVisible { get { return _package4PropertyGridVisible; } set { _package4PropertyGridVisible = value; RaisePropertyChangedEvent("Package4PropertyGridVisible"); } }

        public void Load()
        {
            ns = _documentManager.Document.GetWiXNameSpace();

            if (_documentManager.Document.GetDocumentType() == IsWiXDocumentType.Product)
            {
                Package4PropertyGridVisible = Visibility.Visible;
                Package4  = LoadPackage4();
            }
            SummaryInformation4 = LoadSummaryInformation4();
            SummaryInformation4PropertyGridVisible = Visibility.Visible;
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
    }
}
