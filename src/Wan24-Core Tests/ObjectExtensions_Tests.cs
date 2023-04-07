using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectExtensions_Tests
    {
        [TestMethod]
        public void In_Tests()
        {
            Assert.IsTrue(1.In(1, 2, 3));
            Assert.IsFalse(1.In(4, 5, 6));
        }

        [TestMethod]
        public void TypeConversion_Tests()
        {
            Assert.AreEqual(1, ((ushort)1).ConvertType<int>());
        }
    }
}
