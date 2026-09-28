# Singulink.Globalization.Currency.Cldr

**Singulink.Globalization.Currency.Cldr** provides currency data from the Unicode Common Locale Data Repository (CLDR) for the [Singulink.Globalization.Currency](https://www.nuget.org/packages/Singulink.Globalization.Currency/) library.

The base library builds its default currency registry from the globalization data that ships with the runtime and operating system. That data is convenient but has limitations: it varies between ICU and NLS and between ICU versions, it only includes currencies that are the primary currency of some region, and it does not contain cash rounding rules. This package embeds a compact copy of the CLDR currency data so that registries behave identically on every runtime and include:

- Every currency known to CLDR, including current legal tender, non-tender codes like `XAU` and `XDR`, and historical currencies.
- Localized names and symbols for every CLDR locale.
- Standard rounding rules and cash rounding rules for every currency, so `MonetaryValue.RoundToCash()` works for currencies like `CAD` (nearest 0.05), `DKK` (nearest 0.50) and `SEK` (whole kronor).

## Usage

Register the CLDR data as the default registry once at application startup, before any currencies or monetary values are used:

```c#
CldrCurrencyData.RegisterAsDefault();

var price = new MonetaryValue(10.03m, "CAD");
price.RoundToCash(); // CAD 10.05
```

By default the registry contains currencies that are currently legal tender. Other currency types can be included as well:

```c#
CldrCurrencyData.RegisterAsDefault(CldrCurrencyTypes.CurrentTender | CldrCurrencyTypes.CurrentNonTender);
```

Registries can also be used directly without changing the default registry:

```c#
var registry = CldrCurrencyData.Registry;                                   // Current legal tender, cached
var historical = CldrCurrencyData.CreateRegistry(CldrCurrencyTypes.All);    // Everything CLDR knows about

var gold = CldrCurrencyData.AllCurrencies.First(c => c.CurrencyCode == "XAU");
```

The CLDR version the data was generated from is available via `CldrCurrencyData.CldrVersion`.

## License

CLDR data is provided by the Unicode Consortium under the [Unicode License](https://www.unicode.org/license.txt). The library itself is MIT licensed.
