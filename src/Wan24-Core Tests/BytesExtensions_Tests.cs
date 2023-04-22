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

        [TestMethod]
        public void BitConverter_Tests()
        {
            int value = 123;
            Assert.AreEqual((short)value, ((short)value).GetBytes().AsSpan().ToShort());
            Assert.AreEqual((ushort)value, ((ushort)value).GetBytes().AsSpan().ToUShort());
            Assert.AreEqual(value, value.GetBytes().AsSpan().ToInt());
            Assert.AreEqual((uint)value, ((uint)value).GetBytes().AsSpan().ToUInt());
            Assert.AreEqual(value, ((long)value).GetBytes().AsSpan().ToLong());
            Assert.AreEqual((ulong)value, ((ulong)value).GetBytes().AsSpan().ToULong());
            Assert.AreEqual(value, ((float)value).GetBytes().AsSpan().ToFloat());
            Assert.AreEqual(value, ((double)value).GetBytes().AsSpan().ToDouble());
            Assert.AreEqual(value, ((decimal)value).GetBytes().AsSpan().ToDecimal());
        }

        [TestMethod]
        public void Clear_Tests()
        {
            byte[] arr = new byte[] { 1, 2, 3 },
                temp = (byte[])arr.Clone();
            Assert.IsTrue(arr.SequenceEqual(temp));
            temp.Clear();
            Assert.IsFalse(arr.SequenceEqual(temp));
            Assert.IsTrue(temp.SequenceEqual(Enumerable.Repeat((byte)0, 3)));
        }
    }
}
