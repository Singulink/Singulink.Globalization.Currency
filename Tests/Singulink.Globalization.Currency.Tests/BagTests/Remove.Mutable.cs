namespace Singulink.Globalization.Tests.BagTests;

public static partial class Remove
{
    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        private static readonly MonetaryValue Usd100 = new(100m, "USD");
        private static readonly MonetaryValue Cad50 = new(50m, "CAD");
        private static readonly MonetaryValue Eur25 = new(25m, "EUR");
        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Usd100, Cad50, Eur25];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void RemoveCurrencyCode_CurrencyFound_RemovesValueAndReturnsTrue()
        {
            _bag.Remove("USD").ShouldBeTrue();
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencyCode_CurrencyNotFound_ReturnsFalseAndNoChange()
        {
            _bag.Remove("JPY").ShouldBeFalse();
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencyCode_DisallowedCurrency_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.Remove("XXX"));
        }

        ///////////////////////////

        [TestMethod]
        public void RemoveCurrency_CurrencyFound_RemovesValueAndReturnsTrue()
        {
            _bag.Remove(Usd100.Currency).ShouldBeTrue();
            _bag.Count.ShouldBe(2);
            _bag.ShouldBe([Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrency_CurrencyNotFound_ReturnsFalseAndNoChange()
        {
            var gbpCurrency = Currency.GetCurrency("GBP");
            _bag.Remove(gbpCurrency).ShouldBeFalse();
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrency_DisallowedCurrency_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _bag.Remove(Common.CurrencyX));
        }
    }
}
