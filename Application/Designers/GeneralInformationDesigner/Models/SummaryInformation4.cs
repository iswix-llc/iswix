using FireworksFramework.Types;
using System;
using System.ComponentModel;

namespace Designers.GeneralInformation.Models
{
    public class SummaryInformationModel : ObservableObject
    {
        string _id;
        [CategoryAttribute("Product")]
        [Description(@"The product code GUID for the product.")]
        public string Id { get { return _id; } set { _id = value; RaisePropertyChangedEvent("Id"); } }

        string _codepage;
        [CategoryAttribute("Product")]
        [Description(@"The code page integer value or web name for the resulting MSI.")]
        public string Codepage { get { return _codepage; } set { _codepage = value; RaisePropertyChangedEvent("Codepage"); } }

        Int32 _language;
        [CategoryAttribute("Product")]
        [Description("The decimal language ID (LCID) of the merge module.")]
        public Int32 Language { get { return _language; } set { _language = value; RaisePropertyChangedEvent("Language"); } }

        string _manufacturer;
        [CategoryAttribute("Product")]
        [Description(@"The manufacturer of the product.")]
        public string Manufacturer { get { return _manufacturer; } set { _manufacturer = value; RaisePropertyChangedEvent("Manufacturer"); } }

        string _name;
        [CategoryAttribute("Product")]
        [Description(@"The descriptive name of the product.")]
        public string Name { get { return _name; } set { _name = value; RaisePropertyChangedEvent("Name"); } }

        string _upgradeCode;
        [CategoryAttribute("Product")]
        [Description(@"The upgrade code GUID for the product.")]
        public string UpgradeCode { get { return _upgradeCode; } set { _upgradeCode = value; RaisePropertyChangedEvent("UpgradeCode"); } }

        string _version;
        [CategoryAttribute("Product")]
        [Description(@"The product's version string.")]
        public string Version { get { return _version; } set { _version = value; RaisePropertyChangedEvent("Version"); } }
    }
}
