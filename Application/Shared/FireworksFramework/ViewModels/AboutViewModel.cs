///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;
using FireworksFramework.Managers;
using FireworksFramework.Types;

namespace FireworksFramework.ViewModels
{
    class AboutViewModel : ObservableObject
    {
        BitmapImage _brandingBitmap;
        string _title;
        string _license;
        public BitmapImage BrandingBitmap { get { return _brandingBitmap; } set { _brandingBitmap = value; RaisePropertyChangedEvent("BrandingBitmap"); } }
        public string Title { get { return _title; } set { _title = value; RaisePropertyChangedEvent("Title"); } }
        public string License { get { return _license; } set { _license = value; RaisePropertyChangedEvent("License"); } }

        List<PluginDescription> _pluginDescriptions = new List<PluginDescription>();
        public List<PluginDescription> PluginDescriptions
        {
            get
            {
                return _pluginDescriptions;
            }
            set
            {
                _pluginDescriptions = value;
                RaisePropertyChangedEvent("PluginDescriptions");
            }
        }

        PluginDescription _selectedPluginDescription;
        public PluginDescription SelectedPluginDescription { get { return _selectedPluginDescription; } set { _selectedPluginDescription = value; RaisePropertyChangedEvent("SelectedPluginDescription"); } }


        public AboutViewModel()
        {
            BrandingBitmap = FireworksManager.FireworksManagerInstance.BrandingBitMap;
            Title = FireworksManager.FireworksManagerInstance.ProductName;

            List<PluginDescription> pluginDescriptions = new List<PluginDescription>();
            foreach (var designer in PluginManager.PluginManagerInstance.Designers.Values)
            {
                PluginDescription pluginDescription = new PluginDescription();
                pluginDescription.PluginName = designer.PluginName;
                pluginDescription.PluginAssembly = designer.GetType().Assembly.ToString();
                pluginDescription.PluginImage = designer.PluginImage;
                pluginDescription.PluginLicense = designer.PluginInformation;
                pluginDescriptions.Add(pluginDescription);
            }
            PluginDescriptions = pluginDescriptions;
            if (PluginDescriptions.Any())
            {
                SelectedPluginDescription = PluginDescriptions.First();
            }
        }

        public void SelectionChanged(string license)
        {
            License = license;
        }
    }
    class PluginDescription
    {
        public Image PluginImage { get; set; }
        public string PluginName { get; set; }
        public string PluginAssembly { get; set; }
        public string PluginLicense { get; set; }
    }



}
