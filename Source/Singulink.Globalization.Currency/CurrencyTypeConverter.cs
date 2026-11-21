using System.ComponentModel;

namespace Singulink.Globalization;

/// <summary>
/// Converts <see cref="Currency"/> objects to and from strings for <see cref="TypeDescriptor"/>, which Newtonsoft.Json, configuration binding, model binding
/// and XAML use. A currency is written as its currency code and read by looking the code up in <see cref="CurrencyRegistry.Default"/>.
/// </summary>
internal sealed class CurrencyTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string text)
            return base.ConvertFrom(context, culture, value);

        var registry = CurrencyRegistry.Default;

        if (!registry.TryGetCurrency(text.Trim(), out var currency))
            throw new ArgumentException($"'{text}' is not a valid value for {nameof(Currency)}. {registry.GetCodeNotFoundMessage(text.Trim())}", nameof(value));

        return currency;
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Currency currency)
            return currency.CurrencyCode;

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
