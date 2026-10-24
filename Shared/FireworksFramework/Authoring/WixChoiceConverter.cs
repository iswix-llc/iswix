using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using IsWiX.WiXModel.Core;

namespace FireworksFramework.Authoring
{
    /// <summary>
    /// A property grid drop-down of an attribute's values, for string properties holding the attribute text. Empty
    /// (remove the attribute) is offered when the attribute is optional, and an expression such as $(var.X) can be
    /// typed in.
    /// </summary>
    public abstract class WixChoiceConverter : StringConverter
    {
        protected abstract IEnumerable<string> Choices { get; }

        protected virtual bool Optional => true;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) =>
            new StandardValuesCollection((Optional ? new[] { string.Empty }.Concat(Choices) : Choices).ToArray());

        /// <summary>The attribute text of each value of an enum.</summary>
        protected static IEnumerable<string> ChoicesOf<T>(WixValueConverter<T> converter) where T : struct, Enum =>
            Enum.GetValues(typeof(T)).Cast<T>().Select(converter.Format);
    }

    /// <summary>yes / no.</summary>
    public class YesNoChoiceConverter : WixChoiceConverter
    {
        protected override IEnumerable<string> Choices => new[] { WixConverters.YesNo.Format(true), WixConverters.YesNo.Format(false) };
    }

    /// <summary>Converts between typed values and the attribute text the designers show.</summary>
    public static class WixText
    {
        /// <summary>The attribute text of a value, or <see langword="null"/> when the attribute is absent.</summary>
        public static string? Of<T>(WixValue<T>? value, WixValueConverter<T> converter) =>
            value.HasValue ? converter.ToText(value.Value) : null;

        /// <summary>
        /// Parses text a user entered. Empty gives <see langword="null"/> (remove the attribute); returns
        /// <see langword="false"/> when the text is neither a valid value nor an expression.
        /// </summary>
        public static bool TryParse<T>(string? text, WixValueConverter<T> converter, out WixValue<T>? value)
        {
            value = null;
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            var parsed = converter.Parse(text);
            if (parsed.Kind == WixValueKind.Invalid)
            {
                return false;
            }

            value = parsed;
            return true;
        }
    }
}
