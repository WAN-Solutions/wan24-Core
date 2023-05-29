using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Bitmap_Tests
    {
        [TestMethod]
        public void Bits_Tests()
        {
            Bitmap bmp = new();
            Assert.AreEqual(0, bmp.BitCount);
            bmp.AddBits(true);
            Assert.AreEqual(1, bmp.BitCount);
            Assert.IsTrue(bmp[0]);
            Assert.AreEqual(1u, bmp.ToUInt());
            bmp.AddBits(true);
            Assert.AreEqual(2, bmp.BitCount);
            Assert.IsTrue(bmp[0]);
            Assert.IsTrue(bmp[1]);
            Assert.AreEqual(3u, bmp.ToUInt());
            bmp[0] = false;
            Assert.IsFalse(bmp[0]);
            Assert.IsTrue(bmp[1]);
            Assert.AreEqual(2u, bmp.ToUInt());
        }

        [TestMethod]
        public void Casting_Tests()
        {
            Bitmap bmp = 123;
            Assert.AreEqual((byte)123, bmp.ToByte());
            Assert.AreEqual((sbyte)123, bmp.ToSByte());
            Assert.AreEqual((ushort)123, bmp.ToUShort());
            Assert.AreEqual((short)123, bmp.ToShort());
            Assert.AreEqual((uint)123, bmp.ToUInt());
            Assert.AreEqual(123, bmp.ToInt());
            Assert.AreEqual((ulong)123, bmp.ToULong());
            Assert.AreEqual(123, bmp.ToLong());
        }
    }
}
