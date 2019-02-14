///////////////////////////////////////////////
// Copyright (C) 2013 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Text;
using System.Windows.Forms;
using FireworksFramework.Interfaces;
using ICSharpCode.XmlEditor;
using ICSharpCode.Core;
using IsWiXAutomationInterface;
using FireworksFramework.Managers;
using static FireworksFramework.Types.Enums;

namespace XmlEditorDesigner
{
    public partial class XmlEditor : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;

        XmlEditorControl editor;
        bool _validXML;
            
        public XmlEditor()
        {
            string applicationDirectory = new FileInfo(Application.ExecutablePath).DirectoryName;
            PropertyService.InitializeService(applicationDirectory, applicationDirectory, "Fireworks");
            InitializeComponent();
            editor = new XmlEditorControl();
            editor.Dock = DockStyle.Fill;
            this.splitContainer1.Panel1.Controls.Add(editor);
        }

        public void LoadData()
        {
            editor.Text = _documentManager.Document.ToString();
            editor.TextEditorProperties.EnableFolding = true;
            editor.Document.FoldingManager.FoldingStrategy = new XmlFoldingStrategy();
            editor.Document.FoldingManager.UpdateFoldings("", null);
            this.editor.TextChanged += new System.EventHandler(this.editor_TextChanged);
            editor.Validating += new CancelEventHandler(editor_Validating);
            this.editor.SetHighlighting("XML");
            ValidateXML(false);
            XmlSchemaCompletionDataCollection schemas = new XmlSchemaCompletionDataCollection();

            string schemasDir;
            try
            {

                if (_documentManager.Document.GetWiXNameSpace() == "http://schemas.microsoft.com/wix/2006/wi")
                {
                    schemasDir = Path.Combine(PropertyService.DataDirectory, "Schemas");
                }
                else
                {
                    schemasDir = Path.Combine(PropertyService.DataDirectory, @"Schemas\v4");
                }


                foreach (var file in new DirectoryInfo(schemasDir).GetFiles("*.xsd"))
                {
                    schemas.Add(new XmlSchemaCompletionData(file.FullName));
                }
            }
            catch (Exception)
            {
            }
            editor.SchemaCompletionDataItems = schemas;

        }

        private void ValidateXML(bool textChanged)
        {

            _validXML = false;

            string xmlValidationMessage = string.Empty;
            XDocument tempDocument = new XDocument();

            try
            {
                tempDocument = XDocument.Parse(editor.Text);
                _validXML = true;
                if (_documentManager.Schemas.Contains(_documentManager.DefaultNamespace.ToString()))
                {
                    xmlValidationMessage = "Valid XML ( Validated against available schemas )";
                    tempDocument.Validate(_documentManager.Schemas, (o, ex) => {
                        _validXML = false;
                        xmlValidationMessage = ex.Exception.Message;
                    });
                }
            }
            catch (Exception ex)
            {
                xmlValidationMessage = ex.Message;
            }
            finally
            {
            }

            if (_validXML)
            {
                textBoxStatus.Text = xmlValidationMessage;
                if (textChanged)
                {
                    if (_documentManager.Document!= tempDocument )
                    {
                        _documentManager.Document = tempDocument;
                        _documentManager.RefreshNamespaces();
                    }
                }
                _documentManager.CanSave = true;
                
            }
            else
            {
                textBoxStatus.Text = xmlValidationMessage;
                _documentManager.CanSave = false;
            }

        }

        #region IFireworksDesigner Members

        private void editor_TextChanged(object sender, EventArgs e)
        {
            editor.Document.FoldingManager.UpdateFoldings("", null);
            ValidateXML(true);
        }

        public string PluginName
        {
            get { return "Xml Editor"; }
        }

        public Image PluginImage
        {
            get
            { return Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("XmlEditorDesigner.XmlEditor.ico")); }  
        }

        public PluginType PluginType
        {
            get { return PluginType.Designer; }
        }

        public string PluginOrder
        {
            get { return "XmlEditor"; }
        }

        public string PluginInformation
        {
            get { return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("XmlEditorDesigner.License.txt")).ReadToEnd(); }
        }

        public bool IsValidContext()
        {
            return true;
        }

        #endregion

        private void XmlEditor_Leave(object sender, EventArgs e)
        {
            editor.TextChanged -= new System.EventHandler(this.editor_TextChanged);
            editor.Validating -= new CancelEventHandler(editor_Validating);

        }

        void editor_Validating(object sender, CancelEventArgs e)
        {
            if (!_validXML)
            {
                if (MessageBox.Show("Invalid XML has been detected.  Are you sure you want to discard it?", Application.ProductName, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    editor.Text = _documentManager.Document.ToString();
                }
            }
        }

        private void XmlEditor_Validating(object sender, CancelEventArgs e)
        {
            if (!_validXML)
            {
                if (MessageBox.Show("Invalid XML has been detected.  Are you sure you want to discard it?", Application.ProductName, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    editor.Text = _documentManager.Document.ToString();
                    
                }
            }
        }

    }
}
