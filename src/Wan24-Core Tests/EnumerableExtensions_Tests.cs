using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class EnumerableExtensions_Tests : TestBase
    {
        [TestMethod]
        public void Combine_Tests()
        {
            bool[] test = new bool[][] { new bool[] { true }, new bool[] { false, true } }.Combine().ToArray();
            Assert.IsTrue(test[0]);
            Assert.IsFalse(test[1]);
            Assert.IsTrue(test[2]);
        }

        [TestMethod]
        public void ChunkEnum_Tests()
        {
            bool[][] test = new bool[] { true, true, false, false, true }.ChunkEnum(2).ToArray();
            Assert.AreEqual(3, test.Length);
            Assert.AreEqual(2, test[0].Length);
            Assert.AreEqual(2, test[1].Length);
            Assert.AreEqual(1, test[2].Length);
            Assert.IsTrue(test[0][0]);
            Assert.IsTrue(test[0][1]);
            Assert.IsFalse(test[1][0]);
            Assert.IsFalse(test[1][1]);
            Assert.IsTrue(test[2][0]);
        }
    }
}
