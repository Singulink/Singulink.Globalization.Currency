<div class="article">

# CLDR Currency Data

The `Singulink.Globalization.Currency.Cldr` package embeds currency data from the Unicode Common Locale Data Repository (CLDR), the same source that ICU and most operating systems derive their locale data from. This guide explains what the package adds over the system data, how to register it, and how its versioning works.

### Why use it

Currency data comes from one of three places. How they compare:

| | System data | CLDR package | Your own provider |
| --- | --- | --- | --- |
| Package to add | None | `Singulink.Globalization.Currency.Cldr` | None; you implement a [data provider](data-providers.md) |
| Same results on every platform | No | Yes | Yes |
| Currencies that are not a region's currency, such as `XAU` | No | Yes, when included | The ones you supply |
| Historical currencies, such as `DEM` | No | Yes, when included | The ones you supply |
| Cash rounding rules | No | Yes | Yes, equal to standard rounding unless you set them |
| Localized names and symbols | From the runtime | Every CLDR locale | The ones you supply |

The core library's system registry is built from the globalization data that ships with the runtime, which has three limitations:

| Limit | What it means |
| --- | --- |
| **It varies by platform.** | .NET uses ICU on Linux, macOS and modern Windows, and NLS on older Windows and .NET Framework. The two disagree on some symbols and names. ICU versions also change symbol data over time, so the same code can format differently on different machines. |
| **It only knows regional currencies.** | Codes that are not the primary currency of a region, such as gold (`XAU`), special drawing rights (`XDR`) and historical currencies, are missing. |
| **It has no cash rounding rules.** | <xref:Singulink.Globalization.MonetaryValue.RoundToCash*> and <xref:Singulink.Globalization.MonetaryValue.AllocateToCash*> throw <xref:System.NotSupportedException> for every currency. |

The CLDR package fixes all three. Its data is embedded in the package, so behavior is identical everywhere, every currency CLDR knows about is available, and every currency has a cash rounding policy.

## Registering the Data

Register the provider once at application startup:

```csharp
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);
```

From then on <xref:Singulink.Globalization.CurrencyRegistry.Default> is a registry named `CLDR` containing the currencies that are currently legal tender somewhere. The call must come before anything uses the default registry; see [Currencies and Registries](currencies-and-registries.md#set-once-before-first-use).

To include other kinds of currency in the default registry, pass <xref:Singulink.Globalization.CurrencyTypes> flags:

```csharp
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider, CurrencyTypes.CurrentTender | CurrencyTypes.CurrentNonTender);
```

The types are:

| Type | What it holds | Examples | In the default registry |
| --- | --- | --- | --- |
| <xref:Singulink.Globalization.CurrencyTypes.CurrentTender> | Legal tender in at least one region | `USD`, `EUR`, `JPY` | Yes |
| <xref:Singulink.Globalization.CurrencyTypes.CurrentNonTender> | In current use but not legal tender: precious metals, special drawing rights, funds codes, and the testing and "no currency" codes | `XAU`, `XDR`, `CHE`, `XTS`, `XXX` | Only when passed |
| <xref:Singulink.Globalization.CurrencyTypes.Historical> | No longer used anywhere | `DEM`, `FRF` | Only when passed |

Include historical currencies only when you process old records. Many of them share symbols with current currencies, which makes symbol parsing more ambiguous.

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

Data is loaded once per provider instance and cached, so all registries created from the same provider share the same <xref:Singulink.Globalization.Currency> instances. That means a value created from `cldr.Registry["USD"]` is compatible with a bag bound to a registry created by `cldr.CreateRegistry(...)`. See [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md).

## Versioning

The package contains only data and depends only on the small `Singulink.Globalization.Currency.DataProviders` contract package, never on the core library. Updating it never changes the core library version, and updating the core library never requires new data. Any version of one works with any version of the other.

Because of that, the package version simply mirrors the CLDR version the data was generated from: `48.0.0` contains CLDR 48 data, `48.1.0` contains CLDR 48.1 data, and so on. A fourth version component appears only if the package is republished with the same data. The version and release date of the data are also available at runtime through <xref:Singulink.Globalization.CldrCurrencyData.CldrVersion> and <xref:Singulink.Globalization.CldrCurrencyData.CldrReleaseDate>.

New versions are published automatically when Unicode releases new CLDR data, so keeping the package updated is enough to keep currency data current.

## Localization Coverage

The data includes localized names and symbols for every locale in CLDR, over six hundred of them. It stays compact by recording only the values that differ from the parent locale. Lookups walk up the culture name: `fr-CA` falls back to `fr` and then to English, and a .NET culture name that CLDR has no data for inherits from its language. See [How Names and Symbols Are Localized](../concepts/localization.md#provider-data).

## Next Steps

Continue with these related articles:

- [Rounding and Allocation](rounding-and-allocation.md) - Using the cash rounding rules the data provides.
- [Implementing a Data Provider](data-providers.md) - Supplying currency data from another source.
- [Currencies and Registries](currencies-and-registries.md) - The default registry lifecycle.

</div>
