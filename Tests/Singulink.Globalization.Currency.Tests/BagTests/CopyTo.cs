namespace Singulink.Globalization.Tests.BagTests;
public static class CopyTo
{
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
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");

        private readonly IReadOnlyMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void DestinationSufficientSize_Successful()
        {
            var array = new MonetaryValue[3];

            ((ICollection<MonetaryValue>)_bag).CopyTo(array, 0);
            array.ShouldBe([Usd100, Cad50, Eur25], ignoreOrder: true);

            array = new MonetaryValue[10];

            ((ICollection<MonetaryValue>)_bag).CopyTo(array, 5);

            array.Take(5).ShouldAllBe(m => m.IsDefault);
            array.Skip(5).Take(3).ShouldBe([Usd100, Cad50, Eur25], ignoreOrder: true);
            array.Skip(8).ShouldAllBe(m => m.IsDefault);
        }

        [TestMethod]
        public void DestinationInsufficientSize_Throws()
        {
            var array = new MonetaryValue[2];
            Should.Throw<ArgumentException>(() => ((ICollection<MonetaryValue>)_bag).CopyTo(array, 1));
            Should.Throw<ArgumentOutOfRangeException>(() => ((ICollection<MonetaryValue>)_bag).CopyTo(array, 15));

            array = new MonetaryValue[10];
            Should.Throw<ArgumentException>(() => ((ICollection<MonetaryValue>)_bag).CopyTo(array, 8));
        }
    }
}
