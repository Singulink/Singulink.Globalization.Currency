namespace Singulink.Globalization;

/// <summary>
/// Provides Unicode Common Locale Data Repository (CLDR) currency data that is embedded in this package as an <see cref="ICurrencyDataProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// Pass <see cref="Provider"/> to <c>CurrencyRegistry.SetDefault(ICurrencyDataProvider, CurrencyTypes)</c> at application startup to make the default
/// currency registry use CLDR data, or to <c>CurrencyData.Load(ICurrencyDataProvider)</c> to work with the data directly. Both are provided by the
/// <c>Singulink.Globalization.Currency</c> package.
/// </para>
/// <para>
/// This package contains only data. It can be updated to the latest CLDR release without affecting the version of the main library that is used.
/// </para>
/// </remarks>
public static partial class CldrCurrencyData
{
    /// <summary>
    /// Gets the CLDR currency data provider.
    /// </summary>
    public static ICurrencyDataProvider Provider { get; } = new DataProvider();

    private sealed class DataProvider : ICurrencyDataProvider
    {
        public string Name => "CLDR";

        public string Version => CldrVersion;

        public DateTime? ReleaseDate => CldrReleaseDate;

        public IEnumerable<CurrencyDataEntry> GetCurrencies() => CldrDataReader.Read().Currencies;

        public IEnumerable<CurrencyLocalizationEntry> GetLocalizations() => CldrDataReader.Read().Localizations;
    }
}
