namespace Singulink.Globalization.Tests.BagTests;
public static class Contains
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
        public void MonetaryValue_ValueExists_ReturnsTrue()
        {
            Bag.Contains(Usd100).ShouldBeTrue();
            Bag.Contains(Cad50).ShouldBeTrue();
            Bag.Contains(Eur25).ShouldBeTrue();
        }

        [TestMethod]
        public void MonetaryValue_ValueDoesNotExist_ReturnsFalse()
        {
            Bag.Contains(MonetaryValue.Create(0m, "GBP")).ShouldBeFalse();
            Bag.Contains(MonetaryValue.Create(0m, Common.CurrencyX)).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void AmountAndCurrency_ValueExists_ReturnsTrue()
        {
            Bag.Contains(Usd100.Amount, Usd100.Currency).ShouldBeTrue();
            Bag.Contains(Cad50.Amount, Cad50.Currency).ShouldBeTrue();
            Bag.Contains(Eur25.Amount, Eur25.Currency).ShouldBeTrue();
        }

        [TestMethod]
        public void AmountAndCurrency_ValueDoesNotExist_ReturnsFalse()
        {
            Bag.Contains(0m, Currency.GetCurrency("GBP")).ShouldBeFalse();
            Bag.Contains(0m, Common.CurrencyX).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void AmountAndCurrencyCode_ValueExists_ReturnsTrue()
        {
            Bag.Contains(Usd100.Amount, Usd100.Currency.CurrencyCode).ShouldBeTrue();
            Bag.Contains(Cad50.Amount, Cad50.Currency.CurrencyCode).ShouldBeTrue();
            Bag.Contains(Eur25.Amount, Eur25.Currency.CurrencyCode).ShouldBeTrue();
        }

        [TestMethod]
        public void AmountAndCurrencyCode_ValueDoesNotExist_ReturnsFalse()
        {
            Bag.Contains(0m, "GBP").ShouldBeFalse();
            Bag.Contains(0m, "XXXX").ShouldBeFalse();
        }
    }
}
