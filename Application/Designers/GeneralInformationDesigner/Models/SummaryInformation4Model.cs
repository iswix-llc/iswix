using FireworksFramework.Types;
using System;
using System.ComponentModel;

namespace Designers.GeneralInformation.Models
{
    public class SummaryInformation4Model : ObservableObject
    {

        string _codepage;
        [CategoryAttribute("SummaryInformation")]
        [Description(@"The code page integer value or web name for summary info strings only.")]
        public string Codepage { get { return _codepage; } set { _codepage = value; RaisePropertyChangedEvent("Codepage"); } }


        string _description;
        [CategoryAttribute("SummaryInformation")]
        [Description(@"The product full name or description.")]
        public string Description { get { return _description; } set { _description = value; RaisePropertyChangedEvent("Description"); } }

        string _keywords;
        [CategoryAttribute("SummaryInformation")]
        [Description(@"Optional keywords for browsing.")]
        public string Keywords { get { return _keywords; } set { _keywords = value; RaisePropertyChangedEvent("Keywords"); } }

        string _manufacturer;
        [CategoryAttribute("SummaryInformation")]
        [Description(@"The code page integer value or web name for the resulting MSI.")]
        public string Manufacturer { get { return _manufacturer; } set { _manufacturer = value; RaisePropertyChangedEvent("Manufacturer"); } }
    }
}
