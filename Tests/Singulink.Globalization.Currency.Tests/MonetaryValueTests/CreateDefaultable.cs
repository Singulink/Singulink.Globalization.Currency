namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class CreateDefaultable
{
    [TestMethod]
    public void NonZeroAmountWithNullCurrency_Throws()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.CreateDefaultable(123, (Currency)null));
    }

    [TestMethod]
    public void NonZeroAmountWithNullCurrencyCode_Throws()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.CreateDefaultable(123, (string)null));
    }
}