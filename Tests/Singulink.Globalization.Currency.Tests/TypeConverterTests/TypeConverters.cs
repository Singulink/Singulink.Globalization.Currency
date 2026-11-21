using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Singulink.Globalization.Tests.TypeConverterTests;

[PrefixTestClass]
public class TypeConverters
{
    private const char Sp = ' ';

    private static readonly string[] ThreadCultures = ["en-US", "de-DE", "fr-FR", "ar-SA", "tr-TR", "hi-IN", "kea-CV"];

    private static readonly MonetaryValue Price = new(12.3m, "USD");

    private static TypeConverter ValueConverter => TypeDescriptor.GetConverter(typeof(MonetaryValue));

    private static TypeConverter CurrencyConverter => TypeDescriptor.GetConverter(typeof(Currency));

    [TestMethod]
    public void Converters_AreFound()
    {
        ValueConverter.CanConvertFrom(typeof(string)).ShouldBeTrue();
        ValueConverter.CanConvertTo(typeof(string)).ShouldBeTrue();
        CurrencyConverter.CanConvertFrom(typeof(string)).ShouldBeTrue();
        CurrencyConverter.CanConvertTo(typeof(string)).ShouldBeTrue();
    }

    [TestMethod]
    public void InvariantCulture_UsesRoundTripFormat_InEveryThreadCulture()
    {
        var previous = CultureInfo.CurrentCulture;

        try
        {
            foreach (string name in ThreadCultures)
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);

                ValueConverter.ConvertToInvariantString(Price).ShouldBe("USD 12.30", name);
                ValueConverter.ConvertFromInvariantString("USD 12.30").ShouldBe(Price, name);
                ValueConverter.ConvertFromInvariantString("USD 12.3").ShouldBe(Price, name);
                ValueConverter.ConvertTo(null, CultureInfo.InvariantCulture, Price, typeof(string)).ShouldBe("USD 12.30", name);
                ValueConverter.ConvertFrom(null, CultureInfo.InvariantCulture, "USD 12.30").ShouldBe(Price, name);

                CurrencyConverter.ConvertToInvariantString(Currency.GetCurrency("EUR")).ShouldBe("EUR", name);
                CurrencyConverter.ConvertFromInvariantString("EUR").ShouldBeSameAs(Currency.GetCurrency("EUR"), name);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public void SpecificCulture_UsesDefaultFormatAndCultureParsing()
    {
        var deDE = CultureInfo.GetCultureInfo("de-DE");
        var enUS = CultureInfo.GetCultureInfo("en-US");

        ValueConverter.ConvertTo(null, deDE, new MonetaryValue(1234.5m, "USD"), typeof(string)).ShouldBe($"1.234,50{Sp}USD");
        ValueConverter.ConvertFrom(null, deDE, "1.234,50 USD").ShouldBe(new MonetaryValue(1234.5m, "USD"));

        ValueConverter.ConvertTo(null, enUS, new MonetaryValue(1234.5m, "USD"), typeof(string)).ShouldBe($"USD{Sp}1,234.50");
        ValueConverter.ConvertFrom(null, enUS, "USD 1,234.50").ShouldBe(new MonetaryValue(1234.5m, "USD"));
    }

    [TestMethod]
    public void NoCulture_MeansCurrentCulture()
    {
        // ConvertToString passes the current culture and ConvertFromString passes none, so no culture means the current culture, as for DecimalConverter.

        var previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            ValueConverter.ConvertToString(Price).ShouldBe($"12,30{Sp}USD");
            ValueConverter.ConvertFromString("12,30 USD").ShouldBe(Price);
            ValueConverter.ConvertFromString(ValueConverter.ConvertToString(Price)!).ShouldBe(Price);
            ValueConverter.ConvertTo(null, null, Price, typeof(string)).ShouldBe($"12,30{Sp}USD");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public void DefaultValue_IsZero()
    {
        ValueConverter.ConvertToInvariantString(MonetaryValue.Default).ShouldBe("0");
        ValueConverter.ConvertTo(null, CultureInfo.GetCultureInfo("de-DE"), MonetaryValue.Default, typeof(string)).ShouldBe("0");
        ValueConverter.ConvertFromInvariantString("0").ShouldBe(MonetaryValue.Default);
        ValueConverter.ConvertFrom(null, CultureInfo.GetCultureInfo("de-DE"), "0").ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void InvalidText_ThrowsArgumentException()
    {
        var ex = Should.Throw<ArgumentException>(() => ValueConverter.ConvertFromInvariantString("$12.30"));
        ex.Message.ShouldStartWith("'$12.30' is not a valid value for MonetaryValue. ");
        ex.InnerException.ShouldBeOfType<FormatException>();

        Should.Throw<ArgumentException>(() => ValueConverter.ConvertFromInvariantString("ZZZ 1.00"))
            .Message.ShouldContain("'ZZZ'");

        Should.Throw<ArgumentException>(() => CurrencyConverter.ConvertFromInvariantString("ZZZ"))
            .Message.ShouldStartWith("'ZZZ' is not a valid value for Currency. Currency code 'ZZZ' was not found in the 'System' currency registry.");

        Should.Throw<NotSupportedException>(() => ValueConverter.ConvertFrom(12.3m));
    }

    [TestMethod]
    public void Nullable_UsesTheConverter()
    {
        var converter = TypeDescriptor.GetConverter(typeof(MonetaryValue?));

        converter.ConvertFromInvariantString("USD 12.30").ShouldBe(Price);
        converter.ConvertFromInvariantString(string.Empty).ShouldBeNull();
        converter.ConvertToInvariantString(Price).ShouldBe("USD 12.30");
    }

    [TestMethod]
    public void NewtonsoftJson_WritesAndReadsTheRoundTripString()
    {
        JsonConvert.SerializeObject(Price).ShouldBe("\"USD 12.30\"");
        JsonConvert.DeserializeObject<MonetaryValue>("\"USD 12.30\"").ShouldBe(Price);
        JsonConvert.SerializeObject(MonetaryValue.Default).ShouldBe("\"0\"");

        JsonConvert.SerializeObject(Currency.GetCurrency("USD")).ShouldBe("\"USD\"");
        JsonConvert.DeserializeObject<Currency>("\"USD\"").ShouldBeSameAs(Currency.GetCurrency("USD"));

        var order = new Order { Total = Price, Discount = null, Currency = Currency.GetCurrency("EUR"), Lines = new() { [new MonetaryValue(5m, "JPY")] = 2 } };
        string json = JsonConvert.SerializeObject(order);
        json.ShouldBe("""{"Total":"USD 12.30","Discount":null,"Currency":"EUR","Lines":{"JPY 5":2}}""");

        var result = JsonConvert.DeserializeObject<Order>(json)!;
        result.Total.ShouldBe(Price);
        result.Discount.ShouldBeNull();
        result.Currency.ShouldBeSameAs(Currency.GetCurrency("EUR"));
        result.Lines![new MonetaryValue(5m, "JPY")].ShouldBe(2);

        JsonConvert.DeserializeObject<Order>("""{"Discount":"USD 1.5"}""")!.Discount.ShouldBe(new MonetaryValue(1.5m, "USD"));
    }

    [TestMethod]
    public void NewtonsoftJson_InvalidText_Throws()
    {
        Should.Throw<JsonSerializationException>(() => JsonConvert.DeserializeObject<MonetaryValue>("\"$12.30\""));
    }

    [TestMethod]
    public void Configuration_BindsValues()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Shop:Total"] = "USD 12.30",
                ["Shop:Discount"] = "EUR 1",
                ["Shop:Currency"] = "CAD",
            })
            .Build();

        var settings = configuration.GetSection("Shop").Get<Order>()!;
        settings.Total.ShouldBe(Price);
        settings.Discount.ShouldBe(new MonetaryValue(1m, "EUR"));
        settings.Currency.ShouldBeSameAs(Currency.GetCurrency("CAD"));

        configuration.GetValue<MonetaryValue>("Shop:Total").ShouldBe(Price);
    }

    [TestMethod]
    public void Configuration_InvalidValue_Throws()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Total"] = "12.30" }).Build();

        Should.Throw<InvalidOperationException>(() => configuration.GetValue<MonetaryValue>("Total"));
    }

    public class Order
    {
        public MonetaryValue Total { get; set; }

        public MonetaryValue? Discount { get; set; }

        public Currency? Currency { get; set; }

        public Dictionary<MonetaryValue, int>? Lines { get; set; }
    }
}
