<div class="article">

# Why Two USD Currencies Are Not Equal

A <xref:Singulink.Globalization.Currency> is compared by reference, not by code. Two currency objects with the code `USD` are two different currencies unless they are the same object. This article explains why the library works this way, what it means in practice, and how to avoid the situations where it bites.

### The rule

```csharp
var systemUsd = Currency.GetCurrency("USD");                                 // from the default registry, built from system data
var cldrUsd = CurrencyData.Load(CldrCurrencyData.Provider).Registry["USD"];  // from the CLDR data

systemUsd == cldrUsd;                    // false: two currencies with the same code

var a = new MonetaryValue(10m, systemUsd);
var b = new MonetaryValue(10m, cldrUsd);

a == b;                                  // false, although both format as "USD 10.00"
var sum = a + b;                         // throws ArgumentException
```

Every <xref:Singulink.Globalization.CurrencyRegistry> populated from a data source creates its own currency instances. The system registry has one `USD`, a registry loaded from the CLDR package has another, and a registry loaded from a second provider instance has a third. Across data sources, they are distinct.

Within a registry a currency is a singleton: every lookup of `USD` returns the same instance, and registries built by hand from existing currencies share those instances. <xref:Singulink.Globalization.MonetaryValue> equality compares the currency instance and the amount, so values inherit the rule.

## Why Not Compare by Code

Two currencies with the same code but from different data sources are not interchangeable. They can have different rounding policies, a different cash policy or none at all, different symbols, and different localized names. If they were equal, a value with one set of rules could be combined with a value with another, and the result would follow whichever value happened to be on the left. Reference identity makes the data source part of the identity, so a currency always means exactly one set of rules.

The other reason is the same one that makes the library reject arithmetic across currencies: mistakes should fail loudly. An application that accidentally builds two registries from two data sources and mixes their values has a real configuration problem, and a thrown exception at the point of mixing is far easier to diagnose than a subtle inconsistency in totals.

## What This Means in Practice

For most applications the rule is invisible. There is one default registry, every currency code resolves through it, and every value uses one of its currencies. These are the situations where identity matters.

#### Changing the default registry after use

The default can only be set before it is first accessed, precisely so that no currencies from a previous default can exist. See [Currencies and Registries](../guides/currencies-and-registries.md#set-once-before-first-use).

```csharp
var price = new MonetaryValue(10m, "USD");                       // the system registry is now the default
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);  // throws InvalidOperationException
```

#### Loading the same provider twice

<xref:Singulink.Globalization.CurrencyData.Load*> caches by provider instance, so use one provider instance per data source. The CLDR package exposes a single <xref:Singulink.Globalization.CldrCurrencyData.Provider>, so this happens naturally unless a custom provider is constructed repeatedly:

```csharp
var first = CurrencyData.Load(new DatabaseCurrencyProvider(db)).Registry["USD"];
var second = CurrencyData.Load(new DatabaseCurrencyProvider(db)).Registry["USD"];

first == second;      // false: two provider instances, two loads, two currencies
```

#### Mixing the system registry and a provider

Values from the provider's registry are not compatible with values from a system-based registry that you created explicitly. Register the provider at startup and use it exclusively.

#### Tests

Tests that create their own registries should create values from those registries, not from currency codes, since codes resolve through the process-wide default. See [Registries in Tests](../guides/currencies-and-registries.md#registries-in-tests).

```csharp
var cldr = CurrencyData.Load(CldrCurrencyData.Provider);
var registry = cldr.CreateRegistry(CurrencyTypes.All);

var good = new MonetaryValue(10m, registry["USD"]);   // a currency from the test's registry
var bad = new MonetaryValue(10m, "USD");              // a currency from the default registry
```

### Registries that share instances

Registries created from the same data share currency instances, so they are all compatible with each other. That includes registries from <xref:Singulink.Globalization.CurrencyData.CreateRegistry*> with different <xref:Singulink.Globalization.CurrencyTypes>, and hand-built registries that contain subsets of one source.

## Checking Compatibility

To check whether two values are compatible, compare their <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault> currencies. A <xref:Singulink.Globalization.Currency> compares by reference, so this answers the question directly. Comparing <xref:Singulink.Globalization.Currency.CurrencyCode> answers a different question: whether two currencies represent the same real-world currency. That is the right check when you reconcile data that came from outside the process.

```csharp
a.CurrencyOrDefault == b.CurrencyOrDefault;                  // can a and b be added? false here
a.Currency.CurrencyCode == b.Currency.CurrencyCode;          // the same real-world currency? true
```

## Further Reading

These articles cover the related topics in more depth:

- [Currencies and Registries](../guides/currencies-and-registries.md) - The default registry lifecycle and custom registries.
- [Monetary Values](../guides/monetary-values.md) - Equality and comparison semantics.
- [CLDR Currency Data](../guides/cldr-data.md) - Loading data once and sharing it.

</div>
