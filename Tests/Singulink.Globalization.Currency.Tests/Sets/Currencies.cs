namespace Singulink.Globalization.Tests.Sets;
public static class Currencies
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
        private static readonly ImmutableArray<Currency> Currencies = [Currency.Get("USD"), Currency.Get("CAD"), Currency.Get("EUR")];

        private readonly IReadOnlyMoneySet _set = T.Create(CurrencyRegistry.Default, [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")]);

        [TestMethod]
        public void GetCurrencies_GetsValueCurrencies()
        {
            _set.Currencies.Count.ShouldBe(3);
            _set.Currencies.ShouldBe(Currencies, ignoreOrder: true);
        }

        [TestMethod]
        public void GetCurrencies_GetsReadOnlyCollection()
        {
            ((ICollection<Currency>)_set.Currencies).ShouldBeReadOnlyCollection();
        }

        [TestMethod]
        public void CopyTo_DestinationSufficientSize_Successful()
        {
            var array = new Currency[3];

            ((ICollection<Currency>)_set.Currencies).CopyTo(array, 0);
            array.ShouldBe(Currencies, ignoreOrder: true);

            array = new Currency[10];

            ((ICollection<Currency>)_set.Currencies).CopyTo(array, 5);

            array.Take(5).ShouldAllBe(m => m == null);
            array.Skip(5).Take(3).ShouldBe(Currencies, ignoreOrder: true);
            array.Skip(8).ShouldAllBe(m => m == null);
        }

        [TestMethod]
        public void CopyTo_DestinationInsufficientSize_Successful()
        {
            var array = new Currency[2];
            Should.Throw<ArgumentException>(() => ((ICollection<Currency>)_set.Currencies).CopyTo(array, 1));
            Should.Throw<ArgumentOutOfRangeException>(() => ((ICollection<Currency>)_set.Currencies).CopyTo(array, 15));

            array = new Currency[10];
            Should.Throw<ArgumentException>(() => ((ICollection<Currency>)_set.Currencies).CopyTo(array, 8));
        }
    }
}
