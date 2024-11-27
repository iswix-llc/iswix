///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using FireworksFramework.Managers;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FireworksFramework.Views
{
    /// <summary>
    /// Interaction logic for FireworksView.xaml
    /// </summary>
    public partial class FireworksView : Window
    {
        System.Windows.Forms.Integration.WindowsFormsHost _host;
      
        public FireworksView()
        {
            try
            {
                Environment.CurrentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            }
            catch (Exception)
            {
            }

            InitializeComponent();
            PopulateMenuStrip();


            _host = new System.Windows.Forms.Integration.WindowsFormsHost();

            if (!string.IsNullOrEmpty(FireworksManager.FireworksManagerInstance.FilePath))
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CommonCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PopulateMenuStrip()
        {
            DirectoryInfo di = new DirectoryInfo("Templates");
            if (di.Exists)
            {
                foreach (var dir in di.GetDirectories())
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = dir.Name;
                    MenuItem menuItem2 = new MenuItem();
                    menuItem2.Header = dir.Name;

                    menuItemNew.Items.Add(menuItem);
                    toolbarMenuItemNew.Items.Add(menuItem2);

                    foreach (var file in dir.GetFiles())
                    {
                        MenuItem subMenuItem = new MenuItem();
                        subMenuItem.Header = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        subMenuItem.Click += new RoutedEventHandler(ToolbarMenuItemNew_Click);
                        subMenuItem.Tag = System.IO.Path.Combine(dir.Name, file.Name);
                        menuItem.Items.Add(subMenuItem);

                        MenuItem subMenuItem2 = new MenuItem();
                        subMenuItem2.Header = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        subMenuItem2.Click += new RoutedEventHandler(ToolbarMenuItemNew_Click);
                        subMenuItem2.Tag = System.IO.Path.Combine(dir.Name, file.Name);
                        menuItem2.Items.Add(subMenuItem2);
                    }
                }
            }
        }

        private void ToolbarMenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            viewModel.CreateFromTemplate(menuItem.Tag.ToString());
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Open();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Save();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SaveAs();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }

        private void DifferencesViewer_Click(object sender, RoutedEventArgs e)
        {
            viewModel.DifferencesViewer();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowAbout();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!viewModel.Exit())
            {
                e.Cancel = true;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.gridControlContainer.Children.Clear();
            if (viewModel.ActiveDesigner != null)
            {
                System.Windows.Forms.Control winFormControl = viewModel.ActiveDesigner as System.Windows.Forms.Control;
                UIElement wpfControl = viewModel.ActiveDesigner as UIElement;

                if(winFormControl != null)
                {
                    _host.Child = winFormControl;
                    this.gridControlContainer.Children.Add(_host);

                }

                if(wpfControl != null)
                {
                    this.gridControlContainer.Children.Add(wpfControl);
                }
                
                viewModel.ActiveDesigner.LoadData();
            }
        }

        private void CommandBinding_Save(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FireworksManager.FireworksManagerInstance.FilePath))
            {
                viewModel.TryLoad(FireworksManager.FireworksManagerInstance.FilePath);
            }
        }
    }
}