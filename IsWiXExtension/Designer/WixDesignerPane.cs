using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.Integration;
using System.Windows.Media;
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
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using static FireworksFramework.Types.Enums;
using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace IsWiX.Designer
{
    /// <summary>
    /// The .wxs document window: a Design view with IsWiX's designers above an XML view with Visual Studio's XML editor,
    /// split by a draggable bar, both over Visual Studio's text buffer for the file. A designer edit is written to
    /// the buffer as one edit covering only the text that changed; text edits (typing, undo, a merge) reach the designers
    /// once they pause.
    /// </summary>
    internal sealed class WixDesignerPane : WindowPane, IOleCommandTarget, IPublishable, IVsTextBufferDataEvents
    {
        // How long text changes must pause before a showing Design view reloads.
        static readonly TimeSpan ReloadDelay = TimeSpan.FromMilliseconds(400);

        // Visual Studio's XML language service: colorizing and IntelliSense in the XML view.
        static readonly Guid XmlLanguageService = new Guid("f6819a78-a205-47b5-be1c-675b3c7f0b8e");

        readonly IVsTextLines _textLines;
        readonly string _filePath;
        readonly DocumentManager _documentManager = new DocumentManager();
        readonly object _editTag = new object();
        readonly DispatcherTimer _reloadTimer;

        // Both views are always shown, the Design view above the XML view, with a splitter between them to resize.
        readonly Grid _root = new Grid();
        readonly GridSplitter _splitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, ResizeDirection = GridResizeDirection.Rows };
        readonly DockPanel _designView = new DockPanel { LastChildFill = true };
        readonly Border _xmlView = new Border();
        readonly TextBlock _status = new TextBlock { Margin = new Thickness(6, 3, 6, 3), TextWrapping = TextWrapping.Wrap, Visibility = Visibility.Collapsed };
        readonly ListView _designerList = new ListView { SelectionMode = SelectionMode.Single };
        readonly Grid _designerHost = new Grid();
        readonly Dictionary<IFireworksDesigner, WindowsFormsHost> _winFormsHosts = new Dictionary<IFireworksDesigner, WindowsFormsHost>();
        readonly HashSet<System.Windows.Forms.Control> _themedWinForms = new HashSet<System.Windows.Forms.Control>();

        // How long the caret must rest before the designer follows it.
        static readonly TimeSpan CaretDelay = TimeSpan.FromMilliseconds(150);

        readonly DispatcherTimer _caretTimer;

        IReadOnlyList<IFireworksDesigner> _designers = Array.Empty<IFireworksDesigner>();
        ITextBuffer? _buffer;
        IWpfTextView? _wpfView;
        // Set while one side's selection is being moved to follow the other's, so that doesn't move it back.
        bool _syncing;
        bool _initialSelectionPending = true;
        IVsCodeWindow? _codeWindow;
        CodeWindowHost? _codeWindowHost;
        IConnectionPoint? _dataEventsPoint;
        uint _dataEventsCookie;
        bool _writePending;
        bool _reloading;

        // The theme the views were last given (VsTheme.Background then).
        System.Drawing.Color _themeApplied;

        public WixDesignerPane(IServiceProvider serviceProvider, IVsTextLines textLines, string filePath)
            : base(serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _textLines = textLines;
            _filePath = filePath;
            _reloadTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = ReloadDelay };
            _reloadTimer.Tick += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _reloadTimer.Stop();
                ReloadDesigners();
            };

            _caretTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = CaretDelay };
            _caretTimer.Tick += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _caretTimer.Stop();
                SelectDesignerItemAtCaret();
            };

            BuildDesignView();
            _root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 80 });
            _root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 80 });
            Grid.SetRow(_splitter, 1);
            Grid.SetRow(_xmlView, 2);
            _splitter.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.ToolWindowBorderBrushKey);
            _root.Children.Add(_designView);
            _root.Children.Add(_splitter);
            _root.Children.Add(_xmlView);
            _root.IsVisibleChanged += OnRootVisibleChanged;
            _themeApplied = VsTheme.Background;
            Content = _root;
        }

        // The XML editor's window is a window of its own, so WPF doesn't track focus in it: ask Windows.
        // The text view is WPF inside that window, so its own focus is asked too.
        bool XmlFocused =>
            _wpfView?.HasAggregateFocus == true ||
            (_codeWindowHost is { Handle: var host } && host != IntPtr.Zero && IsChild(host, GetFocus()));

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetFocus();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool IsChild(IntPtr parent, IntPtr window);

        // The standalone app's layout: the designers that apply to the document on the left, the chosen one on the right.
        void BuildDesignView()
        {
            var image = new FrameworkElementFactory(typeof(Image));
            // Bound to the designer, which may carry an icon drawn for dark themes as well as its PluginImage.
            image.SetBinding(Image.SourceProperty, new Binding { Converter = new VsTheme.ThemedImageConverter() });
            image.SetValue(FrameworkElement.WidthProperty, 16.0);
            image.SetValue(FrameworkElement.HeightProperty, 16.0);
            image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
            var name = new FrameworkElementFactory(typeof(TextBlock));
            name.SetBinding(TextBlock.TextProperty, new Binding(nameof(IFireworksDesigner.PluginName)));
            name.SetValue(FrameworkElement.MarginProperty, new Thickness(5, 0, 0, 0));
            name.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            var item = new FrameworkElementFactory(typeof(StackPanel));
            item.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            item.AppendChild(image);
            item.AppendChild(name);
            _designerList.ItemTemplate = new DataTemplate { VisualTree = item };
            _designerList.SelectionChanged += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                ShowSelectedDesigner();
                // Choosing a designer shows what it has selected in the XML too. A reload keeps the XML where it is.
                if (!_reloading && !_syncing)
                {
                    ShowCurrentDesignerSelection();
                }
            };

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

            DockPanel.SetDock(_status, Dock.Top);
            _designView.Children.Add(_status);
            _designView.Children.Add(grid);

            // Visual Studio's colors (light, dark, blue) for the view and the WPF designers in it.
            VsTheme.Apply(_designView);
        }

        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            base.Initialize();
            _documentManager.Subscribe("vsdesigner", this);
            _designers = PluginManager.PluginManagerInstance.CreateDesigners(_documentManager)
                .Where(d => d.PluginType == PluginType.Designer)
                .ToList();
            foreach (var linked in _designers.OfType<ISelectionLinked>())
            {
                linked.SelectedElementChanged += OnDesignerSelectionChanged;
            }

            VSColorTheme.ThemeChanged += OnThemeChanged;
            // The dialogs the designers open look like the designers, and are owned by Visual Studio's window.
            FireworksFramework.Services.DesignerDialogs.Theme = VsTheme.ThemeDialog;
            FireworksFramework.Services.DesignerDialogs.OwnerHandle = () => System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

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

        bool TryAttachBuffer()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            _buffer = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetDataBuffer(_textLines);
            if (_buffer is null)
            {
                return false;
            }

            // A .wxs is XML; without HeatWave nothing else says so. Set before the code window is created, which is
            // when the language service attaches.
            if (ErrorHandler.Succeeded(_textLines.GetLanguageServiceID(out var language)) && language != XmlLanguageService)
            {
                _textLines.SetLanguageServiceID(XmlLanguageService);
            }

            _buffer.Changed += OnBufferChanged;
            Log("Attached to the text buffer of " + _filePath);

            try
            {
                CreateCodeWindow();
            }
            catch (Exception ex)
            {
                Log("The XML editor failed to open: " + ex);
                _xmlView.Child = new TextBlock { Margin = new Thickness(6), TextWrapping = TextWrapping.Wrap, Text = "The XML editor failed to open: " + ex.Message };
            }

            ReloadDesigners();
            QueueInitialSelection();
            return true;
        }

        // The code window the XML editor itself uses, over the same buffer: colorizing, IntelliSense, find, outlining.
        // Keyboard commands reach it through this pane's IOleCommandTarget.
        void CreateCodeWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            var adapters = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            _codeWindow = adapters.CreateVsCodeWindowAdapter((IOleServiceProvider)GetService(typeof(IOleServiceProvider)));
            var behavior = (uint)(_codewindowbehaviorflags.CWB_DISABLESPLITTER | _codewindowbehaviorflags.CWB_DISABLEDROPDOWNBAR);
            ErrorHandler.ThrowOnFailure(((IVsCodeWindowEx)_codeWindow).Initialize(behavior, VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_Filter, string.Empty, string.Empty, 0, new INITVIEW[1]));
            ErrorHandler.ThrowOnFailure(_codeWindow.SetBuffer(_textLines));
            _codeWindowHost = new CodeWindowHost(_codeWindow);
            // The text view exists once the host's window has been made.
            _codeWindowHost.Loaded += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                HookTextView();
                QueueInitialSelection();
            };
            _xmlView.Child = _codeWindowHost;
        }

        // When the designers have loaded the file and the editor is up, whatever the designer showing has selected (the
        // first feature, say) is shown in the XML, once. Each can come first, so both ask; the editor is laid out first.
        void QueueInitialSelection()
        {
            if (_initialSelectionPending)
            {
                ThreadHelper.JoinableTaskFactory.StartOnIdle(ShowInitialSelection).FileAndForget("iswix/designer/initialselection");
            }
        }

        void ShowInitialSelection()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_initialSelectionPending && _buffer != null && _designerList.SelectedItem != null && HookTextView() != null)
            {
                _initialSelectionPending = false;
                ShowCurrentDesignerSelection();
            }
        }

        void ShowCurrentDesignerSelection()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_designerList.SelectedItem is ISelectionLinked designer)
            {
                ShowDesignerSelection(designer, false);
            }
        }

        IWpfTextView? HookTextView()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_wpfView is null && TextView is { } view)
            {
                var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
                _wpfView = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(view);
                if (_wpfView != null)
                {
                    _wpfView.Caret.PositionChanged += OnCaretMoved;
                }
            }

            return _wpfView;
        }

        // Linking the selections: choosing something in a designer puts the editor's caret at its element, and moving
        // the caret in the editor selects the item for the element it is in. Elements are named by their path in the
        // document (see XmlPositions), and the editor's text and the designers' document are the same text while no
        // change is waiting to reach either.
        void OnDesignerSelectionChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowDesignerSelection(sender, false);
        }

        void ShowDesignerSelection(object sender, bool waited)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var linked = sender as ISelectionLinked;
            if (_syncing || _reloading || !ReferenceEquals(sender, _designerList.SelectedItem) || linked?.SelectedElement is not { } element)
            {
                return;
            }

            // Selecting in a designer can touch the document, and the text it is written to waits for idle time: wait for
            // that, as the element's place in the text isn't known until it is there.
            if (_writePending && !waited)
            {
                ThreadHelper.JoinableTaskFactory.StartOnIdle(() => ShowDesignerSelection(sender, true)).FileAndForget("iswix/designer/showselection");
                return;
            }

            var attribute = linked.SelectedAttribute;

            var path = PathOf(element);
            if (path is null || _buffer is null || _reloadTimer.IsEnabled || _writePending || HookTextView() is not { } view)
            {
                Log($"Did not show the designer's selection: path {(path is null ? "none" : "found")}, text {(_buffer is null ? "none" : "found")}, reload {(_reloadTimer.IsEnabled ? "pending" : "idle")}, write {(_writePending ? "pending" : "idle")}");
                return;
            }

            try
            {
                var snapshot = _buffer.CurrentSnapshot;
                if (XmlPositions.RangeOf(snapshot.GetText(), path, attribute) is { } range)
                {
                    // Selected rather than just the caret, which the editor doesn't show while the designer has focus.
                    var span = new SnapshotSpan(snapshot, range[0], range[1] - range[0]);
                    _syncing = true;
                    view.Selection.Select(span, false);
                    view.Caret.MoveTo(span.End);
                    view.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.ShowStart);
                    Log($"Selected {span.Length} characters at {span.Start} for the designer's selection");
                }
            }
            catch (XmlException)
            {
                // Not well-formed: the designers are read-only until it is, and show the last good version.
            }
            finally
            {
                _syncing = false;
            }
        }

        void OnCaretMoved(object sender, CaretPositionChangedEventArgs e)
        {
            // Whether the editor has focus is checked when the caret has rested: on a click the caret can move first.
            if (!_syncing)
            {
                _caretTimer.Stop();
                _caretTimer.Start();
            }
        }

        void SelectDesignerItemAtCaret()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_buffer is null || _wpfView is null || _reloadTimer.IsEnabled || _writePending || !XmlFocused)
            {
                return;
            }

            try
            {
                var location = XmlPositions.LocationAt(_buffer.CurrentSnapshot.GetText(), _wpfView.Caret.Position.BufferPosition.Position);
                var element = location is null ? null : ElementAt(location.Path);
                if (element is null)
                {
                    return;
                }

                // The designer showing if it has an item for the element, else the first one that does.
                var designer = _designerList.SelectedItem as ISelectionLinked;
                if (designer?.Handles(element, location!.Attribute) != true)
                {
                    designer = _designerList.Items.OfType<ISelectionLinked>().FirstOrDefault(d => d.Handles(element, location!.Attribute));
                }

                if (designer is null)
                {
                    return;
                }

                _syncing = true;
                if (!ReferenceEquals(designer, _designerList.SelectedItem))
                {
                    _designerList.SelectedItem = designer;
                }

                designer.SelectElement(element, location!.Attribute);
            }
            catch (XmlException)
            {
                // Not well-formed: there is no element to find until it is.
            }
            finally
            {
                _syncing = false;
            }
        }

        int[]? PathOf(XElement element)
        {
            // An element of another document, a stale one say, has no place in this one's text.
            return ReferenceEquals(element.Document, _documentManager.Document) ? XmlPositions.PathOf(element) : null;
        }

        XElement? ElementAt(int[] path) => _documentManager.Document is { } document ? XmlPositions.ElementAt(document, path) : null;

        IVsTextView? TextView
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return _codeWindowHost?.TextView;
            }
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
            _reloadTimer.Stop();
            _reloadTimer.Start();
        }

        void ReloadDesigners()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_buffer is null)
            {
                return;
            }

            _reloading = true;
            try
            {
                _documentManager.LoadText(_buffer.CurrentSnapshot.GetText(), _filePath);
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
                    ShowStatus("None of IsWiX's designers applies to this file; edit it in the XML below.");
                }

                Log("Loaded the designers from the buffer");
            }
            catch (XmlException ex)
            {
                // Mid-edit text is often not well-formed; keep showing the last good version until it is again.
                Log("The buffer is not well-formed XML: " + ex.Message);
                ShowStatus($"The XML has an error at line {ex.LineNumber}; the designers show the last version that loaded, read-only, until it's fixed in the XML below. {ex.Message}");
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
            ThreadHelper.ThrowIfNotOnUIThread();
            UIElement? view = null;
            if (_designerList.SelectedItem is System.Windows.Forms.Control winForms and IFireworksDesigner winFormsDesigner)
            {
                // Each WinForms designer keeps a host of its own, made once with the theme's background and the
                // designer themed before it's first shown: moving a designer into another host re-creates its window,
                // which shows white until it paints.
                if (!_winFormsHosts.TryGetValue(winFormsDesigner, out var host))
                {
                    host = new WindowsFormsHost();
                    host.SetResourceReference(WindowsFormsHost.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
                    VsTheme.Apply(winForms);
                    host.Child = winForms;
                    _winFormsHosts.Add(winFormsDesigner, host);
                }

                view = host;
            }
            else if (_designerList.SelectedItem is UIElement wpf)
            {
                view = wpf;
            }

            // Designers shown before stay in the view, hidden, so switching back to one doesn't rebuild it.
            if (view != null && !_designerHost.Children.Contains(view))
            {
                _designerHost.Children.Add(view);
            }

            foreach (UIElement child in _designerHost.Children)
            {
                child.Visibility = ReferenceEquals(child, view) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (_designerList.SelectedItem is not IFireworksDesigner designer)
            {
                return;
            }

            designer.LoadData();
            if (designer is System.Windows.Forms.Control loaded)
            {
                // After LoadData, which fills the trees and lists.
                ThemeWinForms(loaded);
            }
        }

        void ThemeWinForms(System.Windows.Forms.Control control)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VsTheme.Apply(control);
            VsTheme.ThemeScrollBars(this, control);
            foreach (var child in Descendants(control))
            {
                VsTheme.ThemeScrollBars(this, child);
            }

            _themedWinForms.Add(control);
        }

        static IEnumerable<System.Windows.Forms.Control> Descendants(System.Windows.Forms.Control control) =>
            control.Controls.Cast<System.Windows.Forms.Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));

        void OnThemeChanged(ThemeChangedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ApplyTheme();
        }

        // A pane can miss a theme change (seen with documents Visual Studio restored at startup: the WPF views kept the
        // old theme's colors), so one that's shown in colors other than the theme's catches up then.
        void OnRootVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_root.IsVisible && _themeApplied != VsTheme.Background)
            {
                Log("Showing in an earlier theme's colors; applying the current theme");
                ApplyTheme();
            }
        }

        void ApplyTheme()
        {
            // The WPF views' theme resources are looked up afresh; the WinForms designers' colors and all the icons are
            // set, so they're set again.
            ThreadHelper.ThrowIfNotOnUIThread();
            _themeApplied = VsTheme.Background;
            VsTheme.Apply(_designView);
            _designerList.Items.Refresh();
            foreach (var control in _themedWinForms)
            {
                ThemeWinForms(control);
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

        // Commands: everything goes to the XML editor while its view shows. On the Design view, undo and redo go to the
        // buffer's undo manager (they cover designer edits too, which are buffer edits); other commands are left to the
        // designer controls.
        static bool IsUndo(Guid group, uint id) =>
            group == VSConstants.GUID_VSStandardCommandSet97 && (id == (uint)VSConstants.VSStd97CmdID.Undo || id == (uint)VSConstants.VSStd97CmdID.MultiLevelUndo);

        static bool IsRedo(Guid group, uint id) =>
            group == VSConstants.GUID_VSStandardCommandSet97 && (id == (uint)VSConstants.VSStd97CmdID.Redo || id == (uint)VSConstants.VSStd97CmdID.MultiLevelRedo);

        IOleUndoManager? UndoManager
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return ErrorHandler.Succeeded(_textLines.GetUndoManager(out var manager)) ? manager : null;
            }
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (XmlFocused)
            {
                return TextView is IOleCommandTarget target
                    ? target.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText)
                    : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            if (cCmds == 1 && UndoManager is { } undo && (IsUndo(pguidCmdGroup, prgCmds[0].cmdID) || IsRedo(pguidCmdGroup, prgCmds[0].cmdID)))
            {
                var available = CanUndoOrRedo(undo, IsUndo(pguidCmdGroup, prgCmds[0].cmdID));
                prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | (available ? OLECMDF.OLECMDF_ENABLED : 0));
                return VSConstants.S_OK;
            }

            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (XmlFocused)
            {
                return TextView is IOleCommandTarget target
                    ? target.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
                    : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            if (UndoManager is { } undo)
            {
                if (IsUndo(pguidCmdGroup, nCmdID))
                {
                    undo.UndoTo(null);
                    return VSConstants.S_OK;
                }

                if (IsRedo(pguidCmdGroup, nCmdID))
                {
                    undo.RedoTo(null);
                    return VSConstants.S_OK;
                }
            }

            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        // The undo manager reports an empty stack by failing (an exception), not with a result.
        static bool CanUndoOrRedo(IOleUndoManager undo, bool isUndo)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                string description;
                if (isUndo)
                {
                    undo.GetLastUndoDescription(out description);
                }
                else
                {
                    undo.GetLastRedoDescription(out description);
                }

                return true;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (disposing)
            {
                _reloadTimer.Stop();
                _caretTimer.Stop();
                if (_wpfView != null)
                {
                    _wpfView.Caret.PositionChanged -= OnCaretMoved;
                }

                foreach (var linked in _designers.OfType<ISelectionLinked>())
                {
                    linked.SelectedElementChanged -= OnDesignerSelectionChanged;
                }

                VSColorTheme.ThemeChanged -= OnThemeChanged;
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

                _codeWindowHost?.Dispose();
                _codeWindow?.Close();
                foreach (var host in _winFormsHosts.Values)
                {
                    host.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
