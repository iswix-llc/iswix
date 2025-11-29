using FireworksFramework.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamespacesDesigner.Models
{
    public class NamespaceModel : ObservableObject
    {
        string _prefix;
        string _uri;
        bool _selected;
        public bool Selected { get { return _selected; } set { _selected = value; RaisePropertyChangedEvent("Selected"); } }
        public string Prefix { get { return _prefix; } set { _prefix = value; RaisePropertyChangedEvent("Prefix"); } }
        public string Uri { get { return _uri; } set { _uri = value; RaisePropertyChangedEvent("Uri"); } }

    }
}
