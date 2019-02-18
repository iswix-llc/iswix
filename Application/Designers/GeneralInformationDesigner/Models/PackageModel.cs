using System;
using System.ComponentModel;
using System.Windows.Forms;
using IsWiXAutomationInterface;
using FireworksFramework.Managers;
using FireworksFramework.Types;

namespace Designers.GeneralInformation.Models
{
    public class PackageModel : ObservableObject
    {
        string _id;
        [CategoryAttribute("Package")]
        [Description(@"The package code GUID for a product or merge module. When compiling a product, this attribute should not be set in order to allow the package code to be generated for each build. When compiling a merge module, this attribute must be set to the modularization guid.")]
        public string Id { get { return _id; } set { _id = value; RaisePropertyChangedEvent("Id"); } }

        Int32 _installerVersion;
        [CategoryAttribute("Package")]
        [Description("The minimum version of the Windows Installer required to install this package. Take the major version of the required Windows Installer and multiply by a 100 then add the minor version of the Windows Installer. For example, \"200\" would represent Windows Installer 2.0 and \"405\" would represent Windows Installer 4.5. For 64-bit Windows Installer packages, this property must be set to 200 or greater. ")]
        public Int32 InstallerVersion { get { return _installerVersion; } set { _installerVersion = value; RaisePropertyChangedEvent("InstallerVersion"); } }

        string _manufacturer;
        [CategoryAttribute("Package")]
        [Description(@"The vendor releasing the package.")]
        public string Manufacturer { get { return _manufacturer; } set { _manufacturer = value; RaisePropertyChangedEvent("Manufacturer"); } }

        YesNo? _adminImage;
        [CategoryAttribute("Package")]
        [Description(@"Set to 'yes' if the source is an admin image.")]
        public YesNo? AdminImage { get { return _adminImage; } set { _adminImage = value; RaisePropertyChangedEvent("AdminImage"); } }

        string _comments;
        [CategoryAttribute("Package")]
        [Description(@"Optional comments for browsing.")]
        public string Comments { get { return _comments; } set { _comments = value; RaisePropertyChangedEvent("Comments"); } }

        YesNo? _compressed;
        [CategoryAttribute("Package")]
        [Description("Set to 'yes' to have compressed files in the source. This attribute cannot be set for merge modules. ")]
        public YesNo? Compressed { get { return _compressed; } set { _compressed = value; RaisePropertyChangedEvent("Compressed"); } }

        string _description;
        [CategoryAttribute("Package")]
        [Description(@"The product full name or description.")]
        public string Description { get { return _description; } set { _description = value; RaisePropertyChangedEvent("Description"); } }

        InstallPrivileges? _installPrivileges;
        [CategoryAttribute("Package")]
        [Description(@"Use this attribute to specify the priviliges required to install the package on Windows Vista and above. This attribute's value must be one of the following:
limited
Set this value to declare that the package does not require elevated privileges to install. 
elevated
Set this value to declare that the package requires elevated privileges to install. This is the default value. ")]
        public InstallPrivileges? InstallPrivileges { get { return _installPrivileges; } set { _installPrivileges = value; RaisePropertyChangedEvent("InstallPrivileges"); } }

        InstallScope? _installScope;
        [CategoryAttribute("Package")]
        [Description(@"Use this attribute to specify the installation scope of this package: per-machine or per-user. This attribute's value must be one of the following:
perMachine Set this value to declare that the package is a per-machine installation and requires elevated privileges to install. Sets the ALLUSERS property to 1. 
perUser Set this value to declare that the package is a per-user installation and does not require elevated privileges to install. Sets the package's InstallPrivileges attribute to 'limited.'")]
        public InstallScope? InstallScope { get { return _installScope; } set { _installScope = value; RaisePropertyChangedEvent("InstallScope"); } }

        string _keywords;
        [CategoryAttribute("Package")]
        [Description(@"Optional keywords for browsing.")]
        public string Keywords { get { return _keywords; } set { _keywords = value; RaisePropertyChangedEvent("Keywords"); } }

        string _languages;
        [CategoryAttribute("Package")]
        [Description(@"The list of language IDs (LCIDs) supported in the package.")]
        public string Languages { get { return _languages; } set { _languages = value; RaisePropertyChangedEvent("Languages"); } }

        Platform? _platform;
        [CategoryAttribute("Package")]
        [Description(@"The platform supported by the package. This attribute's value must be one of the following:
x86 Set this value to declare that the package is an x86 package. 
ia64 Set this value to declare that the package is an ia64 package. This value requires that the InstallerVersion property be set to 200 or greater. 
x64 Set this value to declare that the package is an x64 package. This value requires that the InstallerVersion property be set to 200 or greater. ")]
        public Platform? Platform { get { return _platform; } set { _platform = value; RaisePropertyChangedEvent("Platform"); } }

        YesNoDefault? _readOnly;
        [CategoryAttribute("Package")]
        [Description(@"The value of this attribute conveys whether the package should be opened as read-only. A database editing tool should not modify a read-only enforced database and should issue a warning at attempts to modify a read-only recommended database. ")]
        public YesNoDefault? ReadOnly { get { return _readOnly; } set { _readOnly = value; RaisePropertyChangedEvent("ReadOnly"); } }

        YesNo? _shortNames;
        [CategoryAttribute("Package")]
        [Description(@"Set to 'yes' to have short filenames in the source.")]
        public YesNo? ShortNames { get { return _shortNames; } set { _shortNames = value; RaisePropertyChangedEvent("ShortNames"); } }

        string _summaryCodepage;
        [CategoryAttribute("Package")]
        [Description(@" The code page integer value or web name for summary info strings only. See remarks for more information.   ")]
        public string SummaryCodepage { get { return _summaryCodepage; } set { _summaryCodepage = value; RaisePropertyChangedEvent("SummaryCodepage"); } }
    }
}
