<div class="article">

# CLDR Currency Data

The `Singulink.Globalization.Currency.Cldr` package embeds currency data from the Unicode Common Locale Data Repository (CLDR), the same source that ICU and most operating systems derive their locale data from. This guide explains what the package adds over the system data, how to register it, and how its versioning works.

### Why use it

The core library's system registry is built from the globalization data that ships with the runtime, which has three limitations:

- **It varies by platform.** ICU and NLS disagree on some symbols and names, and ICU versions change over time, so the same code can format differently on different machines.
- **It only knows regional currencies.** Codes that are not the primary currency of a region, such as gold (`XAU`), special drawing rights (`XDR`) and historical currencies, are missing.
- **It has no cash rounding rules**, so <xref:Singulink.Globalization.MonetaryValue.RoundToCash*> and <xref:Singulink.Globalization.MonetaryValue.AllocateToCash*> throw for every currency.

The CLDR package fixes all three. Its data is embedded in the package, so behavior is identical everywhere, every currency CLDR knows about is available, and every currency has a cash rounding policy.

## Registering the Data

Register the provider once at application startup:

```csharp
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);
```

From then on <xref:Singulink.Globalization.CurrencyRegistry.Default> is a registry named `CLDR` containing the currencies that are currently legal tender somewhere. To include other kinds of currency in the default registry, pass <xref:Singulink.Globalization.CurrencyTypes> flags:

```csharp
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider, CurrencyTypes.CurrentTender | CurrencyTypes.CurrentNonTender);
```

The types are:

- <xref:Singulink.Globalization.CurrencyTypes.CurrentTender> - legal tender in at least one region, i.e. `USD`, `EUR`, `JPY`.
- <xref:Singulink.Globalization.CurrencyTypes.CurrentNonTender> - in current use but not legal tender: precious metals, special drawing rights, funds codes such as `CHE`, and the testing and "no currency" codes `XTS` and `XXX`.
- <xref:Singulink.Globalization.CurrencyTypes.Historical> - currencies no longer used anywhere, such as `DEM` and `FRF`.

Historical currencies are worth including only when processing old records, since many of them share symbols with current currencies and make symbol parsing more ambiguous.

## Using the Data Directly

<xref:Singulink.Globalization.CurrencyData.Load*> exposes the data without changing the default registry, which is useful for tools, tests, or components that need a different set of currencies than the application default:

```csharp
var cldr = CurrencyData.Load(CldrCurrencyData.Provider);

var current = cldr.Registry;                                    // current tender, cached
var everything = cldr.CreateRegistry(CurrencyTypes.All);
var gold = everything["XAU"];

cldr.GetCurrencyType(gold);                                     // CurrentNonTender
cldr.AllCurrencies.Length;                                      // every currency in the data
```

Data is loaded once per provider instance and cached, so all registries created from the same provider share the same <xref:Singulink.Globalization.Currency> instances. That means a value created from `cldr.Registry["USD"]` is compatible with a bag bound to a registry created by `cldr.CreateRegistry(...)`. See [Currency Identity](../concepts/currency-identity.md).

## Versioning

The package contains only data and depends only on the small `Singulink.Globalization.Currency.DataProviders` contract package, never on the core library. Updating it never changes the core library version, and updating the core library never requires new data. Any version of one works with any version of the other.

Because of that, the package version simply mirrors the CLDR version the data was generated from: `48.0.0` contains CLDR 48 data, `48.1.0` contains CLDR 48.1 data, and so on. A fourth version component appears only if the package is republished with the same data. The version and release date of the data are also available at runtime through <xref:Singulink.Globalization.CldrCurrencyData.CldrVersion> and <xref:Singulink.Globalization.CldrCurrencyData.CldrReleaseDate>.

New versions are published automatically when Unicode releases new CLDR data, so keeping the package updated is enough to keep currency data current.

## Localization Coverage

The data includes localized names and symbols for every locale in CLDR, over six hundred of them, stored compactly by only recording values that differ from the parent locale. Lookups walk up the culture name, so `fr-CA` falls back to `fr` and then to English, and .NET culture names that CLDR does not have data for inherit from their language. See [Localization](../concepts/localization.md).

## Next Steps

Continue with these related articles:

- [Rounding and Allocation](rounding-and-allocation.md) - Using the cash rounding rules the data provides.
- [Implementing a Data Provider](data-providers.md) - Supplying currency data from another source.
- [Currencies and Registries](currencies-and-registries.md) - The default registry lifecycle.

</div>
