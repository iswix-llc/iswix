///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using FireworksFramework.Properties;
using FireworksFramework.Types;
using Microsoft.Win32;

namespace FireworksFramework.Managers
{
    public partial class DocumentManager : Component, IDocumentManager
    {
        DocumentStates _documentState = DocumentStates.Closed;
        XDocument       _document;
        string          _documentPath = string.Empty;
        string          _prevdocumentPath = string.Empty;
        IPublishable    _subscriber;
        XNamespace _previousNamespace;
        string _documentExtension = string.Empty;
        string _templateName = string.Empty;
        string _documentDirectoryMRU;
        XmlSchemaSet _schemas = new XmlSchemaSet();
        bool _canSave = true;

        public void Subscribe( IPublishable subscriber )
        {
            _subscriber = subscriber;
        }

        public void UnSubscribe()
        {
            _subscriber = null;
        }

        public DocumentManager(IContainer container)
        {
            InitializeComponent();

            _documentDirectoryMRU  = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

            try
            {
                foreach (var file in new DirectoryInfo("Schemas").GetFiles("*.xsd"))
                {
                    _schemas.Add(null, file.FullName);
                }
                _schemas.CompilationSettings.EnableUpaCheck = true;
                _schemas.Compile();
            }
            catch (Exception)
            {
            }

            try
            {
                string settingsMRU = Properties.Settings.Default.DocumentDirectoryMRU;
                if (Directory.Exists(settingsMRU))
                {
                    _documentDirectoryMRU = settingsMRU;
                }
            }
            catch(Exception)
            {
            }

            _document = new XDocument();
        }

        public DocumentStates DocumentState
        {
            get
            {
                return _documentState;
            }
        }

        public XNamespace DefaultNamespace
        {
            get
            {
                return NameSpaces[""];
            }
        }

        public Dictionary<string, XNamespace> NameSpaces
        {
            get
            {
                var schemas = new Dictionary<string, XNamespace>();

                try
                {
                    var result = _document.Root.Attributes().
                            Where(a => a.IsNamespaceDeclaration).
                            GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                                    a => XNamespace.Get(a.Value)).
                                    ToDictionary(g => g.Key, g => g.First());
                    foreach (var item in result)
                    {
                        schemas.Add(item.Key, item.Value);
                    }
                }
                catch (Exception)
                {
                    schemas.Add("", XNamespace.None);
                }

                if (schemas.Count.Equals(0))
                {
                    schemas.Add("", XNamespace.None);
                }
                return schemas;
            }
        }


        public XDocument Document
        {
            get
            {
                return _document;
            }
        }

        public void Close()
        {
            if (_canSave)
            {
                if (AskClose())
                {
                    ClearDocument();
                }
            }
            else
            {
                MessageBox.Show("This document is not ready to be closed.");
            }
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

                if (result == DialogResult.OK)
                {
                    try
                    {
                        XDocument _tmpDocument = XDocument.Load(openFileDialog.FileName);
                        _documentPath = openFileDialog.FileName;
                        _document = XDocument.Load(_documentPath);
                        _documentState = DocumentStates.Clean;
                        _document.Changed += new EventHandler<XObjectChangeEventArgs>(DocumentChanged);
                        PublishToSubscriber();
                        _subscriber.Publish(DefaultNamespace);
                        _subscriber.PublishDocumentChange();
                        UpdateMRU();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
//                        ClearDocument();
                    }
                }
            }
        }

        public void OpenFilePath(string filePath )
        {
            try
            {
                XDocument _tmpDocument = XDocument.Load(filePath);
                _documentPath = filePath;
                _document = _tmpDocument;
                _documentState = DocumentStates.Clean;
                _document.Changed += new EventHandler<XObjectChangeEventArgs>(DocumentChanged);
                PublishToSubscriber();

            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred loading '{filePath}'. Please make sure it's a valid XML file.");
                ClearDocument();
            }
        }

        public bool Save()
        {
            bool status = false;

            if (_canSave)
            {

                if (new FileInfo(_documentPath).IsReadOnly == true)
                {
                    status = SaveAs(new FileInfo(DocumentPath).DirectoryName);
                }
                else
                {
                    try
                    {
                        _document.Save(_documentPath);
                        _documentState = DocumentStates.Clean;
                        _subscriber.Publish(_documentState);
                        status = true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("The document could not be saved.");
                    }
                }
            }
            else
            {
                MessageBox.Show("The document is not ready to be saved.");
            }
            return status;
        }

        public bool SaveAs( string initialDirectory )
        {
            bool saved = false;
            if (_canSave)
            {
                var saveDialog = new SaveFileDialog();

                saveDialog.InitialDirectory = initialDirectory;
                saveDialog.FileName = _documentPath;
                saveDialog.Filter = Path.GetFileNameWithoutExtension(_templateName) + " files (*" + _documentExtension + ")|*" + _documentExtension + "|All files (*.*)|*.*";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _document.Save(saveDialog.FileName);
                        _documentPath = saveDialog.FileName;
                        _documentState = DocumentStates.Clean;
                        saved = true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("The document could not be saved.");
                    }

                    if (saved)
                    {
                        PublishToSubscriber();
                        _subscriber.PublishDocumentChange();
                        UpdateMRU();
                    }
                }
            }
            else
            {
                MessageBox.Show("The document is not ready to be saved.");
            }

            return saved;
        }

        public bool AskClose()
        {
            bool canContinue = true;
            if (_canSave)
            {
                if (_documentState == DocumentStates.Dirty)
                {
                    var result = MessageBox.Show("Would you like to save your changes?", Application.ProductName, MessageBoxButtons.YesNoCancel);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            Save();
                            // do stuff
                            break;
                        case DialogResult.No:
                            // do stuff
                            break;
                        case DialogResult.Cancel:
                            canContinue = false;
                            // do stuff
                            break;
                    }
                }
            }
            else
            {
                canContinue = false;
                MessageBox.Show("The document is not ready to be saved.");
            }
            return canContinue;
        }


        public void CreateFromTemplate( string templateName )
        {
            if (AskClose())
            {
                try
                {
                    string templatePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Templates\\" + templateName);
                    _templateName = templateName;
                    _documentExtension = Path.GetExtension(templatePath);
                    _document = XDocument.Load(templatePath);
                    _documentState = DocumentStates.Clean;
                    _document.Changed += new EventHandler<XObjectChangeEventArgs>(DocumentChanged);
                    if (SaveAs(_documentDirectoryMRU ) == false)
                    {
                        ClearDocument();
                    }
                    PublishToSubscriber();
                    _subscriber.PublishDocumentChange();
                   
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        void ClearDocument( )
        {
            _documentState = DocumentStates.Closed;
            _documentPath = string.Empty;
            _document = null;
            PublishToSubscriber();
        }

        void DocumentChanged(object sender, XObjectChangeEventArgs e)
        {
            _documentState = DocumentStates.Dirty;
            PublishToSubscriber();
        }

        void PublishToSubscriber()
        {
            _subscriber.Publish(_documentState);

            if (_documentState != DocumentStates.Closed)
            {
                if (DefaultNamespace != _previousNamespace)
                {
                    _previousNamespace = DefaultNamespace;
                    _subscriber.Publish(DefaultNamespace);
                }
            }
            else
            {
                _previousNamespace = null;
            }
            #region Publish DocumentPath
            if( DocumentPath != _prevdocumentPath)
            {
                _prevdocumentPath = _documentPath;
                _subscriber.Publish( DocumentPath );
            }
            #endregion
        }

        public string DocumentPath
        {
            get { return _documentPath;  }
        }

        public string DocumentText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                try
                {
                    if (_document.Declaration == null)
                    {
                        _document.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                        _document.Declaration.Standalone = null;
                    }
                    sb.AppendLine(_document.Declaration.ToString());
                    sb.AppendLine(_document.ToString());
                }
                catch(Exception)
                {
                }

                return sb.ToString();
            }
            set
            {
                try
                {
                    XDocument _tmpDocument = XDocument.Parse(value);
                    _document = _tmpDocument;
                    _documentState = DocumentStates.Dirty;
                    _document.Changed += new EventHandler<XObjectChangeEventArgs>(DocumentChanged);
                    PublishToSubscriber();
                }
                catch(Exception)
                {
                }
                finally
                {
                }
            }
        }

        private void UpdateMRU()
        {
            _documentDirectoryMRU = new FileInfo(_documentPath).Directory.FullName;
            Settings.Default.DocumentDirectoryMRU = _documentDirectoryMRU;
            Settings.Default.Save();
        }

        public string ValidateXML(string DocumentText)
        {
            string message = string.Empty;

            try
            {
                XDocument.Parse(DocumentText);
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return message;
        }

        public XmlSchemaSet Schemas
        {
            get { return _schemas; }
        }

        public bool CanSave
        {
            set { _canSave = value; }
        }
    }
}
