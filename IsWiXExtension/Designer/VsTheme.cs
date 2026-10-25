using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace IsWiX.Designer
{
    /// <summary>
    /// Gives the designers Visual Studio's theme colors (light, dark, blue). The WPF designers get implicit styles
    /// whose brushes are Visual Studio's theme resources, so they follow theme changes by themselves; the WinForms
    /// designers have their colors set, again whenever the theme changes.
    /// </summary>
    internal static class VsTheme
    {
        // WPF ----------------------------------------------------------------------------------------------------

        static readonly ConditionalWeakTable<FrameworkElement, ResourceDictionary> AppliedStyles = new ConditionalWeakTable<FrameworkElement, ResourceDictionary>();

        /// <summary>
        /// Applies the theme to <paramref name="root"/> and everything inside it. Applying it again replaces the styles
        /// with new ones, so every theme resource in the tree is looked up afresh: for a tree that missed a theme change.
        /// </summary>
        public static void Apply(FrameworkElement root)
        {
            root.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
            root.SetResourceReference(TextElement.ForegroundProperty, EnvironmentColors.ToolWindowTextBrushKey);
            if (AppliedStyles.TryGetValue(root, out var previous))
            {
                root.Resources.MergedDictionaries.Remove(previous);
                AppliedStyles.Remove(root);
            }

            var styles = Styles(root);
            root.Resources.MergedDictionaries.Add(styles);
            AppliedStyles.Add(root, styles);
            ImageThemingUtilities.SetThemeScrollBars(root, true);
        }

        /// <summary>The theme's window background, which tells one theme from another.</summary>
        public static Drawing.Color Background => Color(TreeViewColors.BackgroundColorKey);

        /// <summary>
        /// Gives a designer's icon for the designer list (bound to the designer itself). A designer that embeds a
        /// <c>.dark.ico</c> beside its icon has one drawn for each kind of background: the dark one is used in dark
        /// themes and the other in light ones, as drawn. A designer without one has its icon adapted the way Visual
        /// Studio adapts its own: drawn for a light background, it's inverted in luminance on a dark one, keeping its hues.
        /// </summary>
        public sealed class ThemedImageConverter : IValueConverter
        {
            readonly FireworksFramework.Services.ImageConverter _toWpf = new FireworksFramework.Services.ImageConverter();

            public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is not FireworksFramework.Interfaces.IFireworksDesigner designer)
                {
                    return null;
                }

                var background = Color(TreeViewColors.BackgroundColorKey);
                var dark = background.GetBrightness() < 0.5f;
                if (DesignerIcon(designer, dark: true) != null)
                {
                    return DesignerIcon(designer, dark);
                }

                var source = DesignerIcon(designer, dark: false) ?? _toWpf.Convert(designer.PluginImage, targetType, parameter, culture) as BitmapSource;
                return source is null ? null : ThemedBitmapSource(source);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
                throw new NotSupportedException();
        }

        // The designer list shows icons at 16 x 16 logical pixels.
        const int ListIconSize = 16;

        // A designer assembly's icon, or its .dark.ico, at the image size closest above what 16 logical pixels are on
        // this screen, so the 16 px drawing (often simplified) is the one shown at 100% scaling.
        static BitmapSource? DesignerIcon(FireworksFramework.Interfaces.IFireworksDesigner designer, bool dark)
        {
            var assembly = designer.GetType().Assembly;
            var name = assembly.GetManifestResourceNames().FirstOrDefault(n =>
                n.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) &&
                n.EndsWith(".dark.ico", StringComparison.OrdinalIgnoreCase) == dark);
            if (name is null)
            {
                return null;
            }

            using var stream = assembly.GetManifestResourceStream(name)!;
            var frames = new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames;
            var scale = Application.Current?.MainWindow is { } window ? VisualTreeHelper.GetDpi(window).DpiScaleX : 1.0;
            var wanted = (int)Math.Round(ListIconSize * scale);
            var frame = frames.OrderBy(f => f.PixelWidth).FirstOrDefault(f => f.PixelWidth >= wanted) ?? frames.OrderBy(f => f.PixelWidth).Last();
            frame.Freeze();
            return frame;
        }

        static BitmapSource ThemedBitmapSource(BitmapSource source)
        {
            var background = Color(TreeViewColors.BackgroundColorKey);
            var wpfBackground = System.Windows.Media.Color.FromArgb(background.A, background.R, background.G, background.B);
            return ImageThemingUtilities.GetOrCreateThemedBitmapSource(source, wpfBackground, true, Colors.White, SystemParameters.HighContrast, true);
        }

        static ResourceDictionary Styles(FrameworkElement root)
        {
            var styles = new ResourceDictionary();

            // Visual Studio's own styles for the common controls.
            AddBasedOn(styles, root, typeof(TextBox), VsResourceKeys.TextBoxStyleKey);
            AddBasedOn(styles, root, typeof(ComboBox), VsResourceKeys.ComboBoxStyleKey);
            AddBasedOn(styles, root, typeof(Button), VsResourceKeys.ButtonStyleKey);
            AddBasedOn(styles, root, typeof(CheckBox), VsResourceKeys.CheckBoxStyleKey);

            var dataGrid = new Style(typeof(DataGrid));
            dataGrid.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.BackgroundBrushKey));
            dataGrid.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.BackgroundTextBrushKey));
            dataGrid.Setters.Add(Dynamic(DataGrid.RowBackgroundProperty, TreeViewColors.BackgroundBrushKey));
            dataGrid.Setters.Add(Dynamic(DataGrid.HorizontalGridLinesBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            dataGrid.Setters.Add(Dynamic(DataGrid.VerticalGridLinesBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            dataGrid.Setters.Add(Dynamic(Control.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            styles[typeof(DataGrid)] = dataGrid;

            var header = new Style(typeof(DataGridColumnHeader));
            header.Setters.Add(Dynamic(Control.BackgroundProperty, HeaderColors.DefaultBrushKey));
            header.Setters.Add(Dynamic(Control.ForegroundProperty, HeaderColors.DefaultTextBrushKey));
            header.Setters.Add(Dynamic(Control.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            header.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
            header.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4, 2, 4, 2)));
            styles[typeof(DataGridColumnHeader)] = header;

            styles[typeof(DataGridRow)] = Selectable(typeof(DataGridRow), DataGridRow.IsSelectedProperty);
            var cell = Selectable(typeof(DataGridCell), DataGridCell.IsSelectedProperty);
            cell.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            styles[typeof(DataGridCell)] = cell;

            var list = new Style(typeof(ListView));
            list.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.BackgroundBrushKey));
            list.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.BackgroundTextBrushKey));
            list.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            styles[typeof(ListView)] = list;
            styles[typeof(ListViewItem)] = ListItem();

            var splitter = new Style(typeof(GridSplitter));
            splitter.Setters.Add(Dynamic(Control.BackgroundProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            styles[typeof(GridSplitter)] = splitter;

            var menu = new Style(typeof(ContextMenu));
            menu.Setters.Add(Dynamic(Control.BackgroundProperty, EnvironmentColors.CommandBarMenuBackgroundGradientBeginBrushKey));
            menu.Setters.Add(Dynamic(Control.ForegroundProperty, EnvironmentColors.CommandBarTextActiveBrushKey));
            menu.Setters.Add(Dynamic(Control.BorderBrushProperty, EnvironmentColors.CommandBarMenuBorderBrushKey));
            styles[typeof(ContextMenu)] = menu;

            // Xceed's property grid (General Information): the grid, its rows and its editors, which are TextBox and
            // ComboBox subclasses that implicit TextBox and ComboBox styles don't reach.
            var propertyGrid = new Style(typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid));
            propertyGrid.Setters.Add(new EventSetter(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnPropertyGridLoaded)));
            propertyGrid.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.BackgroundBrushKey));
            propertyGrid.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.BackgroundTextBrushKey));
            propertyGrid.Setters.Add(Dynamic(Control.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            styles[typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)] = propertyGrid;
            var propertyItem = new Style(typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem));
            propertyItem.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.BackgroundBrushKey));
            propertyItem.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.BackgroundTextBrushKey));
            propertyItem.Setters.Add(Dynamic(Control.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            styles[typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem)] = propertyItem;
            var propertyItems = new Style(typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemsControl));
            propertyItems.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.BackgroundBrushKey));
            styles[typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemsControl)] = propertyItems;
            AddBasedOn(styles, root, typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorTextBox), VsResourceKeys.TextBoxStyleKey);
            AddBasedOn(styles, root, typeof(Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorComboBox), VsResourceKeys.ComboBoxStyleKey);

            return styles;
        }

        // Parts of Xceed's property grid template paint a fixed white (the area below the rows) that no style setter
        // reaches; once the template exists, those parts are given the theme's background instead.
        static void OnPropertyGridLoaded(object sender, RoutedEventArgs e)
        {
            var fixedParts = new System.Collections.Generic.List<string>();
            foreach (var element in Descendants((DependencyObject)sender))
            {
                var background = element switch
                {
                    Control control => Control.BackgroundProperty,
                    Border => Border.BackgroundProperty,
                    Panel => Panel.BackgroundProperty,
                    _ => null,
                };
                if (background != null && element.GetValue(background) is SolidColorBrush brush &&
                    (brush.Color == Colors.White || brush.Color == SystemColors.WindowColor))
                {
                    ((FrameworkElement)element).SetResourceReference(background, TreeViewColors.BackgroundBrushKey);
                    fixedParts.Add(element.GetType().Name);
                }
            }

            if (fixedParts.Count > 0)
            {
                ActivityLog.TryLogInformation("IsWiX Designer", "Themed the property grid's white parts: " + string.Join(", ", fixedParts.Distinct()));
            }
        }

        static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject parent)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                yield return child;
                foreach (var descendant in Descendants(child))
                {
                    yield return descendant;
                }
            }
        }

        static void AddBasedOn(ResourceDictionary styles, FrameworkElement root, Type type, object vsStyleKey)
        {
            if (root.TryFindResource(vsStyleKey) is Style vsStyle)
            {
                styles[type] = new Style(type, vsStyle);
            }
        }

        static Setter Dynamic(DependencyProperty property, object resourceKey) =>
            new Setter(property, new DynamicResourceExtension(resourceKey));

        // Rows and cells: the tree view's colors, and its selection colors when selected.
        static Style Selectable(Type type, DependencyProperty isSelected)
        {
            var style = new Style(type);
            style.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.BackgroundBrushKey));
            style.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.BackgroundTextBrushKey));
            var selected = new Trigger { Property = isSelected, Value = true };
            selected.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.SelectedItemActiveBrushKey));
            selected.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.SelectedItemActiveTextBrushKey));
            style.Triggers.Add(selected);
            return style;
        }

        // The designer list's items. The default template draws the system's light selection, so this one is simpler.
        static Style ListItem()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            border.SetValue(Border.PaddingProperty, new Thickness(4, 2, 4, 2));
            border.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));
            var style = new Style(typeof(ListViewItem));
            style.Setters.Add(new Setter(Control.TemplateProperty, new ControlTemplate(typeof(ListViewItem)) { VisualTree = border }));
            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.BackgroundTextBrushKey));
            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(Dynamic(Control.BackgroundProperty, EnvironmentColors.CommandBarMenuItemMouseOverBrushKey));
            style.Triggers.Add(hover);
            var selected = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selected.Setters.Add(Dynamic(Control.BackgroundProperty, TreeViewColors.SelectedItemActiveBrushKey));
            selected.Setters.Add(Dynamic(Control.ForegroundProperty, TreeViewColors.SelectedItemActiveTextBrushKey));
            style.Triggers.Add(selected);
            return style;
        }

        /// <summary>
        /// A strip of tabs, drawn like Visual Studio's own tabs, for <paramref name="tabs"/>: just the headers, since
        /// the views they switch between are kept alive outside the tab control.
        /// </summary>
        public static void ApplyTabStrip(TabControl tabs)
        {
            var panel = new FrameworkElementFactory(typeof(TabPanel));
            panel.SetValue(Panel.IsItemsHostProperty, true);
            var strip = new FrameworkElementFactory(typeof(Border));
            strip.SetResourceReference(Border.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
            strip.SetResourceReference(Border.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey);
            strip.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
            strip.AppendChild(panel);
            tabs.Template = new ControlTemplate(typeof(TabControl)) { VisualTree = strip };

            var header = new FrameworkElementFactory(typeof(ContentPresenter));
            header.SetValue(ContentPresenter.ContentSourceProperty, "Header");
            header.SetValue(FrameworkElement.MarginProperty, new Thickness(10, 3, 10, 4));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            border.AppendChild(header);
            var item = new Style(typeof(TabItem));
            item.Setters.Add(new Setter(Control.TemplateProperty, new ControlTemplate(typeof(TabItem)) { VisualTree = border }));
            item.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            item.Setters.Add(Dynamic(Control.ForegroundProperty, EnvironmentColors.ToolWindowTabTextBrushKey));
            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(Dynamic(Control.BackgroundProperty, EnvironmentColors.ToolWindowTabMouseOverBackgroundBeginBrushKey));
            hover.Setters.Add(Dynamic(Control.ForegroundProperty, EnvironmentColors.ToolWindowTabMouseOverTextBrushKey));
            item.Triggers.Add(hover);
            var selected = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
            selected.Setters.Add(Dynamic(Control.BackgroundProperty, EnvironmentColors.ToolWindowTabSelectedTabBrushKey));
            selected.Setters.Add(Dynamic(Control.ForegroundProperty, EnvironmentColors.ToolWindowTabSelectedTextBrushKey));
            item.Triggers.Add(selected);
            tabs.Resources[typeof(TabItem)] = item;
        }

        // WinForms -----------------------------------------------------------------------------------------------

        static Drawing.Color Color(ThemeResourceKey key) => VSColorTheme.GetThemedColor(key);

        /// <summary>Sets <paramref name="control"/>'s and its children's colors (and context menus') from the theme.</summary>
        public static void Apply(WinForms.Control control)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var background = Color(EnvironmentColors.ToolWindowBackgroundColorKey);
            var text = Color(EnvironmentColors.ToolWindowTextColorKey);
            var border = Color(EnvironmentColors.ToolWindowBorderColorKey);
            var window = Color(TreeViewColors.BackgroundColorKey);
            var windowText = Color(TreeViewColors.BackgroundTextColorKey);
            var input = Color(CommonControlsColors.TextBoxBackgroundColorKey);
            var inputText = Color(CommonControlsColors.TextBoxTextColorKey);

            switch (control)
            {
                case WinForms.TreeView tree:
                    tree.BackColor = window;
                    tree.ForeColor = windowText;
                    tree.LineColor = border;
                    ThemeImages(tree.ImageList, window);
                    ThemeImages(tree.StateImageList, window);
                    break;
                case WinForms.ListView list:
                    list.BackColor = window;
                    list.ForeColor = windowText;
                    ThemeImages(list.SmallImageList, window);
                    ThemeImages(list.LargeImageList, window);
                    ThemeImages(list.StateImageList, window);
                    OwnerDraw(list);
                    break;
                case WinForms.ListBox listBox:
                    listBox.BackColor = window;
                    listBox.ForeColor = windowText;
                    OwnerDraw(listBox);
                    break;
                case WinForms.ToolStrip strip:
                    // Toolbars, including the PropertyGrid's own.
                    Apply(strip);
                    break;
                case WinForms.TextBoxBase or WinForms.ComboBox:
                    control.BackColor = input;
                    control.ForeColor = inputText;
                    break;
                case WinForms.Button button:
                    button.FlatStyle = WinForms.FlatStyle.Flat;
                    button.BackColor = Color(CommonControlsColors.ButtonColorKey);
                    button.ForeColor = Color(CommonControlsColors.ButtonTextColorKey);
                    button.FlatAppearance.BorderColor = Color(CommonControlsColors.ButtonBorderColorKey);
                    break;
                case WinForms.PropertyGrid grid:
                    grid.BackColor = background;
                    grid.ForeColor = text;
                    grid.ViewBackColor = window;
                    grid.ViewForeColor = windowText;
                    grid.ViewBorderColor = border;
                    grid.LineColor = Color(HeaderColors.DefaultColorKey);
                    grid.CategoryForeColor = Color(HeaderColors.DefaultTextColorKey);
                    grid.CategorySplitterColor = border;
                    grid.HelpBackColor = background;
                    grid.HelpForeColor = text;
                    grid.HelpBorderColor = border;
                    grid.CommandsBackColor = background;
                    grid.CommandsForeColor = text;
                    grid.SelectedItemWithFocusBackColor = Color(TreeViewColors.SelectedItemActiveColorKey);
                    grid.SelectedItemWithFocusForeColor = Color(TreeViewColors.SelectedItemActiveTextColorKey);
                    grid.DisabledItemForeColor = Color(EnvironmentColors.SystemGrayTextColorKey);
                    break;
                case WinForms.DataGridView dataGrid:
                    dataGrid.BackgroundColor = window;
                    dataGrid.GridColor = border;
                    dataGrid.EnableHeadersVisualStyles = false;
                    dataGrid.DefaultCellStyle.BackColor = window;
                    dataGrid.DefaultCellStyle.ForeColor = windowText;
                    dataGrid.DefaultCellStyle.SelectionBackColor = Color(TreeViewColors.SelectedItemActiveColorKey);
                    dataGrid.DefaultCellStyle.SelectionForeColor = Color(TreeViewColors.SelectedItemActiveTextColorKey);
                    dataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color(HeaderColors.DefaultColorKey);
                    dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color(HeaderColors.DefaultTextColorKey);
                    dataGrid.RowHeadersDefaultCellStyle.BackColor = Color(HeaderColors.DefaultColorKey);
                    dataGrid.RowHeadersDefaultCellStyle.ForeColor = Color(HeaderColors.DefaultTextColorKey);
                    break;
                default:
                    control.BackColor = background;
                    control.ForeColor = text;
                    break;
            }

            if (control.ContextMenuStrip is { } contextMenu)
            {
                Apply(contextMenu);
            }

            foreach (WinForms.Control child in control.Controls)
            {
                Apply(child);
            }
        }

        static void Apply(WinForms.ToolStrip strip)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            strip.Renderer = new WinForms.ToolStripProfessionalRenderer(new MenuColors()) { RoundedEdges = false };
            strip.BackColor = Color(EnvironmentColors.CommandBarMenuBackgroundGradientBeginColorKey);
            strip.ForeColor = Color(EnvironmentColors.CommandBarTextActiveColorKey);
            foreach (WinForms.ToolStripItem item in strip.Items)
            {
                item.ForeColor = strip.ForeColor;
                ThemeImage(item, strip.BackColor);
                if (item is WinForms.ToolStripDropDownItem { HasDropDownItems: true } dropDown)
                {
                    Apply(dropDown.DropDown);
                }
            }
        }

        // List views and list boxes draw their selection and headers in the system's light colors, whatever their own
        // colors are, so they're drawn here instead, with the tree view's colors as Solution Explorer uses them.
        static readonly ConditionalWeakTable<WinForms.Control, object> OwnerDrawn = new ConditionalWeakTable<WinForms.Control, object>();

        static void OwnerDraw(WinForms.ListView list)
        {
            if (OwnerDrawn.TryGetValue(list, out _))
            {
                list.Invalidate();
                return;
            }

            OwnerDrawn.Add(list, new object());
            DoubleBuffer(list);
            list.OwnerDraw = true;
            list.DrawColumnHeader += (s, e) =>
            {
                using (var back = new Drawing.SolidBrush(Color(HeaderColors.DefaultColorKey)))
                using (var line = new Drawing.Pen(Color(EnvironmentColors.ToolWindowBorderColorKey)))
                {
                    e.Graphics.FillRectangle(back, e.Bounds);
                    e.Graphics.DrawLine(line, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                    e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                }

                var text = new Drawing.Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
                WinForms.TextRenderer.DrawText(e.Graphics, e.Header?.Text, list.Font, text, Color(HeaderColors.DefaultTextColorKey), TextFlags(e.Header?.TextAlign ?? WinForms.HorizontalAlignment.Left));
            };
            list.DrawItem += (s, e) =>
            {
                // Details views draw per column, in DrawSubItem.
                if (list.View != WinForms.View.Details)
                {
                    DrawListItem(list, e.Graphics, e.Item, e.Bounds, 0, e.Item.Text);
                }
            };
            list.DrawSubItem += (s, e) =>
            {
                // The first column's bounds are the whole row.
                var bounds = e.ColumnIndex == 0 && list.Columns.Count > 0
                    ? new Drawing.Rectangle(e.Bounds.X, e.Bounds.Y, list.Columns[0].Width, e.Bounds.Height)
                    : e.Bounds;
                DrawListItem(list, e.Graphics, e.Item!, bounds, e.ColumnIndex, e.SubItem?.Text ?? string.Empty);
            };
            list.GotFocus += (s, e) => list.Invalidate();
            list.LostFocus += (s, e) => list.Invalidate();
        }

        // Owner-drawn lists erase and redraw each row on screen, which flickers whenever they repaint (the mouse moving
        // over them, focus changes, Visual Studio redrawing around them); painting off screen first stops it.
        static void DoubleBuffer(WinForms.Control control)
        {
            typeof(WinForms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(control, true);
        }

        static void DrawListItem(WinForms.ListView list, Drawing.Graphics graphics, WinForms.ListViewItem item, Drawing.Rectangle bounds, int column, string text)
        {
            var (back, fore) = ItemColors(item.Selected, list.Focused, list.BackColor, list.ForeColor);
            using (var brush = new Drawing.SolidBrush(back))
            {
                graphics.FillRectangle(brush, bounds);
            }

            var x = bounds.Left + 2;
            if (column == 0)
            {
                x = DrawListImage(graphics, list.StateImageList, item.StateImageIndex, null, x, bounds);
                x = DrawListImage(graphics, list.View == WinForms.View.LargeIcon ? list.LargeImageList : list.SmallImageList, item.ImageIndex, item.ImageKey, x, bounds);
            }

            var textBounds = new Drawing.Rectangle(x, bounds.Y, Math.Max(0, bounds.Right - x - 2), bounds.Height);
            var align = column > 0 && column < list.Columns.Count ? list.Columns[column].TextAlign : WinForms.HorizontalAlignment.Left;
            WinForms.TextRenderer.DrawText(graphics, text, item.Font ?? list.Font, textBounds, fore, TextFlags(align));
        }

        static int DrawListImage(Drawing.Graphics graphics, WinForms.ImageList? images, int index, string? key, int x, Drawing.Rectangle bounds)
        {
            if (images is null)
            {
                return x;
            }

            if (index < 0 && !string.IsNullOrEmpty(key) && images.Images.ContainsKey(key))
            {
                index = images.Images.IndexOfKey(key);
            }

            if (index < 0 || index >= images.Images.Count)
            {
                return x;
            }

            images.Draw(graphics, x, bounds.Y + (bounds.Height - images.ImageSize.Height) / 2, index);
            return x + images.ImageSize.Width + 3;
        }

        static void OwnerDraw(WinForms.ListBox listBox)
        {
            if (OwnerDrawn.TryGetValue(listBox, out _))
            {
                listBox.Invalidate();
                return;
            }

            OwnerDrawn.Add(listBox, new object());
            DoubleBuffer(listBox);
            listBox.DrawMode = WinForms.DrawMode.OwnerDrawFixed;
            listBox.ItemHeight = listBox.Font.Height + 4;
            listBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= listBox.Items.Count)
                {
                    return;
                }

                var selected = (e.State & WinForms.DrawItemState.Selected) != 0;
                var (back, fore) = ItemColors(selected, listBox.Focused, listBox.BackColor, listBox.ForeColor);
                using (var brush = new Drawing.SolidBrush(back))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                var text = new Drawing.Rectangle(e.Bounds.X + 3, e.Bounds.Y, e.Bounds.Width - 6, e.Bounds.Height);
                WinForms.TextRenderer.DrawText(e.Graphics, listBox.GetItemText(listBox.Items[e.Index]), listBox.Font, text, fore, TextFlags(WinForms.HorizontalAlignment.Left));
            };
            listBox.GotFocus += (s, e) => listBox.Invalidate();
            listBox.LostFocus += (s, e) => listBox.Invalidate();
        }

        // Selected items as Solution Explorer draws them: the active colors with focus, the inactive ones without.
        static (Drawing.Color Back, Drawing.Color Fore) ItemColors(bool selected, bool focused, Drawing.Color back, Drawing.Color fore) =>
            !selected
                ? (back, fore)
                : focused
                    ? (Color(TreeViewColors.SelectedItemActiveColorKey), Color(TreeViewColors.SelectedItemActiveTextColorKey))
                    : (Color(TreeViewColors.SelectedItemInactiveColorKey), Color(TreeViewColors.SelectedItemInactiveTextColorKey));

        static WinForms.TextFormatFlags TextFlags(WinForms.HorizontalAlignment align) =>
            WinForms.TextFormatFlags.VerticalCenter | WinForms.TextFormatFlags.EndEllipsis | WinForms.TextFormatFlags.NoPrefix |
            WinForms.TextFormatFlags.SingleLine |
            (align == WinForms.HorizontalAlignment.Right ? WinForms.TextFormatFlags.Right
                : align == WinForms.HorizontalAlignment.Center ? WinForms.TextFormatFlags.HorizontalCenter
                : WinForms.TextFormatFlags.Left);

        // The images as the designers made them, so each theme change starts from the originals.
        static readonly ConditionalWeakTable<WinForms.ImageList, Drawing.Image[]> OriginalLists = new ConditionalWeakTable<WinForms.ImageList, Drawing.Image[]>();
        static readonly ConditionalWeakTable<WinForms.ToolStripItem, Drawing.Image> OriginalItemImages = new ConditionalWeakTable<WinForms.ToolStripItem, Drawing.Image>();

        // Adapts an image list's icons to the background they're drawn on, keeping their indexes and keys.
        static void ThemeImages(WinForms.ImageList? list, Drawing.Color background)
        {
            if (list is null || list.Images.Count == 0)
            {
                return;
            }

            if (!OriginalLists.TryGetValue(list, out var originals))
            {
                originals = Enumerable.Range(0, list.Images.Count).Select(i => list.Images[i]).ToArray();
                OriginalLists.Add(list, originals);
            }

            for (var i = 0; i < originals.Length && i < list.Images.Count; i++)
            {
                list.Images[i] = ImageThemingUtilities.GetThemedBitmap(new Drawing.Bitmap(originals[i]), background);
            }
        }

        static void ThemeImage(WinForms.ToolStripItem item, Drawing.Color background)
        {
            if (item.Image is null)
            {
                return;
            }

            if (!OriginalItemImages.TryGetValue(item, out var original))
            {
                original = item.Image;
                OriginalItemImages.Add(item, original);
            }

            item.Image = ImageThemingUtilities.GetThemedBitmap(new Drawing.Bitmap(original), background);
        }

        /// <summary>Themes the scrollbars of a WinForms control's window, which its colors don't reach.</summary>
        public static void ThemeScrollBars(IServiceProvider services, WinForms.Control control)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (services.GetService(typeof(SVsUIShell)) is IVsUIShell5 shell && control.IsHandleCreated)
            {
                shell.ThemeWindow(control.Handle);
            }
        }

        sealed class MenuColors : WinForms.ProfessionalColorTable
        {
            static Drawing.Color Menu => Color(EnvironmentColors.CommandBarMenuBackgroundGradientBeginColorKey);
            static Drawing.Color Hover => Color(EnvironmentColors.CommandBarMenuItemMouseOverColorKey);
            static Drawing.Color Border => Color(EnvironmentColors.CommandBarMenuBorderColorKey);

            public override Drawing.Color ToolStripDropDownBackground => Menu;
            public override Drawing.Color ImageMarginGradientBegin => Menu;
            public override Drawing.Color ImageMarginGradientMiddle => Menu;
            public override Drawing.Color ImageMarginGradientEnd => Menu;
            public override Drawing.Color MenuBorder => Border;
            public override Drawing.Color MenuItemBorder => Hover;
            public override Drawing.Color MenuItemSelected => Hover;
            public override Drawing.Color MenuItemSelectedGradientBegin => Hover;
            public override Drawing.Color MenuItemSelectedGradientEnd => Hover;
            public override Drawing.Color SeparatorDark => Color(EnvironmentColors.CommandBarMenuSeparatorColorKey);
            public override Drawing.Color SeparatorLight => Menu;
        }
    }
}
