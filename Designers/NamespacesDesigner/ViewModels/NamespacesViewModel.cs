using FireworksFramework.Managers;
using FireworksFramework.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IsWiXAutomationInterface;
using NamespacesDesigner.Models;
using System.Collections.ObjectModel;

namespace NamespacesDesigner.ViewModels
{
    public class NamespacesViewModel : ObservableObject
    {

        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        WiXNamespaces _iswixNamespaces;

        ObservableCollection<NamespaceModel> _namespaces = new ObservableCollection<NamespaceModel>();
        public ObservableCollection<NamespaceModel> Namespaces
        {
            get
            {
                return _namespaces;
            }
            set
            {
                _namespaces = value;
                RaisePropertyChangedEvent("Namespaces");
            }
        }

        public void LoadData()
        {
            ObservableCollection<NamespaceModel> namespaces = new ObservableCollection<NamespaceModel>();
            _iswixNamespaces = new WiXNamespaces();

            foreach (var iswixNamespace in _iswixNamespaces.PossibleNamespaces)
            {
                NamespaceModel namespaceModel = new NamespaceModel();
                if (_documentManager.Document.NameSpaces().ContainsKey(iswixNamespace.Key))
                {
                    namespaceModel.Selected = true;
                }
                namespaceModel.Prefix = iswixNamespace.Key;
                namespaceModel.Uri = iswixNamespace.Value;
                namespaceModel.PropertyChanged += PropertyModel_PropertyChanged;
                namespaces.Add(namespaceModel);
            }
            Namespaces = namespaces;

        }

        private void PropertyModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NamespaceModel namespaceModel = sender as NamespaceModel;
            if(namespaceModel.Selected)
            {
                _iswixNamespaces.Add(namespaceModel.Prefix, namespaceModel.Uri);
            }
            else
            {
                _iswixNamespaces.Remove(namespaceModel.Prefix);
            }

        }
    }
}
