using IsWiXAutomationInterface;
using System;
using System.Collections;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace GeneralInformationDesigner.Views
{
    public class CustomEditor<T> : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ComboBoxEditor
    {


        protected override IValueConverter CreateValueConverter()
        {
            return new CustomValueConverter<T>();
        }

        protected override ComboBox CreateEditor()
        {
            ComboBox comboBox = base.CreateEditor();
            FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new CustomValueConverter<T>() });
            comboBox.ItemTemplate = new DataTemplate() { VisualTree = textBlock };
            return comboBox;
        }

        protected override IEnumerable CreateItemsSource(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            return new string[1] { CustomValueConverter<T>.Null }
                .Concat(Enum.GetValues(typeof(T)).OfType<T>().Select(x => x.ToString()));
        }
    }

    public class CustomValueConverter<T>: IValueConverter
    {
        internal const string Null = "";
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Null;

            return value.ToString();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string s = value?.ToString();
            if (s == Null)
                return null;

            return Enum.Parse(typeof(T), s);
        }
    }
}
