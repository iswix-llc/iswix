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
using System.Windows;

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

        bool _enableRemove = false;

        public bool EnableRemove
        {
            get
            {
                return _enableRemove;
            }
            set
            {
                _enableRemove = value;
                RaisePropertyChangedEvent("EnableRemove");
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
            EnableRemove = false;
        }

        private void PropertyModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool dupe = false;
            PropertyModel newProperty = sender as PropertyModel;
            
            List<string> keysFound = new List<string>();
            foreach (var property in _properties)
            {
                if(keysFound.Contains(property.Id))
                {
                    dupe = true;
                    break;
                }
                else
                {
                    keysFound.Add(property.Id);
                }
            }

            if(dupe)
            {
                MessageBox.Show("Dupe");
                Load();
            }
            else
            {
                if(_iswixProperties.Any(s=>s.Id==newProperty.Id))
                {
                    IsWiXProperty iswixProperty = _iswixProperties.First(s=>s.Id==newProperty.Id);
                    iswixProperty.Value = newProperty.Value;
                    iswixProperty.Secure = newProperty.Secure;
                    iswixProperty.Admin = newProperty.Admin;
                    iswixProperty.SuppressModularization = newProperty.SuppressModularization;
                    iswixProperty.Hidden = newProperty.Hidden;
                }
                else
                {
                    string oldId = string.Empty;
                    foreach (var iswixProp in _iswixProperties)
                    {
                        if(!_properties.Any(s=>s.Id== iswixProp.Id))
                        {
                            oldId = iswixProp.Id;
                            break;
                        }
                    }
                    IsWiXProperty iswixProperty = _iswixProperties.First(s => s.Id == oldId);
                    iswixProperty.Id = newProperty.Id;
                    iswixProperty.Value = newProperty.Value;
                    iswixProperty.Secure = newProperty.Secure;
                    iswixProperty.Admin = newProperty.Admin;
                    iswixProperty.SuppressModularization = newProperty.SuppressModularization;
                    iswixProperty.Hidden = newProperty.Hidden;
                }
                _iswixProperties.SortXML();
            }
        }

        public void Add()
        {
            bool added = false;
            Int16 index = 0;

            do
            {
                index++;
                string proposedKey = "NewProperty" + index.ToString();

                if (!_iswixProperties.Any(s=> s.Id == proposedKey))
                {
                    IsWiXProperty iswixProperty = _iswixProperties.Create("NewProperty" + index.ToString());
                    PropertyModel propertyModel = new PropertyModel();
                    propertyModel.Id = iswixProperty.Id;
                    propertyModel.Value = iswixProperty.Value;
                    propertyModel.Admin = iswixProperty.Admin;
                    propertyModel.Hidden = iswixProperty.Hidden;
                    propertyModel.Secure = iswixProperty.Secure;
                    propertyModel.SuppressModularization = iswixProperty.SuppressModularization;
                    propertyModel.PropertyChanged += PropertyModel_PropertyChanged;
                    added = true;
                    _iswixProperties.SortXML();
                    Properties.Add(propertyModel);
                }
            }
            while (added == false);
        }

        public void Remove(PropertyModel selectedItem)
        {
            IsWiXProperty iswixProperty = _iswixProperties.First(s => s.Id == selectedItem.Id);
            iswixProperty.Delete();
            Properties.Remove(selectedItem);
        }
    }
}
