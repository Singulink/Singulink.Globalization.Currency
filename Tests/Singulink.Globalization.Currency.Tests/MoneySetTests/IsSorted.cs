using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singulink.Globalization.Tests.MoneySetTests;
public static class IsSorted
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

        private static readonly IReadOnlyMoneySet SortedSet = T.Create(CurrencyRegistry.Default, [Cad50, Usd100, Eur25]);
        private static readonly IReadOnlyMoneySet UnsortedSet = T.Create(CurrencyRegistry.Default, [Usd100, Cad50, Eur25]);
        private static readonly IReadOnlyMoneySet EmptySet = T.Create(CurrencyRegistry.Default, []);

        [TestMethod]
        public void Sorted_IsTrue()
        {
            var sortedSet = SortedSet;
            var unsortedSet = UnsortedSet;
            var emptySet = EmptySet;

            sortedSet.IsSorted.ShouldBeTrue();
            unsortedSet.IsSorted.ShouldBeFalse();
            emptySet.IsSorted.ShouldBeTrue();
        }

        [TestMethod]
        public void Sorted_IsFalse()
        {
            var sortedSet = SortedSet;
            var unsortedSet = UnsortedSet;
            var emptySet = EmptySet;

            sortedSet.IsSorted.ShouldBeFalse();
            unsortedSet.IsSorted.ShouldBeTrue();
            emptySet.IsSorted.ShouldBeFalse();
        }
    }
}