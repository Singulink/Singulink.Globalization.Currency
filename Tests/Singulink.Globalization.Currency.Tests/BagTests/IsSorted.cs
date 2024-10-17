namespace Singulink.Globalization.Tests.BagTests;

[PrefixTestClass]
public class IsSorted
{
    [TestMethod]
    public void UnsortedBags_GetsFalse()
    {
        IReadOnlyMoneyBag sortedMoneyBag = new MoneyBag();
        IReadOnlyMoneyBag immutableSortedMoneyBag = ImmutableMoneyBag.Create();

        sortedMoneyBag.IsSorted.ShouldBeFalse();
        immutableSortedMoneyBag.IsSorted.ShouldBeFalse();
    }

    [TestMethod]
    public void SortedBags_GetsTrue()
    {
        IReadOnlyMoneyBag sortedMoneyBag = new SortedMoneyBag();
        IReadOnlyMoneyBag immutableSortedMoneyBag = ImmutableSortedMoneyBag.Create();

        sortedMoneyBag.IsSorted.ShouldBeTrue();
        immutableSortedMoneyBag.IsSorted.ShouldBeTrue();
    }
}