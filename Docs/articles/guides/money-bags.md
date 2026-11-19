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

All four implement <xref:Singulink.Globalization.IReadOnlyMoneyBag>. The mutable ones also implement <xref:Singulink.Globalization.IMoneyBag>, and the immutable ones implement <xref:Singulink.Globalization.IImmutableMoneyBag>, whose mutating methods return a new bag. The unsorted bags are hash-based and slightly faster; the sorted bags enumerate in a stable order, which matters for display and for deterministic output. <xref:Singulink.Globalization.IReadOnlyMoneyBag.IsSorted> reports which kind a bag is when only the interface is known.

## Creating Bags

Bags support collection expressions, collection initializers, constructors and factory methods:

```csharp
MoneyBag bag = [new(100m, "USD"), new(50m, "EUR")];
var bag2 = new MoneyBag { new(100m, "USD"), new(50m, "EUR") };
var bag3 = new MoneyBag(values);   // values: any sequence of monetary values

ImmutableSortedMoneyBag snapshot = [new(1m, "CAD"), new(2m, "USD")];
var snapshot2 = ImmutableMoneyBag.CreateRange(values);

IReadOnlyMoneyBag readOnly = [new(1m, "CAD")];   // creates a MoneyBag
```

Every bag is bound to a <xref:Singulink.Globalization.CurrencyRegistry>, which its <xref:Singulink.Globalization.IReadOnlyMoneyBag.Registry> property returns. Unless you pass a registry to the constructor or factory method, the bag uses <xref:Singulink.Globalization.CurrencyRegistry.Default>. Passing a registry restricts the bag to that set of currencies: adding a value whose currency is not in the registry throws <xref:System.ArgumentException>. See [Currencies and Registries](currencies-and-registries.md).

The <xref:Singulink.Globalization.MoneyCollectionExtensions> methods such as <xref:Singulink.Globalization.MoneyCollectionExtensions.ToImmutableMoneyBag*> convert between bag types and from any sequence of values.

## Adding and Subtracting

Bags combine values by currency. Adding a value in a currency the bag already holds adds to the existing amount, and subtracting works the same way. When an amount reaches zero, its currency stays in the bag until you remove it or trim zero amounts:

```csharp
var bag = new MoneyBag();

bag.Add(new MonetaryValue(100m, "USD"));
bag.Add(25m, "USD");                    // USD 125
bag.Subtract(125m, "USD");              // USD 0, still present
bag.AddRange(otherBag);                 // adds every value of another bag
bag.SubtractRange(refunds);             // subtracts every value of a sequence

bag.TrimZeroAmounts();                  // removes USD 0
```

Adding or subtracting a default value does nothing, so an accumulator can start from <xref:Singulink.Globalization.MonetaryValue.Default> without special cases.

<xref:Singulink.Globalization.IMoneyBag.SetValue*> and <xref:Singulink.Globalization.IMoneyBag.SetAmount*> replace a currency's amount instead of combining with it. <xref:Singulink.Globalization.IMoneyBag.Remove*> and <xref:Singulink.Globalization.IMoneyBag.RemoveAll*> drop currencies entirely.

Immutable bags have the same operations, but each one returns a new bag:

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

Bags enumerate as <xref:Singulink.Globalization.MonetaryValue> sequences, so LINQ works directly. Equality across currencies is safe but comparison is not, so group or filter by currency before ordering by amount.

## Transforming Values

<xref:Singulink.Globalization.IMoneyBag.TransformAmounts*> and <xref:Singulink.Globalization.IMoneyBag.TransformValues*> apply a function to every value in the bag. The overloads that take a function returning a nullable amount remove the currency when the function returns `null`:

```csharp
bag.TransformAmounts(amount => amount * 1.13m);              // apply tax to everything
bag.TransformValues(value => value.Amount > 0 ? value.Amount : null); // drop non-positive values
```

<xref:Singulink.Globalization.IMoneyBag.Round*> and <xref:Singulink.Globalization.IMoneyBag.RoundToCash*> round every value according to its own currency's policy, which is the usual last step after applying rates or percentages. See [Rounding and Allocation](rounding-and-allocation.md).

## Formatting

A bag formats as a comma-separated list of its values. It takes the same format strings as <xref:Singulink.Globalization.MonetaryValue>, plus an optional `!` prefix that omits zero amounts. See [Formatting](formatting.md).

## Next Steps

Continue with these related articles:

- [Monetary Values](monetary-values.md) - The values a bag contains.
- [Currencies and Registries](currencies-and-registries.md) - Restricting a bag to a registry.
- [Formatting](formatting.md) - Bag format strings.

</div>
