using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace IsWiX.Designer
{
    // The layout of the Design and XML views: side by side or one above the other, either first, one shown alone, and
    // either one floating in a window of its own. A bar over the views and the commands in the View menu (IsWiXCommands.vsct)
    // change it, and the choice is kept for the next editor.
    internal sealed partial class WixDesignerPane
    {
        // IsWiXCommands.vsct: the command set and the layout commands in it.
        static readonly Guid LayoutCommandSet = new Guid("f9d8589d-bf0e-4b75-ab03-eb3d9cffdc9e");
        const uint CmdHorizontal = 0x0110;
        const uint CmdVertical = 0x0111;
        const uint CmdCycle = 0x0112;
        const uint CmdFloatDesign = 0x0115;
        const uint CmdFloatXml = 0x0116;

        // The smallest a view is made by the splitter.
        const double MinimumPane = 80;

        static int _floatingCount;

        // The editor's content: the layout bar, then the views.
        readonly Grid _root = new Grid();
        readonly Grid _panes = new Grid();
        readonly TextBlock _bothFloating = new TextBlock { Margin = new Thickness(12), TextWrapping = TextWrapping.Wrap, Text = "The Design and XML views are both floating. Close their windows, or use Float Design and Float XML again, to dock them here.", Visibility = Visibility.Collapsed };

        DesignerLayoutSettings _layout = new DesignerLayoutSettings();
        readonly ToggleButton _horizontalButton = LayoutToggle("Horizontal", "Design and XML, one above the other (Alt+Shift+1)");
        readonly ToggleButton _verticalButton = LayoutToggle("Vertical", "Design and XML, side by side (Alt+Shift+2)");
        readonly Button _viewButton = new Button { ToolTip = "Cycle what shows: both views, the Design view alone, the XML view alone (Alt+Shift+3)", Margin = new Thickness(6, 0, 0, 0), Padding = new Thickness(8, 2, 8, 2), MinWidth = 90 };
        readonly ToggleButton _floatDesignButton = LayoutToggle("Float Design", "Move the Design view to a window of its own, or dock it back (Alt+Shift+4)");
        readonly ToggleButton _floatXmlButton = LayoutToggle("Float XML", "Move the XML view to a window of its own, or dock it back (Alt+Shift+5)");

        // The windows the views float in, while they do.
        FloatingHost? _designFloat;
        FloatingHost? _xmlFloat;
        bool _disposed;

        static ToggleButton LayoutToggle(string text, string tip) =>
            new ToggleButton { Content = text, ToolTip = tip, Margin = new Thickness(6, 0, 0, 0), Padding = new Thickness(8, 2, 8, 2) };

        // Builds the bar and the views' grid and puts the layout last used in place.
        void BuildLayout()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _layout = DesignerLayoutSettings.Load(this);

            var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 3, 2, 3) };
            bar.Children.Add(_horizontalButton);
            bar.Children.Add(_verticalButton);
            bar.Children.Add(_viewButton);
            bar.Children.Add(new Separator { Margin = new Thickness(8, 0, 2, 0), Width = 1 });
            bar.Children.Add(_floatDesignButton);
            bar.Children.Add(_floatXmlButton);
            var border = new Border { Child = bar, BorderThickness = new Thickness(0, 0, 0, 1) };
            border.SetResourceReference(Border.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey);
            VsTheme.Apply(border);

            _horizontalButton.Click += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); SetSplit(LayoutMode.Horizontal); };
            _verticalButton.Click += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); SetSplit(LayoutMode.Vertical); };
            _viewButton.Click += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); CycleView(); };
            _floatDesignButton.Click += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); ToggleFloat(true); };
            _floatXmlButton.Click += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); ToggleFloat(false); };

            _splitter.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.ToolWindowBorderBrushKey);
            // Dragging the splitter sets the share each view has, kept for the next editor; double-clicking it hides the second view.
            _splitter.DragCompleted += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); CaptureRatio(); _layout.Save(this); };
            _splitter.MouseDoubleClick += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                ToggleOnly(LayoutMode.DesignOnly);
            };

            _root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(_panes, 1);
            _root.Children.Add(border);
            _root.Children.Add(_panes);

            _panes.Children.Add(_bothFloating);
            _panes.Children.Add(_designView);
            _panes.Children.Add(_splitter);
            _panes.Children.Add(_xmlView);
            ApplyLayout();
        }

        // ---------------------------------------------------------------- changing the layout

        void SetSplit(LayoutMode split)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            CaptureRatio();
            _layout.Split = split;
            _layout.Mode = split;
            ChangedLayout();
        }

        // Shows one view alone; asked again, goes back to the split.
        void ToggleOnly(LayoutMode only)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            CaptureRatio();
            _layout.Mode = _layout.Mode == only ? _layout.Split : only;
            ChangedLayout();
        }

        // Both views, then the Design view alone, then the XML view alone, then both again.
        void CycleView()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            CaptureRatio();
            _layout.Mode = _layout.Mode == LayoutMode.DesignOnly ? LayoutMode.XmlOnly
                : _layout.Mode == LayoutMode.XmlOnly ? _layout.Split
                : LayoutMode.DesignOnly;
            ChangedLayout();
        }

        void ChangedLayout()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ApplyLayout();
            _layout.Save(this);
        }

        // The splitter has moved: what share the first view has now.
        void CaptureRatio()
        {
            if (_layout.Mode != LayoutMode.Horizontal && _layout.Mode != LayoutMode.Vertical)
            {
                return;
            }

            if (_designFloat != null || _xmlFloat != null)
            {
                return;
            }

            var horizontal = _layout.Mode == LayoutMode.Horizontal;
            var a = horizontal ? _designView.ActualHeight : _designView.ActualWidth;
            var b = horizontal ? _xmlView.ActualHeight : _xmlView.ActualWidth;
            if (a > 0 && b > 0)
            {
                _layout.Ratio = Math.Min(0.9, Math.Max(0.1, a / (a + b)));
            }
        }

        // Puts the views where the layout says. A view stays in the grid when it is hidden, so its window keeps
        // its place; only a floating one is out of it.
        void ApplyLayout()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var design = _designFloat == null;
            var xml = _xmlFloat == null;
            var split = design && xml && (_layout.Mode == LayoutMode.Horizontal || _layout.Mode == LayoutMode.Vertical);

            SetDocked(_designView, design);
            SetDocked(_xmlView, xml);
            _panes.RowDefinitions.Clear();
            _panes.ColumnDefinitions.Clear();
            _bothFloating.Visibility = !design && !xml ? Visibility.Visible : Visibility.Collapsed;

            if (split)
            {
                var horizontal = _layout.Mode == LayoutMode.Horizontal;
                var first = _designView;
                var second = _xmlView;
                var ratio = Math.Min(0.9, Math.Max(0.1, _layout.Ratio));
                var firstSize = new GridLength(ratio, GridUnitType.Star);
                var secondSize = new GridLength(1 - ratio, GridUnitType.Star);
                _designView.Visibility = Visibility.Visible;
                _xmlView.Visibility = Visibility.Visible;
                _splitter.Visibility = Visibility.Visible;
                if (horizontal)
                {
                    _panes.RowDefinitions.Add(new RowDefinition { Height = firstSize, MinHeight = MinimumPane });
                    _panes.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    _panes.RowDefinitions.Add(new RowDefinition { Height = secondSize, MinHeight = MinimumPane });
                    _splitter.Width = double.NaN;
                    _splitter.Height = 5;
                    _splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _splitter.VerticalAlignment = VerticalAlignment.Center;
                    _splitter.ResizeDirection = GridResizeDirection.Rows;
                    Place(first, 0, 0);
                    Place(_splitter, 1, 0);
                    Place(second, 2, 0);
                }
                else
                {
                    _panes.ColumnDefinitions.Add(new ColumnDefinition { Width = firstSize, MinWidth = MinimumPane });
                    _panes.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    _panes.ColumnDefinitions.Add(new ColumnDefinition { Width = secondSize, MinWidth = MinimumPane });
                    _splitter.Height = double.NaN;
                    _splitter.Width = 5;
                    _splitter.HorizontalAlignment = HorizontalAlignment.Center;
                    _splitter.VerticalAlignment = VerticalAlignment.Stretch;
                    _splitter.ResizeDirection = GridResizeDirection.Columns;
                    Place(first, 0, 0);
                    Place(_splitter, 0, 1);
                    Place(second, 0, 2);
                }
            }
            else
            {
                // One view shows: the one chosen, or the one that is left when the other floats.
                var showDesign = design && (!xml || _layout.Mode != LayoutMode.XmlOnly);
                _splitter.Visibility = Visibility.Collapsed;
                // A floating view is shown in its window, whatever shows here.
                _designView.Visibility = !design || showDesign ? Visibility.Visible : Visibility.Collapsed;
                _xmlView.Visibility = !xml || !showDesign ? Visibility.Visible : Visibility.Collapsed;
                Place(_designView, 0, 0);
                Place(_xmlView, 0, 0);
            }

            UpdateBar();
        }

        static void Place(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
        }

        // A view is in the grid, or floating and out of it.
        void SetDocked(FrameworkElement view, bool docked)
        {
            var inGrid = _panes.Children.Contains(view);
            if (docked && !inGrid)
            {
                _panes.Children.Add(view);
            }
            else if (!docked && inGrid)
            {
                _panes.Children.Remove(view);
            }
        }

        void UpdateBar()
        {
            var floating = _designFloat != null || _xmlFloat != null;
            _horizontalButton.IsChecked = _layout.Mode == LayoutMode.Horizontal;
            _verticalButton.IsChecked = _layout.Mode == LayoutMode.Vertical;
            _viewButton.Content = _layout.Mode == LayoutMode.DesignOnly ? "Design only" : _layout.Mode == LayoutMode.XmlOnly ? "XML only" : "Both views";
            _floatDesignButton.IsChecked = _designFloat != null;
            _floatXmlButton.IsChecked = _xmlFloat != null;
            _horizontalButton.IsEnabled = !floating;
            _verticalButton.IsEnabled = !floating;
            _viewButton.IsEnabled = !floating;
        }

        // ---------------------------------------------------------------- floating

        // Moves a view into a window of its own, or closes that window, which docks the view back. The window is a
        // Visual Studio tool window; if Visual Studio won't make one, an ordinary window owned by Visual Studio's.
        void ToggleFloat(bool design)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var existing = design ? _designFloat : _xmlFloat;
            if (existing != null)
            {
                existing.Close();
                return;
            }

            CaptureRatio();
            var view = design ? (FrameworkElement)_designView : _xmlView;
            var caption = Path.GetFileName(_filePath) + (design ? " - Design" : " - XML");
            SetDocked(view, false);
            view.Visibility = Visibility.Visible;
            var host = TryToolWindow(design, view, caption) ?? PlainWindow(design, view, caption);
            Log("The " + (design ? "Design" : "XML") + " view is floating in " + (host.IsToolWindow ? "a tool window." : "an ordinary window."));
            UpdateBar();
        }

        void SetFloat(bool design, FloatingHost? host)
        {
            if (design)
            {
                _designFloat = host;
            }
            else
            {
                _xmlFloat = host;
            }
        }

        // The view in a Visual Studio tool window, or null (the view not touched) if that can't be done.
        FloatingHost? TryToolWindow(bool design, FrameworkElement view, string caption)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            FloatingViewWindow? window = null;
            FloatingHost? host = null;
            try
            {
                var shell = (IVsShell)GetService(typeof(SVsShell));
                var packageGuid = new Guid(IsWiXPackage.PackageGuidString);
                ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref packageGuid, out var loaded));
                window = (FloatingViewWindow)((Package)loaded).FindToolWindow(typeof(FloatingViewWindow), ++_floatingCount, true);
                var frame = window?.Frame as IVsWindowFrame ?? throw new InvalidOperationException("The tool window has no frame.");
                window.Caption = caption;
                window.Owner = this;
                host = new FloatingHost(window);
                window.Content = view;
                window.Closing += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); DockBack(design, host); };
                SetFloat(design, host);
                ApplyLayout();
                frame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Float);
                ErrorHandler.ThrowOnFailure(frame.Show());
                return host;
            }
            catch (Exception ex)
            {
                Log("A tool window could not be made for a floating view: " + ex);
                if (host != null)
                {
                    SetFloat(design, null);
                    host.Release();
                    host.Close();
                }

                return null;
            }
        }

        // The view in an ordinary window, owned by Visual Studio's so it stays with it.
        FloatingHost PlainWindow(bool design, FrameworkElement view, string caption)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var window = new Window { Title = caption, Width = design ? 900 : 800, Height = 600, ShowInTaskbar = false };
            window.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
            if (GetService(typeof(SVsUIShell)) is IVsUIShell uiShell && ErrorHandler.Succeeded(uiShell.GetDialogOwnerHwnd(out var owner)))
            {
                new WindowInteropHelper(window).Owner = owner;
            }

            var host = new FloatingHost(window);
            window.Content = view;
            window.Closing += (s, e) => { ThreadHelper.ThrowIfNotOnUIThread(); DockBack(design, host); };
            SetFloat(design, host);
            ApplyLayout();
            window.Show();
            return host;
        }

        // The floating window is closing (or the editor is): the view comes back to the editor.
        void DockBack(bool design, FloatingHost host)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!ReferenceEquals(design ? _designFloat : _xmlFloat, host))
            {
                return;
            }

            host.Release();
            SetFloat(design, null);
            if (!_disposed)
            {
                ApplyLayout();
            }
        }

        // The editor is closing: the floating windows go with it, the views first taken back so they close with the editor.
        void CloseFloatingWindows()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _disposed = true;
            foreach (var pair in new[] { (design: true, host: _designFloat), (design: false, host: _xmlFloat) })
            {
                if (pair.host == null)
                {
                    continue;
                }

                DockBack(pair.design, pair.host);
                pair.host.Close();
            }
        }

        // ---------------------------------------------------------------- commands

        // The layout commands of the View menu. They are answered wherever the focus is in the editor, the XML view included.
        bool TryLayoutQuery(Guid group, uint count, OLECMD[] commands)
        {
            if (group != LayoutCommandSet)
            {
                return false;
            }

            var floating = _designFloat != null || _xmlFloat != null;
            for (var i = 0; i < count; i++)
            {
                bool? latched;
                var enabled = true;
                switch (commands[i].cmdID)
                {
                    case CmdHorizontal:
                        latched = _layout.Mode == LayoutMode.Horizontal;
                        enabled = !floating;
                        break;
                    case CmdVertical:
                        latched = _layout.Mode == LayoutMode.Vertical;
                        enabled = !floating;
                        break;
                    case CmdCycle:
                        latched = null;
                        enabled = !floating;
                        break;
                    case CmdFloatDesign:
                        latched = _designFloat != null;
                        break;
                    case CmdFloatXml:
                        latched = _xmlFloat != null;
                        break;
                    default:
                        commands[i].cmdf = 0;
                        continue;
                }

                commands[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | (enabled ? OLECMDF.OLECMDF_ENABLED : 0) | (latched == true ? OLECMDF.OLECMDF_LATCHED : 0));
            }

            return true;
        }

        bool TryLayoutExec(Guid group, uint id)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (group != LayoutCommandSet)
            {
                return false;
            }

            var floating = _designFloat != null || _xmlFloat != null;
            switch (id)
            {
                case CmdHorizontal when !floating:
                    SetSplit(LayoutMode.Horizontal);
                    return true;
                case CmdVertical when !floating:
                    SetSplit(LayoutMode.Vertical);
                    return true;
                case CmdCycle when !floating:
                    CycleView();
                    return true;
                case CmdFloatDesign:
                    ToggleFloat(true);
                    return true;
                case CmdFloatXml:
                    ToggleFloat(false);
                    return true;
                default:
                    return false;
            }
        }
    }
    /// <summary>The window a view floats in: a Visual Studio tool window, or an ordinary window if Visual Studio wouldn't make one.</summary>
    internal sealed class FloatingHost
    {
        readonly FloatingViewWindow? _tool;
        readonly Window? _plain;

        public FloatingHost(FloatingViewWindow tool) => _tool = tool;

        public FloatingHost(Window plain) => _plain = plain;

        public bool IsToolWindow => _tool != null;

        /// <summary>Takes the view out of the window, so closing the window doesn't take it down.</summary>
        public void Release()
        {
            if (_tool != null)
            {
                _tool.Content = null;
                _tool.Owner = null;
            }
            else if (_plain != null)
            {
                _plain.Content = null;
            }
        }

        public void Close()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_tool != null)
            {
                if (_tool.Frame is IVsWindowFrame frame)
                {
                    frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                }
            }
            else
            {
                _plain?.Close();
            }
        }
    }
}
