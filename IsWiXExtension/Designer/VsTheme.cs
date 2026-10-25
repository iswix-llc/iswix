using System;
using System.Windows;
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

        /// <summary>Applies the theme to <paramref name="root"/> and everything inside it.</summary>
        public static void Apply(FrameworkElement root)
        {
            root.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey);
            root.SetResourceReference(TextElement.ForegroundProperty, EnvironmentColors.ToolWindowTextBrushKey);
            root.Resources.MergedDictionaries.Add(Styles(root));
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

            var propertyGrid = new Style(typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid));
            propertyGrid.Setters.Add(Dynamic(Control.BackgroundProperty, EnvironmentColors.ToolWindowBackgroundBrushKey));
            propertyGrid.Setters.Add(Dynamic(Control.ForegroundProperty, EnvironmentColors.ToolWindowTextBrushKey));
            propertyGrid.Setters.Add(Dynamic(Control.BorderBrushProperty, EnvironmentColors.ToolWindowBorderBrushKey));
            styles[typeof(Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)] = propertyGrid;

            return styles;
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
                    break;
                case WinForms.ListView list:
                    list.BackColor = window;
                    list.ForeColor = windowText;
                    break;
                case WinForms.TextBoxBase or WinForms.ComboBox or WinForms.ListBox:
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
                if (item is WinForms.ToolStripDropDownItem { HasDropDownItems: true } dropDown)
                {
                    Apply(dropDown.DropDown);
                }
            }
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
