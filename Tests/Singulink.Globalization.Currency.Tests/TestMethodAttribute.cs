using MVTU = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Singulink.Globalization.Tests
{
    public class TestMethodAttribute : MVTU.TestMethodAttribute
    {
        private readonly MVTU.TestMethodAttribute _testMethodAttribute;

        public TestMethodAttribute()
        {
            _testMethodAttribute = this;
        }

        public TestMethodAttribute(MVTU.TestMethodAttribute testMethodAttribute)
        {
            _testMethodAttribute = testMethodAttribute;
        }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var results = base.Execute(testMethod);

            foreach (var result in results)
            {
                string rootNameSpace = typeof(TestMethodAttribute).Namespace + ".";
                string className = testMethod.TestClassName;

                if (className.StartsWith(rootNameSpace))
                    className = className.Substring(rootNameSpace.Length);

                result.DisplayName = $"[{className}] {result.DisplayName ?? testMethod.TestMethodName}";
            }

            return results;
        }
    }
}