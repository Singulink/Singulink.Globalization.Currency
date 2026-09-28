namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class CreateOrDefault
{
    [TestMethod]
    public void NonZeroAmountWithNullCurrency_Throws()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.CreateOrDefault(123, (Currency)null));
    }

    [TestMethod]
    public void NonZeroAmountWithNullCurrencyCode_Throws()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.CreateOrDefault(123, (string)null));
    }
}