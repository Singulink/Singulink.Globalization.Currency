<div class="article">

# How Names and Symbols Are Localized

Currency names and symbols depend on the culture. This article explains where localized names and symbols come from, how they are resolved for a culture, and how that affects formatting and parsing.

### One currency, three cultures

The Canadian dollar, with CLDR data:

| Culture | Name | Symbol |
| --- | --- | --- |
| `en-CA` | Canadian Dollar | `$` |
| `en-US` | Canadian Dollar | `CA$` |
| `fr-FR` | dollar canadien | `$CA` |

```csharp
var cad = Currency.GetCurrency("CAD");

cad.GetLocalizedSymbol(CultureInfo.GetCultureInfo("en-US"));   // "CA$"
cad.GetLocalizedName(CultureInfo.GetCultureInfo("fr-FR"));     // "dollar canadien"
cad.Symbol;                                                    // "CA$", the invariant symbol
```

### Two sources of localization

Each <xref:Singulink.Globalization.Currency> delegates localization to an <xref:Singulink.Globalization.ICurrencyLocalizer>. The library provides two implementations behind the scenes, and custom currencies have a third:

| Currencies | Localized by | Same on every platform |
| --- | --- | --- |
| The system registry | Runtime globalization data, see [System Data](#system-data) | No |
| Registries loaded through <xref:Singulink.Globalization.CurrencyData> | The data provider's localization entries, see [Provider Data](#provider-data) | Yes |
| Custom currencies created with a name and symbol | A fixed localizer that returns the same values for every culture | Yes |

The <xref:Singulink.Globalization.Currency> constructor that takes a localizer allows any other strategy.

<xref:Singulink.Globalization.Currency.Name> and <xref:Singulink.Globalization.Currency.Symbol> return the invariant values, which are English. <xref:Singulink.Globalization.Currency.GetLocalizedName*> and <xref:Singulink.Globalization.Currency.GetLocalizedSymbol*> return the values for a culture, defaulting to the current culture.

## System Data

The system registry collects its names and symbols from every region-specific culture the runtime knows. Each culture reports the symbol and native name of its own region's currency, so the data for a currency is assembled from the cultures that use it.

When cultures disagree about a value for a shared parent culture, the library prefers, in this order:

1. the less specific culture;
2. a Latin-script culture;
3. a value that is all ASCII;
4. for symbols, the shorter symbol.

This produces sensible results for cases like the many `$` currencies.

Lookups walk the <xref:System.Globalization.CultureInfo.Parent> chain of the requested culture until they find a value, and fall back to the currency code if none has one. The result depends on the runtime's data, so it can differ between ICU and NLS and across ICU versions.

## Provider Data

Registries loaded from a data provider use the provider's <xref:Singulink.Globalization.CurrencyLocalizationEntry> values. Lookups walk up the culture name one hyphen-separated subtag at a time until they reach the invariant values on the currency entry:

```csharp
cad.GetLocalizedSymbol(CultureInfo.GetCultureInfo("fr-CA"));   // the first of these that has a symbol wins:
// 1. an entry for "fr-CA" and CAD
// 2. an entry for "fr" and CAD
// 3. the invariant values on the CAD currency entry
```

Names and symbols are resolved independently: an entry may set only one, and the other continues to come from the parent. That lets a provider override only the symbol for a regional variant while the name continues to come from the language.

The CLDR package supplies entries for every CLDR locale. .NET culture names that CLDR does not have data for, such as some legacy or script-qualified names, inherit from the nearest parent that CLDR has data for, which is normally the language.

## Effect on Formatting and Parsing

The `C` and `L` formats use the localized symbol for the format provider's culture, so with CLDR data the same Canadian dollar value renders as `$1,234.50` in `en-CA` and `CA$1,234.50` in `en-US`. See [Formatting](../guides/formatting.md).

Symbol parsing is also per culture. <xref:Singulink.Globalization.MonetaryStyles.AllowLocalSymbol> matches the symbol of the culture's regional currency, and <xref:Singulink.Globalization.MonetaryStyles.AllowUnambiguousSymbols> matches any symbol that maps to exactly one currency in the registry for that culture. Whether `$` is ambiguous therefore depends on the culture and on which currencies the registry contains.

<xref:Singulink.Globalization.CurrencyRegistry.TryGetCurrenciesBySymbol*> exposes the same per-culture mapping for inspection. See [Parsing](../guides/parsing.md).

Localized symbol lookups are cached per culture inside each localizer, so repeated formatting and parsing in the same culture does not repeat the resolution work.

## Further Reading

These articles cover the related topics in more depth:

- [Currencies and Registries](../guides/currencies-and-registries.md) - Custom currencies and localizers.
- [CLDR Currency Data](../guides/cldr-data.md) - Platform-independent localization data.
- [Implementing a Data Provider](../guides/data-providers.md) - Supplying localization entries.

</div>
