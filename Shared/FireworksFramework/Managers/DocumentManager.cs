///////////////////////////////////////////////
// Copyright (C) 2010-2019 ISWIX, LLC
// Web: http://www.iswix.com
// All Rights Reserved
///////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FireworksFramework.Interfaces;
using FireworksFramework.Types;
using IsWiX.WiXModel.Core;
using IsWiX.WiXModel.Core.Xml;
using CanonicalOrder = IsWiX.WiXModel.CanonicalOrder;

namespace FireworksFramework.Managers
{
    /// <summary>
    /// One open document and its state. Each window (or Visual Studio editor) creates its own and passes it to its
    /// designers' constructors, so several documents can be open in one process.
    /// </summary>
    public partial class DocumentManager
    {
        DocumentStates _documentState = DocumentStates.Closed;
        // Never null: an empty document while none is open.
        XDocument _document = new XDocument();
        XDocument _savedDocument = new XDocument();
        string _prevdocumentPath = string.Empty;
        Dictionary<string, IPublishable> _subscribers = new Dictionary<string, IPublishable>();
        XNamespace? _previousNamespace;
        string _documentExtension = string.Empty;
        string _templateName = string.Empty;
        bool _canSave = true;
        Dictionary<string, XNamespace> _namespaces = new Dictionary<string, XNamespace> { [""] = XNamespace.None };

        public Dictionary<string, IPublishable> Subscribers { get { return _subscribers; } }

        string _documentPath = string.Empty;
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
                var result = (_document.Root?.Attributes() ?? Enumerable.Empty<XAttribute>()).
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
                // Lossless load: the source text of every node is kept, so saving writes unchanged parts of the
                // file back exactly as they were (whitespace, quotes, comments, line endings, encoding).
                XDocument document;
                using (var stream = File.OpenRead(filePath))
                {
                    document = LosslessXml.Load(stream);
                }

                Open(document, filePath);
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Load Failed", ex);
            }
        }

        /// <summary>
        /// Loads a document from text, such as an editor's in-memory copy of the file, instead of reading the file.
        /// </summary>
        /// <param name="text">The XML text.</param>
        /// <param name="filePath">The file the text belongs to; nothing is read from it.</param>
        /// <exception cref="System.Xml.XmlException">The text is not well-formed XML.</exception>
        /// <exception cref="NotSupportedException">The text is a WiX v3 or merge module source.</exception>
        public void LoadText(string text, string filePath)
        {
            // The same lossless parse as Load, so ToText gives back the text unchanged until a designer edits it.
            Open(LosslessXml.Parse(text), filePath);
        }

        /// <summary>
        /// Gets the document's text as it would be saved: unchanged parts exactly as loaded, edits indented like
        /// their surroundings.
        /// </summary>
        public string ToText() => LosslessXml.ToXmlString(_document);

        void Open(XDocument document, string filePath)
        {
            // WiX v3 sources are not supported. Reject them before replacing the current document.
            if (document.Root?.Name.Namespace == WixNamespaces.WixV3)
            {
                var fileName = Path.GetFileName(filePath);
                throw new NotSupportedException(
                    $"'{fileName}' is a WiX v3 source file, which IsWiX no longer supports.{Environment.NewLine}{Environment.NewLine}" +
                    $"Convert it to WiX v4 or later with 'wix convert \"{fileName}\"', then open the converted file.");
            }

            // Merge module sources are not supported either. Packages that consume merge modules
            // (<Merge> and <MergeRef>) are fine, as XML; this is only about editing a <Module> itself.
            if (document.Root?.Elements().FirstOrDefault()?.Name == WixNamespaces.Wxs + "Module")
            {
                throw new NotSupportedException(
                    $"'{Path.GetFileName(filePath)}' is a merge module source file, which IsWiX no longer supports.{Environment.NewLine}{Environment.NewLine}" +
                    "IsWiX edits WiX packages and fragments. Packages can still include merge modules by editing their XML.");
            }

            // Elements added through the typed model go to IsWiX's canonical position, so a file's layout
            // depends only on its content, which keeps branches and merges clean.
            WixOrdering.Set(document, CanonicalOrder.Create());

            // Stop watching the document being replaced, so a reload doesn't leave it publishing changes.
            DisableChangeWatcher();
            _document = document;
            _savedDocument = new XDocument(_document);
            _documentPath = filePath;
            _documentState = DocumentStates.Clean;
            RefreshNamespaces();
            EnableChangeWatcher();
            PublishToSubscriber();
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
            // Only what the designers changed is rewritten; new elements are indented like their siblings. The file
            // is written to a temporary file and swapped in, so a failed save never truncates it. The whole file is
            // then brought to the indentation it mostly uses, so it is evenly indented after every save.
            LosslessXml.Save(_document, filePath);
            EnforceIndentation(filePath);
            _savedDocument = new XDocument(_document);
            _documentPath = filePath;
            _documentState = DocumentStates.Clean;
            PublishToSubscriber();
        }

        // Re-indents a saved file to the style it mostly uses. Best effort: the save itself has already succeeded, so a
        // file that is locked or not UTF-8 is left as saved.
        static void EnforceIndentation(string filePath)
        {
            var temp = filePath + ".indent.tmp";
            try
            {
                var bytes = File.ReadAllBytes(filePath);
                var bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
                var text = new UTF8Encoding(false, true).GetString(bytes, bom, bytes.Length - bom);
                var formatted = XmlIndentation.Format(text);
                if (formatted == text)
                {
                    return;
                }

                var body = new UTF8Encoding(false).GetBytes(formatted);
                var output = new byte[bom + body.Length];
                Array.Copy(bytes, output, bom);
                Array.Copy(body, 0, output, bom, body.Length);
                File.WriteAllBytes(temp, output);
                File.Replace(temp, filePath, null);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is DecoderFallbackException)
            {
                try
                {
                    File.Delete(temp);
                }
                catch (Exception)
                {
                }
            }
        }

        public void Close()
        {
            _documentState = DocumentStates.Closed;
            _document = new XDocument();
            _documentPath = string.Empty;
            PublishToSubscriber();
        }

        void DocumentChanged(object? sender, XObjectChangeEventArgs e)
        {
            if (_batchDepth > 0)
            {
                _changedInBatch = true;
                return;
            }

            CompareDocuments();
        }

        int _batchDepth;
        bool _changedInBatch;

        /// <summary>
        /// Holds back the work a change to the document sets off (comparing it with the saved one, and telling the
        /// subscribers) until the returned scope is disposed, which does it once. An edit that changes many nodes, such as
        /// removing a folder's hundreds of components, would otherwise do it for each node, which takes time that grows
        /// with the square of the document's size. Scopes nest.
        /// </summary>
        public IDisposable Batch()
        {
            _batchDepth++;
            return new BatchScope(this);
        }

        sealed class BatchScope : IDisposable
        {
            DocumentManager? _owner;

            public BatchScope(DocumentManager owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner is { } owner)
                {
                    _owner = null;
                    if (--owner._batchDepth == 0 && owner._changedInBatch)
                    {
                        owner._changedInBatch = false;
                        owner.CompareDocuments();
                    }
                }
            }
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

        public bool CanSave
        {
            get { return _canSave; }
            set { _canSave = value; }
        }
    }
}
