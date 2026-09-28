<div class="article">

# Money Bags

A money bag is a collection that holds one <xref:Singulink.Globalization.MonetaryValue> per currency. It is the type to reach for whenever an amount can be spread across currencies: a multi-currency account balance, a shopping cart that accepts several currencies, or a report total. This guide covers the four bag types, how values combine, and the operations they support.

### The four types

| Type | Mutable | Ordered by currency code |
| --- | --- | --- |
| <xref:Singulink.Globalization.MoneyBag> | Yes | No |
| <xref:Singulink.Globalization.SortedMoneyBag> | Yes | Yes |
| <xref:Singulink.Globalization.ImmutableMoneyBag> | No | No |
| <xref:Singulink.Globalization.ImmutableSortedMoneyBag> | No | Yes |

All four implement <xref:Singulink.Globalization.IReadOnlyMoneyBag>, the mutable ones implement <xref:Singulink.Globalization.IMoneyBag>, and the immutable ones implement <xref:Singulink.Globalization.IImmutableMoneyBag>, whose mutating methods return a new bag. The unsorted bags are hash-based and slightly faster; the sorted bags enumerate in a stable order, which matters for display and for deterministic output. <xref:Singulink.Globalization.IReadOnlyMoneyBag.IsSorted> reports which kind a bag is when only the interface is known.

## Creating Bags

Bags support collection expressions, collection initializers, constructors and factory methods:

```csharp
MoneyBag bag = [new(100m, "USD"), new(50m, "EUR")];
var bag2 = new MoneyBag { new(100m, "USD"), new(50m, "EUR") };
var bag3 = new MoneyBag(values);

ImmutableSortedMoneyBag snapshot = [new(1m, "CAD"), new(2m, "USD")];
var snapshot2 = ImmutableMoneyBag.CreateRange(values);

IReadOnlyMoneyBag readOnly = [new(1m, "CAD")];   // creates a MoneyBag
```

Every bag is bound to a <xref:Singulink.Globalization.CurrencyRegistry>, available through <xref:Singulink.Globalization.IReadOnlyMoneyBag.Registry>, which defaults to <xref:Singulink.Globalization.CurrencyRegistry.Default>. Adding a value whose currency is not in the registry throws <xref:System.ArgumentException>. Pass a registry to the constructor or factory to restrict a bag to a specific set of currencies. See [Currencies and Registries](currencies-and-registries.md).

The <xref:Singulink.Globalization.MoneyCollectionExtensions> methods such as <xref:Singulink.Globalization.MoneyCollectionExtensions.ToImmutableMoneyBag*> convert between bag types and from any sequence of values.

## Adding and Subtracting

Bags combine values by currency. Adding a value in a currency the bag already holds adds to the existing amount, and subtracting works the same way. A currency stays in the bag when its amount reaches zero until it is explicitly removed or trimmed:

```csharp
var bag = new MoneyBag();

bag.Add(new MonetaryValue(100m, "USD"));
bag.Add(25m, "USD");                    // USD 125
bag.Subtract(125m, "USD");              // USD 0, still present
bag.AddRange(otherBag);
bag.SubtractRange(refunds);

bag.TrimZeroAmounts();                  // removes USD 0
```

Default values are ignored when added or subtracted, which lets accumulators start from <xref:Singulink.Globalization.MonetaryValue.Default> without special cases.

<xref:Singulink.Globalization.IMoneyBag.SetValue*> and <xref:Singulink.Globalization.IMoneyBag.SetAmount*> replace a currency's amount instead of combining with it. <xref:Singulink.Globalization.IMoneyBag.Remove*> and <xref:Singulink.Globalization.IMoneyBag.RemoveAll*> drop currencies entirely.

Immutable bags expose the same operations returning new instances:

```csharp
var balance = ImmutableMoneyBag.Create(new MonetaryValue(100m, "USD"));
var updated = balance.Add(50m, "EUR").Subtract(10m, "USD");
```

## Reading Values

The indexer returns the value for a currency, or the default value when the bag does not contain it, so lookups never throw for a missing currency:

```csharp
var usd = bag["USD"];                 // USD 125
var jpy = bag["JPY"];                 // default value, IsDefault is true

if (bag.TryGetValue("EUR", out var eur)) { }
if (bag.TryGetAmount("EUR", out decimal amount)) { }

bag.Count;                            // number of currencies
bag.Currencies;                       // the currencies present
bag.ContainsCurrency("USD");
bag.Contains(new MonetaryValue(125m, "USD"));
```

Bags enumerate as <xref:Singulink.Globalization.MonetaryValue> sequences, so LINQ works directly. Because equality across currencies is safe and comparison is not, group or filter by currency before ordering by amount.

## Transforming Values

<xref:Singulink.Globalization.IMoneyBag.TransformAmounts*> and <xref:Singulink.Globalization.IMoneyBag.TransformValues*> apply a function to every value in the bag. The overloads that take a function returning a nullable amount remove the currency when the function returns `null`:

```csharp
bag.TransformAmounts(amount => amount * 1.13m);              // apply tax to everything
bag.TransformValues(value => value.Amount > 0 ? value.Amount : null); // drop non-positive values
```

<xref:Singulink.Globalization.IMoneyBag.Round*> and <xref:Singulink.Globalization.IMoneyBag.RoundToCash*> round every value according to its own currency's policy, which is the usual last step after applying rates or percentages. See [Rounding and Allocation](rounding-and-allocation.md).

## Formatting

Bags format as a comma-separated list of their values using the same format strings as <xref:Singulink.Globalization.MonetaryValue>, with an optional `!` prefix that omits zero amounts. See [Formatting](formatting.md).

## Next Steps

Continue with these related articles:

- [Monetary Values](monetary-values.md) - The values a bag contains.
- [Currencies and Registries](currencies-and-registries.md) - Restricting a bag to a registry.
- [Formatting](formatting.md) - Bag format strings.

</div>
