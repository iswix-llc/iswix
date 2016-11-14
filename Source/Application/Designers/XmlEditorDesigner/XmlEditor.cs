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
using FireworksFramework.Types;
using ICSharpCode.XmlEditor;
using ICSharpCode.Core;
using IsWiXAutomationInterface;

namespace XmlEditorDesigner
{
    public partial class XmlEditor : UserControl, IFireworksDesigner
    {
        IDocumentManager _mgr;
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
            editor.Text = _mgr.Document.ToString();
            editor.TextEditorProperties.EnableFolding = true;
            editor.Document.FoldingManager.FoldingStrategy = new XmlFoldingStrategy();
            editor.Document.FoldingManager.UpdateFoldings("", null);
            this.editor.TextChanged += new System.EventHandler(this.editor_TextChanged);
            editor.Validating += new CancelEventHandler(editor_Validating);
            this.editor.SetHighlighting("XML");
            ValidateXML();
            XmlSchemaCompletionDataCollection schemas = new XmlSchemaCompletionDataCollection();

            string schemasDir;
            try
            {

                if (_mgr.Document.GetWiXNameSpace() == "http://schemas.microsoft.com/wix/2006/wi")
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

        private void ValidateXML()
        {

            _validXML = false;

            string xmlValidationMessage = string.Empty;
            XDocument doc = new XDocument();

            try
            {
                doc = XDocument.Parse(editor.Text);
                if (_mgr.Schemas.Contains(_mgr.DefaultNamespace.ToString()))
                {
                    xmlValidationMessage = "Valid XML ( Validated against available schemas )";
                    doc.Validate(_mgr.Schemas, (o, ex) => { _validXML = false; xmlValidationMessage = ex.Exception.Message; });
                }
                _validXML = true;
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
                _mgr.DocumentText = editor.Text;
                _mgr.CanSave = true;
                
            }
            else
            {
                textBoxStatus.Text = xmlValidationMessage;
                _mgr.CanSave = false;
            }

        }

        #region IFireworksDesigner Members

        private void editor_TextChanged(object sender, EventArgs e)
        {
            editor.Document.FoldingManager.UpdateFoldings("", null);
            ValidateXML();
        }

        public IDesignerManager DesignerManager
        {
            set { _mgr = value.DocumentManager; }
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
            get { return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("XmlEditorDesigner.MS-PL.txt")).ReadToEnd(); }
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
                    editor.Text = _mgr.Document.ToString();
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
                    editor.Text = _mgr.Document.ToString();
                    
                }
            }
        }

    }
}
