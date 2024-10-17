namespace Singulink.Globalization.Tests.BagTests;
public static class ContainsCurrency
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

        private static readonly IReadOnlyMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void Currency_ValueExists_ReturnsTrue()
        {
            Bag.ContainsCurrency(Usd100.Currency).ShouldBeTrue();
            Bag.ContainsCurrency(Cad50.Currency).ShouldBeTrue();
            Bag.ContainsCurrency(Eur25.Currency).ShouldBeTrue();
        }

        [TestMethod]
        public void Currency_ValueDoesNotExist_ReturnsFalse()
        {
            Bag.ContainsCurrency(Currency.GetCurrency("GBP")).ShouldBeFalse();
            Bag.ContainsCurrency(Common.CurrencyX).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void CurrencyCode_ValueExists_ReturnsTrue()
        {
            Bag.ContainsCurrency(Usd100.Currency.CurrencyCode).ShouldBeTrue();
            Bag.ContainsCurrency(Cad50.Currency.CurrencyCode).ShouldBeTrue();
            Bag.ContainsCurrency(Eur25.Currency.CurrencyCode).ShouldBeTrue();
        }

        [TestMethod]
        public void CurrencyCode_ValueDoesNotExist_ReturnsFalse()
        {
            Bag.ContainsCurrency("GBP").ShouldBeFalse();
            Bag.ContainsCurrency(Common.CurrencyX).ShouldBeFalse();
        }
    }
}
