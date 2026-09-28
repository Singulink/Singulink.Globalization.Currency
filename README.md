# Singulink.Globalization.Currency

[![Chat on Discord](https://img.shields.io/discord/906246067773923490)](https://discord.gg/EkQhJFsBu6)
[![Build and Test](https://github.com/Singulink/Singulink.Globalization.Currency/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink/Singulink.Globalization.Currency/actions?query=workflow%3A%22build+and+test%22)

| Library | Package |
| --- | --- |
| **Singulink.Globalization.Currency** | [![View nuget packages](https://img.shields.io/nuget/v/Singulink.Globalization.Currency.svg)](https://www.nuget.org/packages/Singulink.Globalization.Currency/) |
| **Singulink.Globalization.Currency.Cldr** | [![View nuget packages](https://img.shields.io/nuget/v/Singulink.Globalization.Currency.Cldr.svg)](https://www.nuget.org/packages/Singulink.Globalization.Currency.Cldr/) |

**Singulink.Globalization.Currency** is a .NET library that provides types like `Currency`, `MonetaryValue` and a range of collections and interfaces that make it easy to work with money and currencies in your applications while following best practices. The library is well-documented and follows the same design principles as built-in .NET types and collections. It has been painstakingly tested and optimized to ensure that it is both fast and reliable.

**Singulink.Globalization.Currency.Cldr** is an optional companion package that provides an up-to-date currency registry built from Unicode CLDR data instead of the globalization data that ships with the runtime. It includes localized names and symbols for every locale, currencies that are not tied to a region (such as `XAU` or `XDR`), historical currencies, and cash rounding rules, and it behaves identically on every runtime and operating system.

### Key Features

✔️ Wide range of string formatting options for every scenario (including `ISpanFormattable` support)  
✔️ Four types of money bag collections for working with multiple currencies at once  
✔️ Collection expression/literal syntax support  
✔️ Builds currency data from built-in system globalization data, or from embedded Unicode CLDR data with the optional CLDR package  
✔️ Standard and cash rounding rules for every currency (cash rounding requires the CLDR package)  
✔️ Support for custom currency registries, i.e. for cryptocurrency support  
✔️ Generic math support (.NET)  
✔️ Extensive `ReadOnlySpan<char>` lookup support (.NET 9+)  
✔️ Full AOT and WinRT support  
✔️ Extensive test coverage  

### About Singulink

We are a small team of engineers and designers dedicated to building beautiful, functional, and well-engineered software solutions. We offer very competitive rates as well as fixed-price contracts and welcome inquiries to discuss any custom development / project support needs you may have.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

## Installation

The packages are available on NuGet - simply install the `Singulink.Globalization.Currency` package, and optionally the `Singulink.Globalization.Currency.Cldr` package for CLDR-based currency data.

**Supported Runtimes**: Everywhere .NET Standard 2.0 is supported, including:
- .NET
- .NET Framework
- Mono / Xamarin

End-of-life runtime versions that are no longer officially supported are not tested or supported by this library.

## Usage

### Currencies and monetary values

```c#
// Currencies come from the default registry, which is built from system globalization data unless a different registry is registered:
var usd = Currency.GetCurrency("USD");

// Monetary values are immutable and carry their currency:
var price = new MonetaryValue(19.99m, usd);
var total = price * 3;                                // USD 59.97
var withTax = (total * 1.13m).Round();                // USD 67.77 (rounded to the currency's 2 decimal digits)

// Values of different currencies can't be mixed by accident:
var eur = new MonetaryValue(10m, "EUR");
// total + eur                                        // throws ArgumentException

// Formatting and parsing are culture-aware:
withTax.ToString();                                   // "USD 67.77" (current culture)
withTax.ToString("C", CultureInfo.GetCultureInfo("en-US")); // "$67.77"
MonetaryValue.Parse("$67.77", MonetaryStyles.LocalSymbol, CultureInfo.GetCultureInfo("en-US"));
```

### Money bags

Money bags hold amounts in multiple currencies at once and come in mutable, immutable, sorted and unsorted flavors:

```c#
var bag = new MoneyBag { new(100m, "USD"), new(50m, "EUR") };
bag.Add(new MonetaryValue(25m, "USD"));               // USD 125, EUR 50
bag.RoundAll();

ImmutableSortedMoneyBag snapshot = [new(1m, "CAD"), new(2m, "USD")];
```

### Rounding and cash rounding

Every currency has a `RoundingPolicy` describing its decimal digits and any rounding increment. Currencies can also have a `CashRoundingPolicy` for physical cash transactions, which differs from the standard policy for currencies whose smallest coins have been withdrawn:

```c#
CldrCurrencyData.RegisterAsDefault(); // At application startup, see below

var amount = new MonetaryValue(10.03m, "CAD");
amount.Round();                                       // CAD 10.03
amount.RoundToCash();                                 // CAD 10.05 (Canada withdrew the penny)

new MonetaryValue(10.49m, "SEK").RoundToCash();       // SEK 10 (Sweden rounds cash to whole kronor)
```

Cash rounding rules are not part of the globalization data that ships with .NET, so `RoundToCash()` throws `NotSupportedException` for currencies from the system registry. Install the `Singulink.Globalization.Currency.Cldr` package to get them, or initialize the `CashRoundingPolicy` property when creating custom currencies.

### CLDR currency data

The `Singulink.Globalization.Currency.Cldr` package embeds currency data from the Unicode Common Locale Data Repository (CLDR). Register it as the default registry once at application startup, before any currencies or monetary values are used:

```c#
CldrCurrencyData.RegisterAsDefault();
```

The default CLDR registry contains currencies that are currently legal tender. Non-tender codes like `XAU` (gold) and historical currencies can be included as well, or used through separate registries:

```c#
CldrCurrencyData.RegisterAsDefault(CldrCurrencyTypes.CurrentTender | CldrCurrencyTypes.CurrentNonTender);

var everything = CldrCurrencyData.CreateRegistry(CldrCurrencyTypes.All);
var gold = everything["XAU"];
```

### Custom currencies and registries

```c#
var bitcoin = new Currency("BTC", "Bitcoin", "₿") {
    RoundingPolicy = new RoundingPolicy(8),
};

var registry = new CurrencyRegistry("Crypto", [bitcoin]);
var value = new MonetaryValue(0.5m, bitcoin);
```

## Further Reading

You can view the fully documented API on the [project documentation site](https://www.singulink.com/Docs/Singulink.Globalization.Currency/api/Singulink.Globalization.Currency.html).
