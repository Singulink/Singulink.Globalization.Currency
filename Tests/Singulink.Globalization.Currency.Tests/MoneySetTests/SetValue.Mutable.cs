namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class SetValue
{
    [PrefixTestClass]
    public class Set : Mutable<MoneySet>;

    [PrefixTestClass]
    public class SortedSet : Mutable<SortedMoneySet>;

    public class Mutable<T> where T : IMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Money Aud75 = new(75m, "AUD");
        private static readonly ImmutableArray<Money> DefaultSetValues = [Usd100, Cad50, Eur25];

        private readonly IMoneySet _set = T.Create(CurrencyRegistry.Default, DefaultSetValues);

        [TestMethod]
        public void SetValue_CurrencyDoesNotExist_AddsValue()
        {
            _set.SetValue(Aud75);
            _set.Count.ShouldBe(4);
            _set.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_CurrencyExists_UpdatesValue()
        {
            _set.SetValue(new(200m, "USD"));
            _set.Count.ShouldBe(3);
            _set.ShouldBe([new(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_DefaultValue_NoChange()
        {
            _set.SetValue(default);
            _set.Count.ShouldBe(3);
            _set.ShouldBe(DefaultSetValues, ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new Money(100, Common.CurrencyX);
            Should.Throw<ArgumentException>(() => _set.SetValue(value));
        }
    }
}