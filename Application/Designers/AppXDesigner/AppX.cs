using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IsWiXAutomationInterface;

namespace AppXDesigner
{
    public partial class AppX : Component
    {
        IsWiXFGAppX _isWiXFGAppX;

        public AppX()
        {
            InitializeComponent();
        }

        public AppX(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        const string APPX = "1. AppX (Required)";
        const string APPXOPT = "2. AppX (Optional)";

        [CategoryAttribute(APPX)]
        [Description(@"Identity of the AppX package.")]
        [ReadOnly(true)]
        public string Id { get; set; }

        [CategoryAttribute(APPX)]
        [Description(@"Publisher of the AppX package. Must be in the form of a certificate name, e.g. CN=FireGiant.")]
        public string Publisher { get; set; }

        [CategoryAttribute(APPX)]
        [Description(@"Target device family for the AppX package.")]
        public TargetType Target { get; set; }

        [CategoryAttribute(APPXOPT)]
        [Description(@"The AppX package name (aka: Appx/@Id) of the parent package. Use this only when this package is to be referenced by a sparse bundle.")]
        public string MainPackage { get; set; }

        [CategoryAttribute(APPXOPT)]
        [Description(@"Overrides the description provided by the 'ARPCOMMENTS' Property. It is recommended to use the 'ARPCOMMENTS' Property instead of using this attribute.")]
        public string Description { get; set; }

        [CategoryAttribute(APPXOPT)]
        [Description(@"Overrides the display name provided by the Product/@Name. It is not recommended to use this attribute.")]
        public string DisplayName { get; set; }

        [CategoryAttribute(APPXOPT)]
        [Description(@"Overrides the manufacturer provided by the Product/@Manufacturer. It is not recommended to use this attribute.")]
        public string Manufacturer { get; set; }

        [CategoryAttribute(APPXOPT)]
        [Description(@"Overrides the package icon provided by the 'ARPPRODUCTICON' with a path to a image file. It is recommended to use the 'ARPPRODUCTICON' Property instead of using this attribute.")]
        public string LogoFile { get; set; }

        [CategoryAttribute(APPXOPT)]
        [Description(@"Overrides the version provided by the Product/@Version. It is not recommended to use this attribute.")]
        public string Version { get; set; }


        public void Read(IsWiXFGAppX isWiXFGAppX)
        {
            _isWiXFGAppX = isWiXFGAppX;

            Id = _isWiXFGAppX.Id;
            Publisher = _isWiXFGAppX.Publisher;
            Target = _isWiXFGAppX.Target;

            MainPackage = _isWiXFGAppX.MainPackage;
            Description = _isWiXFGAppX.Description;
            DisplayName = _isWiXFGAppX.DisplayName;
            Manufacturer = _isWiXFGAppX.Manufacturer;
            LogoFile = _isWiXFGAppX.LogoFile;
            Version = _isWiXFGAppX.Version;

        }

        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {

                case "Id":
                    _isWiXFGAppX.Id = Id;
                    break;
                case "Publisher":
                    _isWiXFGAppX.Publisher = Publisher;
                    break;
                case "Target":
                    _isWiXFGAppX.Target = Target;
                    break;
                case "MainPackage":
                    _isWiXFGAppX.MainPackage = MainPackage;
                    break;
                case "Description":
                    _isWiXFGAppX.Description = Description;
                    break;
                case "DisplayName":
                    _isWiXFGAppX.DisplayName = DisplayName;
                    break;
                case "Manufacturer":
                    _isWiXFGAppX.Manufacturer = Manufacturer;
                    break;
                case "LogoFile":
                    _isWiXFGAppX.LogoFile = LogoFile;
                    break;
                case "Version":
                    _isWiXFGAppX.Version = Version;
                    break;
            }
        }
    }
}
