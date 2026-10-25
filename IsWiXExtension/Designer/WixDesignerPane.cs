using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.Integration;
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
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using static FireworksFramework.Types.Enums;
using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace IsWiX.Designer
{
    /// <summary>
    /// The .wxs document window: a Design tab with IsWiX's designers and an XML tab with Visual Studio's XML editor,
    /// both over Visual Studio's text buffer for the file. A designer edit is written to the buffer as one edit
    /// covering only the text that changed; text edits (typing, undo, a merge) reach the designers when the Design tab
    /// is shown, or right away if it already is.
    /// </summary>
    internal sealed class WixDesignerPane : WindowPane, IOleCommandTarget, IPublishable, IVsTextBufferDataEvents
    {
        // How long text changes must pause before a showing Design tab reloads.
        static readonly TimeSpan ReloadDelay = TimeSpan.FromMilliseconds(400);

        // Visual Studio's XML language service, for colorizing and IntelliSense in the XML tab.
        static readonly Guid XmlLanguageService = new Guid("f6819a78-a205-47b5-be1c-675b3c7f0b8e");

        readonly IVsTextLines _textLines;
        readonly string _filePath;
        readonly DocumentManager _documentManager = new DocumentManager();
        readonly object _editTag = new object();
        readonly DispatcherTimer _reloadTimer;

        readonly TabControl _tabs = new TabControl { TabStripPlacement = Dock.Bottom };
        readonly TabItem _designTab = new TabItem { Header = "Design" };
        readonly TabItem _xmlTab = new TabItem { Header = "XML" };
        readonly TextBlock _status = new TextBlock { Margin = new Thickness(6, 3, 6, 3), TextWrapping = TextWrapping.Wrap, Visibility = Visibility.Collapsed };
        readonly ListView _designerList = new ListView { SelectionMode = SelectionMode.Single };
        readonly Grid _designerHost = new Grid();
        readonly WindowsFormsHost _winFormsHost = new WindowsFormsHost();

        IReadOnlyList<IFireworksDesigner> _designers = Array.Empty<IFireworksDesigner>();
        ITextBuffer? _buffer;
        IVsTextView? _textView;
        IConnectionPoint? _dataEventsPoint;
        uint _dataEventsCookie;
        bool _designStale = true;
        bool _writePending;
        bool _reloading;

        public WixDesignerPane(IServiceProvider serviceProvider, IVsTextLines textLines, string filePath)
            : base(serviceProvider)
        {
            _textLines = textLines;
            _filePath = filePath;
            _reloadTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = ReloadDelay };
            _reloadTimer.Tick += (s, e) => { _reloadTimer.Stop(); ReloadDesigners(); };

            _designTab.Content = BuildDesignView();
            _tabs.Items.Add(_designTab);
            _tabs.Items.Add(_xmlTab);
            _tabs.SelectionChanged += OnTabChanged;
            Content = _tabs;
        }

        bool DesignShowing => ReferenceEquals(_tabs.SelectedItem, _designTab);

        // The standalone app's layout: the designers that apply to the document on the left, the chosen one on the right.
        FrameworkElement BuildDesignView()
        {
            var image = new FrameworkElementFactory(typeof(Image));
            image.SetBinding(Image.SourceProperty, new Binding(nameof(IFireworksDesigner.PluginImage)) { Converter = new FireworksFramework.Services.ImageConverter() });
            image.SetValue(FrameworkElement.WidthProperty, 16.0);
            var name = new FrameworkElementFactory(typeof(TextBlock));
            name.SetBinding(TextBlock.TextProperty, new Binding(nameof(IFireworksDesigner.PluginName)));
            name.SetValue(FrameworkElement.MarginProperty, new Thickness(5, 0, 0, 0));
            name.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            var item = new FrameworkElementFactory(typeof(StackPanel));
            item.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            item.AppendChild(image);
            item.AppendChild(name);
            _designerList.ItemTemplate = new DataTemplate { VisualTree = item };
            _designerList.SelectionChanged += (s, e) => ShowSelectedDesigner();

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.Children.Add(_designerList);
            var splitter = new GridSplitter { Width = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            Grid.SetColumn(splitter, 1);
            grid.Children.Add(splitter);
            Grid.SetColumn(_designerHost, 2);
            grid.Children.Add(_designerHost);

            var panel = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(_status, Dock.Top);
            panel.Children.Add(_status);
            panel.Children.Add(grid);
            return panel;
        }

        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            base.Initialize();
            _documentManager.Subscribe("vsdesigner", this);
            _designers = PluginManager.PluginManagerInstance.CreateDesigners(_documentManager)
                .Where(d => d.PluginType == PluginType.Designer)
                .ToList();

            // An existing buffer (the file was open in another editor) has its text already; a new one is loaded by
            // Visual Studio after the editor is created, and says so through IVsTextBufferDataEvents.
            if (!TryAttachBuffer())
            {
                Log("Waiting for Visual Studio to load " + _filePath);
                var container = (IConnectionPointContainer)_textLines;
                var iid = typeof(IVsTextBufferDataEvents).GUID;
                container.FindConnectionPoint(ref iid, out _dataEventsPoint);
                _dataEventsPoint.Advise(this, out _dataEventsCookie);
            }
        }

        // Visual Studio's own editor over the same buffer, so the XML tab has its colorizing, IntelliSense, find and
        // keyboard commands, which reach it through this pane's IOleCommandTarget.
        FrameworkElement CreateXmlEditor()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            var adapters = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            var editorFactory = componentModel.GetService<ITextEditorFactoryService>();
            var roles = editorFactory.CreateTextViewRoleSet(
                PredefinedTextViewRoles.Analyzable, PredefinedTextViewRoles.Document, PredefinedTextViewRoles.Editable,
                PredefinedTextViewRoles.Interactive, PredefinedTextViewRoles.PrimaryDocument, PredefinedTextViewRoles.Structured,
                PredefinedTextViewRoles.Zoomable);
            _textView = adapters.CreateVsTextViewAdapter((IOleServiceProvider)GetService(typeof(IOleServiceProvider)), roles);
            var flags = (uint)TextViewInitFlags.VIF_HSCROLL | (uint)TextViewInitFlags.VIF_VSCROLL | (uint)TextViewInitFlags3.VIF_NO_HWND_SUPPORT;
            ErrorHandler.ThrowOnFailure(_textView.Initialize(_textLines, IntPtr.Zero, flags, new[] { new INITVIEW { fSelectionMargin = 0, fWidgetMargin = 0, fVirtualSpace = 0, fDragDropMove = 1 } }));
            return adapters.GetWpfTextViewHost(_textView).HostControl;
        }

        bool TryAttachBuffer()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            _buffer = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetDataBuffer(_textLines);
            if (_buffer is null)
            {
                return false;
            }

            // A .wxs is XML; without HeatWave nothing else says so, and the XML tab would be plain text.
            if (ErrorHandler.Succeeded(_textLines.GetLanguageServiceID(out var language)) && language != XmlLanguageService)
            {
                _textLines.SetLanguageServiceID(XmlLanguageService);
            }

            _buffer.Changed += OnBufferChanged;
            Log("Attached to the text buffer of " + _filePath);

            // The text view can only be created over a buffer that has its text.
            try
            {
                _xmlTab.Content = CreateXmlEditor();
            }
            catch (Exception ex)
            {
                Log("The XML editor failed to open: " + ex);
                _xmlTab.Content = new TextBlock { Margin = new Thickness(6), TextWrapping = TextWrapping.Wrap, Text = "The XML editor failed to open: " + ex.Message };
            }

            // Open on the Design tab when a designer applies to the file, else on the XML.
            ReloadDesigners();
            _tabs.SelectedItem = _designerList.Items.Count > 0 ? _designTab : _xmlTab;
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
            if (_buffer is null)
            {
                TryAttachBuffer();
            }
            else
            {
                // Reloaded from disk (changed outside Visual Studio).
                MarkDesignStale();
            }

            return VSConstants.S_OK;
        }

        void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // The designers' own writes need no reload.
            if (e.EditTag != _editTag)
            {
                MarkDesignStale();
            }
        }

        void MarkDesignStale()
        {
            _designStale = true;
            if (DesignShowing)
            {
                _reloadTimer.Stop();
                _reloadTimer.Start();
            }
        }

        void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, _tabs))
            {
                return;
            }

            if (DesignShowing && _designStale)
            {
                ReloadDesigners();
            }
        }

        void ReloadDesigners()
        {
            if (_buffer is null)
            {
                return;
            }

            _reloading = true;
            try
            {
                _documentManager.LoadText(_buffer.CurrentSnapshot.GetText(), _filePath);
                _designStale = false;
                ShowStatus(null);
                _designerHost.IsEnabled = true;

                // The designers that apply can change with the text (a Fragment becoming a Package, say).
                var selected = _designerList.SelectedItem as IFireworksDesigner;
                var applicable = _designers.Where(d => d.IsValidContext()).ToList();
                _designerList.ItemsSource = applicable;
                _designerList.SelectedItem = applicable.Contains(selected) ? selected : applicable.FirstOrDefault();
                if (ReferenceEquals(_designerList.SelectedItem, selected))
                {
                    // Same designer as before: SelectionChanged won't fire, so reload it here.
                    ShowSelectedDesigner();
                }

                if (applicable.Count == 0)
                {
                    ShowStatus("None of IsWiX's designers applies to this file; edit it on the XML tab.");
                }

                Log("Loaded the designers from the buffer");
            }
            catch (XmlException ex)
            {
                // Mid-edit text is often not well-formed; keep showing the last good version until it is again.
                Log("The buffer is not well-formed XML: " + ex.Message);
                ShowStatus($"The XML has an error at line {ex.LineNumber}; the designers show the last version that loaded, read-only, until it's fixed on the XML tab. {ex.Message}");
                _designerHost.IsEnabled = false;
            }
            catch (NotSupportedException ex)
            {
                ShowStatus(ex.Message);
                _designerHost.IsEnabled = false;
            }
            catch (Exception ex)
            {
                // Shown in the window rather than lost: this is a preview.
                Log("The designers failed to load: " + ex);
                ShowStatus("The designers failed to load: " + ex.Message);
            }
            finally
            {
                _reloading = false;
            }
        }

        void ShowSelectedDesigner()
        {
            _designerHost.Children.Clear();
            _winFormsHost.Child = null;
            if (_designerList.SelectedItem is not IFireworksDesigner designer)
            {
                return;
            }

            if (designer is System.Windows.Forms.Control winForms)
            {
                _winFormsHost.Child = winForms;
                _designerHost.Children.Add(_winFormsHost);
            }
            else if (designer is UIElement wpf)
            {
                _designerHost.Children.Add(wpf);
            }

            designer.LoadData();
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

            var oldText = _buffer.CurrentSnapshot.GetText();
            var newText = _documentManager.ToText();
            if (oldText == newText)
            {
                return;
            }

            // Replace only the span that differs, so the rest of the buffer is untouched and the change is one undo step.
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
            }

            Log($"Wrote a designer change to the buffer: {oldText.Length - prefix - suffix} characters replaced at {prefix}");
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

        // Commands: everything goes to the XML editor while its tab shows. On the Design tab, undo and redo still go to
        // it (they are the buffer's, and so cover designer edits); other commands are left to the designer controls.
        IOleCommandTarget? CommandTarget(ref Guid group, uint id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _textView as IOleCommandTarget is { } target && (!DesignShowing || IsUndoRedo(group, id)) ? target : null;
        }

        static bool IsUndoRedo(Guid group, uint id) =>
            group == VSConstants.GUID_VSStandardCommandSet97 &&
            (id == (uint)VSConstants.VSStd97CmdID.Undo || id == (uint)VSConstants.VSStd97CmdID.Redo ||
             id == (uint)VSConstants.VSStd97CmdID.MultiLevelUndo || id == (uint)VSConstants.VSStd97CmdID.MultiLevelRedo);

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var target = cCmds == 1 ? CommandTarget(ref pguidCmdGroup, prgCmds[0].cmdID) : (DesignShowing ? null : _textView as IOleCommandTarget);
            return target?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText) ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return CommandTarget(ref pguidCmdGroup, nCmdID)?.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
                ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
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

                _textView?.CloseView();
                _textView = null;
                _winFormsHost.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
