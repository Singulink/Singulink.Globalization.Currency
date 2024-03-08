namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class Clear
{
    [PrefixTestClass]
    public class ImmutableSet : Immutable<ImmutableMoneySet>;

    [PrefixTestClass]
    public class ImmutableSortedSet : Immutable<ImmutableSortedMoneySet>;

    public class Immutable<T> where T : IImmutableMoneySet
    {
        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")]);

        [TestMethod]
        public void Clear_PopulatedSet_RemovesAllValues()
        {
            var resultSet = Set.Clear();

            resultSet.Count.ShouldBe(0);
            resultSet.ShouldBe([]);
        }

        [TestMethod]
        public void Clear_EmptySet_ReturnsDefault()
        {
            var emptySet = T.Create(CurrencyRegistry.Default, []);
            var resultSet = emptySet.Clear();

            resultSet.ShouldBeSameAs(emptySet);
        }
    }
}