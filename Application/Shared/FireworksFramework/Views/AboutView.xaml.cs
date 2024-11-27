///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.Windows;
using System.Windows.Controls;
using FireworksFramework.ViewModels;

namespace FireworksFramework.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {
        public AboutView()
        {

            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            var pluginDescription = (PluginDescription)listView.SelectedValue;
            viewModel.SelectionChanged(pluginDescription.PluginLicense);
        }
    }
}
