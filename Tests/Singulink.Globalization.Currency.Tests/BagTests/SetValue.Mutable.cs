namespace Singulink.Globalization.Tests.BagTests;

public static partial class SetValue
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
        private static readonly MonetaryValue Aud75 = new(75m, "AUD");
        private static readonly ImmutableArray<MonetaryValue> DefaultBagValues = [Usd100, Cad50, Eur25];

        private readonly IMoneyBag _bag = TBag.Create(CurrencyRegistry.Default, DefaultBagValues);

        [TestMethod]
        public void SetValue_CurrencyDoesNotExist_AddsValue()
        {
            _bag.SetValue(Aud75);
            _bag.Count.ShouldBe(4);
            _bag.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_CurrencyExists_UpdatesValue()
        {
            _bag.SetValue(new(200m, "USD"));
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe([new(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_DefaultValue_NoChange()
        {
            _bag.SetValue(default);
            _bag.Count.ShouldBe(3);
            _bag.ShouldBe(DefaultBagValues, ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new MonetaryValue(100, Common.CurrencyX);
            Should.Throw<ArgumentException>(() => _bag.SetValue(value));
        }
    }
}