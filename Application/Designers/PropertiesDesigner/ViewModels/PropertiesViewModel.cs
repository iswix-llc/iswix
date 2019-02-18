using FireworksFramework.Managers;
using FireworksFramework.Types;
using IsWiXAutomationInterface;
using PropertiesDesigner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PropertiesDesigner.ViewModels
{
    public class PropertiesViewModel : ObservableObject
    {

        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        XNamespace ns;
        IsWiXProperties _iswixProperties;

        ObservableCollection<PropertyModel> _properties = new ObservableCollection<PropertyModel>();
        public ObservableCollection<PropertyModel> Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
                RaisePropertyChangedEvent("Properties");
            }
        }

        public void Load()
        {
            ObservableCollection<PropertyModel> properties = new ObservableCollection<PropertyModel>();
            _iswixProperties = new IsWiXProperties();

            foreach (var iswixProperty in _iswixProperties)
            {
                PropertyModel propertyModel = new PropertyModel();
                propertyModel.Id = iswixProperty.Id;
                propertyModel.Value = iswixProperty.Value;
                propertyModel.Admin = iswixProperty.Admin;
                propertyModel.Hidden = iswixProperty.Hidden;
                propertyModel.Secure = iswixProperty.Secure;
                propertyModel.SuppressModularization = iswixProperty.SuppressModularization;
                propertyModel.PropertyChanged += PropertyModel_PropertyChanged;
                properties.Add(propertyModel);
            }
            Properties = properties;
        }

        private void PropertyModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }
    }
}
