using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using FireworksFramework.Interfaces;
using FireworksFramework.Managers;
using FireworksFramework.Types;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PropertiesDesigner.Views;
using IServiceProvider = System.IServiceProvider;

namespace IsWiX.Designer
{
    /// <summary>
    /// The designer's document window. It keeps an IsWiX <see cref="DocumentManager"/> and Visual Studio's text buffer
    /// for the file in step: a designer edit is written to the buffer as one edit covering only the text that
    /// changed, and typing in the XML editor reloads the designer once the text is well-formed again.
    /// </summary>
    /// <remarks>
    /// Proof of concept: it hosts only the Properties designer.
    /// </remarks>
    internal sealed class WixDesignerPane : WindowPane, IPublishable, IVsTextBufferDataEvents
    {
        // How long typing in the XML editor must pause before the designer reloads.
        static readonly TimeSpan ReloadDelay = TimeSpan.FromMilliseconds(400);

        readonly IVsTextLines _textLines;
        readonly string _filePath;
        readonly DocumentManager _documentManager = new DocumentManager();
        readonly object _editTag = new object();
        readonly DockPanel _root = new DockPanel { LastChildFill = true };
        readonly TextBlock _status = new TextBlock { Margin = new Thickness(6, 3, 6, 3), TextWrapping = TextWrapping.Wrap, Visibility = Visibility.Collapsed };
        readonly DispatcherTimer _reloadTimer;

        ITextBuffer? _buffer;
        PropertiesView? _designer;
        IConnectionPoint? _dataEventsPoint;
        uint _dataEventsCookie;
        bool _writePending;
        bool _reloading;

        public WixDesignerPane(IServiceProvider serviceProvider, IVsTextLines textLines, string filePath)
            : base(serviceProvider)
        {
            _textLines = textLines;
            _filePath = filePath;
            _reloadTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = ReloadDelay };
            _reloadTimer.Tick += (s, e) => { _reloadTimer.Stop(); ReloadFromBuffer(); };

            DockPanel.SetDock(_status, Dock.Top);
            _root.Children.Add(_status);
            Content = _root;
        }

        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            base.Initialize();
            _documentManager.Subscribe("vsdesigner", this);

            // An existing buffer (the file was open in the XML editor) has its text already; a new one is loaded
            // by Visual Studio after the editor is created, and says so through IVsTextBufferDataEvents.
            if (!TryAttachBuffer())
            {
                Log("Waiting for Visual Studio to load " + _filePath);
                var container = (IConnectionPointContainer)_textLines;
                var iid = typeof(IVsTextBufferDataEvents).GUID;
                container.FindConnectionPoint(ref iid, out _dataEventsPoint);
                _dataEventsPoint.Advise(this, out _dataEventsCookie);
            }
        }

        bool TryAttachBuffer()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            var adapters = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            _buffer = adapters.GetDataBuffer(_textLines);
            if (_buffer is null)
            {
                return false;
            }

            _buffer.Changed += OnBufferChanged;
            Log("Attached to the text buffer of " + _filePath);
            ReloadFromBuffer();
            return true;
        }

        // Visual Studio's activity log (devenv /log) records what the designer does, for diagnosing the preview.
        static void Log(string message) => ActivityLog.TryLogInformation("IsWiX Designer", message);

        void IVsTextBufferDataEvents.OnFileChanged(uint grfChange, uint dwFileAttrs)
        {
        }

        int IVsTextBufferDataEvents.OnLoadCompleted(int fReload)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Log("Visual Studio finished loading " + _filePath);
            if (_buffer is null)
            {
                TryAttachBuffer();
            }
            else
            {
                // Reloaded from disk (changed outside Visual Studio).
                ReloadFromBuffer();
            }

            return VSConstants.S_OK;
        }

        void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // The designer's own writes need no reload; anything else (typing, undo, a merge) does, once typing pauses.
            if (e.EditTag == _editTag)
            {
                return;
            }

            _reloadTimer.Stop();
            _reloadTimer.Start();
        }

        void ReloadFromBuffer()
        {
            if (_buffer is null)
            {
                return;
            }

            _reloading = true;
            try
            {
                _documentManager.LoadText(_buffer.CurrentSnapshot.GetText(), _filePath);
                ShowStatus(null);
                ShowDesigner();
                Log("Loaded the designer from the buffer");
            }
            catch (XmlException ex)
            {
                Log("The buffer is not well-formed XML: " + ex.Message);
                // Mid-edit text is often not well-formed; keep showing the last good version until it is again.
                ShowStatus($"The XML has an error at line {ex.LineNumber}, so the designer shows the last version that loaded: {ex.Message}");
                if (_designer != null)
                {
                    _designer.IsEnabled = false;
                }
            }
            catch (NotSupportedException ex)
            {
                ShowStatus(ex.Message);
            }
            catch (Exception ex)
            {
                // Shown in the window rather than lost: this is a preview.
                Log("The designer failed to load: " + ex);
                ShowStatus("The designer failed to load: " + ex.Message);
            }
            finally
            {
                _reloading = false;
            }
        }

        void ShowDesigner()
        {
            if (_designer is null)
            {
                _designer = new PropertiesView(_documentManager);
                _root.Children.Add(_designer);
            }

            _designer.IsEnabled = true;
            if (_designer.IsValidContext())
            {
                _designer.Visibility = Visibility.Visible;
                _designer.LoadData();
            }
            else
            {
                _designer.Visibility = Visibility.Collapsed;
                ShowStatus("The Properties designer edits a WiX package or fragment; this file has neither.");
            }
        }

        void ShowStatus(string? message)
        {
            _status.Text = message ?? string.Empty;
            _status.Visibility = message is null ? Visibility.Collapsed : Visibility.Visible;
        }

        // IPublishable: the DocumentManager calls DocumentUpdated after every change to the document. A designer edit
        // can change several nodes in a row, so the text is written once, after the edit finishes.
        void IPublishable.DocumentUpdated()
        {
            if (_reloading || _writePending || _buffer is null)
            {
                return;
            }

            _writePending = true;
            ThreadHelper.JoinableTaskFactory.StartOnIdle(WriteToBuffer).FileAndForget("iswix/designer/writetobuffer");
        }

        void WriteToBuffer()
        {
            _writePending = false;
            if (_buffer is null)
            {
                return;
            }

            var snapshot = _buffer.CurrentSnapshot;
            var oldText = snapshot.GetText();
            var newText = _documentManager.ToText();
            if (oldText == newText)
            {
                return;
            }

            // Replace only the span that differs, so the rest of the buffer (and the XML editor's view of it) is
            // untouched and the change is one undo step.
            var prefix = 0;
            var max = Math.Min(oldText.Length, newText.Length);
            while (prefix < max && oldText[prefix] == newText[prefix])
            {
                prefix++;
            }

            var suffix = 0;
            while (suffix < max - prefix && oldText[oldText.Length - 1 - suffix] == newText[newText.Length - 1 - suffix])
            {
                suffix++;
            }

            using (var edit = _buffer.CreateEdit(EditOptions.None, null, _editTag))
            {
                edit.Replace(prefix, oldText.Length - prefix - suffix, newText.Substring(prefix, newText.Length - prefix - suffix));
                edit.Apply();
                Log($"Wrote a designer change to the buffer: {oldText.Length - prefix - suffix} characters replaced at {prefix}");
            }
        }

        void IPublishable.Publish(DocumentStates documentState)
        {
        }

        void IPublishable.Publish(string documentPath)
        {
        }

        void IPublishable.Publish(XNamespace @namespace)
        {
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (disposing)
            {
                _reloadTimer.Stop();
                _documentManager.UnSubscribe("vsdesigner");
                if (_buffer != null)
                {
                    _buffer.Changed -= OnBufferChanged;
                }

                if (_dataEventsPoint != null)
                {
                    _dataEventsPoint.Unadvise(_dataEventsCookie);
                    _dataEventsPoint = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
