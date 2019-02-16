using FireworksFramework.Types;
using System;
using System.ComponentModel;

namespace Designers.GeneralInformation.Models
{
     class ModuleModel : ObservableObject
    {
        string _id;
        string _codepage;
        string _langauge;
        string _version;

        [CategoryAttribute("Module")]
        [Description(@"Identifiers may contain ASCII characters A-Z, a-z, digits, underscores (_), or periods (.).  Every identifier must begin with either a letter or an underscore and may not exceed 35 characters.")]
        public string Id{get{return _id;}set { _id = value; RaisePropertyChangedEvent("Id"); } }

        [CategoryAttribute("Module")]
        [Description(@"The code page integer value or web name for the resulting MSM.")]
        public string Codepage{get{return _codepage;}set { _codepage = value; RaisePropertyChangedEvent("CodePage"); } }

        [CategoryAttribute("Module")]
        [Description("The decimal language ID (LCID) of the merge module.")]
        public String Language { get { return _langauge; } set { _langauge = value; RaisePropertyChangedEvent("Language"); } }

        [CategoryAttribute("Module")]
        [Description("The major and minor versions of the merge module.")]
        public string Version { get { return _version; } set { _version = value; RaisePropertyChangedEvent("Version"); } }
    }
}
