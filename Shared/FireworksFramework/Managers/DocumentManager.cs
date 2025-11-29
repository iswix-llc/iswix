///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using FireworksFramework.Interfaces;
using FireworksFramework.Types;

namespace FireworksFramework.Managers
{
    public partial class DocumentManager
    {
        DocumentStates _documentState = DocumentStates.Closed;
        XDocument _document;
        XDocument _savedDocument;
        string _prevdocumentPath = string.Empty;
        Dictionary<string, IPublishable> _subscribers = new Dictionary<string, IPublishable>();
        XNamespace _previousNamespace;
        string _documentExtension = string.Empty;
        string _templateName = string.Empty;
        XmlSchemaSet _schemas = new XmlSchemaSet();
        bool _canSave = true;
        Dictionary<string, XNamespace> _namespaces;

        public Dictionary<string, IPublishable> Subscribers { get { return _subscribers; } }
        private static readonly Lazy<DocumentManager> lazy = new Lazy<DocumentManager>(() => new DocumentManager());

        public static DocumentManager DocumentManagerInstance { get { return lazy.Value; } }

        string _documentPath;
        public string DocumentPath { get { return _documentPath; } }

        public void Subscribe(string name, IPublishable subscriber)
        {
            if (_subscribers.ContainsKey(name))
            {
                _subscribers.Remove(name);
            }
            _subscribers.Add(name, subscriber);
        }

        public void UnSubscribe(string name)
        {
            if (_subscribers.ContainsKey(name))
            {
                _subscribers.Remove(name);
            }
            Refresh();
        }

        private DocumentManager()
        {
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
                return _namespaces;
            }
        }


        public void RefreshNamespaces()
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
            _namespaces = schemas;
        }

        public XDocument Document
        {
            get
            {
                return _document;
            }
            set
            {
                _document = value;
                EnableChangeWatcher();
            }
        }

        public XDocument SavedDocument { get { return new XDocument(_savedDocument); } }

        public void Load(string filePath)
        {
            try
            {
                _document = XDocument.Load(filePath);
                _savedDocument = new XDocument(_document);
                _documentPath = filePath;
                _documentState = DocumentStates.Clean;
                RefreshNamespaces();
                EnableChangeWatcher();
                PublishToSubscriber();

            }
            catch (Exception ex)
            {
                throw new Exception("Load Failed");
            }
        }

        public void DisableChangeWatcher()
        {
            try
            {
                _document.Changed -= new EventHandler<XObjectChangeEventArgs>(DocumentChanged);
            }
            catch (Exception)
            {
            }
        }

        public void EnableChangeWatcher()
        {
            try
            {
                _document.Changed += new EventHandler<XObjectChangeEventArgs>(DocumentChanged);
                CompareDocuments();
            }
            catch (Exception)
            {
            }
        }

        public void Save(string filePath)
        {
            _document.Save(filePath);
            _savedDocument = new XDocument(_document);
            _documentPath = filePath;
            _documentState = DocumentStates.Clean;
            PublishToSubscriber();
        }

        public void Close()
        {
            _documentState = DocumentStates.Closed;
            _document = null;
            _documentPath = string.Empty;
            PublishToSubscriber();
        }

        void DocumentChanged(object sender, XObjectChangeEventArgs e)
        {
            CompareDocuments();
        }

        private void CompareDocuments()
        {
            if (_document.ToString() == _savedDocument.ToString())
            {
                _documentState = DocumentStates.Clean;
            }
            else
            {
                _documentState = DocumentStates.Dirty;
            }
            PublishToSubscriber();
        }

        public void Refresh()
        {
            PublishToSubscriber();

        }

        void PublishToSubscriber()
        {
            foreach (var subscriber in _subscribers.Values)
            {
                if (subscriber != null)
                {
                    subscriber.Publish(_documentState);

                    if (_documentState != DocumentStates.Closed)
                    {
                        if (DefaultNamespace != _previousNamespace)
                        {
                            _previousNamespace = DefaultNamespace;
                            subscriber.Publish(DefaultNamespace);
                        }
                    }
                    else
                    {
                        _previousNamespace = null;
                    }
                    if (DocumentPath != _prevdocumentPath)
                    {
                        _prevdocumentPath = _documentPath;
                        subscriber.Publish(DocumentPath);
                    }
                    subscriber.DocumentUpdated();
                }
            }
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
            get { return _canSave; }
            set { _canSave = value; }
        }
    }
}
