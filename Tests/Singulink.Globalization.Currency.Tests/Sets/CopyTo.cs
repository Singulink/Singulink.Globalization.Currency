namespace Singulink.Globalization.Tests.Sets;
public static class CopyTo
{
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
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");

        private readonly IReadOnlyMoneySet _set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void DestinationSufficientSize_Successful()
        {
            var array = new Money[3];

            ((ICollection<Money>)_set).CopyTo(array, 0);
            array.ShouldBe([Usd100, Cad50, Eur25], ignoreOrder: true);

            array = new Money[10];

            ((ICollection<Money>)_set).CopyTo(array, 5);

            array.Take(5).ShouldAllBe(m => m.IsDefault);
            array.Skip(5).Take(3).ShouldBe([Usd100, Cad50, Eur25], ignoreOrder: true);
            array.Skip(8).ShouldAllBe(m => m.IsDefault);
        }

        [TestMethod]
        public void DestinationInsufficientSize_Successful()
        {
            var array = new Money[2];
            Should.Throw<ArgumentException>(() => ((ICollection<Money>)_set).CopyTo(array, 1));
            Should.Throw<ArgumentOutOfRangeException>(() => ((ICollection<Money>)_set).CopyTo(array, 15));

            array = new Money[10];
            Should.Throw<ArgumentException>(() => ((ICollection<Money>)_set).CopyTo(array, 8));
        }
    }
}
