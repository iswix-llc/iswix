using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using IsWiX.WiXModel.Core;
using Designers.GeneralInformation.Models;
using Wxs = IsWiX.WiXModel;

namespace GeneralInformationDesigner.ViewModels
{

    class GeneralInformationViewModel : ObservableObject
    {
        // The template's placeholder: replaced with a new GUID when the designer opens the package, as before.
        const string EmptyUpgradeCode = "00000000-0000-0000-0000-000000000000";

        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        Package4Model _package4 = new Package4Model();
        SummaryInformation4Model summaryInformation4Model = new SummaryInformation4Model();
        // Set by Load, before the grids are shown.
        Wxs.Package _package = null!;
        // Created when a field is first set and removed again when its last attribute is cleared.
        Wxs.SummaryInformation? _summaryInformation;

        Visibility _summaryInformation4PropertyGridVisible = Visibility.Hidden;
        Visibility _package4PropertyGridVisible = Visibility.Hidden;
        public Package4Model Package4 { get { return _package4; } set { _package4 = value; RaisePropertyChangedEvent("Package4"); } }
        public SummaryInformation4Model SummaryInformation4 { get { return summaryInformation4Model; } set { summaryInformation4Model = value; RaisePropertyChangedEvent("SummaryInformation4"); } }
        public Visibility SummaryInformation4PropertyGridVisible { get { return _summaryInformation4PropertyGridVisible; } set { _summaryInformation4PropertyGridVisible = value; RaisePropertyChangedEvent("SummaryInformation4PropertyGridVisible"); } }
        public Visibility Package4PropertyGridVisible { get { return _package4PropertyGridVisible; } set { _package4PropertyGridVisible = value; RaisePropertyChangedEvent("Package4PropertyGridVisible"); } }

        /// <summary>Gets whether the document is one this designer edits: a WiX package.</summary>
        public static bool IsValidDocument(XDocument? document)
        {
            var root = document?.Root;
            return root?.Name == Wxs.Wix.ElementName && root.Elements().FirstOrDefault()?.Name == Wxs.Package.ElementName;
        }

        public void Load()
        {
            var document = Wxs.WixDocument.FromXml(_documentManager.Document);
            _package = document.Root.Packages.First();
            _summaryInformation = _package.SummaryInformationElements.FirstOrDefault();

            Package4 = LoadPackage4();
            Package4PropertyGridVisible = Visibility.Visible;
            SummaryInformation4 = LoadSummaryInformation4();
            SummaryInformation4PropertyGridVisible = Visibility.Visible;
        }

        SummaryInformation4Model LoadSummaryInformation4()
        {
            SummaryInformation4Model summaryInformation4Model = new SummaryInformation4Model();
            summaryInformation4Model.Codepage = _summaryInformation?.Codepage;
            summaryInformation4Model.Description = _summaryInformation?.Description;
            summaryInformation4Model.Keywords = _summaryInformation?.Keywords;
            summaryInformation4Model.Manufacturer = _summaryInformation?.Manufacturer;
            summaryInformation4Model.PropertyChanged += SummaryInformation_PropertyChanged;
            return summaryInformation4Model;
        }

        Package4Model LoadPackage4()
        {
            if (_package.UpgradeCode == EmptyUpgradeCode)
            {
                _package.UpgradeCode = Guid.NewGuid().ToString();
            }

            // Values are shown as written, so expressions such as $(var.Version) can be seen and edited.
            Package4Model package = new Package4Model();
            package.Codepage = _package.Codepage;
            package.ProductCode = _package.ProductCode;
            package.UpgradeCode = _package.UpgradeCode;
            package.Compressed = Text(_package.Compressed, WixConverters.YesNo);
            package.Version = _package.Version;
            package.InstallerVersion = Text(_package.InstallerVersion, WixConverters.Int32);
            package.Language = Text(_package.Language, WixConverters.Int32);
            package.Manufacturer = _package.Manufacturer;
            package.Name = _package.Name;
            package.Scope = Text(_package.Scope, Wxs.WixEnumConverters.PackageScopeType);
            package.ShortNames = Text(_package.ShortNames, WixConverters.YesNo);
            package.PropertyChanged += Package4_PropertyChanged;
            return package;
        }

        // Only the attribute that changed is written, so the others keep their exact text.
        private void Package4_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Scope":
                    SetValue(Package4.Scope, Wxs.WixEnumConverters.PackageScopeType, "Scope", value => _package.Scope = value);
                    break;
                case "Manufacturer":
                    _package.Manufacturer = NullIfEmpty(Package4.Manufacturer);
                    break;
                case "Version":
                    _package.Version = NullIfEmpty(Package4.Version);
                    break;
                case "InstallerVersion":
                    SetValue(Package4.InstallerVersion, WixConverters.Int32, "InstallerVersion", value => _package.InstallerVersion = value);
                    break;
                case "Language":
                    SetValue(Package4.Language, WixConverters.Int32, "Language", value => _package.Language = value);
                    break;
                case "Codepage":
                    _package.Codepage = NullIfEmpty(Package4.Codepage);
                    break;
                case "Compressed":
                    SetValue(Package4.Compressed, WixConverters.YesNo, "Compressed", value => _package.Compressed = value);
                    break;
                case "ShortNames":
                    SetValue(Package4.ShortNames, WixConverters.YesNo, "ShortNames", value => _package.ShortNames = value);
                    break;
                case "Name":
                    _package.Name = NullIfEmpty(Package4.Name);
                    break;
                case "ProductCode":
                    _package.ProductCode = NullIfEmpty(Package4.ProductCode);
                    break;
                case "UpgradeCode":
                    _package.UpgradeCode = NullIfEmpty(Package4.UpgradeCode);
                    break;
            }
        }

        private void SummaryInformation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Manufacturer":
                    SetSummaryInformation(s => s.Manufacturer = NullIfEmpty(SummaryInformation4.Manufacturer));
                    break;
                case "Description":
                    SetSummaryInformation(s => s.Description = NullIfEmpty(SummaryInformation4.Description));
                    break;
                case "Codepage":
                    SetSummaryInformation(s => s.Codepage = NullIfEmpty(SummaryInformation4.Codepage));
                    break;
                case "Keywords":
                    SetSummaryInformation(s => s.Keywords = NullIfEmpty(SummaryInformation4.Keywords));
                    break;
            }
        }

        void SetSummaryInformation(Action<Wxs.SummaryInformation> set)
        {
            if (_summaryInformation == null)
            {
                // Added through the model, so it lands at its canonical position (first in the package).
                _summaryInformation = _package.Add(new Wxs.SummaryInformation());
            }

            set(_summaryInformation);

            if (!_summaryInformation.Xml.HasAttributes && _summaryInformation.Xml.IsEmpty)
            {
                _summaryInformation.RemoveFromParent();
                _summaryInformation = null;
            }
        }

        // Empty clears the attribute; anything else must be a valid value or a preprocessor/localization expression.
        void SetValue<T>(string? text, WixValueConverter<T> converter, string name, Action<WixValue<T>?> set)
        {
            if (string.IsNullOrEmpty(text))
            {
                set(null);
                return;
            }

            var value = converter.Parse(text);
            if (value.Kind == WixValueKind.Invalid)
            {
                MessageBox.Show($"'{text}' is not a valid {name}.");
                Load();
                return;
            }

            set(value);
        }

        static string? Text<T>(WixValue<T>? value, WixValueConverter<T> converter) => value.HasValue ? converter.ToText(value.Value) : null;

        static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;
    }
}
