namespace Singulink.Globalization.Tests.MoneySetTests;
public static class ContainsCurrency
{
    [PrefixTestClass]
    public class TMoneySet : Tests<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Tests<SortedMoneySet>;

    [PrefixTestClass]
    public class TImmutableMoneySet : Tests<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Tests<ImmutableSortedMoneySet>;

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private static readonly Money Usd100 = new(100m, "USD");
        private static readonly Money Cad50 = new(50m, "CAD");
        private static readonly Money Eur25 = new(25m, "EUR");

        private static readonly IReadOnlyMoneySet Set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void Currency_ValueExists_ReturnsTrue()
        {
            Set.ContainsCurrency(Usd100.Currency).ShouldBeTrue();
            Set.ContainsCurrency(Cad50.Currency).ShouldBeTrue();
            Set.ContainsCurrency(Eur25.Currency).ShouldBeTrue();
        }

        [TestMethod]
        public void Currency_ValueDoesNotExist_ReturnsFalse()
        {
            Set.ContainsCurrency(Currency.Get("GBP")).ShouldBeFalse();
            Set.ContainsCurrency(Common.CurrencyX).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void CurrencyCode_ValueExists_ReturnsTrue()
        {
            Set.ContainsCurrency(Usd100.Currency.CurrencyCode).ShouldBeTrue();
            Set.ContainsCurrency(Cad50.Currency.CurrencyCode).ShouldBeTrue();
            Set.ContainsCurrency(Eur25.Currency.CurrencyCode).ShouldBeTrue();
        }

        [TestMethod]
        public void CurrencyCode_ValueDoesNotExist_ReturnsFalse()
        {
            Set.ContainsCurrency("GBP").ShouldBeFalse();
            Set.ContainsCurrency(Common.CurrencyX).ShouldBeFalse();
        }
    }
}
