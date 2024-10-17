namespace Singulink.Globalization.Tests.BagTests;

public static partial class Remove
{
    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");

        private static readonly IImmutableMoneyBag Bag = TBag.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void RemoveCurrencyCode_CurrencyFound_RemovesValue()
        {
            var resultBag = Bag.Remove("USD");
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencyCode_CurrencyNotFound_NoChange()
        {
            var resultBag = Bag.Remove("JPY");
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void RemoveCurrencyCode_DisallowedCurrency_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.Remove("XXX"));
        }

        ///////////////////////////

        [TestMethod]
        public void RemoveCurrency_CurrencyFound_RemovesValue()
        {
            var resultBag = Bag.Remove(Usd100.Currency);
            resultBag.Count.ShouldBe(2);
            resultBag.ShouldBe([Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrency_CurrencyNotFound_NoChange()
        {
            var gbpCurrency = Currency.GetCurrency("GBP");
            var resultBag = Bag.Remove(gbpCurrency);
            resultBag.ShouldBeSameAs(Bag);
        }

        [TestMethod]
        public void RemoveCurrency_DisallowedCurrency_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => Bag.Remove(Common.CurrencyX));
        }
    }
}
