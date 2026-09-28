# Singulink.Globalization.Currency.Cldr

**Singulink.Globalization.Currency.Cldr** provides currency data from the Unicode Common Locale Data Repository (CLDR) for the [Singulink.Globalization.Currency](https://www.nuget.org/packages/Singulink.Globalization.Currency/) library.

The main library builds its default currency registry from the globalization data that ships with the runtime and operating system. That data is convenient but has limitations: it varies between ICU and NLS and between ICU versions, it only includes currencies that are the primary currency of some region, and it does not contain cash rounding rules. This package embeds a compact copy of the CLDR currency data so that registries behave identically on every runtime and include:

- Every currency known to CLDR, including current legal tender, non-tender codes like `XAU` and `XDR`, and historical currencies.
- Localized names and symbols for every CLDR locale.
- Standard rounding rules and cash rounding rules for every currency, so `MonetaryValue.RoundToCash()` works for currencies like `CAD` (nearest 0.05), `DKK` (nearest 0.50) and `SEK` (whole kronor).

## Versioning

This package contains only data and does not depend on the main library, so it can always be updated to the latest CLDR release without changing the version of the main library that is used. The package version mirrors the CLDR version that the data was generated from, so `48.0.0` contains CLDR 48 data and `48.1.0` contains CLDR 48.1 data. A fourth version component is only added if the package is republished with the same data. The CLDR version and release date are also available at runtime via `CldrCurrencyData.CldrVersion` and `CldrCurrencyData.CldrReleaseDate`.

## Usage

Register the CLDR data as the default registry once at application startup, before any currencies or monetary values are used:

```c#
CurrencyRegistry.SetDefault(CldrCurrencyData.Provider);

var price = new MonetaryValue(10.03m, "CAD");
price.RoundToCash(); // CAD 10.05
```

By default the registry contains currencies that are currently legal tender. Other currency types can be included as well:

```c#
CurrencyRegistry.SetDefault(CldrCurrencyData.Provider, CurrencyTypes.CurrentTender | CurrencyTypes.CurrentNonTender);
```

The data can also be used directly without changing the default registry:

```c#
var cldr = CurrencyData.Load(CldrCurrencyData.Provider);

var registry = cldr.Registry;                                   // Current legal tender, cached
var historical = cldr.CreateRegistry(CurrencyTypes.Historical); // Currencies no longer in use
var gold = cldr.AllCurrencies.First(c => c.CurrencyCode == "XAU");
```

## License

CLDR data is provided by the Unicode Consortium under the [Unicode License](https://www.unicode.org/license.txt). The library itself is MIT licensed.
