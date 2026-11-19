namespace Singulink.Globalization.Tests.BagTests;

[PrefixTestClass]
public class MoneyBagFactory
{
    private static readonly MonetaryValue[] Values = [new(1m, "USD"), new(2m, "EUR"), new(3m, "USD")];

    [TestMethod]
    public void Create_ReturnsTheConcreteType()
    {
        MoneyBag mutable = CreateBag<MoneyBag>(CurrencyRegistry.Default, Values);
        SortedMoneyBag sorted = CreateBag<SortedMoneyBag>(CurrencyRegistry.Default, Values);
        ImmutableMoneyBag immutable = CreateBag<ImmutableMoneyBag>(CurrencyRegistry.Default, Values);
        ImmutableSortedMoneyBag immutableSorted = CreateBag<ImmutableSortedMoneyBag>(CurrencyRegistry.Default, Values);

        IReadOnlyMoneyBag[] bags = [mutable, sorted, immutable, immutableSorted];

        foreach (var bag in bags)
        {
            bag.Registry.ShouldBeSameAs(CurrencyRegistry.Default);
            bag["USD"].ShouldBe(new MonetaryValue(4m, "USD"));
            bag["EUR"].ShouldBe(new MonetaryValue(2m, "EUR"));
        }
    }

    [TestMethod]
    public void Create_UsesTheRegistry()
    {
        var registry = new CurrencyRegistry("Small", [Currency.GetCurrency("USD")]);

        CreateBag<SortedMoneyBag>(registry, [new(1m, "USD")]).Registry.ShouldBeSameAs(registry);
        Should.Throw<ArgumentException>(() => CreateBag<ImmutableMoneyBag>(registry, [new(1m, "EUR")]));
    }

    private static TBag CreateBag<TBag>(CurrencyRegistry registry, IEnumerable<MonetaryValue> values)
        where TBag : IReadOnlyMoneyBag, IMoneyBagFactory<TBag>
    {
        return TBag.Create(registry, values);
    }
}
