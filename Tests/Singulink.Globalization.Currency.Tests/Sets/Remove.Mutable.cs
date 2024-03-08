namespace Singulink.Globalization.Tests.Sets;

public static partial class Remove
{
    [PrefixTestClass]
    public class TMoneySet : Mutable<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Mutable<SortedMoneySet>;

    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50, Eur25];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

        [TestMethod]
        public void RemoveCurrencyCode_CurrencyFound_RemovesValueAndReturnsTrue()
        {
            _set.Remove("USD").ShouldBeTrue();
            _set.Count.ShouldBe(2);
            _set.ShouldBe([Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencyCode_CurrencyNotFound_ReturnsFalseAndNoChange()
        {
            _set.Remove("JPY").ShouldBeFalse();
            _set.Count.ShouldBe(3);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrencyCode_DisallowedCurrency_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _set.Remove("XXX"));
        }

        ///////////////////////////

        [TestMethod]
        public void RemoveCurrency_CurrencyFound_RemovesValueAndReturnsTrue()
        {
            _set.Remove(Usd100.Currency).ShouldBeTrue();
            _set.Count.ShouldBe(2);
            _set.ShouldBe([Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrency_CurrencyNotFound_ReturnsFalseAndNoChange()
        {
            var gbpCurrency = Currency.Get("GBP");
            _set.Remove(gbpCurrency).ShouldBeFalse();
            _set.Count.ShouldBe(3);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void RemoveCurrency_DisallowedCurrency_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _set.Remove(Common.CurrencyX));
        }
    }
}
