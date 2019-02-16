using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Schema;
using static FireworksFramework.Types.Enums;

namespace XmlEditorDesigner.Views
{
    /// <summary>
    /// Interaction logic for XmlEditor.xaml
    /// </summary>
    public partial class XmlEditor : UserControl, IFireworksDesigner
    {
        DocumentManager _documentManager = DocumentManager.DocumentManagerInstance;
        bool _validXML;
        FoldingManager _foldingManager;
        XmlFoldingStrategy _foldingStrategy;
        TextDocument _textDocument;
        bool _previouslyLoaded = false;

        public XmlEditor()
        {
            InitializeComponent();
        }

        public string PluginName
        {
            get { return "Xml Editor"; }
        }

        public System.Drawing.Image PluginImage
        {
            get
            { return System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("XmlEditorDesigner.XmlEditor.ico")); }
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

        public void LoadData()
        {
            if (!_previouslyLoaded)
            {
                _textDocument = new TextDocument();
                TextEditor.Document = _textDocument;
                _foldingManager = FoldingManager.Install(TextEditor.TextArea);
                _foldingStrategy = new XmlFoldingStrategy();
            }
            _textDocument.Text = _documentManager.Document.ToString();
            _foldingStrategy.UpdateFoldings(_foldingManager, TextEditor.Document);
            _previouslyLoaded = true;



        }

        private void ValidateXML(bool textChanged)
        {

            _validXML = false;

            string xmlValidationMessage = string.Empty;
            XDocument tempDocument = new XDocument();

            try
            {
                tempDocument = XDocument.Parse(TextEditor.Document.Text);
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

                if (_foldingStrategy != null)
                {
                    _foldingStrategy.UpdateFoldings(_foldingManager, TextEditor.Document);
                }
                TextBox.Text = xmlValidationMessage;
                if (textChanged)
                {
                    if (_documentManager.Document != tempDocument)
                    {
                        _documentManager.Document = tempDocument;
                        _documentManager.RefreshNamespaces();
                    }
                }
                _documentManager.CanSave = true;

            }
            else
            {
                TextBox.Text = xmlValidationMessage;
                _documentManager.CanSave = false;
            }

        }
        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            ValidateXML(true);
        }
    }
}
