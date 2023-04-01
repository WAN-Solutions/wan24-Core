using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BytesExtensions_Tests
    {
        [TestMethod]
        public void Endian_Tests()
        {
            int test = 12345;
            byte[] bits = BitConverter.GetBytes(test);
            Assert.AreEqual(test, BitConverter.ToInt32(bits.AsSpan().ConvertEndian().ConvertEndian()));
            Assert.AreEqual(test, BitConverter.ToInt32(bits.AsMemory().ConvertEndian().ConvertEndian().Span));
        }

        [TestMethod]
        public void SlowCompare_Tests()
        {
            byte[] a = new byte[] { 0, 1, 2 },
                b = new byte[] { 0, 1, 2 };
            Assert.IsTrue(a.AsSpan().SlowCompare(b));
            Assert.IsTrue(b.AsSpan().SlowCompare(a));
            b[0] = 1;
            Assert.IsFalse(a.AsSpan().SlowCompare(b));
            Assert.IsFalse(b.AsSpan().SlowCompare(a));
            b = new byte[] { 0, 1 };
            Assert.IsFalse(a.AsSpan().SlowCompare(b));
            Assert.IsFalse(b.AsSpan().SlowCompare(a));
            a = b;
            b = new byte[] { 0, 1, 2 };
            Assert.IsFalse(a.AsSpan().SlowCompare(b));
            Assert.IsFalse(b.AsSpan().SlowCompare(a));
        }
    }
}
