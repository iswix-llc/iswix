using System;
using System.ComponentModel;
using System.Windows;
using DifferencesViewer.Base;
using DocumentManagement.Managers;
using DocumentManagement.Interfaces;
using DocumentManagement.Types;
using System.Xml.Linq;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DifferencesViewer.ViewModels
{
	public class DifferencesViewModel : ViewModel, IPublishable
    {
        DocumentManager _documentManager;

        public string BeforeText { get; set; }
        public string AfterText { get; set; }
        public SideBySideDiffModel Diff { get; set; }

        public DifferencesViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Setup();
            }
        }

        
        private void Setup()
        {
             _documentManager = DocumentManager.DocumentManagerInstance;
            _documentManager.Subscribe("differences", this);
            _documentManager.Refresh();
        }

        public void Publish(DocumentStates DocumentState)
        {
        }

        public void Publish(string DocumentPath)
        {
        }

        public void Publish(XNamespace Namespace)
        {
        }

        public void DocumentUpdated()
        {
            Differ differ = new Differ();
            SideBySideDiffBuilder diffBuilder = new SideBySideDiffBuilder(differ);
            Diff= diffBuilder.BuildDiffModel(_documentManager.SavedDocument.ToString(), _documentManager.Document.ToString());
            NotifyChanged("Diff");
        }
    }
}
