using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using IsWiXAutomationInterface;

namespace WixShield.Designers.GeneralInformation
{
    public partial class Package : Component
    {
        IsWiXDocument _document;
        IsWiXPackage _package;
        string _id;

        public Package()
        {
            InitializeComponent();
        }

        public Package(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #region
        [CategoryAttribute("Package")]
        [Description(@"The package code GUID for a product or merge module. When compiling a product, this attribute should not be set in order to allow the package code to be generated for each build. When compiling a merge module, this attribute must be set to the modularization guid.")]
        public String Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _id = Guid.NewGuid().ToString("D");
                }
                else
                {
                    _id = value;
                }
            }
        }

        [CategoryAttribute("Package")]
        [Description("The minimum version of the Windows Installer required to install this package. Take the major version of the required Windows Installer and multiply by a 100 then add the minor version of the Windows Installer. For example, \"200\" would represent Windows Installer 2.0 and \"405\" would represent Windows Installer 4.5. For 64-bit Windows Installer packages, this property must be set to 200 or greater. ")]
        public Int32 InstallerVersion { get; set; }
        
        [CategoryAttribute("Package")]
        [Description(@"The vendor releasing the package.")]
        public string Manufacturer { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"Set to 'yes' if the source is an admin image.")]
        public YesNo? AdminImage { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"Optional comments for browsing.")]
        public string Comments { get; set; }

        [CategoryAttribute("Package")]
        [Description("Set to 'yes' to have compressed files in the source. This attribute cannot be set for merge modules. ")]
        public YesNo? Compressed { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"The product full name or description.")]
        public string Description { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"Use this attribute to specify the priviliges required to install the package on Windows Vista and above. This attribute's value must be one of the following:
limited
Set this value to declare that the package does not require elevated privileges to install. 
elevated
Set this value to declare that the package requires elevated privileges to install. This is the default value. ")]
        public InstallPrivileges? InstallPrivileges { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"Use this attribute to specify the installation scope of this package: per-machine or per-user. This attribute's value must be one of the following:
perMachine
Set this value to declare that the package is a per-machine installation and requires elevated privileges to install. Sets the ALLUSERS property to 1. 
perUser
Set this value to declare that the package is a per-user installation and does not require elevated privileges to install. Sets the package's InstallPrivileges attribute to 'limited.'")]
        public InstallScope? InstallScope { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"Optional keywords for browsing.")]
        public string Keywords { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"The list of language IDs (LCIDs) supported in the package.")]
        public string Languages { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"The platform supported by the package. This attribute's value must be one of the following:
x86
Set this value to declare that the package is an x86 package. 
ia64
Set this value to declare that the package is an ia64 package. This value requires that the InstallerVersion property be set to 200 or greater. 
x64
Set this value to declare that the package is an x64 package. This value requires that the InstallerVersion property be set to 200 or greater. ")]
        public Platform? Platform { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"The value of this attribute conveys whether the package should be opened as read-only. A database editing tool should not modify a read-only enforced database and should issue a warning at attempts to modify a read-only recommended database. ")]
        public YesNoDefault? ReadOnly { get; set; }

        [CategoryAttribute("Package")]
        [Description(@"Set to 'yes' to have short filenames in the source.")]
        public YesNo? ShortNames { get; set; }

        [CategoryAttribute("Package")]
        [Description(@" The code page integer value or web name for summary info strings only. See remarks for more information.   ")]
        public string SummaryCodepage { get; set; }

        #endregion

        public void Read(IsWiXDocument Document, IsWiXPackage Package )
        {
            _document = Document;
            _package = Package;

            try
            {
                AdminImage = _package.AdminImage;
                Comments = _package.Comments;
                Compressed = _package.Compressed;
                Description = _package.Description;
                InstallPrivileges = _package.InstallPrivileges;
                ReadOnly = _package.ReadOnly;
                Keywords = _package.Keywords;
                Languages = _package.Languages;
                InstallScope = _package.InstallScope;
                Platform = _package.Platform;
                ShortNames = _package.ShortNames;
                SummaryCodepage = _package.SummaryCodepage;
                Manufacturer = _package.Manufacturer;
                Id = _package.Id;
                InstallerVersion = _package.InstallerVersion;

            }
            catch (Exception)
            {
                MessageBox.Show("Error reading the Package Element.");
            }
        }

        public void Write( string PropertyLabel )
        {

            switch (PropertyLabel)
            {
                case "AdminImage":
                    _package.AdminImage = AdminImage;
                    break;

                case "Comments":
                    _package.Comments = Comments;
                    break;

                case "Compressed":
                    if (Compressed != null && _document.DocumentType == IsWiXDocumentType.Module)
                    {
                        MessageBox.Show("The compressed attribute cannot be set in a Merge Module.");
                        Compressed = null;
                    }
                    else
                    {
                        _package.Compressed = Compressed;
                    }
                    break;

                case "Description":
                    _package.Description = Description;
                    break;

                case "InstallPrivileges":
                    _package.InstallPrivileges = InstallPrivileges;
                    break;

                case "ReadOnly":
                    _package.ReadOnly = ReadOnly;
                    break;

                case "Keywords":
                    _package.Keywords = Keywords;
                    break;

                case "Languages":
                    _package.Languages = Languages;
                    break;

                case "InstallScope":
                    _package.InstallScope = InstallScope;
                    break;

                case "Platform":
                    _package.Platform = Platform;
                    break;

                case "ShortNames":

                    _package.ShortNames = ShortNames;
                    break;

                case "SummaryCodepage":
                    _package.SummaryCodepage = SummaryCodepage;
                    break;

                case "Manufacturer":
                    _package.Manufacturer = Manufacturer;
                    break;

                case "Id":
                    _package.Id = Id;
                    break;

                case "InstallerVersion":
                    _package.InstallerVersion = InstallerVersion;
                    break;
            }
        }
    }
}
