namespace Singulink.Globalization.Tests.MoneyCollectionExtensionsTests;

[PrefixTestClass]
public class ToSortedMoneySet
{
    private static readonly CurrencyRegistry newRegistry = new("New Registry", [Currency.Get("USD"), Currency.Get("CAD"), Currency.Get("EUR")]);
    private static readonly ImmutableArray<Money> SetValues = [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")];

    [TestMethod]
    public void OtherCollection_SetsRegistryAndCopiesValues()
    {
        var set = SetValues.ToSortedMoneySet(newRegistry);
        set.Registry.ShouldBeSameAs(newRegistry);
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [PrefixTestClass]
    public class TMoneySet : Tests<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Tests<SortedMoneySet>;

    [PrefixTestClass]
    public class TImmutableMoneySet : Tests<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Tests<ImmutableSortedMoneySet>;

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private static readonly IReadOnlyMoneySet Set = T.Create(newRegistry, SetValues);

        [TestMethod]
        public void DefaultMoneySet_SetsRegistryAndCopiesValues()
        {
            var set = Set.ToSortedMoneySet();
            set.Registry.ShouldBeSameAs(newRegistry);
            set.ShouldBe(SetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void MoneySet_SetsRegistryAndCopiesValues()
        {
            var set = Set.ToSortedMoneySet(CurrencyRegistry.Default);
            set.Registry.ShouldBeSameAs(CurrencyRegistry.Default);
            set.ShouldBe(SetValues, ignoreOrder: true);
        }
    }
}
