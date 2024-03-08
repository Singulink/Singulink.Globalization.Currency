namespace Singulink.Globalization.Tests.MoneyCollectionExtensionsTests;

public class ToMoneySet
{
    private static readonly CurrencyRegistry NewRegistry = new("New Registry", [Currency.Get("USD"), Currency.Get("CAD"), Currency.Get("EUR")]);
    private static readonly ImmutableArray<Money> SetValues = [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")];

    [PrefixTestClass]
    public class Set : Tests<MoneySet> { }

    [PrefixTestClass]
    public class SortedSet : Tests<SortedMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSet : Tests<ImmutableMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSortedSet : Tests<ImmutableSortedMoneySet> { }

    [TestMethod]
    public void OtherCollection_SetsRegistry()
    {
        var set = SetValues.ToMoneySet(NewRegistry);
        set.Registry.ShouldBeSameAs(NewRegistry);
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private static readonly IReadOnlyMoneySet Set = T.Create(NewRegistry, SetValues);

        [TestMethod]
        public void DefaultMoneySet_SetsRegistry()
        {
            var set = Set.ToMoneySet();
            set.Registry.ShouldBeSameAs(NewRegistry);
            set.ShouldBe(SetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void MoneySet_SetsRegistry()
        {
            var set = Set.ToMoneySet(CurrencyRegistry.Default);
            set.Registry.ShouldBeSameAs(CurrencyRegistry.Default);
            set.ShouldBe(SetValues, ignoreOrder: true);
        }
    }
}
