using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class NumberExtensions_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            Assert.IsFalse(1.IsUnsigned());
            Assert.IsTrue(1u.IsUnsigned());
            Assert.IsTrue(typeof(int).IsNumeric());
            Assert.IsFalse(typeof(int).IsNumericAndUnsigned());
            Assert.IsTrue(typeof(uint).IsNumericAndUnsigned());
            Assert.IsTrue(1.IsBetween(0, 2));
            Assert.IsFalse(3.IsBetween(0, 2));
        }
    }
}
