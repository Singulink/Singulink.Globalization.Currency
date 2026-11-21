using System.Text.Json;

namespace Singulink.Globalization.Tests.CurrencyJsonConverterTests;

[PrefixTestClass]
public class DictionaryKeys
{
    private static readonly Currency TestUsd = new("USD", "Test Dollar");
    private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd]);

    [TestMethod]
    public void CurrencyKeys_RoundTrip()
    {
        var usd = Currency.GetCurrency("USD");
        var eur = Currency.GetCurrency("EUR");
        var rates = new Dictionary<Currency, decimal> { [usd] = 1m, [eur] = 0.92m };

        string json = JsonSerializer.Serialize(rates);
        json.ShouldBe("""{"USD":1,"EUR":0.92}""");

        var result = JsonSerializer.Deserialize<Dictionary<Currency, decimal>>(json)!;
        result.Count.ShouldBe(2);
        result[usd].ShouldBe(1m);
        result[eur].ShouldBe(0.92m);
    }

    [TestMethod]
    public void CurrencyKeys_ReadFromOptionsRegistry()
    {
        var options = new JsonSerializerOptions { Converters = { new CurrencyJsonConverter(TestRegistry) } };

        var result = JsonSerializer.Deserialize<Dictionary<Currency, MonetaryValue>>("""{"USD":{"amount":1.00,"currency":"EUR"}}""", options)!;
        result.Keys.Single().ShouldBeSameAs(TestUsd);
        result[TestUsd].Currency.ShouldBeSameAs(Currency.GetCurrency("EUR"));
    }

    [TestMethod]
    public void UnknownKey_ThrowsJsonException()
    {
        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Dictionary<Currency, int>>("""{"USD":1,"ZZZ":2}"""));
        ex.Message.ShouldStartWith("Currency code 'ZZZ' was not found in the '");
    }

    [TestMethod]
    public void MonetaryValueKeys_UseRoundTripText()
    {
        // Monetary value keys are covered in MonetaryValueJsonConverterTests.Forms; this checks that they work next to currency keys.

        var values = new Dictionary<MonetaryValue, int> { [new MonetaryValue(1m, "USD")] = 1 };

        JsonSerializer.Serialize(values).ShouldBe("""{"USD 1.00":1}""");
    }
}
