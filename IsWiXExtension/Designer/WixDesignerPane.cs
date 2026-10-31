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
using FireworksFramework.Services;
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
    internal sealed partial class WixDesignerPane : WindowPane, IOleCommandTarget, IPublishable, IVsTextBufferDataEvents
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

        // The Design and XML views share the editor with a splitter between them (WixDesignerPane.Layout.cs).
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

        // How long a designer's selection must rest before the XML shows it.
        static readonly TimeSpan SelectionDelay = TimeSpan.FromMilliseconds(60);

        readonly DispatcherTimer _selectionTimer;
        object? _selectionSender;

        IReadOnlyList<IFireworksDesigner> _designers = Array.Empty<IFireworksDesigner>();
        ITextBuffer? _buffer;
        SaveIndentationHook? _saveHook;
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

            _selectionTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = SelectionDelay };
            _selectionTimer.Tick += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _selectionTimer.Stop();
                if (_selectionSender is { } sender)
                {
                    _selectionSender = null;
                    ShowDesignerSelection(sender, false);
                }
            };

            _caretTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = CaretDelay };
            _caretTimer.Tick += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _caretTimer.Stop();
                SelectDesignerItemAtCaret();
            };

            BuildDesignView();
            BuildLayout();
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
            // A designer can take the user to the file that defines something its element refers to.
            FireworksFramework.Services.DesignerNavigation.GoToDefinition = DefinitionNavigator.GoTo;
            // The designers' lines go to the same trace as the window's, when it is on.
            if (Tracing.Enabled)
            {
                FireworksFramework.Services.DesignerTrace.Sink = (category, message) => Tracing.Write(category, message);
                Tracing.Write("Pane", () => $"Opened {_filePath}");
            }

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

            if (GetService(typeof(SVsRunningDocumentTable)) is IVsRunningDocumentTable documentTable)
            {
                _saveHook = new SaveIndentationHook(documentTable, _filePath, IndentBeforeSave);
            }

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
            if (_initialSelectionPending && _buffer != null && HookTextView() != null)
            {
                _initialSelectionPending = false;
                if (_pendingDefinition != null)
                {
                    // The file was opened to show a definition: that is the selection, not the designer's first.
                    ShowPendingDefinition();
                }
                else if (_designerList.SelectedItem != null)
                {
                    ShowCurrentDesignerSelection();
                }
            }
        }

        // Go to Definition: the window is asked to select an element (a component group, say) by its name and Id. A file
        // that was only just opened isn't ready for it, so it waits for the first selection.
        (string Element, string Id)? _pendingDefinition;

        /// <summary>Selects the element with the given name and Id in the XML, and in the designers, and takes the focus there.</summary>
        internal void ShowDefinition(string elementName, string id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _pendingDefinition = (elementName, id);
            Tracing.Write("GoTo", () => $"Asked to show {elementName} '{id}' in {_filePath}: {(_initialSelectionPending ? "the editor is not ready, waiting" : "showing now")}");
            if (_initialSelectionPending)
            {
                QueueInitialSelection();
            }
            else
            {
                ShowPendingDefinition();
            }
        }

        void ShowPendingDefinition()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_pendingDefinition is not { } target || _buffer is null || HookTextView() is not { } view)
            {
                Tracing.Write("GoTo", () => _pendingDefinition is null ? "Nothing to show" : "Not ready to show the definition yet: it stays pending");
                return;
            }

            _pendingDefinition = null;
            var snapshot = _buffer.CurrentSnapshot;
            XmlPositions.Found? found;
            try
            {
                found = XmlPositions.FindElement(snapshot.GetText(), target.Element, "Id", target.Id);
            }
            catch (XmlException)
            {
                found = null;
            }

            if (found is null)
            {
                Log($"Did not find the {target.Element} '{target.Id}' to show");
                return;
            }

            var span = new SnapshotSpan(snapshot, found.Start, found.End - found.Start);
            Tracing.Write("GoTo", () => $"Showing {target.Element} '{target.Id}' at {span.Start}, {span.Length} characters");
            _syncing = true;
            try
            {
                view.Selection.Select(span, false);
                view.Caret.MoveTo(span.End);
                view.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);
            }
            finally
            {
                _syncing = false;
            }

            if (ElementAt(found.Path) is { } element)
            {
                SelectInDesigners(element, null);
            }

            TextView?.SendExplicitFocus();
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
                    _wpfView.VisualElement.PreviewMouseRightButtonDown += OnEditorRightClick;
                    _wpfView.VisualElement.PreviewMouseLeftButtonDown += OnEditorOtherInput;
                    _wpfView.VisualElement.PreviewKeyDown += OnEditorOtherInput;

                    // In front of the XML editor's handlers on the view: its context menu's commands go to the view.
                    _viewFilter = new GoToDefinitionFilter(this);
                    if (ErrorHandler.Succeeded(view.AddCommandFilter(_viewFilter, out var next)))
                    {
                        _viewFilter.Next = next;
                        _filteredView = view;
                    }
                }
            }

            return _wpfView;
        }

        GoToDefinitionFilter? _viewFilter;
        IVsTextView? _filteredView;

        /// <summary>Gets whether the command is Go to Definition and the caret (or the last right-click) is on a component group reference.</summary>
        internal bool CanGoToDefinition(Guid group, uint id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return IsGoToDefinition(group, id) && TryGetComponentGroupRefAtCaret(out _);
        }

        /// <summary>Runs Go to Definition if the command is that and it is on a component group reference.</summary>
        internal bool TryGoToDefinition(Guid group, uint id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!IsGoToDefinition(group, id) || !TryGetComponentGroupRefAtCaret(out var componentGroupId))
            {
                return false;
            }

            // Used up: the next one is from wherever the caret or the next right-click is.
            _rightClickOffset = null;
            GoToComponentGroup(componentGroupId);
            return true;
        }

        // Where in the text the last right-click was, until the caret or the keyboard is used again: the editor's context
        // menu acts on what was clicked, and a right-click doesn't always move the caret there.
        int? _rightClickOffset;

        void OnEditorRightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _rightClickOffset = null;
            if (_wpfView is not { } view)
            {
                return;
            }

            var point = e.GetPosition(view.VisualElement);
            var line = view.TextViewLines.GetTextViewLineContainingYCoordinate(point.Y + view.ViewportTop);
            if (line is null)
            {
                return;
            }

            // Past the end of a line is its end.
            _rightClickOffset = (line.GetBufferPositionFromXCoordinate(point.X + view.ViewportLeft) ?? line.End).Position;
            Tracing.Write("Command", () => $"Right-click in the editor at offset {_rightClickOffset}, the caret at {view.Caret.Position.BufferPosition.Position}");
        }

        void OnEditorOtherInput(object sender, EventArgs e) => _rightClickOffset = null;

        // Linking the selections: choosing something in a designer puts the editor's caret at its element, and moving
        // the caret in the editor selects the item for the element it is in. Elements are named by their path in the
        // document (see XmlPositions), and the editor's text and the designers' document are the same text while no
        // change is waiting to reach either.
        void OnDesignerSelectionChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Tracing.Write("Link", () => $"{sender.GetType().Name} raised a selection: {(_syncing ? "ignored, a selection is being moved" : _reloading ? "ignored, the designers are reloading" : "queued")}");
            if (_syncing || _reloading)
            {
                return;
            }

            // A designer raises this for each item as a list selects many (select all), and each is a scan of the text
            // and a move of the XML selection: only where the selection settles is shown.
            _selectionSender = sender;
            _selectionTimer.Stop();
            _selectionTimer.Start();
        }

        void ShowDesignerSelection(object sender, bool waited)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var linked = sender as ISelectionLinked;
            if (_syncing || _reloading || !ReferenceEquals(sender, _designerList.SelectedItem) || linked?.SelectedElement is not { } element)
            {
                Tracing.Write("Link", () => $"{sender.GetType().Name}'s selection not shown: " +
                    (_syncing ? "a selection is being moved" : _reloading ? "the designers are reloading" : !ReferenceEquals(sender, _designerList.SelectedItem) ? "it is not the designer showing" : "it has no element selected"));
                return;
            }

            Tracing.Write("Link", () => $"Showing {sender.GetType().Name}'s selection: <{element.Name.LocalName}> attribute {linked.SelectedAttribute ?? "(none)"}");

            // Selecting in a designer can touch the document, and the text it is written to waits for idle time: wait for
            // that, as the element's place in the text isn't known until it is there.
            if (_writePending && !waited)
            {
                Tracing.Write("Link", () => "A write to the text is pending: waiting for idle time to show the selection");
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
                    if (!view.Selection.IsEmpty && view.Selection.StreamSelectionSpan.SnapshotSpan == span)
                    {
                        // Already showing it: selecting and scrolling again would only repaint the editor.
                        Tracing.Write("Link", () => "The XML already shows it");
                        return;
                    }

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
                // The caret is where the user is working now; a right-click's position was from before.
                _rightClickOffset = null;
                _caretTimer.Stop();
                _caretTimer.Start();
            }
        }

        void SelectDesignerItemAtCaret()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_buffer is null || _wpfView is null || _reloadTimer.IsEnabled || _writePending || !XmlFocused)
            {
                Tracing.Write("Caret", () => "The caret rested but the designers were not updated: " +
                    (_buffer is null || _wpfView is null ? "the editor is not ready" : _reloadTimer.IsEnabled ? "a reload is pending" : _writePending ? "a write to the text is pending" : "the XML editor does not have the focus"));
                return;
            }

            try
            {
                var offset = _wpfView.Caret.Position.BufferPosition.Position;
                var location = XmlPositions.LocationAt(_buffer.CurrentSnapshot.GetText(), offset);
                var element = location is null ? null : ElementAt(location.Path);
                Tracing.Write("Caret", () => $"Caret at {offset}: " + (location is null ? "in no element" : $"<{location.ElementName}> path {string.Join("/", location.Path)} attribute {location.Attribute ?? "(none)"}, document element {(element is null ? "not found" : "found")}"));
                if (element is null)
                {
                    return;
                }

                SelectInDesigners(element, location!.Attribute);
            }
            catch (XmlException)
            {
                // Not well-formed: there is no element to find until it is.
            }
        }

        // Selects the element in the designer showing if it has an item for it, else in the first that does, switching to
        // it. The XML's selection isn't touched.
        void SelectInDesigners(XElement element, string? attribute)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var designer = _designerList.SelectedItem as ISelectionLinked;
            if (designer?.Handles(element, attribute) != true)
            {
                designer = _designerList.Items.OfType<ISelectionLinked>().FirstOrDefault(d => d.Handles(element, attribute));
            }

            if (designer is null)
            {
                Tracing.Write("Caret", () => $"No designer has an item for <{element.Name.LocalName}> attribute {attribute ?? "(none)"}");
                return;
            }

            Tracing.Write("Caret", () => $"Selecting <{element.Name.LocalName}> in {designer.GetType().Name}{(ReferenceEquals(designer, _designerList.SelectedItem) ? string.Empty : ", switching to it")}");
            _syncing = true;
            try
            {
                if (!ReferenceEquals(designer, _designerList.SelectedItem))
                {
                    _designerList.SelectedItem = designer;
                }

                designer.SelectElement(element, attribute);
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
        // These also go to the trace, when it is on.
        static void Log(string message)
        {
            ActivityLog.TryLogInformation("IsWiX Designer", message);
            Tracing.Write("Pane", message);
        }

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

            ReplaceChangedSpan(oldText, newText, "Wrote a designer change to the buffer");
        }

        // Replaces only the span that differs, so the rest of the buffer is untouched and the change is one undo step.
        void ReplaceChangedSpan(string oldText, string newText, string what)
        {
            if (_buffer is null)
            {
                return;
            }

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

            Log($"{what}: {oldText.Length - prefix - suffix} characters replaced at {prefix}");
        }

        // Visual Studio is about to save the file: indent the whole text in the style the file mostly uses, so the saved
        // file is evenly indented whatever was typed, pasted or merged in. Text that is not well-formed XML is left alone.
        void IndentBeforeSave()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_buffer is null)
            {
                return;
            }

            var oldText = _buffer.CurrentSnapshot.GetText();
            var newText = XmlIndentation.Format(oldText);
            if (newText != oldText)
            {
                ReplaceChangedSpan(oldText, newText, "Indented the document before saving");
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

        // Commands: everything goes to the XML editor while its view shows. On the Design view, undo and redo go to the
        // buffer's undo manager (they cover designer edits too, which are buffer edits); other commands are left to the
        // designer controls.
        static bool IsUndo(Guid group, uint id) =>
            group == VSConstants.GUID_VSStandardCommandSet97 && (id == (uint)VSConstants.VSStd97CmdID.Undo || id == (uint)VSConstants.VSStd97CmdID.MultiLevelUndo);

        static bool IsRedo(Guid group, uint id) =>
            group == VSConstants.GUID_VSStandardCommandSet97 && (id == (uint)VSConstants.VSStd97CmdID.Redo || id == (uint)VSConstants.VSStd97CmdID.MultiLevelRedo);

        static bool IsGoToDefinition(Guid group, uint id) =>
            group == VSConstants.GUID_VSStandardCommandSet97 && id == (uint)VSConstants.VSStd97CmdID.GotoDefn;

        // The Id of the component group the caret's element refers to, if it is in a ComponentGroupRef.
        bool TryGetComponentGroupRefAtCaret(out string id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            id = string.Empty;
            if (_buffer is null || _wpfView is null)
            {
                return false;
            }

            try
            {
                // Where the user right-clicked, if that is what opened the menu this was chosen from; else the caret.
                var offset = _rightClickOffset ?? _wpfView.Caret.Position.BufferPosition.Position;
                var location = XmlPositions.LocationAt(_buffer.CurrentSnapshot.GetText(), offset);
                Log($"Go to Definition at offset {offset} ({(_rightClickOffset.HasValue ? "right-click" : "caret")}): {(location is null ? "no element" : location.ElementName)}");
                if (location is { ElementName: "ComponentGroupRef", Id: { Length: > 0 } found })
                {
                    id = found;
                    return true;
                }
            }
            catch (XmlException)
            {
                // Not well-formed: no reference can be told from it.
            }

            return false;
        }

        // Opens the file that defines the component group, which may be this one, with it selected there.
        void GoToComponentGroup(string id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string? file = null;
            try
            {
                file = ProjectComponentGroups.FindDefinition(_filePath, XDocument.Parse(_buffer!.CurrentSnapshot.GetText()), id);
            }
            catch (XmlException)
            {
                // Not well-formed: this file's own groups can't be read, but the others' can.
                file = ProjectComponentGroups.FindInOtherFiles(_filePath).FirstOrDefault(g => g.Id == id)?.FilePath;
            }

            Tracing.Write("GoTo", () => $"Component group '{id}' is defined in {file ?? "no file of the project"} (project folder {ProjectComponentGroups.ProjectRoot(_filePath)})");
            if (file is null || !DefinitionNavigator.GoTo(file, "ComponentGroup", id))
            {
                VsShellUtilities.ShowMessageBox(
                    this,
                    file is null ? $"The component group '{id}' isn't defined in any .wxs file of this project." : $"Couldn't open {file}.",
                    "Go to Definition",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

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
            if (TryLayoutQuery(pguidCmdGroup, cCmds, prgCmds))
            {
                return VSConstants.S_OK;
            }

            if (cCmds == 1 && IsGoToDefinition(pguidCmdGroup, prgCmds[0].cmdID))
            {
                Tracing.Write("Command", () => $"Go to Definition queried; the XML editor {(XmlFocused ? "has" : "does not have")} the focus");
            }

            if (XmlFocused)
            {
                // Go to Definition is on a reference to a component group; the editor has nothing for it in XML.
                if (cCmds == 1 && CanGoToDefinition(pguidCmdGroup, prgCmds[0].cmdID))
                {
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                    return VSConstants.S_OK;
                }

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
            if (TryLayoutExec(pguidCmdGroup, nCmdID))
            {
                return VSConstants.S_OK;
            }

            if (IsGoToDefinition(pguidCmdGroup, nCmdID))
            {
                Tracing.Write("Command", () => $"Go to Definition run; the XML editor {(XmlFocused ? "has" : "does not have")} the focus");
            }

            if (XmlFocused)
            {
                if (TryGoToDefinition(pguidCmdGroup, nCmdID))
                {
                    return VSConstants.S_OK;
                }

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
                _selectionTimer.Stop();
                if (_wpfView != null)
                {
                    _wpfView.Caret.PositionChanged -= OnCaretMoved;
                    _wpfView.VisualElement.PreviewMouseRightButtonDown -= OnEditorRightClick;
                    _wpfView.VisualElement.PreviewMouseLeftButtonDown -= OnEditorOtherInput;
                    _wpfView.VisualElement.PreviewKeyDown -= OnEditorOtherInput;
                }

                if (_filteredView != null && _viewFilter != null)
                {
                    _filteredView.RemoveCommandFilter(_viewFilter);
                    _filteredView = null;
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

                _saveHook?.Dispose();
                _saveHook = null;

                if (_dataEventsPoint != null)
                {
                    _dataEventsPoint.Unadvise(_dataEventsCookie);
                    _dataEventsPoint = null;
                }

                CloseFloatingWindows();
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
