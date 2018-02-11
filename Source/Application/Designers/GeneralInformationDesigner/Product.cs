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
    public partial class Product : Component
    {
        IsWiXProduct _product;
        string _upgradeCode;

        public Product()
        {
            InitializeComponent();
        }

        public Product(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        [CategoryAttribute("Product")]
        [Description(@"The product code GUID for the product.")]
        public string Id { get; set; }

        [CategoryAttribute("Product")]
        [Description(@"The code page integer value or web name for the resulting MSI.")]
        public string Codepage { get; set; }

        [CategoryAttribute("Product")]
        [Description("The decimal language ID (LCID) of the merge module.")]
        public Int32 Language { get; set; }

        [CategoryAttribute("Product")]
        [Description(@"The manufacturer of the product.")]
        public string Manufacturer { get; set; }

        [CategoryAttribute("Product")]
        [Description(@"The descriptive name of the product.")]
        public string Name { get; set; }

        [CategoryAttribute("Product")]
        [Description(@"The upgrade code GUID for the product.")]
        public string UpgradeCode
        {
            get
            {
                return _upgradeCode;
            }
            set
            {
                if (string.IsNullOrEmpty(_upgradeCode))
                {
                    _upgradeCode = Guid.NewGuid().ToString("D");
                }
                else
                {
                    _upgradeCode = value;
                }
            }
        }

        [CategoryAttribute("Product")]
        [Description(@"The product's version string.")]
        public string Version { get; set; }

        public void Read(IsWiXProduct Product )
        {
            _product = Product;
            Id = _product.Id;
            Codepage = _product.Codepage;
            Language = _product.Language;
            Manufacturer = _product.Manufacturer;
            Name = _product.Name;
            UpgradeCode = _product.UpgradeCode;
            Version = _product.Version;
        }

        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {
                case "Id":
                    _product.Id = Id;
                    break;

                case "Codepage":
                    _product.Codepage = Codepage;
                    break;

                case "Language":
                    _product.Language = Language;
                    break;

                case "Manufacturer":
                    _product.Manufacturer = Manufacturer;
                    break;

                case "Name":
                    _product.Name = Name;
                    break;
                case "UpgradeCode":
                    _product.UpgradeCode = UpgradeCode;
                    break;
                case "Version":
                    _product.Version = Version;
                    break;
            }
        }
    }
}
