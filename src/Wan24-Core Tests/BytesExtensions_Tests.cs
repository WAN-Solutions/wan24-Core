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
    }
}
