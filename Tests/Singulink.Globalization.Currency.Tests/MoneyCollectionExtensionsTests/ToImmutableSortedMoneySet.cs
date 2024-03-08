namespace Singulink.Globalization.Tests.MoneyCollectionExtensionsTests;

public class ToImmutableSortedMoneySet
{
    private static readonly CurrencyRegistry newRegistry = new("New Registry", [Currency.Get("USD"), Currency.Get("CAD"), Currency.Get("EUR")]);
    private static readonly ImmutableArray<Money> SetValues = [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")];

    [PrefixTestClass]
    public class Set : Tests<MoneySet>;

    [PrefixTestClass]
    public class SortedSet : Tests<SortedMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSet : Tests<ImmutableMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSortedSet : Tests<ImmutableSortedMoneySet> { }

    [TestMethod]
    public void OtherCollection_SetsRegistryAndCopiesValues()
    {
        var set = SetValues.ToImmutableSortedMoneySet(newRegistry);
        set.Registry.ShouldBeSameAs(newRegistry);
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private static readonly IReadOnlyMoneySet Set = T.Create(newRegistry, SetValues);

        [TestMethod]
        public void DefaultMoneySet_SetsRegistryAndCopiesValues()
        {
            var set = Set.ToImmutableSortedMoneySet();
            set.Registry.ShouldBeSameAs(newRegistry);
            set.ShouldBe(SetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void MoneySet_SetsRegistryAndCopiesValues()
        {
            var set = Set.ToImmutableSortedMoneySet(CurrencyRegistry.Default);
            set.Registry.ShouldBeSameAs(CurrencyRegistry.Default);
            set.ShouldBe(SetValues, ignoreOrder: true);
        }
    }
}
