namespace Singulink.Globalization.Tests.BagTests;

public static partial class Clear
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")]);

        [TestMethod]
        public void Clear_PopulatedBag_RemovesAllValues()
        {
            var resultBag = Bag.Clear();

            resultBag.Count.ShouldBe(0);
            resultBag.ShouldBe([]);
        }

        [TestMethod]
        public void Clear_EmptyBag_ReturnsDefault()
        {
            var emptyBag = TBag.Create(CurrencyRegistry.Default, []);
            var resultBag = emptyBag.Clear();

            resultBag.ShouldBeSameAs(emptyBag);
        }
    }
}