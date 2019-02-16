using Designers.GeneralInformation;
using FireworksFramework.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralInformationDesigner.ViewModels
{
    class GeneralInformationViewModel : ObservableObject
    {
        Module _module = new Module();
        Package _package = new Package();
        public Module Module { get { return _module; } set { _module = value; RaisePropertyChangedEvent("Module"); } }
        public Package Package { get { return _package; } set { _package = value; RaisePropertyChangedEvent("Package"); } }
    }
}
