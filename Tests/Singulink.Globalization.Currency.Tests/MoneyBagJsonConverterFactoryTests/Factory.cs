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
    [DataRow(typeof(IReadOnlyMoneyBag))]
    [DataRow(typeof(IMoneyBag))]
    [DataRow(typeof(IImmutableMoneyBag))]
    public void CanConvert_BagTypesAndInterfaces(Type type)
    {
        new MoneyBagJsonConverterFactory().CanConvert(type).ShouldBeTrue();
    }

    [TestMethod]
    [DataRow(typeof(List<MonetaryValue>))]
    [DataRow(typeof(IReadOnlyCollection<MonetaryValue>))]
    [DataRow(typeof(MonetaryValue))]
    [DataRow(typeof(object))]
    public void CanConvert_OtherTypes_ReturnsFalse(Type type)
    {
        new MoneyBagJsonConverterFactory().CanConvert(type).ShouldBeFalse();
    }

    [TestMethod]
    public void CreateConverter_OtherType_Throws()
    {
        Should.Throw<ArgumentException>(() => new MoneyBagJsonConverterFactory().CreateConverter(typeof(List<MonetaryValue>), JsonSerializerOptions.Default));
    }

    [TestMethod]
    public void InterfaceProperties_WriteTheBagTheyHold()
    {
        var holder = new InterfaceHolder {
            ReadOnlyBag = new SortedMoneyBag(new MonetaryValue(1m, "USD"), new MonetaryValue(2m, "CAD")),
            MutableBag = new MoneyBag(new MonetaryValue(3m, "USD")),
            ImmutableBag = ImmutableSortedMoneyBag.Create(new MonetaryValue(4m, "USD"), new MonetaryValue(5m, "EUR")),
        };

        string json = JsonSerializer.Serialize(holder);
        json.ShouldContain("""{"ReadOnlyBag":[{"amount":2,"currency":"CAD"},{"amount":1,"currency":"USD"}],""");   // sorted, as the bag it holds
        json.ShouldContain("""],"MutableBag":[{"amount":3,"currency":"USD"}],""");
        json.ShouldEndWith("""],"ImmutableBag":[{"amount":5,"currency":"EUR"},{"amount":4,"currency":"USD"}]}""");

        var result = JsonSerializer.Deserialize<InterfaceHolder>(json)!;
        result.ReadOnlyBag.ShouldBeOfType<MoneyBag>().ShouldBe(holder.ReadOnlyBag, ignoreOrder: true);
        result.MutableBag.ShouldBeOfType<MoneyBag>().ShouldBe(holder.MutableBag);
        result.ImmutableBag.ShouldBeOfType<ImmutableMoneyBag>().ShouldBe(holder.ImmutableBag, ignoreOrder: true);
    }

    [TestMethod]
    public void InterfaceReadTypes_MatchCollectionExpressions()
    {
        // There is one answer in the library to what an interface creates: JSON reads the type that a collection expression creates.

        IReadOnlyMoneyBag readOnlyBag = [new(1m, "USD")];
        IMoneyBag mutableBag = [new(1m, "USD")];
        IImmutableMoneyBag immutableBag = [new(1m, "USD")];

        const string json = """[{"amount":1,"currency":"USD"}]""";

        JsonSerializer.Deserialize<IReadOnlyMoneyBag>(json)!.GetType().ShouldBe(readOnlyBag.GetType());
        JsonSerializer.Deserialize<IMoneyBag>(json)!.GetType().ShouldBe(mutableBag.GetType());
        JsonSerializer.Deserialize<IImmutableMoneyBag>(json)!.GetType().ShouldBe(immutableBag.GetType());
    }

    public class InterfaceHolder
    {
        public IReadOnlyMoneyBag? ReadOnlyBag { get; set; }

        public IMoneyBag? MutableBag { get; set; }

        public IImmutableMoneyBag? ImmutableBag { get; set; }
    }
}
