namespace Singulink.Globalization.Tests.Sets;

[PrefixTestClass]
public class IsSorted
{
    [TestMethod]
    public void UnsortedSets_GetsFalse()
    {
        IReadOnlyMoneySet sortedMoneySet = new MoneySet();
        IReadOnlyMoneySet immutableSortedMoneySet = ImmutableMoneySet.Create();

        sortedMoneySet.IsSorted.ShouldBeFalse();
        immutableSortedMoneySet.IsSorted.ShouldBeFalse();
    }

    [TestMethod]
    public void SortedSets_GetsTrue()
    {
        IReadOnlyMoneySet sortedMoneySet = new SortedMoneySet();
        IReadOnlyMoneySet immutableSortedMoneySet = ImmutableSortedMoneySet.Create();

        sortedMoneySet.IsSorted.ShouldBeTrue();
        immutableSortedMoneySet.IsSorted.ShouldBeTrue();
    }
}