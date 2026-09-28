<div class="article">

# Rounding and Allocation

Monetary amounts keep full precision until you decide otherwise. This guide covers the rounding rules attached to currencies, the difference between standard and cash rounding, and how to split a value into parts without losing or inventing minor units.

### Rounding policies

A <xref:Singulink.Globalization.RoundingPolicy> describes the smallest amount that is valid for some purpose. It has two parts: <xref:Singulink.Globalization.RoundingPolicy.DecimalDigits>, the number of decimal places, and <xref:Singulink.Globalization.RoundingPolicy.DecimalUnits>, an optional increment measured in units of the last decimal digit. Some examples:

| Policy | Smallest unit | Used for |
| --- | --- | --- |
| `new RoundingPolicy(2)` | 0.01 | Most currencies |
| `new RoundingPolicy(0)` | 1 | Yen, won, cash in Sweden |
| `new RoundingPolicy(3)` | 0.001 | Bahraini and Kuwaiti dinar |
| `new RoundingPolicy(2, 5)` | 0.05 | Cash in Canada and Switzerland |
| `new RoundingPolicy(2, 50)` | 0.50 | Cash in Denmark |
| `new RoundingPolicy(0, 5)` | 5 | Cash in Hungary |

<xref:Singulink.Globalization.RoundingPolicy.SmallestUnitAmount> returns the unit as a <xref:System.Decimal>, and <xref:Singulink.Globalization.RoundingPolicy.RoundAmount*> rounds any amount to the policy. Policies are immutable and compare by value.

Every <xref:Singulink.Globalization.Currency> carries two policies. <xref:Singulink.Globalization.Currency.RoundingPolicy> applies to monetary amounts in general and always exists; it defaults to two decimal digits for custom currencies. <xref:Singulink.Globalization.Currency.CashRoundingPolicy> applies to physical cash and is `null` when the data source has no information about it.

## Rounding Values

<xref:Singulink.Globalization.MonetaryValue.Round*> rounds to the currency's standard policy and <xref:Singulink.Globalization.MonetaryValue.RoundToCash*> to its cash policy. Both default to banker's rounding (<xref:System.MidpointRounding.ToEven>), which avoids systematic bias when many midpoint values are rounded, and both accept a <xref:System.MidpointRounding> mode:

```csharp
var subtotal = new MonetaryValue(59.97m, "USD");
var withTax = subtotal * 1.13m;                       // USD 67.7661

withTax.Round();                                      // USD 67.77
new MonetaryValue(10.005m, "USD").Round();            // USD 10.00 (to even)
new MonetaryValue(10.005m, "USD").Round(MidpointRounding.AwayFromZero); // USD 10.01
```

Money bags have the same pair of methods, <xref:Singulink.Globalization.IMoneyBag.Round*> and <xref:Singulink.Globalization.IMoneyBag.RoundToCash*>, which round every value in the bag according to its own currency.

### Cash rounding

Several countries have withdrawn their smallest coins while keeping the currency's decimal digits. Canada has no penny, so a cash transaction of 10.03 CAD is settled as 10.05, while a card transaction is still 10.03. Sweden rounds cash to whole kronor, Denmark to 50 øre, Hungary to 5 forints. Cash rounding is the rule for the physical payment, and standard rounding is the rule for the amount owed.

```csharp
var amount = new MonetaryValue(10.03m, "CAD");

amount.Round();        // CAD 10.03
amount.RoundToCash();  // CAD 10.05
```

Cash rounding rules are not part of the globalization data that ships with .NET, so currencies in the system registry have a `null` <xref:Singulink.Globalization.Currency.CashRoundingPolicy>, and <xref:Singulink.Globalization.MonetaryValue.RoundToCash*> throws <xref:System.NotSupportedException> for them. The [CLDR Currency Data](cldr-data.md) package provides cash rules for every currency, and custom currencies can set the policy directly.

> [!NOTE]
> Only a small number of currencies have cash rules that differ from their standard rules. For all the others, cash rounding is identical to standard rounding, and a data provider that knows the rules reports it that way rather than as unknown.

## Allocation

Splitting an amount is the classic place where money arithmetic goes wrong. 100.00 divided three ways is 33.333..., which is not a valid amount, and rounding each part to 33.33 loses a cent. <xref:Singulink.Globalization.MonetaryValue.Allocate*> solves this by rounding each part down to the smallest unit and then handing out the leftover units, so the parts always sum to the original:

```csharp
var total = new MonetaryValue(100m, "USD");

total.Allocate(3);             // USD 33.34, USD 33.33, USD 33.33
total.Allocate(50, 30, 20);    // USD 50.00, USD 30.00, USD 20.00
total.Allocate(1, 2);          // USD 33.33, USD 66.67
```

The single-integer overload allocates equal parts. The ratio overload takes any non-negative weights, which do not need to sum to anything in particular; percentages, fractions and simple counts all work. A zero weight produces a zero part that never receives leftover units. The result is an <xref:System.Collections.Immutable.ImmutableArray`1> with one value per part.

#### How the remainder is distributed

Leftover units go one at a time to the parts that lost the largest fraction when they were rounded down, which is the largest remainder method. When several parts tie, earlier parts win. For equal splits this means the first parts get the extra units, as in the 33.34, 33.33, 33.33 example. For ratio splits it means the distribution is as close to the exact proportions as the smallest unit allows, and the same input always produces the same output.

Negative values are allocated the same way as their absolute value with the parts negated, so a refund splits exactly like the original charge.

#### Amounts that are not representable

An amount with more precision than the policy allows, such as 100.005 USD, cannot be summed to by any set of cents. The parts can only sum to a multiple of the smallest unit, and the multiple closest to the original amount is the amount rounded to the policy, so that is what gets allocated: the total is rounded with banker's rounding first, then split. Round before allocating if a different midpoint rule is needed.

### Cash allocation and custom units

<xref:Singulink.Globalization.MonetaryValue.AllocateToCash*> uses the currency's cash policy so that every part can actually be paid in cash:

```csharp
var bill = new MonetaryValue(100m, "CAD");

bill.Allocate(3);        // CAD 33.34, CAD 33.33, CAD 33.33
bill.AllocateToCash(3);  // CAD 33.35, CAD 33.35, CAD 33.30
```

The overloads that take a <xref:Singulink.Globalization.RoundingPolicy> allocate in any unit, which covers cases like splitting a budget into whole dollars or prices into quarters:

```csharp
total.Allocate(3, new RoundingPolicy(0));       // USD 34, USD 33, USD 33
total.Allocate([1, 3], new RoundingPolicy(0));  // USD 25, USD 75
```

## Next Steps

Continue with these related articles:

- [Monetary Values](monetary-values.md) - Arithmetic, comparison and the default value.
- [CLDR Currency Data](cldr-data.md) - Where cash rounding rules come from.
- [Money Bags](money-bags.md) - Rounding every value in a bag at once.

</div>
