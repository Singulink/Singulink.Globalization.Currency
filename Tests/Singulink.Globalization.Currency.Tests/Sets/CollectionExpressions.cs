namespace Singulink.Globalization.Tests.Sets;

[PrefixTestClass]
public class CollectionExpressions
{
    private static readonly ImmutableArray<Money> SetValues = [Money.Create(123, "USD"), Money.Create(456, "EUR"), Money.Create(789, "JPY")];

    [TestMethod]
    public void CreateMoneySet()
    {
        MoneySet set = [..SetValues];
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateSortedMoneySet()
    {
        SortedMoneySet set = [.. SetValues];
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateImmutableMoneySet()
    {
        ImmutableMoneySet set = [.. SetValues];
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateImmutableSortedMoneySet()
    {
        ImmutableSortedMoneySet set = [.. SetValues];
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateIReadOnlyMoneySet()
    {
        IReadOnlyMoneySet set = [.. SetValues];
        set.ShouldBeOfType<MoneySet>();
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateIMoneySet()
    {
        IMoneySet set = [.. SetValues];
        set.ShouldBeOfType<MoneySet>();
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateIImmutableMoneySet()
    {
        IImmutableMoneySet set = [.. SetValues];
        set.ShouldBeOfType<ImmutableMoneySet>();
        set.ShouldBe(SetValues, ignoreOrder: true);
    }
}
