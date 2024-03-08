namespace Singulink.Globalization.Tests.MoneySetTests;

[PrefixTestClass]
public class IsSorted
{
    [TestMethod]
    public void Sorted_UnsortedSets_IsFalse()
    {
        IReadOnlyMoneySet sortedMoneySet = new MoneySet();
        IReadOnlyMoneySet immutableSortedMoneySet = ImmutableMoneySet.Create();

        sortedMoneySet.IsSorted.ShouldBeFalse();
        immutableSortedMoneySet.IsSorted.ShouldBeFalse();
    }

    [TestMethod]
    public void Sorted_SortedSets_IsTrue()
    {
        IReadOnlyMoneySet sortedMoneySet = new SortedMoneySet();
        IReadOnlyMoneySet immutableSortedMoneySet = ImmutableSortedMoneySet.Create();

        sortedMoneySet.IsSorted.ShouldBeTrue();
        immutableSortedMoneySet.IsSorted.ShouldBeTrue();
    }
}