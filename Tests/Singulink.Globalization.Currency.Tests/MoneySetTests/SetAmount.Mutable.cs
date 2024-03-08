namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class SetAmount
{
    [PrefixTestClass]
    public class TMoneySet : Mutable<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Mutable<SortedMoneySet>;

    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Currency Usd = Currency.Get("USD");
        private static readonly Currency Aud = Currency.Get("AUD");

        private static readonly Money Usd0 = new(0m, "USD");
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Money Aud75 = new(75m, "AUD");

        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50, Eur25];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

        [TestMethod]
        public void SetByCurrencyCode_CurrencyExists_UpdatesValue()
        {
            _set.SetAmount(200m, "USD");
            _set.Count.ShouldBe(3);
            _set.ShouldBe([Money.Create(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_NewCurrency_AddsValue()
        {
            _set.SetAmount(75m, "AUD");
            _set.Count.ShouldBe(4);
            _set.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_ZeroAmount_ZeroesValue()
        {
            _set.SetAmount(0, "USD");
            _set.Count.ShouldBe(3);
            _set.ShouldBe([Usd0, Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _set.SetAmount(123m, "XXXX"));
        }

        ////////////////////////////

        [TestMethod]

        public void SetByCurrency_CurrencyExists_UpdatesValue()
        {
            _set.SetAmount(200m, Usd);
            _set.Count.ShouldBe(3);
            _set.ShouldBe([new(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_CurrencyDoesNotExist_AddsValue()
        {
            _set.SetAmount(75m, Aud);
            _set.Count.ShouldBe(4);
            _set.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_ZeroAmount_ZeroesValue()
        {
            _set.SetAmount(0, Usd);
            _set.Count.ShouldBe(3);
            _set.ShouldBe([Usd0, Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => _set.SetAmount(123m, Common.CurrencyX));
        }
    }
}