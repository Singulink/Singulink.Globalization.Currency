using System.Text.Json;

namespace Singulink.Globalization.Tests.CurrencyJsonConverterTests;

[PrefixTestClass]
public class ReadWrite
{
    private static readonly Currency TestUsd = new("USD", "Test Dollar");
    private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd]);

    [TestMethod]
    public void Currency_RoundTripsAsCode()
    {
        var usd = Currency.GetCurrency("USD");

        string json = JsonSerializer.Serialize(usd);
        json.ShouldBe("\"USD\"");
        JsonSerializer.Deserialize<Currency>(json).ShouldBeSameAs(usd);
    }

    [TestMethod]
    public void Null_RoundTrips()
    {
        JsonSerializer.Serialize<Currency?>(null).ShouldBe("null");
        JsonSerializer.Deserialize<Currency>("null").ShouldBeNull();
    }

    [TestMethod]
    public void Property_RoundTrips()
    {
        var account = new Account { Currency = Currency.GetCurrency("EUR"), Fallback = null };

        string json = JsonSerializer.Serialize(account);
        json.ShouldBe("""{"Currency":"EUR","Fallback":null}""");

        var result = JsonSerializer.Deserialize<Account>(json)!;
        result.Currency.ShouldBeSameAs(account.Currency);
        result.Fallback.ShouldBeNull();
    }

    [TestMethod]
    public void OptionsConverter_ReadsFromItsRegistry_OverAttribute()
    {
        var options = new JsonSerializerOptions { Converters = { new CurrencyJsonConverter(TestRegistry) } };

        JsonSerializer.Deserialize<Currency>("\"USD\"", options).ShouldBeSameAs(TestUsd);
        JsonSerializer.Deserialize<Account>("""{"Currency":"usd"}""", options)!.Currency.ShouldBeSameAs(TestUsd);
    }

    [TestMethod]
    public void ParameterlessConstructor_ReadsFromDefaultRegistry()
    {
        var options = new JsonSerializerOptions { Converters = { new CurrencyJsonConverter() } };

        JsonSerializer.Deserialize<Currency>("\"USD\"", options).ShouldBeSameAs(CurrencyRegistry.Default["USD"]);
    }

    [TestMethod]
    public void Write_DoesNotUseRegistry()
    {
        var options = new JsonSerializerOptions { Converters = { new CurrencyJsonConverter(TestRegistry) } };

        JsonSerializer.Serialize(Currency.GetCurrency("EUR"), options).ShouldBe("\"EUR\"");
        JsonSerializer.Serialize(Common.CurrencyX).ShouldBe("\"XXXX\"");
    }

    [TestMethod]
    public void CodeNotInRegistry_ThrowsWithPath()
    {
        var options = new JsonSerializerOptions { Converters = { new CurrencyJsonConverter(TestRegistry) } };

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Account>("""{"Currency":"EUR"}""", options));
        ex.Message.ShouldBe("Currency code 'EUR' was not found in the 'Test' currency registry.");
        ex.Path.ShouldBe("$.Currency");
    }

    [TestMethod]
    public void LongUnknownCode_Throws()
    {
        string code = new('Q', 200);

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Currency>($"\"{code}\""));
        ex.Message.ShouldContain($"'{code}'");
    }

    [TestMethod]
    [DataRow("840")]
    [DataRow("true")]
    [DataRow("""{"code":"USD"}""")]
    [DataRow("""["USD"]""")]
    public void NotAString_ThrowsJsonException(string json)
    {
        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Currency>(json));
        ex.Message.ShouldContain("Expected a currency code string");
    }

    public class Account
    {
        public Currency? Currency { get; set; }

        public Currency? Fallback { get; set; }
    }
}
