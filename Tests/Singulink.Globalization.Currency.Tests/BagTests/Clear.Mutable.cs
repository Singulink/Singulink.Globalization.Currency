namespace Singulink.Globalization.Tests.BagTests;

public static partial class Clear
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Usd100, Cad50];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void Clear_PopulatedBag_RemovesAllValues()
        {
            _bag.Clear();
            _bag.Count.ShouldBe(0);
        }
    }
}