namespace Singulink.Globalization.Tests.MoneyCollectionExtensionsTests;

[PrefixTestClass]
public class ToImmutableSortedMoneyBag
{
    private static readonly CurrencyRegistry NewRegistry = new("New Registry", [Currency.GetCurrency("USD"), Currency.GetCurrency("CAD"), Currency.GetCurrency("EUR")]);
    private static readonly ImmutableArray<MonetaryValue> Values = [new(50m, "CAD"), new(25m, "EUR"), new(100m, "USD")];

    [TestMethod]
    public void OtherCollection_SetsRegistryAndCopiesValues()
    {
        var bag = Values.ToImmutableSortedMoneyBag(NewRegistry);
        bag.Registry.ShouldBeSameAs(NewRegistry);
        bag.ShouldBe(Values, ignoreOrder: false);
    }

    [PrefixTestClass]
    public class TMoneyBag : Tests<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Tests<SortedMoneyBag>;

    [PrefixTestClass]
    public class TImmutableMoneyBag : Tests<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Tests<ImmutableSortedMoneyBag>;

    public class Tests<TBag> where TBag : IReadOnlyMoneyBag
    {
        private static readonly IReadOnlyMoneyBag Bag = TBag.Create(NewRegistry, Values);

        [TestMethod]
        public void DefaultMoneyBag_SetsRegistryAndCopiesValues()
        {
            var bag = Bag.ToImmutableSortedMoneyBag();
            bag.Registry.ShouldBeSameAs(NewRegistry);
            bag.ShouldBe(Values, ignoreOrder: false);
        }

        [TestMethod]
        public void MoneyBag_SetsRegistryAndCopiesValues()
        {
            var bag = Bag.ToImmutableSortedMoneyBag(CurrencyRegistry.Default);
            bag.Registry.ShouldBeSameAs(CurrencyRegistry.Default);
            bag.ShouldBe(Values, ignoreOrder: false);
        }
    }
}
