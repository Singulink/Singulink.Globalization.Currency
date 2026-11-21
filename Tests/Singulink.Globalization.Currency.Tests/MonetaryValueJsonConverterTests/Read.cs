using System.Text.Json;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public class Read
{
    [TestMethod]
    public void PropertiesInAnyOrder_Reads()
    {
        var result = JsonSerializer.Deserialize<MonetaryValue>("""{"currency":"USD","amount":12.34}""");
        result.ShouldBe(new MonetaryValue(12.34m, "USD"));
    }

    [TestMethod]
    public void UnknownProperties_AreSkipped()
    {
        const string json = """{"note":"x","amount":1.5,"extra":{"a":[1,2,{"b":null}]},"currency":"EUR","more":[true]}""";

        JsonSerializer.Deserialize<MonetaryValue>(json).ShouldBe(new MonetaryValue(1.5m, "EUR"));
    }

    [TestMethod]
    public void CurrencyCode_IsCaseInsensitive()
    {
        // Currency codes are looked up as the registry looks them up, which ignores case.

        var result = JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1.00,"currency":"usd"}""");
        result.Currency.ShouldBeSameAs(Currency.GetCurrency("USD"));
    }

    [TestMethod]
    public void EscapedText_Reads()
    {
        var result = JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1.00,"currency":"USD"}""");
        result.ShouldBe(new MonetaryValue(1m, "USD"));
    }

    [TestMethod]
    public void ExponentAmount_Reads()
    {
        JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1.5e2,"currency":"USD"}""").ShouldBe(new MonetaryValue(150m, "USD"));
    }

    [TestMethod]
    [DataRow("""{"currency":"USD"}""", "'amount'", DisplayName = "Missing amount")]
    [DataRow("""{"amount":1}""", "'currency'", DisplayName = "Missing currency")]
    [DataRow("""{}""", "'amount'", DisplayName = "Empty object")]
    [DataRow("""{"amount":"1.00","currency":"USD"}""", "'amount'", DisplayName = "String amount")]
    [DataRow("""{"amount":null,"currency":"USD"}""", "'amount'", DisplayName = "Null amount")]
    [DataRow("""{"amount":1e400,"currency":"USD"}""", "'amount'", DisplayName = "Amount out of range")]
    [DataRow("""{"amount":1,"currency":null}""", "'currency'", DisplayName = "Null currency")]
    [DataRow("""{"amount":1,"currency":840}""", "'currency'", DisplayName = "Number currency")]
    [DataRow("""{"amount":1,"currency":{"code":"USD"}}""", "'currency'", DisplayName = "Object currency")]
    [DataRow("""{"amount":1.00,"currency":"ZZZ"}""", "'ZZZ'", DisplayName = "Unknown currency code")]
    [DataRow("""{"amount":1,"currency":""}""", "''", DisplayName = "Empty currency code")]
    [DataRow("12.34", "Number", DisplayName = "Number")]
    [DataRow("\"$12.34\"", "not a monetary value in the round-trip format", DisplayName = "Culture-formatted string")]
    [DataRow("[12.34,\"USD\"]", "StartArray", DisplayName = "Array")]
    public void MalformedInput_ThrowsJsonException(string json, string messagePart)
    {
        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>(json));
        ex.Message.ShouldContain(messagePart);
        ex.Path.ShouldNotBeNull();
    }

    [TestMethod]
    public void UnknownCurrencyCode_ExceptionHasPath()
    {
        const string json = """{"Price":{"amount":1.00,"currency":"ZZZ"}}""";

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<RoundTrip.Order>(json));
        ex.Message.ShouldStartWith("Currency code 'ZZZ' was not found in the '");
        ex.Path.ShouldBe("$.Price");   // the path ends at the value that the converter reads
        ex.LineNumber.ShouldBe(0);
    }

    [TestMethod]
    public void TruncatedInput_ThrowsJsonException()
    {
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1.00,"currency":"USD" """));
    }
}
