namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class Clear
{
    [PrefixTestClass]
    public class Set : Mutable<MoneySet>;

    [PrefixTestClass]
    public class SortedSet : Mutable<SortedMoneySet>;

    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);
        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

        [TestMethod]
        public void Clear_PopulatedSet_RemovesAllValues()
        {
            _set.Clear();
            _set.Count.ShouldBe(0);
        }
    }
}