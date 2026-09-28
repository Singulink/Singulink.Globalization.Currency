# Singulink.Globalization.Currency.DataProviders

**Singulink.Globalization.Currency.DataProviders** contains the `ICurrencyDataProvider` interface and the entry types that currency data packages implement so that [Singulink.Globalization.Currency](https://www.nuget.org/packages/Singulink.Globalization.Currency/) can build currency registries from them.

This package is deliberately limited to the data provider abstraction. It exists as a separate package so that data packages such as [Singulink.Globalization.Currency.Cldr](https://www.nuget.org/packages/Singulink.Globalization.Currency.Cldr/) do not depend on the main library. Updating to the latest data never pulls in a new version of the main library, and updating the main library never requires new data.

You do not normally need to reference this package directly. It is brought in as a dependency of the main library and of data packages.

## Implementing a data provider

Implement `ICurrencyDataProvider` to provide currency data from somewhere other than a Singulink data package, for example from a database or a file that your application downloads at runtime:

```c#
public sealed class MyCurrencyDataProvider : ICurrencyDataProvider
{
    public string Name => "My Data";
    public string Version => "1.0";
    public DateTime? ReleaseDate => null;

    public IEnumerable<CurrencyDataEntry> GetCurrencies()
    {
        yield return new CurrencyDataEntry("USD", "US Dollar") { Symbol = "$" };
        yield return new CurrencyDataEntry("CAD", "Canadian Dollar") { Symbol = "CA$", CashDecimalUnits = 5 };
        yield return new CurrencyDataEntry("JPY", "Japanese Yen") { Symbol = "¥", DecimalDigits = 0 };
        yield return new CurrencyDataEntry("XAU", "Gold") { Type = CurrencyTypes.CurrentNonTender };
    }

    public IEnumerable<CurrencyLocalizationEntry> GetLocalizations()
    {
        yield return new CurrencyLocalizationEntry("fr", "CAD") { Name = "dollar canadien", Symbol = "$CA" };
        yield return new CurrencyLocalizationEntry("fr-CA", "CAD") { Symbol = "$" };
        yield return new CurrencyLocalizationEntry("en-CA", "CAD") { Symbol = "$" };
    }
}

CurrencyRegistry.SetDefault(new MyCurrencyDataProvider());
```

Localized values are resolved by walking up the locale chain (`fr-CA`, then `fr`, then the invariant values on the currency entry), so entries only need to be provided for locales that differ from their parent.
