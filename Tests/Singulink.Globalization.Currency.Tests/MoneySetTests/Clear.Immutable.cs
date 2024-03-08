namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class Clear
{
    public class Immutable<T> where T : IImmutableMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);

        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50]);

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