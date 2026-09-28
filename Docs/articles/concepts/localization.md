<div class="article">

# Localization

Currency names and symbols depend on the culture. The Canadian dollar is `$` in Canada, `CA$` in the United States and `$CA` in France, and its name is "Canadian Dollar" in English and "dollar canadien" in French. This article explains where localized names and symbols come from, how they are resolved for a culture, and how that affects formatting and parsing.

### Two sources of localization

Each <xref:Singulink.Globalization.Currency> delegates localization to an <xref:Singulink.Globalization.ICurrencyLocalizer>. The library provides two implementations behind the scenes: one that reads runtime globalization data for the system registry, and one that reads the localization entries of a data provider for registries loaded through <xref:Singulink.Globalization.CurrencyData>. Custom currencies created with a name and symbol use a fixed localizer that returns the same values for every culture, and the <xref:Singulink.Globalization.Currency> constructor that takes a localizer allows any other strategy.

<xref:Singulink.Globalization.Currency.Name> and <xref:Singulink.Globalization.Currency.Symbol> return the invariant values, which are English. <xref:Singulink.Globalization.Currency.GetLocalizedName*> and <xref:Singulink.Globalization.Currency.GetLocalizedSymbol*> return the values for a culture, defaulting to the current culture.

## System Data

The system registry collects its names and symbols from every region-specific culture the runtime knows. Each culture reports the symbol and native name of its own region's currency, so the data for a currency is assembled from the cultures that use it. When cultures disagree about a value for a shared parent culture, the library prefers the less specific culture, then Latin-script cultures, then ASCII values, then shorter symbols, which produces sensible results for cases like the many `$` currencies.

Lookups walk the <xref:System.Globalization.CultureInfo.Parent> chain of the requested culture until a value is found, finally falling back to the currency code. The result depends on the runtime's data, so it can differ between ICU and NLS and across ICU versions.

## Provider Data

Registries loaded from a data provider use the provider's <xref:Singulink.Globalization.CurrencyLocalizationEntry> values. Lookups walk up the culture name one hyphen-separated subtag at a time, so `fr-CA` is checked, then `fr`, then the invariant values on the currency. Names and symbols are resolved independently, which lets a provider override only the symbol for a regional variant while the name continues to come from the language.

The CLDR package supplies entries for every CLDR locale. .NET culture names that CLDR does not have data for, such as some legacy or script-qualified names, inherit from the nearest parent that it does have, which is normally the language.

## Effect on Formatting and Parsing

The `C` and `L` formats use the localized symbol for the format provider's culture, so the same value renders as `$1,234.50` in `en-CA` and `CA$1,234.50` in `en-US`. See [Formatting](../guides/formatting.md).

Symbol parsing is also per culture. <xref:Singulink.Globalization.MonetaryStyles.AllowLocalSymbol> matches the symbol of the culture's regional currency, and <xref:Singulink.Globalization.MonetaryStyles.AllowUnambiguousSymbols> matches any symbol that maps to exactly one currency in the registry for that culture. Whether `$` is ambiguous therefore depends on the culture and on which currencies the registry contains. <xref:Singulink.Globalization.CurrencyRegistry.TryGetCurrenciesBySymbol*> exposes the same per-culture mapping for inspection. See [Parsing](../guides/parsing.md).

Localized symbol lookups are cached per culture inside each localizer, so repeated formatting and parsing in the same culture does not repeat the resolution work.

## Further Reading

These articles cover the related topics in more depth:

- [Currencies and Registries](../guides/currencies-and-registries.md) - Custom currencies and localizers.
- [CLDR Currency Data](../guides/cldr-data.md) - Platform-independent localization data.
- [Implementing a Data Provider](../guides/data-providers.md) - Supplying localization entries.

</div>
