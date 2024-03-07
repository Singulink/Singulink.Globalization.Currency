using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Singulink.Globalization.Tests.MoneySetTests;
public static class Contains
{
    [PrefixTestClass]
    public class Set : Tests<MoneySet> { }

    [PrefixTestClass]
    public class SortedSet : Tests<SortedMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSet : Tests<ImmutableMoneySet> { }

    [PrefixTestClass]
    public class ImmutableSortedSet : Tests<ImmutableSortedMoneySet> { }

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
            var newCurrency = new Currency("New Currency", "XXXX", "X", 2);

            Set.Contains(Money.Create(0m, "GBP")).ShouldBeFalse();
            Set.Contains(Money.Create(0m, newCurrency)).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void CurrencyAmount_ValueExists_ReturnsTrue()
        {
            Set.Contains(Usd100.Amount, Usd100.Currency).ShouldBeTrue();

            Set.Contains(Cad50.Amount, Cad50.Currency).ShouldBeTrue();

            Set.Contains(Eur25.Amount, Eur25.Currency).ShouldBeTrue();
        }

        [TestMethod]
        public void CurrencyAmount_ValueDoesNotExist_ReturnsFalse()
        {
            var newCurrency = new Currency("New Currency", "XXXX", "X", 2);

            Set.Contains(0m, Currency.Get("GBP")).ShouldBeFalse();
            Set.Contains(0m, newCurrency).ShouldBeFalse();
        }

        ///////////////////////////

        [TestMethod]
        public void CurrencyCodeAmount_ValueExists_ReturnsTrue()
        {
            Set.Contains(Usd100.Amount, Usd100.Currency.CurrencyCode).ShouldBeTrue();

            Set.Contains(Cad50.Amount, Cad50.Currency.CurrencyCode).ShouldBeTrue();

            Set.Contains(Eur25.Amount, Eur25.Currency.CurrencyCode).ShouldBeTrue();
        }

        [TestMethod]
        public void CurrencyCodeAmount_ValueDoesNotExist_ReturnsFalse()
        {
            Set.Contains(0m, "GBP").ShouldBeFalse();
            Set.Contains(0m, "XXXX").ShouldBeFalse();
        }
    }
}
