<div class="article">

# Currencies and Registries

This guide covers the <xref:Singulink.Globalization.Currency> type, how currencies are organized into registries, the lifecycle of the default registry, and how to define custom currencies for things like cryptocurrencies or loyalty points.

### Registry model

A <xref:Singulink.Globalization.CurrencyRegistry> is an immutable, named set of currencies. Every registry defines its own currency instances, and a currency belongs to exactly one registry. The library ships two ways to populate a registry: the system registry built from runtime globalization data, and registries built from a currency data provider such as the CLDR package. You can also build registries by hand from any currencies you like.

There is one process-wide default registry, <xref:Singulink.Globalization.CurrencyRegistry.Default>. The APIs that take a currency code string resolve it there:

```csharp
var price = new MonetaryValue(10m, "USD");   // "USD" is looked up in CurrencyRegistry.Default
var usd = Currency.GetCurrency("USD");       // so is this

var bag = new MoneyBag();
bag.Add(5m, "EUR");                          // a money bag looks codes up in its own registry,
                                             // which is the default registry unless you pass another
```

APIs that take a <xref:Singulink.Globalization.Currency> instance never touch the default registry.

## Default Registry

The default registry is created lazily on first access. Unless you register other data, it is the system registry. The system registry holds the currencies that are the current currency of some region known to the runtime, with names, symbols and decimal digits taken from <xref:System.Globalization.RegionInfo> and <xref:System.Globalization.CultureInfo> data.

To use different data, register it at application startup, before anything touches the default registry:

```csharp
// CLDR data from the Singulink.Globalization.Currency.Cldr package:
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);

// Or a registry built by hand:
CurrencyRegistry.SetDefault(new CurrencyRegistry("App", [usd, eur, points]));

// Or a factory that runs the first time the default is accessed:
CurrencyRegistry.SetDefault(() => BuildRegistry());
```

<xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*> optionally takes <xref:Singulink.Globalization.CurrencyTypes> flags to include non-tender codes such as `XAU` or historical currencies in the default registry.

Application code rarely needs to touch registries directly. Register the data once, then use currency codes everywhere. Reach for explicit registries when a component must work with a different set of currencies than the rest of the application.

### Set once, before first use

Once the default registry has been created, <xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*> and both <xref:Singulink.Globalization.CurrencyRegistry.SetDefault*> overloads throw <xref:System.InvalidOperationException>, since currencies and values that reference the old registry may already exist. Anything that reads <xref:Singulink.Globalization.CurrencyRegistry.Default> creates it:

```csharp
var usd = Currency.GetCurrency("USD");             // creates the default registry
var price = new MonetaryValue(10m, "USD");         // so does a currency code
var bag = new MoneyBag();                          // and a money bag without a registry
MonetaryValue.Parse("USD 10");                     // and parsing without a registry

CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider); // too late: throws InvalidOperationException
```

> [!IMPORTANT]
> Make the call the first line of startup. A static field initializer that creates a monetary value from a code counts as first use, and so does a test that ran earlier in the same process.

### System registry and its limits

The system registry depends on the operating system, has no cash rounding rules, and leaves out currencies that are not the currency of a region. [CLDR Currency Data](cldr-data.md#why-use-it) describes each limit and how the CLDR package removes it.

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
var frCA = CultureInfo.GetCultureInfo("fr-CA");

registry.TryGetCurrenciesBySymbol("$", out var dollars);           // USD, CAD, AUD, ... in the current culture
registry.TryGetCurrenciesBySymbol("$", frCA, out var frenchDollars); // symbols are localized, so results vary by culture
```

<xref:Singulink.Globalization.CurrencyRegistry.TryGetCurrenciesBySymbol*> returns the matches ordered by currency code. See [How Names and Symbols Are Localized](../concepts/localization.md) for how symbols vary by culture.

By region, for the local currency of a culture:

```csharp
Currency.TryGetLocalCurrency(out var local);                              // current culture
Currency.TryGetLocalCurrency(CultureInfo.GetCultureInfo("en-CA"), out var cad);
Currency.TryGetLocalCurrency(new RegionInfo("JP"), out var jpy);
```

<xref:Singulink.Globalization.Currency.TryGetLocalCurrency*> requires a region-specific culture such as `en-CA`. Neutral cultures like `en` have no region and return `false`.

## Currency Members

Each currency exposes these members. The values shown are for `USD` from the default registry:

| Member | What it is | USD, system data | USD, CLDR data |
| --- | --- | --- | --- |
| <xref:Singulink.Globalization.Currency.CurrencyCode> | The unique code, normally the three-letter ISO 4217 code | `"USD"` | `"USD"` |
| <xref:Singulink.Globalization.Currency.Name> | The invariant (English) name | `"US Dollar"` | `"US Dollar"` |
| <xref:Singulink.Globalization.Currency.Symbol> | The invariant symbol | `"$"` | `"$"` |
| <xref:Singulink.Globalization.Currency.GetLocalizedName*> | The name for a culture, defaulting to the current culture | `"US Dollar"` in `fr` | `"dollar des États-Unis"` in `fr` |
| <xref:Singulink.Globalization.Currency.GetLocalizedSymbol*> | The symbol for a culture, defaulting to the current culture | `"$"` in `en-CA` | `"US$"` in `en-CA` |
| <xref:Singulink.Globalization.Currency.RoundingPolicy> and <xref:Singulink.Globalization.Currency.DecimalDigits> | How amounts are rounded. See [Rounding and Allocation](rounding-and-allocation.md). | 2 decimal digits | 2 decimal digits |
| <xref:Singulink.Globalization.Currency.CashRoundingPolicy> | How cash amounts are rounded, or `null` when unknown | `null` | 2 decimal digits |

The system values are what .NET reports on Windows with ICU, and they can differ on other systems. <xref:Singulink.Globalization.Currency.ToString*> returns the localized name with the code, such as `"US Dollar (USD)"`, or just the name with the `"S"` format.

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

Codes and symbols are not validated when a currency is created. But a registry cannot parse monetary strings if it has a code or symbol that contains a digit, whitespace, a sign or a parenthesis. [Parsing](parsing.md#registries-that-cannot-parse) has the exact requirement and how to check a code with <xref:Singulink.Globalization.Currency.IsSymbolOrCodeParsable*>.

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

The constructor throws <xref:System.ArgumentException> if two currencies share a code. Registries implement <xref:System.Collections.Generic.IReadOnlySet`1> and the read-only side of <xref:System.Collections.Generic.ISet`1>, so set operations like <xref:Singulink.Globalization.CurrencyRegistry.Overlaps*> and <xref:Singulink.Globalization.CurrencyRegistry.IsSubsetOf*> are available. A registry enumerates its currencies in code order.

A money bag bound to a registry rejects values whose currency is not in it. That is the main reason to create a registry: it defines the set of currencies that a part of the application accepts.

## Registries in Tests

The default registry is shared by the whole process and can be set only once, so a test suite sets it once, before any test runs. A module initializer in the test project does that:

```csharp
internal static class TestSetup
{
    [ModuleInitializer]
    internal static void SetUpCurrencies() => CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);
}
```

A test that needs a different set of currencies does not change the default. It builds its own registry and takes every currency from it, since a currency code in a constructor resolves through the default registry:

```csharp
var registry = new CurrencyRegistry("Test", [new Currency("PTS", "Points") { RoundingPolicy = new RoundingPolicy(0) }]);
var points = registry["PTS"];

var value = new MonetaryValue(10m, points);          // not new MonetaryValue(10m, "PTS"), which looks in the default registry
var bag = new MoneyBag(registry);
bag.Add(5m, "PTS");                                  // a bag looks codes up in its own registry
var parsed = registry.ParseMonetaryValue("PTS 3");   // and so does parsing against a registry
```

See [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md) for what goes wrong when values from two registries meet.

## Next Steps

Continue with these related articles:

- [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md) - What it means that a currency belongs to one registry.
- [How Names and Symbols Are Localized](../concepts/localization.md) - How names and symbols are resolved for a culture.
- [CLDR Currency Data](cldr-data.md) - Registering CLDR data and choosing which currency types to include.
- [Money Bags](money-bags.md) - Bags and their registries.

</div>
