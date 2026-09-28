<div class="article">

# Monetary Values

<xref:Singulink.Globalization.MonetaryValue> is an immutable struct that pairs a <xref:System.Decimal> amount with a <xref:Singulink.Globalization.Currency>. This guide covers creating values, arithmetic and comparison, the special default value, and the helpers for working with amounts safely.

### Design

The type is deliberately close to <xref:System.Decimal> in feel: it is a readonly value type, it supports the usual operators, and it implements the generic math interfaces on .NET so it works with generic algorithms. What it adds is currency safety. Any operation that would combine two currencies is rejected, so a bug that mixes dollars and euros surfaces immediately instead of producing a plausible-looking wrong number.

## Creating Values

Values are created with a currency instance or a currency code, which is resolved against <xref:Singulink.Globalization.CurrencyRegistry.Default>:

```csharp
var a = new MonetaryValue(19.99m, Currency.GetCurrency("USD"));
var b = new MonetaryValue(19.99m, "USD");
var c = MonetaryValue.Create(19.99m, "USD");   // same as the constructor, usable as a method group
```

The amount keeps whatever precision it was given. Nothing is rounded until you ask for it, so intermediate calculations like tax or currency conversion keep full precision and the result is rounded once at the end. See [Rounding and Allocation](rounding-and-allocation.md).

<xref:Singulink.Globalization.MonetaryValue.Amount> and <xref:Singulink.Globalization.MonetaryValue.Currency> expose the parts. Values are equal when both the amount and the currency instance are equal, and they hash accordingly, so they work as dictionary keys.

## The Default Value

Because it is a struct, `default(MonetaryValue)` exists whether the library likes it or not, so the library gives it a meaning: a zero amount with no currency, exposed as <xref:Singulink.Globalization.MonetaryValue.Default>. It behaves like a currency-neutral zero:

- Adding or subtracting it to any value returns that value unchanged.
- <xref:Singulink.Globalization.MonetaryValue.Currency> throws <xref:System.InvalidOperationException> on it. Use <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault>, which returns `null`, or check <xref:Singulink.Globalization.MonetaryValue.IsDefault>.
- It is not equal to a zero in any currency. `default != new MonetaryValue(0m, "USD")`.
- Money bags ignore it when added and return it for currencies they do not contain.

<xref:Singulink.Globalization.MonetaryValue.CreateOrDefault*> creates a value that may be default when the currency is `null` and the amount is zero, and throws if the amount is non-zero without a currency. <xref:Singulink.Globalization.MonetaryValue.ToDefaultIfZero*> converts a zero in any currency to the default value, which is useful for normalizing totals.

> [!NOTE]
> The default value is a convenience for accumulators and optional fields, not a way to represent "zero dollars". A running total can start at <xref:Singulink.Globalization.MonetaryValue.Default> and take on the currency of the first value added to it.

## Arithmetic

Same-currency values add and subtract with each other, and every value scales by a <xref:System.Decimal>:

```csharp
var price = new MonetaryValue(19.99m, "USD");

price + price;              // USD 39.98
price - new MonetaryValue(5m, "USD");
price + 0.01m;              // USD 20.00, decimal operands act on the amount
price * 3;                  // USD 59.97
price / 2;                  // USD 9.995
-price;                     // USD -19.99
price / new MonetaryValue(10m, "USD"); // 1.999m, a ratio with no currency
```

Dividing two values gives a plain <xref:System.Decimal> ratio, which is the only operation that yields a number rather than a value. Increment and decrement operators add or subtract one unit of the currency.

Adding, subtracting or dividing values in different currencies throws <xref:System.ArgumentException>. There is no implicit currency conversion in the library; convert explicitly by multiplying the amount by a rate and creating a value in the target currency.

## Comparison and Equality

The relational operators, <xref:Singulink.Globalization.MonetaryValue.CompareTo*>, <xref:Singulink.Globalization.MonetaryValue.Min*> and <xref:Singulink.Globalization.MonetaryValue.Max*> require the same currency and throw <xref:System.ArgumentException> otherwise:

```csharp
var a = new MonetaryValue(10m, "USD");
var b = new MonetaryValue(20m, "USD");

a < b;                      // true
MonetaryValue.Max(a, b);    // USD 20
a < new MonetaryValue(20m, "EUR"); // throws
```

Equality is the exception. `==` and <xref:Singulink.Globalization.MonetaryValue.Equals*> never throw; values in different currencies are simply not equal. This is what makes values safe to use in dictionaries and LINQ set operations, where equality is called on arbitrary pairs.

Sorting a sequence of values with <xref:System.Linq.Enumerable.OrderBy*> or <xref:System.Collections.Generic.List`1.Sort*> therefore only works when they share a currency. Group by <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault> first, or use a money bag, when a sequence can contain several currencies.

## Other Helpers

- <xref:Singulink.Globalization.MonetaryValue.Abs*> returns the value with a non-negative amount.
- <xref:Singulink.Globalization.MonetaryValue.Round*> and <xref:Singulink.Globalization.MonetaryValue.RoundToCash*> round to the currency's policies. See [Rounding and Allocation](rounding-and-allocation.md).
- <xref:Singulink.Globalization.MonetaryValue.Allocate*> and <xref:Singulink.Globalization.MonetaryValue.AllocateToCash*> split a value into parts without losing minor units.
- <xref:Singulink.Globalization.MonetaryValue.ToString*>, <xref:Singulink.Globalization.MonetaryValue.TryFormat*>, <xref:Singulink.Globalization.MonetaryValue.Parse*> and <xref:Singulink.Globalization.MonetaryValue.TryParse*> convert to and from strings. See [Formatting](formatting.md) and [Parsing](parsing.md).

### Generic math

On .NET, the type implements the <xref:System.Numerics> operator interfaces for addition, subtraction, multiplication and division by <xref:System.Decimal>, comparison, negation, increment and decrement, so it can be used with generic code constrained on those interfaces. It does not implement <xref:System.Numerics.INumber`1>, since concepts like `One` or parsing without a currency do not apply.

## Next Steps

Continue with these related articles:

- [Rounding and Allocation](rounding-and-allocation.md) - Rounding policies, cash rounding and allocation.
- [Formatting](formatting.md) - The format string grammar and culture behavior.
- [Money Bags](money-bags.md) - Collections of values in several currencies.
- [Currency Identity](../concepts/currency-identity.md) - Why the currency instance, not just the code, determines equality.

</div>
