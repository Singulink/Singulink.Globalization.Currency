using System.Text.Json;
using System.Text.Json.Serialization;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public partial class SourceGeneration
{
    private static readonly Currency TestUsd = new("USD", "Test Dollar");
    private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd]);

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

        var result = JsonSerializer.Deserialize<Invoice>("""{"Total":{"amount":1,"currency":"USD"}}""", options)!;
        result.Total.Currency.ShouldBeSameAs(TestUsd);
    }

    public class Invoice
    {
        public MonetaryValue Total { get; set; }

        public MonetaryValue? Discount { get; set; }

        public Currency? Currency { get; set; }

        public Dictionary<Currency, decimal>? Rates { get; set; }
    }

    [JsonSerializable(typeof(Invoice))]
    internal partial class InvoiceContext : JsonSerializerContext;
}
