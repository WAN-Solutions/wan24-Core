using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectHelper_Tests : TestBase
    {
        [TestMethod]
        public void AreEqual_Tests()
        {
            string? a = "a",
                b = "b",
                c = null;
            Assert.IsTrue(ObjectHelper.AreEqual(a, a));
            Assert.IsFalse(ObjectHelper.AreEqual(a, b));
            Assert.IsFalse(ObjectHelper.AreEqual(a, c));
            Assert.IsFalse(ObjectHelper.AreEqual(c, b));
            Assert.IsTrue(ObjectHelper.AreEqual(c, c));
        }

        [TestMethod]
        public void IsNull_Tests()
        {
            Assert.IsTrue(ObjectHelper.IsNull(null));
            Assert.IsFalse(ObjectHelper.IsNull(false));
        }
    }
}
