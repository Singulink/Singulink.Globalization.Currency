using System.Text.Json;

namespace Singulink.Globalization.Tests.MoneyBagJsonConverterFactoryTests;

[PrefixTestClass]
public class Factory
{
    [TestMethod]
    [DataRow(typeof(MoneyBag))]
    [DataRow(typeof(SortedMoneyBag))]
    [DataRow(typeof(ImmutableMoneyBag))]
    [DataRow(typeof(ImmutableSortedMoneyBag))]
    public void CanConvert_ConcreteBags(Type type)
    {
        new MoneyBagJsonConverterFactory().CanConvert(type).ShouldBeTrue();
    }

    [TestMethod]
    [DataRow(typeof(IReadOnlyMoneyBag))]
    [DataRow(typeof(IMoneyBag))]
    [DataRow(typeof(IImmutableMoneyBag))]
    [DataRow(typeof(List<MonetaryValue>))]
    [DataRow(typeof(MonetaryValue))]
    public void CanConvert_OtherTypes_ReturnsFalse(Type type)
    {
        new MoneyBagJsonConverterFactory().CanConvert(type).ShouldBeFalse();
    }

    [TestMethod]
    public void CreateConverter_OtherType_Throws()
    {
        Should.Throw<ArgumentException>(() => new MoneyBagJsonConverterFactory().CreateConverter(typeof(IMoneyBag), JsonSerializerOptions.Default));
    }

    [TestMethod]
    public void InterfaceProperties_WriteButDoNotRead()
    {
        // The bag interfaces have static abstract members, so no converter can be declared for them. The serializer writes them as collections and cannot
        // read them back.

        var holder = new InterfaceHolder {
            ReadOnlyBag = new MoneyBag(new MonetaryValue(1m, "USD")),
            MutableBag = new MoneyBag(new MonetaryValue(2m, "USD")),
            ImmutableBag = ImmutableMoneyBag.Create(new MonetaryValue(3m, "USD")),
        };

        string json = JsonSerializer.Serialize(holder);
        json.ShouldBe("""{"ReadOnlyBag":[{"amount":1,"currency":"USD"}],"MutableBag":[{"amount":2,"currency":"USD"}],"ImmutableBag":[{"amount":3,"currency":"USD"}]}""");

        Should.Throw<NotSupportedException>(() => JsonSerializer.Deserialize<InterfaceHolder>("""{"ReadOnlyBag":[]}"""));
        Should.Throw<NotSupportedException>(() => JsonSerializer.Deserialize<InterfaceHolder>("""{"MutableBag":[]}"""));
        Should.Throw<NotSupportedException>(() => JsonSerializer.Deserialize<InterfaceHolder>("""{"ImmutableBag":[]}"""));
    }

    public class InterfaceHolder
    {
        public IReadOnlyMoneyBag? ReadOnlyBag { get; set; }

        public IMoneyBag? MutableBag { get; set; }

        public IImmutableMoneyBag? ImmutableBag { get; set; }
    }
}
