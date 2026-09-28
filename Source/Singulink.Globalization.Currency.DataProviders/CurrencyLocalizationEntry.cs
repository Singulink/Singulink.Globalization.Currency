namespace Singulink.Globalization;

/// <summary>
/// Describes the localized name and/or symbol of a currency for a locale, provided by an <see cref="ICurrencyDataProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// Localized values are resolved by walking up the locale chain, removing one hyphen-separated subtag from the culture name at a time until the invariant
/// values from the <see cref="CurrencyDataEntry"/> are reached. For example, a lookup for the <c>fr-CA</c> culture checks entries for <c>fr-CA</c>, then
/// <c>fr</c>, then uses the invariant values. Names and symbols are resolved independently, so an entry may provide only one of them and the other is
/// resolved from the parent locale.
/// </para>
/// <para>
/// Entries only need to be provided for locales whose values differ from the values that the locale chain would otherwise produce, which keeps data
/// compact.
/// </para>
/// </remarks>
public sealed class CurrencyLocalizationEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyLocalizationEntry"/> class.
    /// </summary>
    /// <param name="locale">The BCP 47 locale name that the entry applies to, i.e. <c>"fr"</c> or <c>"fr-CA"</c>, matching .NET culture names.</param>
    /// <param name="currencyCode">The code of the currency that the entry applies to, i.e. <c>"CAD"</c>.</param>
    public CurrencyLocalizationEntry(string locale, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(locale))
            throw new ArgumentException("Locale is required. Invariant values are specified on the currency entry.", nameof(locale));

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required.", nameof(currencyCode));

        Locale = locale;
        CurrencyCode = currencyCode;
    }

    /// <summary>
    /// Gets the BCP 47 locale name that the entry applies to, i.e. <c>"fr"</c> or <c>"fr-CA"</c>.
    /// </summary>
    public string Locale { get; }

    /// <summary>
    /// Gets the code of the currency that the entry applies to, i.e. <c>"CAD"</c>.
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    /// Gets or initializes the localized name of the currency for the locale, or <see langword="null"/> if the name is inherited from the parent locale.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or initializes the localized symbol of the currency for the locale, or <see langword="null"/> if the symbol is inherited from the parent locale.
    /// </summary>
    public string? Symbol { get; init; }
}
