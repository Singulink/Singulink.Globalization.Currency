namespace Singulink.Globalization.Tests.MoneySetTests;

public static partial class SetValue
{
    [PrefixTestClass]
    public class TImmutableMoneySet : Immutable<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Immutable<ImmutableSortedMoneySet>;

    public class Immutable<T> where T : IImmutableMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");
        private static readonly Money Aud75 = new(75m, "AUD");
        private static readonly IImmutableMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void SetValue_CurrencyDoesNotExist_AddsValue()
        {
            var resultSet = Set.SetValue(Aud75);
            resultSet.Count.ShouldBe(4);
            resultSet.ShouldBe([Cad50, Eur25, Usd100, Aud75], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_CurrencyExists_UpdatesValue()
        {
            var resultSet = Set.SetValue(new(200m, "USD"));
            resultSet.Count.ShouldBe(3);
            resultSet.ShouldBe([new(200m, "USD"), Cad50, Eur25], ignoreOrder: true);
        }

        [TestMethod]
        public void SetValue_DefaultValue_NoChange()
        {
            var resultSet = Set.SetValue(default);
            resultSet.ShouldBeSameAs(Set);
        }

        [TestMethod]
        public void SetValue_ValueAlreadyExists_NoChange()
        {
            var resultSet = Set.SetAmount(100m, "USD");
            resultSet.ShouldBeSameAs(Set);
        }

        [TestMethod]
        public void SetValue_CurrencyDisallowed_ThrowsArgumentException()
        {
            var value = new Money(100, Common.CurrencyX);
            Should.Throw<ArgumentException>(() => Set.SetValue(value));
        }
    }
}