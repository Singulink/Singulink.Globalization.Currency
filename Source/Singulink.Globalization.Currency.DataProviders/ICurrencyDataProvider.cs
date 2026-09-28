namespace Singulink.Globalization;

/// <summary>
/// Provides currency data, such as the embedded Unicode CLDR data provided by the <c>Singulink.Globalization.Currency.Cldr</c> package. Data providers are
/// consumed by the <c>Singulink.Globalization.Currency</c> package to build currency registries.
/// </summary>
/// <remarks>
/// <para>
/// This interface is deliberately minimal so that data packages can be updated independently of the main library and vice versa. Data is exchanged as
/// simple entry objects that can gain optional properties over time without breaking existing providers or consumers.
/// </para>
/// <para>
/// Implementations should not cache the entries they return. The main library loads the data once per provider instance and caches the result, so the
/// entries are only enumerated once.
/// </para>
/// </remarks>
public interface ICurrencyDataProvider
{
    /// <summary>
    /// Gets the name of the data provider, i.e. <c>"CLDR"</c>. This is used as the name of registries created from the data provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the data provided by the data provider, i.e. <c>"48.0.0"</c> for CLDR 48.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the date that the data provided by the data provider was released, or <see langword="null"/> if it is not known.
    /// </summary>
    DateTime? ReleaseDate { get; }

    /// <summary>
    /// Gets the currencies provided by the data provider. Currency codes must be unique.
    /// </summary>
    IEnumerable<CurrencyDataEntry> GetCurrencies();

    /// <summary>
    /// Gets the localized names and symbols of the currencies provided by the data provider. Entries may be returned in any order and only need to be
    /// provided for locales that differ from their parent locale (see <see cref="CurrencyLocalizationEntry"/>).
    /// </summary>
    IEnumerable<CurrencyLocalizationEntry> GetLocalizations();
}
