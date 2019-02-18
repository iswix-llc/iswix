using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IsWiXAutomationInterface;

namespace MSIXDesigner
{
    public partial class MSIX : Component
    {
        IsWiXFGMSIX _isWiXFGMSIX;

        public MSIX()
        {
            InitializeComponent();
        }

        public MSIX(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        const string MSIXREQ = "1. MSIX (Required)";
        const string MSIXOPT = "2. MSIX (Optional)";

        [CategoryAttribute(MSIXREQ)]
        [Description(@"Identity of the MSIX package.")]
        [ReadOnly(true)]
        public string Id { get; set; }

        [CategoryAttribute(MSIXREQ)]
        [Description(@"Publisher of the MSIX package. Must be in the form of a complete subject name.")]
        public string Publisher { get; set; }

        [CategoryAttribute(MSIXREQ)]
        [Description(@"Target device family for the MSIX package.")]
        public TargetType Target { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"The minimum version of the device family that your app is targeting.")]
        public string MinimumSupportedOS { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"The maximum version of the device family that your app is targeting that you have tested it against. ")]
        public string MaximumTestedOS { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"Declares the access to protected user resources that the package requires.")]
        public string Capabilities { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"The MSIX package name (aka: MSIX/@Id) of the parent package. Use this only when this package is to be referenced by a sparse bundle.")]
        public string MainPackage { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"Overrides the description provided by the 'ARPCOMMENTS' Property. It is recommended to use the 'ARPCOMMENTS' Property instead of using this attribute.")]
        public string Description { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"Overrides the display name provided by the Product/@Name. It is not recommended to use this attribute.")]
        public string DisplayName { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"Overrides the manufacturer provided by the Product/@Manufacturer. It is not recommended to use this attribute.")]
        public string Manufacturer { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"Overrides the package icon provided by the 'ARPPRODUCTICON' with a path to a image file. It is recommended to use the 'ARPPRODUCTICON' Property instead of using this attribute.")]
        public string LogoFile { get; set; }

        [CategoryAttribute(MSIXOPT)]
        [Description(@"Overrides the version provided by the Product/@Version. It is not recommended to use this attribute.")]
        public string Version { get; set; }


        public void Read(IsWiXFGMSIX isWiXFGMSIX)
        {
            _isWiXFGMSIX = isWiXFGMSIX;

            Id = _isWiXFGMSIX.Id;
            Publisher = _isWiXFGMSIX.Publisher;
            Target = _isWiXFGMSIX.Target;

            MainPackage = _isWiXFGMSIX.MainPackage;
            Description = _isWiXFGMSIX.Description;
            DisplayName = _isWiXFGMSIX.DisplayName;
            Manufacturer = _isWiXFGMSIX.Manufacturer;
            Capabilities = _isWiXFGMSIX.Capabilities;
            MinimumSupportedOS = _isWiXFGMSIX.MinimumSupportedOS;
            MaximumTestedOS = _isWiXFGMSIX.MaximumTestedOS;
            LogoFile = _isWiXFGMSIX.LogoFile;
            Version = _isWiXFGMSIX.Version;

        }

        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {

                case "Id":
                    _isWiXFGMSIX.Id = Id;
                    break;
                case "Publisher":
                    _isWiXFGMSIX.Publisher = Publisher;
                    break;
                case "Target":
                    _isWiXFGMSIX.Target = Target;
                    break;
                case "MinimumSupportedOS":
                    _isWiXFGMSIX.MinimumSupportedOS = MinimumSupportedOS;
                    break;
                case "MaximumTestedOS":
                    _isWiXFGMSIX.MaximumTestedOS = MaximumTestedOS;
                    break;
                case "Capabilities":
                    _isWiXFGMSIX.Capabilities = Capabilities;
                    break;
                case "MainPackage":
                    _isWiXFGMSIX.MainPackage = MainPackage;
                    break;
                case "Description":
                    _isWiXFGMSIX.Description = Description;
                    break;
                case "DisplayName":
                    _isWiXFGMSIX.DisplayName = DisplayName;
                    break;
                case "Manufacturer":
                    _isWiXFGMSIX.Manufacturer = Manufacturer;
                    break;
                case "LogoFile":
                    _isWiXFGMSIX.LogoFile = LogoFile;
                    break;
                case "Version":
                    _isWiXFGMSIX.Version = Version;
                    break;
            }
        }
    }
}
