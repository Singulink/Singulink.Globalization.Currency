<div class="article">

# Currency Data Providers

Currency data packages such as `Singulink.Globalization.Currency.Cldr` supply their data to the core library through the `ICurrencyDataProvider` interface from the `Singulink.Globalization.Currency.DataProviders` package. The interface is the only contract between the core library and data packages, which is what allows either to be updated independently of the other. This article describes the contract so that you can implement your own data provider, for example over a database or a file that your application downloads at runtime.

### Registering a provider

Pass a provider to <xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider(Singulink.Globalization.ICurrencyDataProvider,Singulink.Globalization.CurrencyTypes)> at application startup to make <xref:Singulink.Globalization.CurrencyRegistry.Default> use its data, or to <xref:Singulink.Globalization.CurrencyData.Load*> to work with the data directly. Data is loaded once per provider instance and cached, so the entries are only enumerated once.

## The Interface

<xref:Singulink.Globalization.ICurrencyDataProvider> has three metadata properties and two methods:

- <xref:Singulink.Globalization.ICurrencyDataProvider.Name> is used as the name of registries created from the provider.
- <xref:Singulink.Globalization.ICurrencyDataProvider.Version> and <xref:Singulink.Globalization.ICurrencyDataProvider.ReleaseDate> describe the data for diagnostic purposes.
- <xref:Singulink.Globalization.ICurrencyDataProvider.GetCurrencies*> returns one <xref:Singulink.Globalization.CurrencyDataEntry> per currency.
- <xref:Singulink.Globalization.ICurrencyDataProvider.GetLocalizations*> returns <xref:Singulink.Globalization.CurrencyLocalizationEntry> values for localized names and symbols.

## Currency Entries

Each <xref:Singulink.Globalization.CurrencyDataEntry> requires a unique currency code and an invariant (English) name. Everything else is optional:

- <xref:Singulink.Globalization.CurrencyDataEntry.Symbol> is the invariant symbol. When it is omitted the currency code is used as the symbol.
- <xref:Singulink.Globalization.CurrencyDataEntry.DecimalDigits> and <xref:Singulink.Globalization.CurrencyDataEntry.DecimalUnits> define the standard <xref:Singulink.Globalization.RoundingPolicy>. They default to two decimal digits with no additional rounding increment.
- <xref:Singulink.Globalization.CurrencyDataEntry.CashDecimalDigits> and <xref:Singulink.Globalization.CurrencyDataEntry.CashDecimalUnits> define the cash rounding policy. When omitted they follow the standard values, so cash rounding is the same as standard rounding. A cash increment of five with two cash decimal digits rounds cash amounts to the nearest 0.05, which is the rule for the Canadian dollar.
- <xref:Singulink.Globalization.CurrencyDataEntry.Type> is a single <xref:Singulink.Globalization.CurrencyTypes> value that controls which registries include the currency. It defaults to current legal tender.

## Localization Entries

Localized values are resolved by walking up the locale chain. A lookup for the `fr-CA` culture checks entries for `fr-CA`, then `fr`, then falls back to the invariant values on the currency entry. One hyphen-separated subtag is removed at each step, which matches the way .NET culture names are structured.

Names and symbols are resolved independently. An entry may set only a name or only a symbol, and the other value continues to be resolved from the parent locale. Because of this, providers only need to return entries for locales whose values differ from what the chain would otherwise produce, which keeps data compact. The CLDR package, for example, stores roughly forty thousand entries for over six hundred locales in under half a megabyte.

Locale names must not be empty. Invariant values belong on the currency entry.

## Validation

<xref:Singulink.Globalization.CurrencyData.Load*> throws <xref:System.IO.InvalidDataException> if a provider returns duplicate currency codes, localizations for currencies that were not returned, an entry type that is not a single type, or rounding values that are out of range. The exception message names the provider and the offending currency.

</div>
