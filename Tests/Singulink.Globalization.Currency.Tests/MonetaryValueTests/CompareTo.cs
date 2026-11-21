namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class CompareTo
{
    private static readonly MonetaryValue Usd100 = new(100m, "USD");
    private static readonly MonetaryValue Usd200 = new(200m, "USD");

    [TestMethod]
    public void LessThan_MinusOneResult()
    {
        Usd100.CompareTo(Usd200).ShouldBe(-1);
    }

    [TestMethod]
    public void GreaterThan_PlusOneResult()
    {
        Usd200.CompareTo(Usd100).ShouldBe(1);
    }

    [TestMethod]
    public void Equal_ZeroResult()
    {
        Usd100.CompareTo(Usd100).ShouldBe(0);
    }

    [TestMethod]
    public void Object_ComparesAsGenericMethod()
    {
        Usd100.CompareTo((object)Usd200).ShouldBe(-1);
        Usd200.CompareTo((object)Usd100).ShouldBe(1);
        Usd100.CompareTo((object)Usd100).ShouldBe(0);
        Should.Throw<ArgumentException>(() => Usd100.CompareTo((object)new MonetaryValue(200m, "CAD")));
    }

    [TestMethod]
    public void Object_NullIsFirst()
    {
        Usd100.CompareTo(null).ShouldBe(1);
        System.Collections.Comparer.Default.Compare(null, Usd100).ShouldBe(-1);
    }

    [TestMethod]
    public void Object_OtherType_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd100.CompareTo(100m)).Message.ShouldStartWith("Object must be of type MonetaryValue.");
    }

    [TestMethod]
    public void NonGenericComparer_Sorts()
    {
        var list = new System.Collections.ArrayList { Usd200, null, Usd100 };
        list.Sort(System.Collections.Comparer.Default);

        list[0].ShouldBeNull();
        list[1].ShouldBe(Usd100);
        list[2].ShouldBe(Usd200);
    }

    [TestMethod]
    public void DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd100.CompareTo(new MonetaryValue(200m, "CAD")));
    }
}