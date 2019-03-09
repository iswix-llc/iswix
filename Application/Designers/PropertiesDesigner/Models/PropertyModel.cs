using FireworksFramework.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertiesDesigner.Models
{
    public class PropertyModel : ObservableObject
    {
        string _id;
        string _value;
        bool _suppressModularization;
        bool _secure;
        bool _hidden;
        bool _admin;
        public string Id { get { return _id; } set { _id = value; RaisePropertyChangedEvent("Id"); } }
        public string Value { get { return _value; }  set { _value = value; RaisePropertyChangedEvent("Value"); } }
        public bool SuppressModularization { get { return _suppressModularization; } set { _suppressModularization = value; RaisePropertyChangedEvent("SuppressModularization"); } }
        public bool Secure { get { return _secure; } set { _secure = value; RaisePropertyChangedEvent("Secure"); } }
        public bool Hidden { get { return _hidden; } set { _hidden = value; RaisePropertyChangedEvent("Hidden"); } }
        public bool Admin { get { return _admin; } set { _admin = value; RaisePropertyChangedEvent("Admin"); } }
    }
}
