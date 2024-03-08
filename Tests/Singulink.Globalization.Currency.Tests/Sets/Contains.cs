namespace Singulink.Globalization.Tests.Sets;
public static class Contains
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
        public void Money_ValueExists_ReturnsTrue()
        {
            Set.Contains(Usd100).ShouldBeTrue();
            Set.Contains(Cad50).ShouldBeTrue();
            Set.Contains(Eur25).ShouldBeTrue();
        }

        [TestMethod]
        public void Money_ValueDoesNotExist_ReturnsFalse()
        {
            Set.Contains(Money.Create(0m, "GBP")).ShouldBeFalse();
            Set.Contains(Money.Create(0m, Common.CurrencyX)).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void AmountAndCurrency_ValueExists_ReturnsTrue()
        {
            Set.Contains(Usd100.Amount, Usd100.Currency).ShouldBeTrue();
            Set.Contains(Cad50.Amount, Cad50.Currency).ShouldBeTrue();
            Set.Contains(Eur25.Amount, Eur25.Currency).ShouldBeTrue();
        }

        [TestMethod]
        public void AmountAndCurrency_ValueDoesNotExist_ReturnsFalse()
        {
            Set.Contains(0m, Currency.Get("GBP")).ShouldBeFalse();
            Set.Contains(0m, Common.CurrencyX).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void AmountAndCurrencyCode_ValueExists_ReturnsTrue()
        {
            Set.Contains(Usd100.Amount, Usd100.Currency.CurrencyCode).ShouldBeTrue();
            Set.Contains(Cad50.Amount, Cad50.Currency.CurrencyCode).ShouldBeTrue();
            Set.Contains(Eur25.Amount, Eur25.Currency.CurrencyCode).ShouldBeTrue();
        }

        [TestMethod]
        public void AmountAndCurrencyCode_ValueDoesNotExist_ReturnsFalse()
        {
            Set.Contains(0m, "GBP").ShouldBeFalse();
            Set.Contains(0m, "XXXX").ShouldBeFalse();
        }
    }
}
