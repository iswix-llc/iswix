using FireworksFramework.Types;
using System;
using System.ComponentModel;

namespace Designers.GeneralInformation.Models
{
    class Module4Model : ObservableObject
    {
        string _id;
        string _codepage;
        string _langauge;
        string _version;
        Int32 _installerVersion;
        string _guid;

        [CategoryAttribute("Module")]
        [Description(@"Identifiers may contain ASCII characters A-Z, a-z, digits, underscores (_), or periods (.).  Every identifier must begin with either a letter or an underscore and may not exceed 35 characters.")]
        public string Id { get { return _id; } set { _id = value; RaisePropertyChangedEvent("Id"); } }

        [CategoryAttribute("Module")]
        [Description(@"The package code GUID for a product or merge module. When compiling a product, this attribute should not be set in order to allow the package code to be generated for each build. When compiling a merge module, this attribute must be set to the modularization guid.")]
        public string Guid { get { return _guid; } set { _guid = value; RaisePropertyChangedEvent("Guid"); } }

        [CategoryAttribute("Module")]
        [Description("The major and minor versions of the merge module.")]
        public string Version { get { return _version; } set { _version = value; RaisePropertyChangedEvent("Version"); } }

        [CategoryAttribute("Module")]
        [Description("The minimum version of the Windows Installer required to install this package. Take the major version of the required Windows Installer and multiply by a 100 then add the minor version of the Windows Installer. For example, \"200\" would represent Windows Installer 2.0 and \"405\" would represent Windows Installer 4.5. For 64-bit Windows Installer packages, this property must be set to 200 or greater. ")]
        public Int32 InstallerVersion { get { return _installerVersion; } set { _installerVersion = value; RaisePropertyChangedEvent("InstallerVersion"); } }

        [CategoryAttribute("Module")]
        [Description(@"The code page integer value or web name for the resulting MSM.")]
        public string Codepage { get { return _codepage; } set { _codepage = value; RaisePropertyChangedEvent("Codepage"); } }

        [CategoryAttribute("Module")]
        [Description("The decimal language ID (LCID) of the merge module.")]
        public String Language { get { return _langauge; } set { _langauge = value; RaisePropertyChangedEvent("Language"); } }

    }
}
