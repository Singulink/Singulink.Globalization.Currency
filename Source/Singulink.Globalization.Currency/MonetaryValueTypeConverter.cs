using System.ComponentModel;

namespace Singulink.Globalization;

/// <summary>
/// Converts <see cref="MonetaryValue"/> values to and from strings for <see cref="TypeDescriptor"/>, which Newtonsoft.Json, configuration binding, model
/// binding and XAML use. With the invariant culture, values are written in the round-trip format <c>"O"</c> and read with <see
/// cref="MonetaryValue.Parse(string, IFormatProvider?)"/> and the invariant culture. With a specific culture, values are written in the default format and read
/// with the culture's parsing. No culture means the current culture, as it does for <see cref="DecimalConverter"/>, because <see
/// cref="TypeConverter.ConvertToString(object?)"/> passes the current culture and <see cref="TypeConverter.ConvertFromString(string)"/> passes none. The default
/// value is written as <c>"0"</c>. Currencies are looked up in <see cref="CurrencyRegistry.Default"/>.
/// </summary>
internal sealed class MonetaryValueTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string text)
            return base.ConvertFrom(context, culture, value);

        try
        {
            return MonetaryValue.Parse(text.Trim(), MonetaryStyles.CurrencyCode, culture ?? CultureInfo.CurrentCulture);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"'{text}' is not a valid value for {nameof(MonetaryValue)}. {ex.Message}", nameof(value), ex);
        }
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is MonetaryValue monetaryValue)
        {
            culture ??= CultureInfo.CurrentCulture;

            if (culture.Equals(CultureInfo.InvariantCulture))
                return monetaryValue.ToRoundTripString();

            return monetaryValue.ToString(null, culture);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
