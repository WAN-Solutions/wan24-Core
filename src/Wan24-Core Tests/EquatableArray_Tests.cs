using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class EquatableArray_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            byte[] array = new byte[] { 1, 2, 3 },
                b = new byte[] { 4, 5, 6 },
                c = new byte[] { 1, 2, 3 };
            EquatableArray<byte> eArray = array,
                eC = c;
            Assert.IsTrue(array == eArray);
            Assert.IsTrue(eArray == array);
            Assert.IsTrue(c == eArray);
            Assert.IsTrue(eArray == c);
            Assert.IsFalse(eArray == b);
            Assert.IsFalse(b == eArray);
            Assert.IsTrue(eArray == eC);
            Assert.IsTrue(eC == eArray);
            Assert.IsTrue(array.SequenceEqual((byte[])eC));
            Assert.IsFalse(b.SequenceEqual((byte[])eArray));
        }
    }
}
