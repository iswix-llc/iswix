using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace GeneralInformationDesigner.Views
{
    /// <summary>
    /// A drop-down of an attribute's allowed values, plus empty to remove the attribute. A value outside the list,
    /// such as a $(var.X) expression, is listed too so it shows and is kept until another value is picked.
    /// </summary>
    public abstract class ChoiceEditor : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ComboBoxEditor
    {
        protected abstract string[] Choices { get; }

        protected override IValueConverter CreateValueConverter()
        {
            return new ChoiceValueConverter();
        }

        protected override ComboBox CreateEditor()
        {
            ComboBox comboBox = base.CreateEditor();
            FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(".") { Converter = new ChoiceValueConverter() });
            comboBox.ItemTemplate = new DataTemplate() { VisualTree = textBlock };
            return comboBox;
        }

        protected override IEnumerable CreateItemsSource(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            var current = propertyItem.Value as string;
            var choices = new[] { ChoiceValueConverter.Null }.Concat(Choices);
            return string.IsNullOrEmpty(current) || Choices.Contains(current) ? choices : choices.Append(current);
        }

        protected static string[] ChoicesOf<T>(WixValueConverter<T> converter) where T : struct, Enum =>
            Enum.GetValues(typeof(T)).Cast<T>().Select(converter.Format).ToArray();
    }

    public class YesNoEditor : ChoiceEditor
    {
        protected override string[] Choices { get; } = { WixConverters.YesNo.Format(true), WixConverters.YesNo.Format(false) };
    }

    public class ScopeEditor : ChoiceEditor
    {
        protected override string[] Choices { get; } = ChoicesOf(Wxs.WixEnumConverters.PackageScopeType);
    }

    public class ChoiceValueConverter : IValueConverter
    {
        internal const string Null = "";
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? Null;
        }

        public object? ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string? s = value?.ToString();
            return s == Null ? null : s;
        }
    }
}
