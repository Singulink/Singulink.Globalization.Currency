<div class="article">

# Comparison with NodaMoney

NodaMoney and Singulink.Globalization.Currency both give .NET a money type that carries its currency. They make different choices about rounding, defaults and global state, and this page shows those choices side by side so that you can pick the library that fits your application.

Most of the differences follow from two rules that this library keeps:

- **An operation that can lose information is a choice that your code makes, at the place where it happens.** Rounding an amount, dropping the currency of a zero, and writing a symbol that several currencies share are each something you ask for. None of them happens by default.
- **Nothing global can change under running code.** The one piece of global state, the default registry, is chosen at startup and is fixed from its first use: an attempt to change it afterwards throws. So code that misbehaves, in your application or in a library that it uses, cannot change how money behaves in the rest of the process, where such a change would be hard to trace.

The page compares NodaMoney 2.11.0 with Singulink.Globalization.Currency 2.0.0, as of November 2026. See [About the Examples](#about-the-examples) for how the examples were run.

### Two Libraries

NodaMoney provides a `Money` type, ISO 4217 currency data built into the library, and a `MoneyContext` that sets rounding for the values created in it. Its README says that it is inspired by the Java library Joda-Money. This library provides <xref:Singulink.Globalization.MonetaryValue>, currencies that come from a registry of your choice (the operating system's data, the CLDR package or your own provider), and money bags for amounts in several currencies. Both libraries are maintained and released regularly.

## Differences at a Glance

How the two libraries behave:

| Topic | NodaMoney | This library |
|-------|-----------|--------------|
| [When amounts are rounded](#when-amounts-are-rounded) | When created and after every operation | When you call <xref:Singulink.Globalization.MonetaryValue.Round*> |
| [Values without a currency](#values-without-a-currency) | Takes the current culture's currency | You always state the currency |
| [Default text output](#default-text-output) | The culture's symbol, such as `$` | The currency code, such as `CAD` |
| [Formatting options](#formatting-options) | Compact, name and international-symbol formats | Parts that combine, rounding in the format |
| [Round-trip format](#round-trip-format-across-cultures) | `R`, which follows the culture | `O`, which ignores the culture |
| [Splitting an amount](#splitting-an-amount) | The last share takes the remainder | Shares differ by at most one minor unit |
| [Zero in another currency](#zero-in-another-currency) | Accepted; the result takes the other currency | Throws; one zero without a currency |
| [Increment and decrement](#increment-and-decrement) | One unit of the last decimal place | Adds or subtracts 1 |
| [Rounding settings](#process-wide-rounding-context) | A context that any code can change | An argument of each call |
| [Custom currencies](#custom-currencies) | One list for the process | One set for each registry |

What each library includes ([details](#what-each-library-includes)):

| Topic | NodaMoney | This library |
|-------|-----------|--------------|
| Currency data | Built in, the same everywhere | The operating system's, or the CLDR package, which follows each CLDR release |
| Several currencies in one collection | Not available | Four money bag types |
| Cash rounding | Not available | `RoundToCash` and `AllocateToCash`, with the CLDR package |
| Exchange rates | An `ExchangeRate` type | A multiplication and a call to <xref:Singulink.Globalization.MonetaryValue.Round*> |
| Serialization | System.Text.Json, Newtonsoft.Json, XML, `TypeConverter` | System.Text.Json, and a `TypeConverter` that Newtonsoft.Json and configuration use |
| Money types | `Money`, and `FastMoney` for bulk arithmetic | One type, which is faster than `Money` ([measurements](#fastmoney-and-performance)) |

The choice comes down to who decides when an amount is rounded and which currency a value has. NodaMoney decides both for you: a `Money` is always rounded to the currency, and it takes the culture's currency when you give none. This library leaves both to your code: it rounds only where the code says so, takes no currency from the culture and has no setting that running code can change. So you can read the currency and the rounding of each value in the code that produces it, whether the application rounds every amount or calculates with prices, rates and shares finer than the currency.

Each library also has things that the other does not. NodaMoney has support for `XmlSerializer`, formats for compact amounts and English currency names, and `FastMoney`, a second and smaller money type for bulk arithmetic. This library has money bags for amounts in several currencies and, with its CLDR package, cash rounding and symbols and names for each culture.

## When Amounts Are Rounded

NodaMoney rounds every value to the currency's minor unit when it is created and after every operation, so a `Money` is always an amount that can be paid. This library keeps the full amount until you call <xref:Singulink.Globalization.MonetaryValue.Round*>:

```csharp
// NodaMoney
new Money(0.125m, "USD");                              // USD 0.12

// Singulink.Globalization.Currency
new MonetaryValue(0.125m, "USD");                      // USD 0.125
new MonetaryValue(0.125m, "USD").Round();              // USD 0.12
```

So the two libraries keep an amount that is finer than the currency in different places. Unit prices, exchange rates and interest rates are often such amounts. With NodaMoney, the amount stays a `decimal` and the result becomes a `Money`. In this library, the amount is a monetary value that keeps its currency:

```csharp
// NodaMoney: the unit price is a decimal, and the total is a Money
decimal unitPrice = 0.125m;
new Money(unitPrice * 1001, "USD");                    // USD 125.12

// Singulink.Globalization.Currency: the unit price is a monetary value
var unitPrice = new MonetaryValue(0.125m, "USD");      // USD 0.125
(unitPrice * 1001).Round();                            // USD 125.12
```

Both give the same total. With NodaMoney, the steps before the total are plain numbers, with no currency to check. In this library they carry their currency, so adding a unit price in USD to one in EUR throws.

This library treats rounding as a business decision. A unit price, a tax and an exchange rate are each rounded at their own point, sometimes with their own mode, so the library rounds only where your code says so. The trade-off is that a value that you forget to round is stored or shown unrounded.

> [!NOTE]
> NodaMoney documents this rounding as its default, and its maintainer gives the reason in issue [#27](https://github.com/RemyDuijkeren/NodaMoney/issues/27): "Money is based on a real agreed system and can't represent values out of its range." A `MoneyContext` with `NoRounding` keeps the full amount in a `Money`, but its values can't be added to values from the default context.

## Values Without a Currency

NodaMoney can create a value without a currency, and the value takes the currency of the current culture. Parsing does the same for text without a currency, and for a symbol such as `$` that several currencies share. The currency comes from the current culture, not from the provider that you pass:

```csharp
// NodaMoney: the same line in three cultures
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
new Money(6.54m);                                              // USD 6.54
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("nl-NL");
new Money(6.54m);                                              // EUR 6.54
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
new Money(6.54m);                                              // JPY 7

// NodaMoney, with the current culture set to en-US
Money.Parse("10", CultureInfo.GetCultureInfo("ja-JP"));        // USD 10
Money.Parse("$10", CultureInfo.GetCultureInfo("en-CA"));       // USD 10

// Singulink.Globalization.Currency: a constructor always takes a currency, and text must name one
// throws FormatException: "The input string '10.50' does not contain a currency code."
MonetaryValue.Parse("10.50");

// CAD 10: a symbol is read only when you allow it, with the culture that you pass
MonetaryValue.Parse("$10", MonetaryStyles.LocalSymbol, CultureInfo.GetCultureInfo("en-CA"));
```

The same line of code gives a different currency, and in `ja-JP` a different amount, depending on the culture of the thread that runs it. On a server, that is the culture of the machine or of the request. A currency taken from the culture is an input that the code does not show, so in this library a constructor always takes a currency, and text that you parse must name its currency unless you allow a symbol.

> [!NOTE]
> NodaMoney documents this: "Implicit Currency based on current culture/region. When culture is 'NL-nl' code below results in Euros." Setting `MoneyContextOptions.DefaultCurrency` replaces the culture's currency. With `EUR` as the default, `new Money(6.54m)` gives `EUR 6.54` in `ja-JP`.

## Default Text Output

NodaMoney's `ToString()` writes the culture's currency symbol, so currencies that share a symbol write the same text. This library's default format writes the currency code:

```csharp
// NodaMoney
new Money(1234.5m, "CAD").ToString();                              // "$1,234.50"
new Money(1234.5m, "USD").ToString();                              // "$1,234.50"
Money.Parse(new Money(12.34m, "CAD").ToString());                  // USD 12.34

// Singulink.Globalization.Currency
new MonetaryValue(1234.5m, "CAD").ToString();                      // "CAD 1,234.50"
MonetaryValue.Parse(new MonetaryValue(12.34m, "CAD").ToString());  // CAD 12.34

// Singulink.Globalization.Currency, with a symbol format that you choose
new MonetaryValue(1234.5m, "CAD").ToString("C");                   // "$1,234.50" with system data
var symbolText = new MonetaryValue(12.34m, "CAD").ToString("C");
MonetaryValue.Parse(symbolText, MonetaryStyles.Any);               // USD 12.34
```

In a log, an email or an export, the text does not show which of those currencies it was, and reading it back changes the currency without an error. This library writes the currency code by default, because the code names the currency for every reader.

A symbol is something you ask for, knowing that several currencies share it. The last lines show that this library's symbol formats have the same ambiguity. See [Formatting](../guides/formatting.md) for this library's formats.

> [!NOTE]
> NodaMoney's README lists `C` as the default format. Its `G` format writes the code: `ToString("G")` gives `"CAD 12.34"`, which `Money.Parse` reads back as `CAD 12.34`.

## Formatting Options

Each library has formats that the other lacks. A NodaMoney format is one letter with an optional precision digit. A format in this library has up to three parts that combine: the currency, the number and the decimals.

Each cell gives the format and what it wrote for USD 1,234.50 in `en-US` with system data, unless the cell says otherwise:

| What you want to write | NodaMoney | This library |
|------------------------|-----------|--------------|
| The code, placed for each culture | `G`: `USD 1,234.50`, and `USD 1.234,50` in `nl-NL`, where the culture puts its symbol | `G`: `USD 1,234.50`, and `1.234,50 USD` in `nl-NL`. The placement follows each language's convention for codes, as in the EU's [Interinstitutional Style Guide](https://style-guide.europa.eu/en/content/-/isg/topic?identifier=7.3.3-rules-for-expressing-monetary-units), which can differ from the symbol placement |
| The code before or after the amount in every culture | Not available; `R` writes the code first, with no group separators: `USD 1234.5` | `I`: `USD 1.234,50` in `de-DE`; `R`: `1,234.50 USD` |
| The symbol | `C`: `$1,234.50` | `C`: `$1,234.50` |
| A symbol for each culture | Not available; one symbol per currency: CAD is `1 234,50 $` in `fr-FR` | `C` with CLDR data: CAD is `1 234,50 $CA` in `fr-FR` and `CA$1,234.50` in `en-US` |
| An amount where the currency symbol is the decimal separator, as for the Cape Verdean escudo | Not available; `C` writes CVE as `1 234$56 $` in `pt-CV`, which `Parse` then fails on | `C`: CVE is `1 234$56` in `pt-CV`, which parses back |
| The international symbol in every culture | `I`: `US$1,234.50` | By design: `C` writes the culture's own symbol. With CLDR data, that symbol tells currencies apart where the culture needs it, as `US$1,234.50` in `en-CA` |
| The symbol for the local currency, the code for others | Not available | `L`: `$1,234.50` for USD, `CAD 1,234.50` for CAD |
| The currency's name | `L`: `1,234.50 United States dollar`; the singular English name for every amount and culture | Not available; compose it with <xref:Singulink.Globalization.Currency.GetLocalizedName*>, which gives `dollar des États-Unis` in `fr-FR` with CLDR data |
| A compact amount | `c`, `g`, `i`, `l`: `$2.8K` for 2,765.43. They use the letters K, M, B and T in every culture: `2,8M $` for 2,765,430 in `de-DE` | Not available. Compact notation differs by language, and CLDR defines it for each locale: a million is `M` in English and `Mio.` in German, and Japanese counts in ten thousands (`万`) |
| The amount without a currency | `N2`: `1,234.50`; `F2`: `1234.50` | `V`: `1,234.50`, with the currency's decimals and the culture's currency format; `VD`: `1234.50` |
| No group separators, with a currency | Not available, except in `R` | `D` with any currency part: `$1234.50` with `CD`, `USD 1234.50` with `D` |
| A fixed number of decimals | A precision digit: `C0` gives `$1,235`, and `$3` for 2.50 (rounded away from zero) | `B` (banker's rounding) or `A` (away from zero) with a digit: `B0` gives `USD 1,234`, and `A0` gives `USD 1,235` |
| More decimals than the currency | `C3`: `$1.005` for 1.005 in a `NoRounding` context, and `$1.01` with `C` | The default: `USD 1.005`, and `$1.005` with `C` |
| No decimals for a whole amount only | Not available; `C0` drops the decimals of every amount | `*`: `USD 1,000` for 1,000 and `USD 1,000.50` for 1,000.50 |

In short, NodaMoney has compact amounts, the currency's English name and the international symbol as formats. This library has parts that combine, the rounding mode in the format, `L` for a mix of local and foreign amounts, `*` for whole amounts and, with CLDR data, a symbol for each culture. See [Formatting](../guides/formatting.md) for this library's format strings.

If you would find one of the formats marked Not available in this library's column useful, [file a feature request](https://github.com/Singulink/Singulink.Globalization.Currency/issues).

> [!NOTE]
> NodaMoney's formats need no extra package. The symbols and names for each culture in this table need this library's CLDR package: with system data, CAD is `1 234,50 $` in `fr-FR`, as in NodaMoney.

## Round-Trip Format Across Cultures

NodaMoney's README calls its `R` format "Round-trip", and it follows the culture, as .NET's `R` format for numbers does: its text reads back when you parse it with the culture that wrote it. This library's round-trip format is `O`. Like .NET's `O` format for dates, it ignores the culture:

```csharp
// NodaMoney: write and read with the same culture, and with the invariant culture for text that you store
var text = new Money(2765.43m, "USD").ToString("R", CultureInfo.InvariantCulture);   // "USD 2765.43"
Money.Parse(text, CultureInfo.InvariantCulture);                                     // USD 2765.43
new Money(2765.43m, "USD").ToString("R", CultureInfo.GetCultureInfo("nl-NL"));       // "USD 2765,43"

// Singulink.Globalization.Currency: O, here with the current culture set to nl-NL
var roundTrip = new MonetaryValue(2765.43m, "USD").ToString("O");                    // "USD 2765.43"
MonetaryValue.Parse(roundTrip, CultureInfo.InvariantCulture);                        // USD 2765.43
```

With NodaMoney, the culture is yours to pass on both sides. `ToString("R")` with no provider uses the current culture, so text that is written that way under `nl-NL` and read under `en-US` comes back 100 times larger, without an error. This library's `O` ignores the culture and any provider, so stored text reads back the same wherever it was written. See [Round-Trip Text](../guides/formatting.md#round-trip-text), and [Storing Monetary Values](../guides/storing-values.md) for databases and JSON.

In this library, `R` is a different format: it puts the code after the amount and follows the culture.

> [!NOTE]
> NodaMoney's own JSON converter does not use the culture: with the current culture set to `nl-NL`, it writes `"USD 2765.43"`. The culture matters only when you store text written with `R`.

## Splitting an Amount

NodaMoney's `Split` rounds each share and gives what is left to the last one. This library's <xref:Singulink.Globalization.MonetaryValue.Allocate*> spreads the remainder over the first shares, one minor unit each:

```csharp
// NodaMoney
new Money(1m, "USD").Split(6);              // 0.17, 0.17, 0.17, 0.17, 0.17, 0.15
new Money(0.15m, "USD").Split(9);           // 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, -0.01

// Singulink.Globalization.Currency
new MonetaryValue(1m, "USD").Allocate(6);       // 0.17, 0.17, 0.17, 0.17, 0.16, 0.16
new MonetaryValue(0.15m, "USD").Allocate(9);    // 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.01, 0.01, 0.01
```

With NodaMoney, shares can differ by more than one minor unit, and with small amounts the last share can be negative. This library keeps two rules: the shares of an allocation differ by at most one minor unit, and no share has the opposite sign of the amount. So it spreads the remainder one minor unit at a time. See [Rounding and Allocation](../guides/rounding-and-allocation.md) for this library's allocation, including cash and custom units.

> [!NOTE]
> The shares add up to the original amount in both libraries. For common cases such as `100 / 3`, both give the same amounts in a different order. NodaMoney's repository has a design proposal for a largest-remainder policy.

## Zero in Another Currency

NodaMoney accepts a zero in another currency when adding, and the sum takes the currency of the other value. This library treats a zero in a real currency like any other amount, and has one zero without a currency, <xref:Singulink.Globalization.MonetaryValue.Default>:

```csharp
// NodaMoney
Money.Euro(10) + Money.USDollar(0);                 // EUR 10
Money.Euro(10) + Money.USDollar(5);                 // throws InvalidCurrencyException

var total = Money.USDollar(0);
total += Money.Euro(10);
total += Money.Euro(5);                             // EUR 15

// Singulink.Globalization.Currency
new MonetaryValue(10m, "EUR") + new MonetaryValue(0m, "USD");   // throws ArgumentException
new MonetaryValue(10m, "EUR") + MonetaryValue.Default;          // EUR 10

var other = new MonetaryValue(10m, "EUR");
new MonetaryValue(0m, "USD").ToDefaultIfZero() + other;         // EUR 10
new MonetaryValue(5m, "USD").ToDefaultIfZero() + other;         // throws ArgumentException
```

If a total or an account starts at zero in one currency and the first amount added to it is in another currency, it ends up in that other currency without an error. In this library, a zero in USD is still USD, so adding it to EUR is the same mistake as adding any other USD amount. A total with nothing added yet is <xref:Singulink.Globalization.MonetaryValue.Default>, which takes the currency of the first amount added to it.

Where you do want a zero to lose its currency, call <xref:Singulink.Globalization.MonetaryValue.ToDefaultIfZero>, so that the choice shows in the code. It turns a zero into the default value and leaves any other amount in its currency, so adding such an amount still throws, as the last two lines show. See [Monetary Values](../guides/monetary-values.md#default-value) for this library's default value.

> [!NOTE]
> NodaMoney documents this ("`euro10 + zeroDollar; // doesn't throw when adding zero`"). Setting `MoneyContextOptions.EnforceZeroCurrencyMatching` to `true` makes adding a zero in another currency throw. The option is off by default.

## Increment and Decrement

NodaMoney's `++` and `--` add or subtract one unit of the amount's last decimal place, so the result depends on the decimal's scale. This library adds or subtracts 1, as <xref:System.Decimal> does:

```csharp
// NodaMoney
var a = new Money(10m, "USD");      ++a;            // USD 11
var b = new Money(10.00m, "USD");   ++b;            // USD 10.01
new Money(10m, "USD") == new Money(10.00m, "USD");  // true

// Singulink.Globalization.Currency
var c = new MonetaryValue(10.50m, "USD");   ++c;    // USD 11.50
```

Two equal values give different results, depending on trailing zeros that most code does not control. This library's operators add or subtract 1, so that the result depends on the value and not on how it was written.

> [!NOTE]
> NodaMoney's README describes the operators as "Decrement and Increment by minor unit", with `EUR 765.43` becoming `EUR 765.44`. That holds when the amount's scale equals the currency's minor unit, as in that example. `new Money(10m, "USD")` has an amount of `10`, so `++` gives `USD 11`.

## Process-Wide Rounding Context

In NodaMoney, a `MoneyContext` sets the rounding, and its README sets one at startup, so an application that does only that has one rounding rule throughout. The context is still a setting for the whole process that any code can replace later. Values from two contexts can't be added, though they compare as equal when their amounts are equal. In this library, the rounding mode is an argument of each call to <xref:Singulink.Globalization.MonetaryValue.Round*>:

```csharp
// NodaMoney: code that replaces the context after values exist
var before = new Money(10.005m, "EUR");             // EUR 10.00
MoneyContext.DefaultThreadContext = MoneyContext.Create(o =>
    o.RoundingStrategy = new StandardRounding(MidpointRounding.AwayFromZero));
var after = new Money(10.005m, "EUR");              // EUR 10.01, also on other threads

before + after;                                     // throws MoneyContextMismatchException
before < after;                                     // true
```

A library or a test that sets the context changes how amounts are rounded everywhere, and values created before and after the change can't be combined. This library has no setting that one part of a process can change for all of it: the rounding mode is an argument of each call, so any two values in the same currency can be combined.

> [!NOTE]
> NodaMoney's README shows how to set the context once at startup, with `MoneyContext.CreateAndSetDefault` or with its dependency injection package. It documents the mismatch exception and shows how to align contexts with `with { Context = ... }`. Scopes created with `MoneyContext.CreateScope` flow into tasks.

This library has a similar identity rule: a currency belongs to its registry, so the system data's `USD` and the CLDR data's `USD` are two currencies, and adding their values throws. Their values also compare as not equal. The default registry can be set only once, before it is first used. See [Why Two USD Currencies Are Not Equal](currency-identity.md).

## Custom Currencies

NodaMoney registers custom currencies in one list for the process. This library puts currencies in registries, and each registry has its own set:

```csharp
// NodaMoney
var points = CurrencyInfo.Create("PTS");
CurrencyInfo.Register(points with { EnglishName = "Loyalty points", MinorUnit = MinorUnit.Zero });
// throws InvalidCurrencyException: "The currency PTS is already registered."
CurrencyInfo.Register(points with { EnglishName = "Game points" });

var oldEuro = CurrencyInfo.Unregister("EUR");
CurrencyInfo.Register(oldEuro with { Symbol = "EU", MinorUnit = MinorUnit.Zero });
// "EU2", for every EUR in the process
new Money(1.50m, "EUR").ToString(CultureInfo.GetCultureInfo("en-US"));

// Singulink.Globalization.Currency
var loyalty = new Currency("PTS", "Loyalty points") { RoundingPolicy = new RoundingPolicy(0) };
var game = new Currency("PTS", "Game points");

var loyaltyRegistry = new CurrencyRegistry("Loyalty", [loyalty]);
var gameRegistry = new CurrencyRegistry("Game", [game]);

loyaltyRegistry["PTS"].Name;                                         // "Loyalty points"
gameRegistry["PTS"].Name;                                            // "Game points"
new MonetaryValue(1m, loyalty) + new MonetaryValue(1m, game);        // throws ArgumentException
```

With one list, two components or two tests can't each define their own currency with the same code, and a change to a currency applies to all code in the process, including code that did not make the change. In this library, a registry is an object that you create, so two components or two tests can each have their own currencies with the same codes. See [Currencies and Registries](../guides/currencies-and-registries.md) for this library's registries and data providers.

> [!NOTE]
> NodaMoney documents registration as lasting "for the life-time of the app domain", and one list is simpler for an application that has one set of currencies.

## What Each Library Includes

The libraries also differ in what they include, and some of these differences can decide the choice for you.

#### Currency data

NodaMoney ships one fixed set of ISO 4217 data inside the library. The data changes only when you update the library, and it is the same on every operating system. This library lets you choose: the operating system's data, which updates with the system, or the [CLDR package](../guides/cldr-data.md), which is the same everywhere and can be updated without updating the library.

A scheduled build checks for a new CLDR release every week and publishes a new version of the package automatically, numbered after the CLDR release. Updating the package is all it takes to keep the data current.

System data follows the operating system by design. For example, the Chilean peso (`CLP`) has 0 decimal places with ICU and 2 with the older Windows data, while NodaMoney gives it 0 everywhere. Register the CLDR package if you want the same data on every machine. You also need it when the app runs with [invariant globalization](../guides/cldr-data.md#invariant-globalization-mode), where the system data is empty and NodaMoney's built-in data still works.

#### Money bags

This library has four money bag types, which hold amounts in several currencies at once, such as the balances of a wallet or a total over several currencies. NodaMoney has no such type. See [Money Bags](../guides/money-bags.md).

#### Cash rounding

This library rounds and allocates to the coins that exist: `new MonetaryValue(10.03m, "CAD").RoundToCash()` gives `CAD 10.05`. The cash rules need the CLDR package. NodaMoney 2.11.0 has no cash rounding, though you can write a rounding strategy of your own for a context. See [Rounding and Allocation](../guides/rounding-and-allocation.md#cash-rounding).

#### Exchange rates

NodaMoney has an `ExchangeRate` type: `new ExchangeRate("EUR", "USD", 1.2591).Convert(Money.Euro(100.99m))` gives `USD 127.16`. This library has no exchange rate type, on purpose. Where a rate comes from, how precise it is and where the result is rounded differ between applications, and a multiplication followed by a call to <xref:Singulink.Globalization.MonetaryValue.Round*> states each of them in your code. See [Converting Between Currencies](../guides/monetary-values.md#converting-between-currencies).

#### Serialization

NodaMoney supports System.Text.Json, Newtonsoft.Json, `XmlSerializer` and a `TypeConverter`. This library has System.Text.Json converters, and a `TypeConverter` through which Newtonsoft.Json and configuration binding work. With Newtonsoft.Json, both libraries write `"USD 12.30"`. This library does not support `XmlSerializer`. See [Storing Monetary Values](../guides/storing-values.md).

#### FastMoney and performance

NodaMoney has two money types. `Money` holds a `decimal` and rounds it to the currency after every operation. `FastMoney` holds a 64-bit integer with four fixed decimal places, as SQL Server's `money` type does. That makes it smaller and faster at adding, and limits it to four decimals and to amounts up to about 922 trillion.

This library has one type. A <xref:Singulink.Globalization.MonetaryValue> holds a `decimal` and does not round until you ask, so its arithmetic costs little more than the `decimal`'s own.

Times are in nanoseconds for one operation on one value, and the last row is the size of a value:

| Operation | `decimal` | NodaMoney `Money` | NodaMoney `FastMoney` | `MonetaryValue` |
|-----------|-----------|-------------------|-----------------------|-----------------|
| Create from a `decimal` | | 6.1 | 5.2 | 1.4 |
| Add two values | 4.4 | 10.7 | 2.0 | 6.0 |
| Total of 1,000 values, for each value | 2.4 | 15.1 | 2.3 | 7.6 |
| Multiply by a rate | 4.1 | 13.5 | 12.6 | 8.4, or 11.4 with `Round()` |
| Divide by 3 | 11.8 | 25.1 | 7.9 | 16.3, or 23.2 with `Round()` |
| Compare two values | 1.2 | 2.5 | 0.9 | 1.2 |
| Size of a value | 16 bytes | 16 bytes | 12 bytes | 24 bytes |

`MonetaryValue` is faster than `Money` in every row, also when it rounds each result as `Money` does. `FastMoney` is the smallest type and the fastest at adding, dividing and comparing, so it suits code that keeps millions of amounts in memory and totals them, where four decimals are enough. `MonetaryValue` is faster to create and to multiply, and it is the largest of the three.

In most applications these differences do not show. One database query or network request takes many thousands of times longer than any operation in the table.

> [!NOTE]
> Measured with BenchmarkDotNet 0.16.0-preview.2 on .NET 11.0.0 and Windows 11 with an AMD Ryzen 7 9700X, with NodaMoney in its default context. The benchmark project is in this library's repository, in `Tools/Benchmarks`.

#### Minor units

Both libraries convert to and from minor units: `Money.FromMinorUnits(1234, CurrencyInfo.FromCode("USD"))` and `MonetaryValue.FromMinorUnits(1234, "USD")` give `USD 12.34`, and `ToMinorUnits()` gives `1234` in both.

#### Compact amounts and names

NodaMoney has formats for compact amounts, such as `$2.8K`, and for the currency's English name. This library has neither as a format: compact notation differs by language, and you can compose text with the localized name yourself. See [Formatting Options](#formatting-options). If you would find either useful, [file a feature request](https://github.com/Singulink/Singulink.Globalization.Currency/issues).

## About the Examples

NodaMoney's repository has design proposals that would change some of these items, such as splitting by largest remainder, cash rounding and CLDR-based formatting, so later versions may differ.

Every example ran with the current culture set to `en-US` unless it says otherwise. A comment such as `// USD 0.12` gives the currency code and the amount of the result. Text in quotes is the string that was returned, with ordinary spaces where the library wrote a no-break space.

## Further Reading

These articles cover this library's side of the comparison in more depth:

- [Getting Started](../guides/getting-started.md) - The core types in a few minutes.
- [Rounding and Allocation](../guides/rounding-and-allocation.md) - When and how amounts are rounded and split.
- [Formatting](../guides/formatting.md) - The format strings and what they write.
- [Why Two USD Currencies Are Not Equal](currency-identity.md) - How currencies and registries relate.

</div>
