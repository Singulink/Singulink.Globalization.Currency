namespace Singulink.Globalization.Tests.BagTests;

[PrefixTestClass]
public class CollectionExpressions
{
    private static readonly ImmutableArray<MonetaryValue> Values
        = [MonetaryValue.Create(456, "EUR"), MonetaryValue.Create(789, "JPY"), MonetaryValue.Create(123, "USD")];

    [TestMethod]
    public void CreateMoneyBag()
    {
        MoneyBag bag = [..Values];
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateSortedMoneyBag()
    {
        SortedMoneyBag bag = [.. Values];
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateImmutableMoneyBag()
    {
        ImmutableMoneyBag bag = [.. Values];
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateImmutableSortedMoneyBag()
    {
        ImmutableSortedMoneyBag bag = [.. Values];
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateIReadOnlyMoneyBag()
    {
        IReadOnlyMoneyBag bag = [.. Values];
        bag.ShouldBeOfType<MoneyBag>();
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateIMoneyBag()
    {
        IMoneyBag bag = [.. Values];
        bag.ShouldBeOfType<MoneyBag>();
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void CreateIImmutableMoneyBag()
    {
        IImmutableMoneyBag bag = [.. Values];
        bag.ShouldBeOfType<ImmutableMoneyBag>();
        bag.ShouldBe(Values, ignoreOrder: true);
    }
}
