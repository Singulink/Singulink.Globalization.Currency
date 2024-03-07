using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singulink.Globalization.Tests.MoneySetTests;
public static class ContainsCurrency
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
        public void Currency_ValueExists_ReturnsTrue()
        {
            Set.ContainsCurrency(Usd100.Currency).ShouldBeTrue();

            Set.ContainsCurrency(Cad50.Currency).ShouldBeTrue();

            Set.ContainsCurrency(Eur25.Currency).ShouldBeTrue();
        }

        [TestMethod]
        public void Currency_ValueDoesNotExist_ReturnsFalse()
        {
            var newCurrency = new Currency("New Currency", "XXXX", "X", 2);

            Set.ContainsCurrency(Currency.Get("GBP")).ShouldBeFalse();
            Set.ContainsCurrency(newCurrency).ShouldBeFalse();
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
            var newCurrency = new Currency("New Currency", "XXXX", "X", 2);

            Set.ContainsCurrency("GBP").ShouldBeFalse();
            Set.ContainsCurrency(newCurrency).ShouldBeFalse();
        }
    }
}
