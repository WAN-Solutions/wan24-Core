using wan24.Core;
using wan24.Tests;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class RuntimeCountLimit_Tests : TestBase
    {
        public static int MinLimit { get; set; } = 123;

        public static int MaxLimit { get; set; } = 999;

        [TestMethod]
        public void General_Tests()
        {
            RuntimeCountLimitAttribute attr = new("Wan24_Core_Tests.RuntimeCountLimit_Tests.MaxLimit", "Wan24_Core_Tests.RuntimeCountLimit_Tests.MinLimit");
            Assert.AreEqual(MinLimit, attr.Min);
            Assert.AreEqual(MaxLimit, attr.Max);
        }
    }
}
