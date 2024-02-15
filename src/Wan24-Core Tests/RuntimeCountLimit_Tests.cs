using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class RuntimeCountLimit_Tests
    {
        public static int Limit { get; set; } = 123;

        [TestMethod]
        public void General_Tests()
        {
            RuntimeCountLimitAttribute attr = new("Wan24_Core_Tests.RuntimeCountLimit_Tests.Limit", "Wan24_Core_Tests.RuntimeCountLimit_Tests.Limit");
            Assert.AreEqual(Limit, attr.Min);
            Assert.AreEqual(Limit, attr.Max);
        }
    }
}
