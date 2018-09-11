using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using IsWiXAutomationInterface;

namespace Designers.GeneralInformation
{
    public partial class Module : Component
    {
        IsWiXModule _module;

        public Module()
        {
            InitializeComponent();
        }

        public Module(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        [CategoryAttribute("Module")]
        [Description(@"Identifiers may contain ASCII characters A-Z, a-z, digits, underscores (_), or periods (.).  Every identifier must begin with either a letter or an underscore and may not exceed 35 characters.")]
        public string Id { get; set; }

        [CategoryAttribute("Module")]
        [Description(@"The code page integer value or web name for the resulting MSM.")]
        public string Codepage { get; set; }

        [CategoryAttribute("Module")]
        [Description("The decimal language ID (LCID) of the merge module.")]
        public String Language { get; set; }
        
        [CategoryAttribute("Module")]
        [Description("The major and minor versions of the merge module.")]
        public string Version { get; set; }

        public void Read(IsWiXModule Module)
        {
            _module = Module;

            Id = _module.Id;
            Codepage = _module.Codepage;
            Language = _module.Language;
            Version = _module.Version;
        }

        public void Write(string PropertyLabel)
        {
            switch (PropertyLabel)
            {
                case "Codepage":
                    _module.Codepage = Codepage;
                    break;

                case "Id":
                    _module.Id = Id;
                    break;

                case "Language":
                    _module.Language = Language;
                    break;

                case "Version":
                    _module.Version = Version;
                    break;

            }
        }
    }
}
