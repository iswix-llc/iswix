///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Types;

namespace FireworksFramework.ViewModels
{
	public class DifferencesViewModel : ObservableObject, IPublishable
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
            if (_documentManager.Document != null)
            {
                Diff = diffBuilder.BuildDiffModel(_documentManager.SavedDocument.ToString(), _documentManager.Document.ToString());
                RaisePropertyChangedEvent("Diff");
            }
        }
    }
}
