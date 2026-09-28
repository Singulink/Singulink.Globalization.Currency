<div class="article">

# Getting Started

This guide walks through the core types in a few minutes: getting a currency, creating and combining monetary values, formatting and parsing them, and collecting values in more than one currency. Each section points to the in-depth guide for that area.

### Packages

Install `Singulink.Globalization.Currency`. It contains everything shown in this guide and builds its default set of currencies from the globalization data that ships with the runtime.

Optionally install `Singulink.Globalization.Currency.Cldr` as well. It provides currency data from the Unicode Common Locale Data Repository (CLDR) instead of the runtime, which adds cash rounding rules, currencies that are not tied to a region, localized names and symbols for every locale, and identical behavior on every operating system. See [CLDR Currency Data](cldr-data.md) for when it is worth adding.

### How the pieces fit

A <xref:Singulink.Globalization.Currency> describes a currency: its code, names, symbols and rounding rules. A <xref:Singulink.Globalization.CurrencyRegistry> is a set of currencies, and <xref:Singulink.Globalization.CurrencyRegistry.Default> is the registry that string-based lookups use. A <xref:Singulink.Globalization.MonetaryValue> is an immutable amount in a currency, and the money bag types hold one value per currency for working with several currencies at once.

## Registering Currency Data

If the CLDR package is installed, register it once at application startup before any currencies or monetary values are used:

```csharp
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);
```

<xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*> makes <xref:Singulink.Globalization.CurrencyRegistry.Default> return a registry built from the provider's data. Without this call the default registry is built from system globalization data the first time it is accessed, which works fine for most applications that only need current currencies and standard rounding.

> [!IMPORTANT]
> The default registry can only be set before it is first accessed. Calling <xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*> or <xref:Singulink.Globalization.CurrencyRegistry.SetDefault*> afterwards throws <xref:System.InvalidOperationException>, so do it at the very start of the program.

## Currencies

Currencies are looked up by code from the default registry:

```csharp
var usd = Currency.GetCurrency("USD");

usd.Name;           // "US Dollar"
usd.Symbol;         // "$"
usd.DecimalDigits;  // 2

usd.GetLocalizedName(CultureInfo.GetCultureInfo("fr"));   // "dollar des États-Unis"
usd.GetLocalizedSymbol(CultureInfo.GetCultureInfo("en-CA")); // "US$"
```

<xref:Singulink.Globalization.Currency.GetCurrency*> throws for unknown codes and <xref:Singulink.Globalization.Currency.TryGetCurrency*> returns `false` instead. Currencies are singletons within their registry, so two lookups of the same code return the same instance. See [Currencies and Registries](currencies-and-registries.md) for custom currencies, custom registries and lookups by symbol or region.

## Monetary Values

A <xref:Singulink.Globalization.MonetaryValue> pairs a <xref:System.Decimal> amount with a currency. Arithmetic with the same currency works with normal operators, and mixing currencies is an error rather than a silent bug:

```csharp
var price = new MonetaryValue(19.99m, usd);
var subtotal = price * 3;               // USD 59.97
var tax = subtotal * 0.13m;             // USD 7.7961
var total = (subtotal + tax).Round();   // USD 67.77

var euros = new MonetaryValue(10m, "EUR");
var sum = total + euros;                // throws ArgumentException
```

<xref:Singulink.Globalization.MonetaryValue.Round*> rounds to the currency's decimal digits using banker's rounding by default. Values in the same currency compare naturally with `<`, `>` and `==`. See [Monetary Values](monetary-values.md) for the full set of operators and the special default value, and [Rounding and Allocation](rounding-and-allocation.md) for rounding policies, cash rounding and splitting amounts into parts.

## Formatting and Parsing

Formatting is culture-aware and controlled by a short format string. The default format uses the currency code, which is unambiguous in any culture:

```csharp
var enUS = CultureInfo.GetCultureInfo("en-US");
var frFR = CultureInfo.GetCultureInfo("fr-FR");

total.ToString();               // "USD 67.77" in an English culture
total.ToString(null, frFR);     // "67,77 USD"
total.ToString("C", enUS);      // "$67.77"
total.ToString("C", CultureInfo.GetCultureInfo("en-CA")); // "US$67.77"
```

Parsing accepts currency codes by default, and <xref:Singulink.Globalization.MonetaryStyles> controls whether symbols are accepted as well:

```csharp
var parsed = MonetaryValue.Parse("USD 67.77", provider: enUS);
var local = MonetaryValue.Parse("$67.77", MonetaryStyles.LocalSymbol, enUS);

if (MonetaryValue.TryParse(input, MonetaryStyles.Any, enUS, out var value))
{
    // ...
}
```

See [Formatting](formatting.md) for the format string grammar and [Parsing](parsing.md) for the styles and how symbols are resolved.

## Money Bags

A money bag holds one value per currency. Adding a value in a currency the bag already contains adds to that value:

```csharp
var bag = new MoneyBag { new(100m, "USD"), new(50m, "EUR") };
bag.Add(new MonetaryValue(25m, "USD"));    // USD 125, EUR 50
bag.Subtract(new MonetaryValue(20m, "EUR")); // USD 125, EUR 30

var usdTotal = bag["USD"];                 // USD 125
var missing = bag["JPY"];                  // default value, not an exception

ImmutableSortedMoneyBag snapshot = [new(1m, "CAD"), new(2m, "USD")];
```

Mutable, immutable, sorted and unsorted variants exist, all sharing the same read-only interface. See [Money Bags](money-bags.md).

## Next Steps

Continue with these related articles:

- [Currencies and Registries](currencies-and-registries.md) - Custom currencies, custom registries and the default registry lifecycle.
- [Monetary Values](monetary-values.md) - Operators, comparison, equality and the default value.
- [Rounding and Allocation](rounding-and-allocation.md) - Rounding policies, cash rounding and splitting amounts.
- [Formatting](formatting.md) and [Parsing](parsing.md) - Converting values to and from strings.
- [Money Bags](money-bags.md) - Working with several currencies at once.
- [Currency Identity](../concepts/currency-identity.md) - Why currencies from different registries are different currencies.

</div>
