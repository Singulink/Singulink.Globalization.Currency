using System.Globalization;
using System.Text.Json;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public class RoundTrip
{
    [TestMethod]
    [DataRow("0", "USD", "0.00")]
    [DataRow("0.00", "USD", "0.00")]
    [DataRow("-12.34", "USD", "-12.34")]
    [DataRow("12.30", "EUR", "12.30")]
    [DataRow("12.3000000", "EUR", "12.30")]
    [DataRow("12.345", "EUR", "12.345")]
    [DataRow("1234567890123456789.12", "JPY", "1234567890123456789.12")]
    [DataRow("1250.00", "JPY", "1250")]
    [DataRow("1.2", "BHD", "1.200")]
    [DataRow("79228162514264337593543950335", "USD", "79228162514264337593543950335.00")]
    [DataRow("-79228162514264337593543950335", "USD", "-79228162514264337593543950335.00")]
    [DataRow("0.0000000000000000000000000001", "USD", "0.0000000000000000000000000001")]
    [DataRow("3.1415926535897932384626433833", "CAD", "3.1415926535897932384626433833")]
    public void Value_RoundTripsAsEqualValue(string amountText, string currencyCode, string writtenAmount)
    {
        // The amount is written with the currency's decimal digits, or more when the amount needs them. Some of these amounts have more than 15 significant
        // digits, so they are written with LossyNumberObject, which this library reads back with every digit.

        decimal amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);
        var value = new MonetaryValue(amount, currencyCode);
        var options = new JsonSerializerOptions { Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.LossyNumberObject) } };

        string json = JsonSerializer.Serialize(value, options);
        json.ShouldBe($$"""{"amount":{{writtenAmount}},"currency":"{{currencyCode}}"}""");

        var result = JsonSerializer.Deserialize<MonetaryValue>(json);
        result.ShouldBe(value);
        result.Currency.ShouldBeSameAs(Currency.GetCurrency(currencyCode));
    }

    [TestMethod]
    public void Default_WritesZeroWithNullCurrency()
    {
        JsonSerializer.Serialize(MonetaryValue.Default).ShouldBe("""{"amount":0,"currency":null}""");
        JsonSerializer.Deserialize<MonetaryValue>("""{"amount":0,"currency":null}""").IsDefault.ShouldBeTrue();
    }

    [TestMethod]
    public void Null_IntoNonNullableValue_Throws()
    {
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("null"))
            .Message.ShouldBe("Expected a monetary value object or a string but found a token of type 'Null'.");
    }

    [TestMethod]
    public void Nullable_RoundTrips()
    {
        MonetaryValue? value = new MonetaryValue(5.50m, "USD");

        string json = JsonSerializer.Serialize(value);
        json.ShouldBe("""{"amount":5.50,"currency":"USD"}""");
        JsonSerializer.Deserialize<MonetaryValue?>(json).ShouldBe(value);
    }

    [TestMethod]
    public void NullableNull_RoundTripsAsNull()
    {
        MonetaryValue? value = null;

        string json = JsonSerializer.Serialize(value);
        json.ShouldBe("null");
        JsonSerializer.Deserialize<MonetaryValue?>(json).ShouldBeNull();
    }

    [TestMethod]
    public void NullableDefault_ReadsBackAsDefault()
    {
        // A nullable value that holds the default value is distinct from null.

        MonetaryValue? value = MonetaryValue.Default;

        string json = JsonSerializer.Serialize(value);
        json.ShouldBe("""{"amount":0,"currency":null}""");

        var result = JsonSerializer.Deserialize<MonetaryValue?>(json);
        result.ShouldNotBeNull();
        result.Value.IsDefault.ShouldBeTrue();
    }

    [TestMethod]
    public void Property_RoundTrips()
    {
        var order = new Order { Price = new MonetaryValue(19.99m, "USD"), Discount = null, Tip = MonetaryValue.Default };

        string json = JsonSerializer.Serialize(order);
        json.ShouldBe("""{"Price":{"amount":19.99,"currency":"USD"},"Discount":null,"Tip":{"amount":0,"currency":null}}""");

        var result = JsonSerializer.Deserialize<Order>(json)!;
        result.Price.ShouldBe(order.Price);
        result.Discount.ShouldBeNull();
        result.Tip.IsDefault.ShouldBeTrue();
    }

    [TestMethod]
    public void Array_RoundTrips()
    {
        MonetaryValue[] values = [new(1m, "USD"), MonetaryValue.Default, new(-2.5m, "EUR")];

        string json = JsonSerializer.Serialize(values);
        json.ShouldBe("""[{"amount":1.00,"currency":"USD"},{"amount":0,"currency":null},{"amount":-2.50,"currency":"EUR"}]""");
        JsonSerializer.Deserialize<MonetaryValue[]>(json).ShouldBe(values);
    }

    public class Order
    {
        public MonetaryValue Price { get; set; }

        public MonetaryValue? Discount { get; set; }

        public MonetaryValue Tip { get; set; }
    }
}
