using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests
{
    [TestClass]
    public class RoundToCurrencyDigitsTests
    {
        private static readonly Money _roundDownResult = new Money(10, "USD");
        private static readonly Money _roundDownValue = new Money(10.004m, "USD");
        private static readonly Money _midpointValue = new Money(10.005m, "USD");
        private static readonly Money _roundUpValue = new Money(10.006m, "USD");
        private static readonly Money _roundUpResult = new Money(10.01m, "USD");

        [TestMethod]
        public void ToEven()
        {
            const MidpointRounding mode = MidpointRounding.ToEven;
            _roundDownResult.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
            _roundDownValue.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
            _midpointValue.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
            _roundUpValue.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
            _roundUpResult.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
            Money.Default.RoundToCurrencyDigits(mode).ShouldBe(Money.Default);
        }

        [TestMethod]
        public void AwayFromZero()
        {
            const MidpointRounding mode = MidpointRounding.AwayFromZero;
            _roundDownResult.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
            _roundDownValue.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
            _midpointValue.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
            _roundUpValue.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
            _roundUpResult.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
            Money.Default.RoundToCurrencyDigits(mode).ShouldBe(Money.Default);
        }

        [TestMethod]
        public void Default()
        {
            _midpointValue.RoundToCurrencyDigits().ShouldBe(_roundDownResult);
        }
    }
}