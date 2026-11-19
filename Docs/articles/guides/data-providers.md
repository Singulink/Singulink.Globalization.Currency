<div class="article">

# Implementing a Data Provider

Currency data packages supply their data to the core library through the <xref:Singulink.Globalization.ICurrencyDataProvider> interface from the `Singulink.Globalization.Currency.DataProviders` package. This guide describes the contract so that you can supply currency data from another source, such as a database, a configuration file, or a feed your application downloads at runtime.

### When to write one

The CLDR package covers the common need for complete, up-to-date, platform-independent data. Write a provider when the set of currencies, their names or their rounding rules must come from somewhere your application controls: currencies defined by an accounting system, symbols mandated by a client, or a subset of currencies with custom cash rules. You can also combine a provider with the CLDR data: load both, and build a registry from currencies of each.

## Interface

<xref:Singulink.Globalization.ICurrencyDataProvider> has three metadata properties and two methods:

| Member | What it does |
| --- | --- |
| <xref:Singulink.Globalization.ICurrencyDataProvider.Name> | Becomes the name of registries created from the provider |
| <xref:Singulink.Globalization.ICurrencyDataProvider.Version> | Describes the data for diagnostics |
| <xref:Singulink.Globalization.ICurrencyDataProvider.ReleaseDate> | Describes the data for diagnostics. It may be `null`. |
| <xref:Singulink.Globalization.ICurrencyDataProvider.GetCurrencies*> | Returns one <xref:Singulink.Globalization.CurrencyDataEntry> per currency |
| <xref:Singulink.Globalization.ICurrencyDataProvider.GetLocalizations*> | Returns <xref:Singulink.Globalization.CurrencyLocalizationEntry> values for localized names and symbols. It may return an empty sequence. |

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

| Property | Default | Meaning |
| --- | --- | --- |
| <xref:Singulink.Globalization.CurrencyDataEntry.Symbol> | The code | The invariant symbol |
| <xref:Singulink.Globalization.CurrencyDataEntry.DecimalDigits> | `2` | The decimal digits of the standard <xref:Singulink.Globalization.RoundingPolicy> |
| <xref:Singulink.Globalization.CurrencyDataEntry.DecimalUnits> | `0`, no rounding increment | The rounding increment of the standard policy |
| <xref:Singulink.Globalization.CurrencyDataEntry.CashDecimalDigits> | The standard value | The decimal digits of the cash policy |
| <xref:Singulink.Globalization.CurrencyDataEntry.CashDecimalUnits> | The standard value | The rounding increment of the cash policy |
| <xref:Singulink.Globalization.CurrencyDataEntry.Type> | <xref:Singulink.Globalization.CurrencyTypes.CurrentTender> | A single <xref:Singulink.Globalization.CurrencyTypes> value that decides which registries include the currency |

When the cash values are omitted, cash rounding equals standard rounding. Set an increment of `5` with two cash decimal digits to round cash to the nearest 0.05, as for the Canadian dollar:

```csharp
new CurrencyDataEntry("CAD", "Canadian Dollar") {
    Symbol = "$",
    CashDecimalUnits = 5,       // cash rounds to 0.05; DecimalDigits and CashDecimalDigits stay at 2
};
```

Currencies loaded from a provider always have a cash rounding policy. The library assumes that a provider that knows the standard rules knows the cash rules as well.

## Localization Entries

A <xref:Singulink.Globalization.CurrencyLocalizationEntry> names a locale and a currency code and carries an optional name and symbol. A lookup walks up the locale name until it reaches the invariant values on the currency entry. Names and symbols are resolved independently. [How Names and Symbols Are Localized](../concepts/localization.md#provider-data) shows the walk.

So a provider only needs to return an entry where a locale's value differs from what the walk would otherwise produce. An entry may set only the name or only the symbol:

```csharp
yield return new CurrencyLocalizationEntry("fr", "CAD") { Name = "dollar canadien", Symbol = "$CA" };
yield return new CurrencyLocalizationEntry("fr-CA", "CAD") { Symbol = "$" };   // the name still comes from "fr"
```

The CLDR package stores roughly forty thousand entries for over six hundred locales in under half a megabyte this way. A locale name must not be empty, because the invariant values belong on the currency entry. Locale names should also match .NET culture names, so that lookups with <xref:System.Globalization.CultureInfo> find them.

## Validation

<xref:Singulink.Globalization.CurrencyData.Load*> throws <xref:System.IO.InvalidDataException> naming the provider and the offending currency when a provider returns:

| Problem | Message |
| --- | --- |
| Two currencies with the same code | "Currency data from '...' contains multiple currencies with currency code '...'." |
| A localization for a currency it did not return | "Currency data from '...' contains a localization for unknown currency '...'." |
| An entry type that is not a single type | "Currency data from '...' specifies an invalid type for currency '...'." |
| Rounding values outside the valid range | "Currency data from '...' specifies invalid rounding information for currency '...'." |

Entries themselves throw <xref:System.ArgumentException> from their constructors when the code, name or locale is empty.

## Next Steps

Continue with these related articles:

- [CLDR Currency Data](cldr-data.md) - The provider that ships with the library.
- [How Names and Symbols Are Localized](../concepts/localization.md) - How the resolved names and symbols are used.
- [Currencies and Registries](currencies-and-registries.md) - Registering a provider as the default.

</div>
