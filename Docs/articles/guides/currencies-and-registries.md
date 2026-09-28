<div class="article">

# Currencies and Registries

This guide covers the <xref:Singulink.Globalization.Currency> type, how currencies are organized into registries, the lifecycle of the default registry, and how to define custom currencies for things like cryptocurrencies or loyalty points.

### The registry model

A <xref:Singulink.Globalization.CurrencyRegistry> is an immutable, named set of currencies. Every registry defines its own currency instances, and a currency belongs to exactly one registry. The library ships two ways to populate a registry: the system registry built from runtime globalization data, and registries built from a currency data provider such as the CLDR package. You can also build registries by hand from any currencies you like.

There is one process-wide default registry, <xref:Singulink.Globalization.CurrencyRegistry.Default>. Every API that takes a currency code string, such as `new MonetaryValue(10m, "USD")` or `bag.Add(5m, "EUR")`, resolves the code against it. APIs that take a <xref:Singulink.Globalization.Currency> instance never touch the default registry.

## The Default Registry

The default registry is created lazily on first access. Unless told otherwise, it is the system registry: the currencies that are the current currency of some region known to the runtime, with names, symbols and decimal digits taken from <xref:System.Globalization.RegionInfo> and <xref:System.Globalization.CultureInfo> data.

To use different data, register it at application startup, before anything touches the default registry:

```csharp
// CLDR data from the Singulink.Globalization.Currency.Cldr package:
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);

// Or a registry built by hand:
CurrencyRegistry.SetDefault(new CurrencyRegistry("App", [usd, eur, points]));

// Or a factory that runs the first time the default is accessed:
CurrencyRegistry.SetDefault(() => BuildRegistry());
```

<xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*> optionally takes <xref:Singulink.Globalization.CurrencyTypes> flags to include non-tender codes such as `XAU` or historical currencies in the default registry. Once the default registry has been created, all three methods throw <xref:System.InvalidOperationException>, since currencies and values that reference the old registry may already exist.

> [!TIP]
> Application code rarely needs to touch registries directly. Register the data once, then use currency codes everywhere. Reach for explicit registries when a component must work with a different set of currencies than the rest of the application.

### The system registry and its limits

The system registry is convenient but depends on the operating system. .NET uses ICU on Linux, macOS and modern Windows and NLS on older Windows and .NET Framework, and the two disagree on some symbols and names. ICU versions also change symbol data over time. The system registry also has no cash rounding information and does not include currencies that are not the primary currency of a region, such as gold or special drawing rights. The [CLDR Currency Data](cldr-data.md) package addresses all of these.

## Looking Up Currencies

By code, from the default registry or a specific registry:

```csharp
var usd = Currency.GetCurrency("USD");             // throws ArgumentException if not found
Currency.TryGetCurrency("XYZ", out var currency);  // false

var registry = CurrencyRegistry.Default;
var eur = registry["EUR"];
registry.TryGetCurrency("JPY", out var jpy);
registry.Contains("CAD");
```

Codes are matched case-insensitively. On .NET 9 and later, every lookup also accepts a <xref:System.ReadOnlySpan`1> of characters, so codes can be sliced out of larger strings without allocating.

By symbol, which can match several currencies because symbols are not unique:

```csharp
registry.TryGetCurrenciesBySymbol("$", out var dollars);           // USD, CAD, AUD, ... in the current culture
registry.TryGetCurrenciesBySymbol("$", frCA, out var frenchDollars); // symbols are localized, so results vary by culture
```

<xref:Singulink.Globalization.CurrencyRegistry.TryGetCurrenciesBySymbol*> returns the matches ordered by currency code. See [Localization](../concepts/localization.md) for how symbols vary by culture.

By region, for the local currency of a culture:

```csharp
Currency.TryGetLocalCurrency(out var local);                              // current culture
Currency.TryGetLocalCurrency(CultureInfo.GetCultureInfo("en-CA"), out var cad);
Currency.TryGetLocalCurrency(new RegionInfo("JP"), out var jpy);
```

<xref:Singulink.Globalization.Currency.TryGetLocalCurrency*> requires a region-specific culture such as `en-CA`. Neutral cultures like `en` have no region and return `false`.

## Currency Members

Each currency exposes:

- <xref:Singulink.Globalization.Currency.CurrencyCode> - the unique code, normally the three-letter ISO 4217 code.
- <xref:Singulink.Globalization.Currency.Name> and <xref:Singulink.Globalization.Currency.Symbol> - the invariant (English) name and symbol.
- <xref:Singulink.Globalization.Currency.GetLocalizedName*> and <xref:Singulink.Globalization.Currency.GetLocalizedSymbol*> - the name and symbol for a culture, defaulting to the current culture.
- <xref:Singulink.Globalization.Currency.RoundingPolicy> and <xref:Singulink.Globalization.Currency.DecimalDigits> - how amounts are rounded. See [Rounding and Allocation](rounding-and-allocation.md).
- <xref:Singulink.Globalization.Currency.CashRoundingPolicy> - how cash amounts are rounded, or `null` when unknown.

<xref:Singulink.Globalization.Currency.ToString*> returns the localized name with the code, i.e. `"US Dollar (USD)"`, or just the name with the `"S"` format.

## Custom Currencies

Create currencies directly for anything that is not in your data source. The constructor takes the code, the name and an optional symbol, and the rounding policies are init properties:

```csharp
var bitcoin = new Currency("BTC", "Bitcoin", "₿") {
    RoundingPolicy = new RoundingPolicy(8),
};

var points = new Currency("PTS", "Reward Points") {
    RoundingPolicy = new RoundingPolicy(0),
    CashRoundingPolicy = new RoundingPolicy(0),
};
```

A currency created this way has a fixed name and symbol in every culture. To localize custom currencies, pass an <xref:Singulink.Globalization.ICurrencyLocalizer> implementation to the other constructor instead of a name and symbol.

#### Parsability of codes and symbols

Codes and symbols are not validated when a currency is created, so that unusual currencies can still be represented. However, parsing monetary strings depends on being able to tell symbols and codes apart from numbers and signs, so a registry whose currencies use characters like digits, whitespace, `+`, `-` or parentheses cannot be used for parsing. <xref:Singulink.Globalization.Currency.IsSymbolOrCodeParsable*> checks a value up front, which is worth doing for user-supplied codes. Registries with unparsable currencies work normally for everything except parsing, which throws <xref:System.InvalidOperationException> with an explanation.

## Custom Registries

A registry is created from a name and any set of currencies. Currencies from different sources can be mixed freely, and the same currency instance can belong to several registries:

```csharp
var cldr = CurrencyData.Load(CldrCurrencyData.Provider);

var registry = new CurrencyRegistry("Store", [
    cldr.Registry["USD"],
    cldr.Registry["CAD"],
    bitcoin,
    points,
]);

var value = new MonetaryValue(100m, registry["PTS"]);
var bag = new MoneyBag(registry);
```

The constructor throws <xref:System.ArgumentException> if two currencies share a code. Registries implement <xref:System.Collections.Generic.IReadOnlySet`1> and the read-only side of <xref:System.Collections.Generic.ISet`1>, so set operations like `Overlaps` and `IsSubsetOf` are available, and they enumerate their currencies in code order.

Money bags are bound to a registry and reject values whose currency is not in it, which is the main reason to create one: it defines the set of currencies a part of the application accepts.

## Next Steps

Continue with these related articles:

- [Currency Identity](../concepts/currency-identity.md) - What it means that a currency belongs to one registry.
- [Localization](../concepts/localization.md) - How names and symbols are resolved for a culture.
- [CLDR Currency Data](cldr-data.md) - Registering CLDR data and choosing which currency types to include.
- [Money Bags](money-bags.md) - Bags and their registries.

</div>
