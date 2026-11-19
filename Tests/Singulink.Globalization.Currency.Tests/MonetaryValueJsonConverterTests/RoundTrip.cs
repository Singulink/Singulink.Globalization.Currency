using System.Globalization;
using System.Text.Json;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public class RoundTrip
{
    [TestMethod]
    [DataRow("0", "USD")]
    [DataRow("0.00", "USD")]
    [DataRow("-12.34", "USD")]
    [DataRow("12.30", "EUR")]
    [DataRow("12.3000000", "EUR")]
    [DataRow("1234567890123456789.12", "JPY")]
    [DataRow("79228162514264337593543950335", "USD")]
    [DataRow("-79228162514264337593543950335", "USD")]
    [DataRow("0.0000000000000000000000000001", "USD")]
    [DataRow("3.1415926535897932384626433833", "CAD")]
    public void Value_RoundTripsExactlyWithScale(string amountText, string currencyCode)
    {
        decimal amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);
        var value = new MonetaryValue(amount, currencyCode);

        string json = JsonSerializer.Serialize(value);
        json.ShouldBe($$"""{"amount":{{amountText}},"currency":"{{currencyCode}}"}""");

        var result = JsonSerializer.Deserialize<MonetaryValue>(json);
        result.ShouldBe(value);
        result.Currency.ShouldBeSameAs(Currency.GetCurrency(currencyCode));
        result.Amount.ToString(CultureInfo.InvariantCulture).ShouldBe(amountText);
    }

    [TestMethod]
    public void Default_WritesNull()
    {
        JsonSerializer.Serialize(MonetaryValue.Default).ShouldBe("null");
    }

    [TestMethod]
    public void Null_ReadsAsDefault()
    {
        var result = JsonSerializer.Deserialize<MonetaryValue>("null");
        result.IsDefault.ShouldBeTrue();
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
    public void NullableDefault_ReadsBackAsNull()
    {
        // The default value and null have the same JSON, and null in a nullable value reads as null.

        MonetaryValue? value = MonetaryValue.Default;

        string json = JsonSerializer.Serialize(value);
        json.ShouldBe("null");
        JsonSerializer.Deserialize<MonetaryValue?>(json).ShouldBeNull();
    }

    [TestMethod]
    public void Property_RoundTrips()
    {
        var order = new Order { Price = new MonetaryValue(19.99m, "USD"), Discount = null, Tip = MonetaryValue.Default };

        string json = JsonSerializer.Serialize(order);
        json.ShouldBe("""{"Price":{"amount":19.99,"currency":"USD"},"Discount":null,"Tip":null}""");

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
        json.ShouldBe("""[{"amount":1,"currency":"USD"},null,{"amount":-2.5,"currency":"EUR"}]""");
        JsonSerializer.Deserialize<MonetaryValue[]>(json).ShouldBe(values);
    }

    public class Order
    {
        public MonetaryValue Price { get; set; }

        public MonetaryValue? Discount { get; set; }

        public MonetaryValue Tip { get; set; }
    }
}
