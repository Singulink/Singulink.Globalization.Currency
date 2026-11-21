<div class="article">

# Monetary Values

<xref:Singulink.Globalization.MonetaryValue> is an immutable struct that pairs a <xref:System.Decimal> amount with a <xref:Singulink.Globalization.Currency>. This guide covers creating values, arithmetic and comparison, the special default value, and the helpers for working with amounts safely.

### Design

The type is deliberately close to <xref:System.Decimal> in feel: it is a readonly value type, it supports the usual operators, and it implements the generic math interfaces on .NET so it works with generic algorithms. What it adds is currency safety. Any operation that would combine two currencies is rejected, so a bug that mixes dollars and euros surfaces immediately instead of producing a plausible-looking wrong number.

## Creating Values

Create a value with a currency instance or with a currency code. A code is looked up in <xref:Singulink.Globalization.CurrencyRegistry.Default>:

```csharp
var a = new MonetaryValue(19.99m, Currency.GetCurrency("USD"));
var b = new MonetaryValue(19.99m, "USD");
var c = MonetaryValue.Create(19.99m, "USD");   // same as the constructor, usable as a method group
```

The amount keeps whatever precision it was given. Nothing is rounded until you ask for it, so intermediate calculations like tax or currency conversion keep full precision and the result is rounded once at the end. See [Rounding and Allocation](rounding-and-allocation.md).

<xref:Singulink.Globalization.MonetaryValue.Amount> and <xref:Singulink.Globalization.MonetaryValue.Currency> expose the parts. Values are equal when both the amount and the currency instance are equal, and they hash accordingly, so they work as dictionary keys.

## Default Value

Because it is a struct, `default(MonetaryValue)` exists whether the library likes it or not, so the library gives it a meaning: a zero amount with no currency, exposed as <xref:Singulink.Globalization.MonetaryValue.Default>. It behaves like a currency-neutral zero:

- Adding it to any value, or subtracting it from any value, returns that value unchanged.
- <xref:Singulink.Globalization.MonetaryValue.Currency> throws <xref:System.InvalidOperationException> on it. Use <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault>, which returns `null`, or check <xref:Singulink.Globalization.MonetaryValue.IsDefault>.
- It is not equal to a zero in any currency. `default != new MonetaryValue(0m, "USD")`.
- It formats as `0`, a zero without a currency, which parses back as the default value. See [Formatting](formatting.md#default-value).
- Money bags ignore it when added and return it for currencies they do not contain.

<xref:Singulink.Globalization.MonetaryValue.CreateOrDefault*> also accepts a `null` currency: with a zero amount it returns the default value, and with a non-zero amount it throws. <xref:Singulink.Globalization.MonetaryValue.ToDefaultIfZero*> converts a zero in any currency to the default value, which is useful for normalizing totals.

> [!NOTE]
> The default value is a convenience for accumulators and optional fields, not a way to represent "zero dollars". A running total can start at <xref:Singulink.Globalization.MonetaryValue.Default> and take on the currency of the first value added to it.

## Arithmetic

Values in the same currency add and subtract, and any value can be multiplied or divided by a <xref:System.Decimal>:

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

Dividing two values gives a plain <xref:System.Decimal> ratio, which is the only operation that yields a number rather than a value. The increment and decrement operators add or subtract 1, as they do for a <xref:System.Decimal>. They do not add or subtract one cent:

```csharp
var amount = new MonetaryValue(10.50m, "USD");
amount++;                   // USD 11.50
```

Adding, subtracting or dividing values in different currencies throws <xref:System.ArgumentException>. There is no implicit currency conversion in the library; convert explicitly, as shown in [Converting Between Currencies](#converting-between-currencies).

For a change that no operator expresses, <xref:Singulink.Globalization.MonetaryValue.TransformAmount*> applies a function to the amount and keeps the currency:

```csharp
var price = new MonetaryValue(19.97m, "USD");

price.TransformAmount(Math.Ceiling);              // USD 20.00
price.TransformAmount(a => Math.Min(a, 15m));     // USD 15.00, capped
```

On the default value, which has no currency, the function gets 0 and must return 0. Any other result throws <xref:System.ArgumentException>, as `MonetaryValue.Default + 5m` does. When the function can turn zero into another amount, check <xref:Singulink.Globalization.MonetaryValue.IsDefault> first:

```csharp
var fee = total.IsDefault ? total : total.TransformAmount(a => Math.Max(a, 2m));
```

## Converting Between Currencies

The library has no exchange rates and never converts on its own. Convert explicitly by multiplying the amount by a rate and creating a value in the target currency:

```csharp
var price = new MonetaryValue(19.99m, "USD");
decimal usdToEur = 0.9215m;                                          // a rate from your own source

var converted = new MonetaryValue(price.Amount * usdToEur, "EUR");   // EUR 18.420785, full precision
var charged = converted.Round();                                     // EUR 18.42
```

Keep the full precision through the rest of the calculation, such as tax, and round once at the end. See [Rounding and Allocation](rounding-and-allocation.md).

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

The type also implements the non-generic <xref:System.IComparable>, which `Comparer.Default` and the sorting of WPF and WinUI collection views use. It compares as the generic <xref:Singulink.Globalization.MonetaryValue.CompareTo*> does, and `null` sorts first.

## Other Helpers

- <xref:Singulink.Globalization.MonetaryValue.Abs*> returns the value with a non-negative amount.
- <xref:Singulink.Globalization.MonetaryValue.Round*> and <xref:Singulink.Globalization.MonetaryValue.RoundToCash*> round to the currency's policies. See [Rounding and Allocation](rounding-and-allocation.md).
- <xref:Singulink.Globalization.MonetaryValue.Allocate*> and <xref:Singulink.Globalization.MonetaryValue.AllocateToCash*> split a value into parts without losing minor units.
- <xref:Singulink.Globalization.MonetaryValue.ToString*>, <xref:Singulink.Globalization.MonetaryValue.TryFormat*>, <xref:Singulink.Globalization.MonetaryValue.Parse*> and <xref:Singulink.Globalization.MonetaryValue.TryParse*> convert to and from strings. See [Formatting](formatting.md) and [Parsing](parsing.md).

### Deconstruction

A value deconstructs into its amount and its currency. The currency is `null` for the default value, as <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault> is:

```csharp
var (amount, currency) = new MonetaryValue(12.3m, "USD");   // 12.3 and the USD currency
```

### Generic math

On .NET, the type implements the <xref:System.Numerics> operator interfaces, so generic code constrained on those interfaces can use it. They cover addition, subtraction, multiplication and division by <xref:System.Decimal>, comparison, negation, increment and decrement. It also implements <xref:System.Numerics.IAdditiveIdentity`2>, whose identity is <xref:Singulink.Globalization.MonetaryValue.Default>, so a generic sum that starts from the additive identity works with values in any currency. It does not implement <xref:System.Numerics.INumber`1>, because concepts such as `One`, or parsing without a currency, do not apply to money.

## Next Steps

Continue with these related articles:

- [Rounding and Allocation](rounding-and-allocation.md) - Rounding policies, cash rounding and allocation.
- [Formatting](formatting.md) - The format string grammar and culture behavior.
- [Money Bags](money-bags.md) - Collections of values in several currencies.
- [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md) - Why the currency instance, not just the code, determines equality.

</div>
