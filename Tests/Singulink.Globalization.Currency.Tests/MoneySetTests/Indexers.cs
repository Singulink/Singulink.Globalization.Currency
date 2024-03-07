using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singulink.Globalization.Tests.MoneySetTests;
public static class Indexers
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

        private static readonly IReadOnlyMoneySet _set = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);

        [TestMethod]
        public void Currency_ValueExists_ReturnsValue()
        {
            _set[Currency.Get("USD")].ShouldBe(Usd100);

            _set[Currency.Get("CAD")].ShouldBe(Cad50);

            _set[Currency.Get("EUR")].ShouldBe(Eur25);
        }

        [TestMethod]
        public void Currency_ValueDoesNotExist_ReturnsDefault()
        {
            var newCurrency = new Currency("New Currency", "XXXX", "X", 2);

            _set[Currency.Get("GBP")].IsDefault.ShouldBeTrue();
            _set[newCurrency].IsDefault.ShouldBeTrue();
        }

        ///////////////////////////

        [TestMethod]
        public void CurrencyCode_ValueExists_ReturnsValue()
        {
            _set["USD"].ShouldBe(Usd100);

            _set["CAD"].ShouldBe(Cad50);

            _set["EUR"].ShouldBe(Eur25);
        }

        [TestMethod]
        public void CurrencyCode_ValueDoesNotExist_ReturnsDefault()
        {
            _set["GBP"].IsDefault.ShouldBeTrue();
            _set["XXXX"].IsDefault.ShouldBeTrue();
        }
    }
}
