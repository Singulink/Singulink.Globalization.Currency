<div class="article">

# Implementing a Data Provider

Currency data packages supply their data to the core library through the <xref:Singulink.Globalization.ICurrencyDataProvider> interface from the `Singulink.Globalization.Currency.DataProviders` package. This guide describes the contract so that you can supply currency data from another source, such as a database, a configuration file, or a feed your application downloads at runtime.

### When to write one

The CLDR package covers the common need for complete, up-to-date, platform-independent data. Write a provider when the set of currencies, their names or their rounding rules must come from somewhere your application controls: currencies defined by an accounting system, symbols mandated by a client, or a subset of currencies with custom cash rules. Providers can also be combined with the CLDR data by loading both and building a registry from currencies of each.

## The Interface

<xref:Singulink.Globalization.ICurrencyDataProvider> has three metadata properties and two methods:

- <xref:Singulink.Globalization.ICurrencyDataProvider.Name> becomes the name of registries created from the provider.
- <xref:Singulink.Globalization.ICurrencyDataProvider.Version> and <xref:Singulink.Globalization.ICurrencyDataProvider.ReleaseDate> describe the data for diagnostics. The release date may be `null`.
- <xref:Singulink.Globalization.ICurrencyDataProvider.GetCurrencies*> returns one <xref:Singulink.Globalization.CurrencyDataEntry> per currency.
- <xref:Singulink.Globalization.ICurrencyDataProvider.GetLocalizations*> returns <xref:Singulink.Globalization.CurrencyLocalizationEntry> values for localized names and symbols. It may return an empty sequence.

The library enumerates each sequence once when the provider is first loaded and caches the result for the provider instance, so providers do not need to cache anything themselves.

```csharp
public sealed class DatabaseCurrencyProvider(AppDbContext db) : ICurrencyDataProvider
{
    public string Name => "Accounting";
    public string Version => "2026.1";
    public DateTime? ReleaseDate => null;

    public IEnumerable<CurrencyDataEntry> GetCurrencies()
    {
        foreach (var row in db.Currencies)
        {
            yield return new CurrencyDataEntry(row.Code, row.Name) {
                Symbol = row.Symbol,
                DecimalDigits = row.Decimals,
                CashDecimalUnits = row.CashIncrement,
                Type = row.IsRetired ? CurrencyTypes.Historical : CurrencyTypes.CurrentTender,
            };
        }
    }

    public IEnumerable<CurrencyLocalizationEntry> GetLocalizations()
    {
        foreach (var row in db.CurrencyTranslations)
            yield return new CurrencyLocalizationEntry(row.Locale, row.Code) { Name = row.Name, Symbol = row.Symbol };
    }
}

CurrencyRegistry.SetDefaultProvider(new DatabaseCurrencyProvider(db));
```

## Currency Entries

A <xref:Singulink.Globalization.CurrencyDataEntry> requires a unique code and an invariant (English) name. Everything else is optional and has a sensible default:

- <xref:Singulink.Globalization.CurrencyDataEntry.Symbol> is the invariant symbol. When omitted, the code is used as the symbol.
- <xref:Singulink.Globalization.CurrencyDataEntry.DecimalDigits> and <xref:Singulink.Globalization.CurrencyDataEntry.DecimalUnits> define the standard <xref:Singulink.Globalization.RoundingPolicy>. They default to two decimal digits with no rounding increment.
- <xref:Singulink.Globalization.CurrencyDataEntry.CashDecimalDigits> and <xref:Singulink.Globalization.CurrencyDataEntry.CashDecimalUnits> define the cash policy. When omitted they follow the standard values, so cash rounding equals standard rounding. Set an increment of `5` with two cash decimal digits to round cash to the nearest 0.05, as for the Canadian dollar.
- <xref:Singulink.Globalization.CurrencyDataEntry.Type> is a single <xref:Singulink.Globalization.CurrencyTypes> value that decides which registries include the currency. It defaults to current legal tender.

Currencies loaded from a provider always have a cash rounding policy, since a provider that knows the standard rules is presumed to know the cash rules as well.

## Localization Entries

A <xref:Singulink.Globalization.CurrencyLocalizationEntry> names a locale and a currency code and carries an optional name and symbol. Values are resolved by walking up the locale name one hyphen-separated subtag at a time until the invariant values on the currency entry are reached, so a lookup for `fr-CA` checks `fr-CA`, then `fr`, then the currency entry. Names and symbols are resolved independently: an entry may set only one and the other continues to come from the parent.

Providers therefore only need to return entries for locales whose values differ from what the chain would otherwise produce. The CLDR package stores roughly forty thousand entries for over six hundred locales in under half a megabyte this way. Locale names must not be empty, since invariant values belong on the currency entry, and they should match .NET culture names so that lookups with <xref:System.Globalization.CultureInfo> find them.

## Validation

<xref:Singulink.Globalization.CurrencyData.Load*> throws <xref:System.IO.InvalidDataException> naming the provider and the offending currency when a provider returns duplicate currency codes, a localization for a currency it did not return, an entry type that is not a single type, or rounding values outside the valid range. Entries themselves throw <xref:System.ArgumentException> from their constructors when the code, name or locale is empty.

## Next Steps

Continue with these related articles:

- [CLDR Currency Data](cldr-data.md) - The provider that ships with the library.
- [Localization](../concepts/localization.md) - How the resolved names and symbols are used.
- [Currencies and Registries](currencies-and-registries.md) - Registering a provider as the default.

</div>
