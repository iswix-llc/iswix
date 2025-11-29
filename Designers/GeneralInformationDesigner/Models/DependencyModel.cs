using FireworksFramework.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designers.GeneralInformation.Models
{
    class DependencyModel : ObservableObject
    {
        string _requiredId;
        string _requiredLanguage;
        string _requiredVersion;

        public string RequiredId { get { return _requiredId; } set { _requiredId = value; RaisePropertyChangedEvent("RequiredId"); } }
        public string RequiredLanguage { get { return _requiredLanguage; } set { _requiredLanguage = value; RaisePropertyChangedEvent("RequiredLanguage"); } }
        public String RequiredVersion { get { return _requiredVersion; } set { _requiredVersion = value; RaisePropertyChangedEvent("RequiredVersion"); } }

        public override bool Equals(object obj)
        {
            bool equals = false;
            DependencyModel otherDependencyModel = obj as DependencyModel;
            if (otherDependencyModel != null)
            {
                if (RequiredId == otherDependencyModel.RequiredId &&
                    RequiredLanguage == otherDependencyModel.RequiredLanguage &&
                    RequiredVersion == otherDependencyModel.RequiredVersion)
                {
                    equals = true;
                }
            }
            return equals;
        }
    }
}
