///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Properties;
using FireworksFramework.Types;
using FireworksFramework.Views;
using DiffPlex;
using static FireworksFramework.Types.Enums;

namespace FireworksFramework.ViewModels
{
    class FireworksViewModel : ObservableObject, IPublishable
    {
        Differences _differences;
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        PluginManager _pluginManager = PluginManager.PluginManagerInstance;

        string _templateName = string.Empty;
        string _documentExtension = string.Empty;
        string _documentDirectoryMRU = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string Text { get; set; }
        public string FilePath
        {
            get
            {
                // Empty if no parms passed, otherwise the File Path of the default document to be opened.
                string filePath;
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    filePath = args[1];
                }
                else
                {
                    filePath = string.Empty;
                }
                return filePath;
            }
        }

        bool _enableDifferencesView;
        bool _enableClose;
        bool _enableSave;
        bool _enableSaveButton;
        public bool EnableDifferencesView { get { return _enableDifferencesView; } set { _enableDifferencesView = value; RaisePropertyChangedEvent("EnableDifferencesView"); } }
        public bool EnableClose { get { return _enableClose; } set { _enableClose = value; RaisePropertyChangedEvent("EnableClose"); } }
        public bool EnableSave { get { return _enableSave; } set { _enableSave = value; RaisePropertyChangedEvent("EnableSave"); } }
        public bool EnableSaveButton { get { return _enableSaveButton; } set { _enableSaveButton = value; RaisePropertyChangedEvent("EnableSaveButton"); } }

        string _title;
        public string Title { get { return _title; } set { _title = value; RaisePropertyChangedEvent("Title"); } }

        Visibility _gridVisibility;
        public Visibility GridVisibility { get { return _gridVisibility; } set { _gridVisibility = value; RaisePropertyChangedEvent("GridVisibility"); } }

        List<IFireworksDesigner> _activeDesigners;
        public List<IFireworksDesigner> ActiveDesigners
        {
            get
            {
                return _activeDesigners;
            }
            set
            {
                _activeDesigners = value;
                RaisePropertyChangedEvent("ActiveDesigners");
            }
        }

        IFireworksDesigner _activeDesigner;
        public IFireworksDesigner ActiveDesigner
        {
            get
            {
                return _activeDesigner;
            }
            set
            {
                _activeDesigner = value;
                RaisePropertyChangedEvent("ActiveDesigner");
            }
        }

        public FireworksViewModel()
        {
            Title = GetProductName();
            // Pass object reference to document manager
            _documentManager.Subscribe("mainform", this);
            GridVisibility = Visibility.Hidden;
        }


        public void Open()
        {
            if (AskClose())
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = "";
                openFileDialog.InitialDirectory = _documentDirectoryMRU;
                openFileDialog.Filter = "All files (*.*)|*.*";
                var result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    string _documentPath = openFileDialog.FileName;
                    _documentManager.Close();
                    if (TryLoad(_documentPath))
                    {
                        _documentDirectoryMRU = new FileInfo(_documentPath).Directory.FullName;
                        Settings.Default.DocumentDirectoryMRU = _documentDirectoryMRU;
                        Settings.Default.Save();
                        _documentExtension = Path.GetExtension(_documentPath);
                    }
                }
            }
        }
        public void CreateFromTemplate(string templateRelativePath)
        {
            if (AskClose())
            {
                try
                {
                    string templatePath = Path.Combine(Path.GetDirectoryName(GetExecutableFilePath()), "Templates\\" + templateRelativePath);
                    _documentExtension = Path.GetExtension(templatePath);
                    _documentManager.UnSubscribe("mainform");
                    _documentManager.Load(templatePath);
                    if (SaveAs(_documentDirectoryMRU) == false)
                    {
                        _documentManager.Close();
                        _documentManager.Subscribe("mainform", this);
                    }
                    else
                    {
                        _documentManager.Subscribe("mainform", this);
                        _documentManager.Refresh();
                        PublishNamespace();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }
        public bool SaveAs()
        {
            return SaveAs(new FileInfo(_documentManager.DocumentPath).DirectoryName);
        }
        public bool SaveAs(string initialDirectory)
        {

            bool saved = false;
            if (_documentManager.CanSave)
            {
                var saveDialog = new SaveFileDialog();

                saveDialog.InitialDirectory = initialDirectory;
                saveDialog.FileName = Path.GetFileName(_documentManager.DocumentPath);
                saveDialog.Filter = Path.GetFileNameWithoutExtension(_templateName) + " files (*" + _documentExtension + ")|*" + _documentExtension + "|All files (*.*)|*.*";


                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        _documentManager.Save(saveDialog.FileName);
                        _documentDirectoryMRU = new FileInfo(saveDialog.FileName).Directory.FullName;
                        Settings.Default.DocumentDirectoryMRU = _documentDirectoryMRU;
                        Settings.Default.Save();

                        saved = true;
                        _documentManager.Subscribe("mainform", this);
                        _documentManager.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving. {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("The document is not ready to be saved.");
            }

            return saved;
        }
        public void Close()
        {
            if (AskClose())
            {
                ActiveDesigner = null;
                ActiveDesigners = null;
                if (_differences != null)
                {
                    _differences.Close();
                    _differences = null;
                }
                EnableSave = false;
                _documentManager.Close();
               
            }
        }
        public bool AskClose()
        {
            bool canContinue = true;
            if (_documentManager.CanSave)
            {
                if (_documentManager.DocumentState == DocumentStates.Dirty)
                {
                    MessageBoxResult result = MessageBox.Show("Would you like to save your changes?", GetProductName(), MessageBoxButton.YesNoCancel);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            Save();
                            // do stuff
                            break;
                        case MessageBoxResult.No:
                            // do stuff
                            break;
                        case MessageBoxResult.Cancel:
                            canContinue = false;
                            // do stuff
                            break;
                    }
                }
            }
            else
            {
                canContinue = false;
                MessageBoxResult result = MessageBox.Show("The document is not ready to be saved. Would you like to close without saving?", GetProductName(), MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        canContinue = true;
                        // do stuff
                        break;
                    case MessageBoxResult.No:
                        canContinue = false;
                        // do stuff
                        break;
                    case MessageBoxResult.Cancel:
                        canContinue = false;
                        // do stuff
                        break;
                }
            }
            return canContinue;
        }
        public void Save()
        {
            if (_documentManager.CanSave)
            {
                if (string.IsNullOrEmpty(_documentManager.DocumentPath))
                {
                    SaveAs(_documentDirectoryMRU);
                }
                else if (new FileInfo(_documentManager.DocumentPath).IsReadOnly == true)
                {
                    SaveAs(new FileInfo(_documentManager.DocumentPath).DirectoryName);
                }
                else
                {
                    try
                    {
                        _documentManager.Save(_documentManager.DocumentPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving. {ex.Message}");
                    }
                }
            }
        }
        void IPublishable.Publish(DocumentStates DocumentState)
        {
            switch (DocumentState)
            {
                case DocumentStates.Closed:
                    //tabControlMain.Visible = false;
                    EnableClose = false;
                    EnableSave = false;
                    //tabControlMain.SelectedIndex = 0;
                    EnableDifferencesView = false;
                    GridVisibility = Visibility.Hidden;
                    break;

                case DocumentStates.Dirty:
                    //tabControlMain.Visible = true;
                    EnableClose = true;
                    EnableSave = true;
                    EnableSaveButton = true;
                    GridVisibility = Visibility.Visible;
                    if (_documentManager.Subscribers.ContainsKey("differences"))
                    {
                        EnableDifferencesView = false;
                    }
                    else
                    {
                        EnableDifferencesView = true;
                    }
                    break;

                case DocumentStates.Clean:
                    //tabControlMain.Visible = true;
                    EnableClose = true;
                    EnableSave = true;
                    EnableSaveButton = false;
                    GridVisibility = Visibility.Visible;
                    if (_documentManager.Subscribers.ContainsKey("differences"))
                    {
                        EnableDifferencesView = false;
                    }
                    else
                    {
                        EnableDifferencesView = true;
                    }
                    break;

                default:
                    MessageBox.Show("Unkown Documentstate recieved");
                    break;
            }
        }
        void IPublishable.Publish(string DocumentPath)
        {
            if (String.IsNullOrEmpty(DocumentPath))
            {
                if (_documentManager.DocumentState == DocumentStates.Closed)
                {
                    Title = GetProductName();
                }
                else
                {
                    Title = "Untitled - " + GetProductName();
                }
            }
            else
            {
                Title = DocumentPath + " - " + GetProductName();
            }
        }
        void IPublishable.Publish(XNamespace Namespace)
        {
            PublishNamespace();
        }
        void PublishNamespace()
        {
            if (_documentManager.DocumentState != DocumentStates.Closed)
            {
                List<IFireworksDesigner> activeDesigners = new List<IFireworksDesigner>();

                foreach (var designer in _pluginManager.Designers.Values)
                {
                    if (designer.PluginType == PluginType.Designer && designer.IsValidContext())
                    {
                        activeDesigners.Add(designer as IFireworksDesigner);
                    }
                }
                ActiveDesigners = activeDesigners;
                ActiveDesigner = null;
                if (activeDesigners.Any())
                {
                    ActiveDesigner = ActiveDesigners.First();
                }
            }

        }
        public void DocumentUpdated()
        {
        }
        private string GetProductName()
        {
            return FireworksManager.FireworksManagerInstance.ProductName;
        }

        private string GetExecutablePath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private string GetExecutableFilePath()
        {
            return Assembly.GetEntryAssembly().Location;
        }


        public bool TryLoad(string filePath)
        {
            bool success = false;
            try
            {
                _documentManager.Load(filePath);
                success = true;

            }
            catch (Exception)
            {
                MessageBox.Show($"An error occurred loading '{filePath}'. Please make sure it's a valid XML file.");
            }

            return success;
        }
        public void DifferencesViewer()
        {
            _differences = new Differences();
            string executablePath = GetExecutableFilePath();
            Icon icon = Icon.ExtractAssociatedIcon(executablePath);

            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap =
                 Imaging.CreateBitmapSourceFromHBitmap(
                      hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());

            _differences.Icon = wpfBitmap;

            _differences.Title = $"{GetProductName()} - Differences";
            _differences.Show();
        }

        public void ShowAbout()
        {
            AboutView aboutView = new AboutView();
            aboutView.ShowDialog();
        }

        public bool Exit()
        {
            return AskClose();
            //{
            //    // Copy window location to app settings
            //    if (this.WindowState == FormWindowState.Normal)
            //    {
            //        Settings.Default.WindowLocation = this.Location;
            //        Settings.Default.WindowSize = this.Size;
            //        Settings.Default.Save();
            //    }
            //    if (_differnces != null)
            //    {
            //        _differnces.Close();
            //        _differnces = null;
            //    }
            //}
        }
    }
}
