using FireworksFramework.Types;
using GeneralInformationDesigner.Views;
using IsWiXAutomationInterface;
using System;
using System.ComponentModel;

namespace Designers.GeneralInformation.Models
{
    public class Package4Model : ObservableObject
    {
        string _codepage;
        [CategoryAttribute("Package")]
        [Description(@"The code page integer value or web name for the resulting MSI.")]
        public string Codepage { get { return _codepage; } set { _codepage = value; RaisePropertyChangedEvent("Codepage"); } }

        YesNo? _compressed;
        [CategoryAttribute("Package")]
        [Description("Set to 'yes' to have compressed files in the source. This attribute cannot be set for merge modules. ")]
        [Editor(typeof(CustomEditor<YesNo>), typeof(CustomEditor<YesNo>))]
        public YesNo? Compressed { get { return _compressed; } set { _compressed = value; RaisePropertyChangedEvent("Compressed"); } }

        Int32? _installerVersion;
        [CategoryAttribute("Package")]
        [Description("The minimum version of the Windows Installer required to install this package. Take the major version of the required Windows Installer and multiply by a 100 then add the minor version of the Windows Installer. For example, \"200\" would represent Windows Installer 2.0 and \"405\" would represent Windows Installer 4.5. For 64-bit Windows Installer packages, this property must be set to 200 or greater. ")]
        public Int32? InstallerVersion { get { return _installerVersion; } set { _installerVersion = value; RaisePropertyChangedEvent("InstallerVersion"); } }

        Int32 _language;
        [CategoryAttribute("Package")]
        [Description("The decimal language ID (LCID) of the merge module.")]
        public Int32 Language { get { return _language; } set { _language = value; RaisePropertyChangedEvent("Language"); } }

        string _manufacturer;
        [CategoryAttribute("Package")]
        [Description(@"The manufacturer of the product.")]
        public string Manufacturer { get { return _manufacturer; } set { _manufacturer = value; RaisePropertyChangedEvent("Manufacturer"); } }

        string _name;
        [CategoryAttribute("Package")]
        [Description(@"The descriptive name of the product.")]
        public string Name { get { return _name; } set { _name = value; RaisePropertyChangedEvent("Name"); } }

        string _productCode;
        [CategoryAttribute("Package")]
        [Description(@"The product code GUID for the product.")]
        public string ProductCode { get { return _productCode; } set { _productCode = value; RaisePropertyChangedEvent("ProductCode"); } }

        InstallScope? _scope;
        [CategoryAttribute("Package")]
        [Description(@"Use this attribute to specify the installation scope of this package: per-machine or per-user. This attribute's value must be one of the following:
perMachine Set this value to declare that the package is a per-machine installation and requires elevated privileges to install. Sets the ALLUSERS property to 1. 
perUser Set this value to declare that the package is a per-user installation and does not require elevated privileges to install. Sets the package's InstallPrivileges attribute to 'limited.'")]
        [Editor(typeof(CustomEditor<InstallScope>), typeof(CustomEditor<InstallScope>))]
        public InstallScope? Scope { get { return _scope; } set { _scope = value; RaisePropertyChangedEvent("Scope"); } }

        YesNo? _shortNames;
        [CategoryAttribute("Package")]
        [Description(@"Set to 'yes' to have short filenames in the source.")]
        [Editor(typeof(CustomEditor<YesNo>), typeof(CustomEditor<YesNo>))]
        public YesNo? ShortNames { get { return _shortNames; } set { _shortNames = value; RaisePropertyChangedEvent("ShortNames"); } }

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
