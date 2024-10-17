namespace Singulink.Globalization.Tests.BagTests;
public static class Currencies
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
        private static readonly ImmutableArray<Currency> Currencies = [Currency.GetCurrency("USD"), Currency.GetCurrency("CAD"), Currency.GetCurrency("EUR")];

        private readonly IReadOnlyMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")]);

        [TestMethod]
        public void GetCurrencies_GetsValueCurrencies()
        {
            _bag.Currencies.Count.ShouldBe(3);
            _bag.Currencies.ShouldBe(Currencies, ignoreOrder: true);
        }

        [TestMethod]
        public void GetCurrencies_GetsReadOnlyCollection()
        {
            ((ICollection<Currency>)_bag.Currencies).ShouldBeReadOnlyCollection();
        }

        ///////////////////////////

        [TestMethod]
        public void CopyTo_DestinationSufficientSize_Successful()
        {
            var array = new Currency[3];

            ((ICollection<Currency>)_bag.Currencies).CopyTo(array, 0);
            array.ShouldBe(Currencies, ignoreOrder: true);

            array = new Currency[10];

            ((ICollection<Currency>)_bag.Currencies).CopyTo(array, 5);

            array.Take(5).ShouldAllBe(m => m == null);
            array.Skip(5).Take(3).ShouldBe(Currencies, ignoreOrder: true);
            array.Skip(8).ShouldAllBe(m => m == null);
        }

        [TestMethod]
        public void CopyTo_DestinationInsufficientSize_Throws()
        {
            var array = new Currency[2];
            Should.Throw<ArgumentException>(() => ((ICollection<Currency>)_bag.Currencies).CopyTo(array, 1));
            Should.Throw<ArgumentOutOfRangeException>(() => ((ICollection<Currency>)_bag.Currencies).CopyTo(array, 15));

            array = new Currency[10];
            Should.Throw<ArgumentException>(() => ((ICollection<Currency>)_bag.Currencies).CopyTo(array, 8));
        }
    }
}
