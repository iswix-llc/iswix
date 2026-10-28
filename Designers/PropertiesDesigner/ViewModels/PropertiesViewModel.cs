using FireworksFramework.Managers;
using FireworksFramework.Types;
using IsWiX.WiXModel.Core;
using PropertiesDesigner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Wxs = IsWiX.WiXModel;

namespace PropertiesDesigner.ViewModels
{
    public class PropertiesViewModel : ObservableObject
    {
        readonly DocumentManager _documentManager;

        public PropertiesViewModel(DocumentManager documentManager)
        {
            _documentManager = documentManager;
        }

        // The <Package> or <Fragment> whose properties are edited: the document's first element, as before.
        WixElement? _container;

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

        /// <summary>Gets whether the document is one this designer edits: a WiX package or fragment.</summary>
        public static bool IsValidDocument(System.Xml.Linq.XDocument? document)
        {
            var root = document?.Root;
            var first = root?.Elements().FirstOrDefault();
            return root?.Name == Wxs.Wix.ElementName &&
                (first?.Name == Wxs.Package.ElementName || first?.Name == Wxs.Fragment.ElementName);
        }

        public void Load()
        {
            var document = Wxs.WixDocument.FromXml(_documentManager.Document);
            _container = document.Root.Children.FirstOrDefault();

            ObservableCollection<PropertyModel> properties = new ObservableCollection<PropertyModel>();
            foreach (var property in GetProperties())
            {
                properties.Add(CreateModel(property));
            }
            Properties = properties;
            EnableRemove = false;
        }

        private void PropertyModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PropertyModel model)
            {
                return;
            }

            var element = model.Element;
            switch (e.PropertyName)
            {
                case nameof(PropertyModel.Id):
                    if (string.IsNullOrEmpty(model.Id) || _properties.Count(p => p.Id == model.Id) > 1)
                    {
                        MessageBox.Show(string.IsNullOrEmpty(model.Id)
                            ? "A property needs an Id."
                            : $"A property with the Id '{model.Id}' already exists.");
                        Load();
                        return;
                    }

                    // A new Id means a new canonical position: take the property out and add it back.
                    element.Id = model.Id;
                    element.RemoveFromParent();
                    AddProperty(element);
                    break;

                // Only the attribute that changed is written, so expressions such as $(var.X) in the others survive.
                case nameof(PropertyModel.Value):
                    element.Value = string.IsNullOrEmpty(model.Value) ? null : model.Value;
                    break;
                case nameof(PropertyModel.Secure):
                    element.Secure = YesOrAbsent(model.Secure);
                    break;
                case nameof(PropertyModel.Admin):
                    element.Admin = YesOrAbsent(model.Admin);
                    break;
                case nameof(PropertyModel.Hidden):
                    element.Hidden = YesOrAbsent(model.Hidden);
                    break;
                case nameof(PropertyModel.SuppressModularization):
                    element.SuppressModularization = YesOrAbsent(model.SuppressModularization);
                    break;
            }
        }

        public void Add()
        {
            var existing = new HashSet<string>(GetProperties().Select(p => p.Id ?? string.Empty), StringComparer.Ordinal);
            var index = 1;
            while (existing.Contains("NewProperty" + index))
            {
                index++;
            }

            // Added through the model, so it lands at its canonical position (see DocumentManager.Load).
            var property = new Wxs.Property { Id = "NewProperty" + index };
            AddProperty(property);
            Properties.Add(CreateModel(property));
        }

        // All the properties given: a selection of several is removed whole, not just its first.
        public void Remove(IEnumerable<PropertyModel> selectedItems)
        {
            foreach (var selectedItem in selectedItems.ToList())
            {
                selectedItem.Element.RemoveFromParent();
                Properties.Remove(selectedItem);
            }
        }

        private PropertyModel CreateModel(Wxs.Property property)
        {
            PropertyModel propertyModel = new PropertyModel();
            propertyModel.Id = property.Id ?? string.Empty;
            propertyModel.Value = property.Value ?? string.Empty;
            propertyModel.Admin = IsYes(property.Admin);
            propertyModel.Hidden = IsYes(property.Hidden);
            propertyModel.Secure = IsYes(property.Secure);
            propertyModel.SuppressModularization = IsYes(property.SuppressModularization);
            propertyModel.Element = property;
            propertyModel.PropertyChanged += PropertyModel_PropertyChanged;
            return propertyModel;
        }

        private IEnumerable<Wxs.Property> GetProperties() => _container switch
        {
            Wxs.Package package => package.Properties,
            Wxs.Fragment fragment => fragment.Properties,
            _ => Enumerable.Empty<Wxs.Property>(),
        };

        private void AddProperty(Wxs.Property property)
        {
            switch (_container)
            {
                case Wxs.Package package:
                    package.Add(property);
                    break;
                case Wxs.Fragment fragment:
                    fragment.Add(property);
                    break;
            }
        }

        // As before: "yes"/"true" read as set; setting writes "yes", clearing removes the attribute.
        private static bool IsYes(WixValue<bool>? value) => value is { IsLiteral: true } literal && literal.Value;

        private static WixValue<bool>? YesOrAbsent(bool value) => value ? WixValue<bool>.FromLiteral(true) : null;
    }
}
