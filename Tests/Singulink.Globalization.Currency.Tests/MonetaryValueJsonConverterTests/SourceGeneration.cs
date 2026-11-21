using System.Text.Json;
using System.Text.Json.Serialization;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public partial class SourceGeneration
{
    private static readonly Currency TestUsd = new("USD", "Test Dollar");
    private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd]);

    [TestMethod]
    public void GeneratedContext_BagsRoundTrip()
    {
        var wallet = new Wallet {
            Cash = [new(1m, "USD"), new(2.50m, "EUR")],
            Savings = [new(3m, "USD"), new(4m, "CAD")],
            Archive = [new(0m, "JPY")],
            Ledger = null,
        };

        string json = JsonSerializer.Serialize(wallet, InvoiceContext.Default.Wallet);
        json.ShouldStartWith("""{"Cash":[{"amount":1.00,"currency":"USD"},{"amount":2.50,"currency":"EUR"}],""");
        json.ShouldEndWith("""],"Savings":[{"amount":4.00,"currency":"CAD"},{"amount":3.00,"currency":"USD"}],"Archive":[{"amount":0,"currency":"JPY"}],"Ledger":null}""");

        var result = JsonSerializer.Deserialize(json, InvoiceContext.Default.Wallet)!;
        result.Cash.ShouldBe(wallet.Cash);
        result.Savings.ShouldBe(wallet.Savings);
        result.Archive.ShouldBe(wallet.Archive);
        result.Ledger.ShouldBeNull();
    }

    [TestMethod]
    public void GeneratedContextWithOptionsFactory_ReadsBagsIntoItsRegistry()
    {
        var options = new JsonSerializerOptions {
            Converters = { new MoneyBagJsonConverterFactory(TestRegistry) },
            TypeInfoResolver = InvoiceContext.Default,
        };

        var result = JsonSerializer.Deserialize<Wallet>("""{"Cash":[{"amount":1.00,"currency":"USD"}],"Ledger":[]}""", options)!;
        result.Cash!.Registry.ShouldBeSameAs(TestRegistry);
        result.Cash["USD"].Currency.ShouldBeSameAs(TestUsd);
        result.Ledger!.Registry.ShouldBeSameAs(TestRegistry);
    }

    [TestMethod]
    public void GeneratedContext_InterfaceBagsRoundTrip()
    {
        var wallet = new InterfaceWallet {
            Cash = new SortedMoneyBag(new MonetaryValue(1m, "USD"), new MonetaryValue(2m, "CAD")),
            Spending = new MoneyBag(new MonetaryValue(3m, "EUR")),
            Archive = null,
        };

        string json = JsonSerializer.Serialize(wallet, InvoiceContext.Default.InterfaceWallet);
        json.ShouldBe("""{"Cash":[{"amount":2.00,"currency":"CAD"},{"amount":1.00,"currency":"USD"}],"Spending":[{"amount":3.00,"currency":"EUR"}],"Archive":null}""");

        var result = JsonSerializer.Deserialize("""{"Cash":[{"amount":1.00,"currency":"USD"}],"Spending":[],"Archive":[{"amount":0,"currency":"JPY"}]}""", InvoiceContext.Default.InterfaceWallet)!;
        result.Cash.ShouldBeOfType<MoneyBag>().Single().ShouldBe(new MonetaryValue(1m, "USD"));
        result.Spending.ShouldBeOfType<MoneyBag>().Count.ShouldBe(0);
        result.Archive.ShouldBeOfType<ImmutableMoneyBag>().Single().ShouldBe(new MonetaryValue(0m, "JPY"));
    }

    [TestMethod]
    public void GeneratedContextWithOptionsFactory_ReadsInterfaceBagsIntoItsRegistry()
    {
        var options = new JsonSerializerOptions {
            Converters = { new MoneyBagJsonConverterFactory(TestRegistry) },
            TypeInfoResolver = InvoiceContext.Default,
        };

        var result = JsonSerializer.Deserialize<InterfaceWallet>("""{"Cash":[{"amount":1.00,"currency":"USD"}],"Archive":[]}""", options)!;
        result.Cash!.Registry.ShouldBeSameAs(TestRegistry);
        result.Cash["USD"].Currency.ShouldBeSameAs(TestUsd);
        result.Archive!.Registry.ShouldBeSameAs(TestRegistry);
    }

    [TestMethod]
    public void GeneratedContext_TextFormNumberHandlingAndKeys()
    {
        var ledger = new Ledger {
            Total = new MonetaryValue(12.3m, "USD"),
            Bag = [new(1m, "EUR"), new(2.5m, "USD")],
            Counts = new() { [new MonetaryValue(5m, "JPY")] = 3 },
        };

        var textOptions = new JsonSerializerOptions {
            Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Text), new MoneyBagJsonConverterFactory(MonetaryJsonFormat.Text) },
            TypeInfoResolver = InvoiceContext.Default,
        };

        string json = JsonSerializer.Serialize(ledger, textOptions);
        json.ShouldBe("""{"Total":"USD 12.30","Bag":["EUR 1.00","USD 2.50"],"Counts":{"JPY 5":3}}""");

        var fromText = JsonSerializer.Deserialize<Ledger>(json, textOptions)!;
        fromText.Total.ShouldBe(ledger.Total);
        fromText.Bag.ShouldBe(ledger.Bag);
        fromText.Counts![new MonetaryValue(5m, "JPY")].ShouldBe(3);

        var numberOptions = new JsonSerializerOptions {
            NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = InvoiceContext.Default,
        };

        json = JsonSerializer.Serialize(ledger, numberOptions);
        json.ShouldBe("""{"Total":{"amount":"12.30","currency":"USD"},"Bag":[{"amount":"1.00","currency":"EUR"},{"amount":"2.50","currency":"USD"}],"Counts":{"JPY 5":"3"}}""");

        var fromNumbers = JsonSerializer.Deserialize<Ledger>(json, numberOptions)!;
        fromNumbers.Total.ShouldBe(ledger.Total);
        fromNumbers.Bag.ShouldBe(ledger.Bag);

        // The default value, in the generated context's own options.
        string defaultJson = JsonSerializer.Serialize(new Ledger(), InvoiceContext.Default.Ledger);
        defaultJson.ShouldBe("""{"Total":{"amount":0,"currency":null},"Bag":null,"Counts":null}""");
        JsonSerializer.Deserialize(defaultJson, InvoiceContext.Default.Ledger)!.Total.IsDefault.ShouldBeTrue();

        // An amount with more than 15 significant digits throws in the default form and is written in full with LossyNumberObject.
        var exact = new Ledger { Total = new MonetaryValue(10m / 3, "USD") };
        Should.Throw<ArgumentException>(() => JsonSerializer.Serialize(exact, InvoiceContext.Default.Ledger));

        var lossyOptions = new JsonSerializerOptions {
            Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.LossyNumberObject) },
            TypeInfoResolver = InvoiceContext.Default,
        };

        string lossyJson = JsonSerializer.Serialize(exact, lossyOptions);
        lossyJson.ShouldStartWith("""{"Total":{"amount":3.3333333333333333333333333333,"currency":"USD"}""");
        JsonSerializer.Deserialize(lossyJson, InvoiceContext.Default.Ledger)!.Total.ShouldBe(exact.Total);

        // The generated context's own options read the text form as well.
        JsonSerializer.Deserialize("""{"Total":"USD 12.3"}""", InvoiceContext.Default.Ledger)!.Total.ShouldBe(ledger.Total);
    }

    [TestMethod]
    public void GeneratedContext_RoundTrips()
    {
        var invoice = new Invoice {
            Total = new MonetaryValue(12.30m, "USD"),
            Discount = null,
            Currency = Currency.GetCurrency("USD"),
            Rates = new() { [Currency.GetCurrency("EUR")] = 0.92m },
        };

        string json = JsonSerializer.Serialize(invoice, InvoiceContext.Default.Invoice);
        json.ShouldBe("""{"Total":{"amount":12.30,"currency":"USD"},"Discount":null,"Currency":"USD","Rates":{"EUR":0.92}}""");

        var result = JsonSerializer.Deserialize(json, InvoiceContext.Default.Invoice)!;
        result.Total.ShouldBe(invoice.Total);
        result.Discount.ShouldBeNull();
        result.Currency.ShouldBeSameAs(invoice.Currency);
        result.Rates![Currency.GetCurrency("EUR")].ShouldBe(0.92m);
    }

    [TestMethod]
    public void GeneratedContextWithOptionsConverter_ReadsFromItsRegistry()
    {
        var options = new JsonSerializerOptions {
            Converters = { new MonetaryValueJsonConverter(TestRegistry) },
            TypeInfoResolver = InvoiceContext.Default,
        };

        var result = JsonSerializer.Deserialize<Invoice>("""{"Total":{"amount":1.00,"currency":"USD"}}""", options)!;
        result.Total.Currency.ShouldBeSameAs(TestUsd);
    }

    public class Invoice
    {
        public MonetaryValue Total { get; set; }

        public MonetaryValue? Discount { get; set; }

        public Currency? Currency { get; set; }

        public Dictionary<Currency, decimal>? Rates { get; set; }
    }

    public class Wallet
    {
        public MoneyBag? Cash { get; set; }

        public SortedMoneyBag? Savings { get; set; }

        public ImmutableMoneyBag? Archive { get; set; }

        public ImmutableSortedMoneyBag? Ledger { get; set; }
    }

    public class InterfaceWallet
    {
        public IReadOnlyMoneyBag? Cash { get; set; }

        public IMoneyBag? Spending { get; set; }

        public IImmutableMoneyBag? Archive { get; set; }
    }

    public class Ledger
    {
        public MonetaryValue Total { get; set; }

        public SortedMoneyBag? Bag { get; set; }

        public Dictionary<MonetaryValue, int>? Counts { get; set; }
    }

    [JsonSerializable(typeof(Invoice))]
    [JsonSerializable(typeof(Wallet))]
    [JsonSerializable(typeof(InterfaceWallet))]
    [JsonSerializable(typeof(Ledger))]
    internal partial class InvoiceContext : JsonSerializerContext;
}
