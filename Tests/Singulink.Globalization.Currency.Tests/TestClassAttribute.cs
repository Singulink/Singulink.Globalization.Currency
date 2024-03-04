using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVTU = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Singulink.Globalization.Tests
{
    public class TestClassAttribute : MVTU.TestClassAttribute
    {
        public override MVTU.TestMethodAttribute? GetTestMethodAttribute(MVTU.TestMethodAttribute? testMethodAttribute)
        {
            var attribute = base.GetTestMethodAttribute(testMethodAttribute);

            if (attribute == null)
                return null;

            return attribute as TestMethodAttribute ?? new TestMethodAttribute(attribute);
        }
    }
}