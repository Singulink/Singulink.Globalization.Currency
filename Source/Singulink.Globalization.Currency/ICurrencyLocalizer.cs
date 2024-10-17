#if NETSTANDARD
using Singulink.Globalization.Polyfills;
#endif

namespace Singulink.Globalization;

/// <summary>
/// Provides methods to localize currency names and symbols for specific cultures.
/// </summary>
public interface ICurrencyLocalizer
{
    /// <summary>
    /// Gets the localized name of the specified currency for the given culture.
    /// </summary>
    /// <param name="currency">The currency to localize.</param>
    /// <param name="culture">The culture for which to localize the currency name.</param>
    public string GetName(Currency currency, CultureInfo culture);

    /// <summary>
    /// Gets the localized symbol of the specified currency for the given culture.
    /// </summary>
    /// <param name="currency">The currency to localize.</param>
    /// <param name="culture">The culture for which to localize the currency symbol.</param>
    public string GetSymbol(Currency currency, CultureInfo culture);
}
