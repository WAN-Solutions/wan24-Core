using System.Diagnostics;
using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BytesExtensions_Tests : TestBase
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
            Assert.IsTrue(a.SlowCompare(b));
            Assert.IsTrue(b.SlowCompare(a));
            b[0] = 1;
            Assert.IsFalse(a.SlowCompare(b));
            Assert.IsFalse(b.SlowCompare(a));
            b = new byte[] { 0, 1 };
            Assert.IsFalse(a.SlowCompare(b));
            Assert.IsFalse(b.SlowCompare(a));
            a = b;
            b = new byte[] { 0, 1, 2 };
            Assert.IsFalse(a.SlowCompare(b));
            Assert.IsFalse(b.SlowCompare(a));
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
        public void BitConverterBufferSpan_Tests()
        {
            int value = 123;
            byte[] buffer = new byte[20];
            Assert.AreEqual((short)value, ((short)value).GetBytes(buffer.AsSpan()).ToShort());
            Assert.AreEqual((ushort)value, ((ushort)value).GetBytes(buffer.AsSpan()).ToUShort());
            Assert.AreEqual(value, value.GetBytes(buffer.AsSpan()).ToInt());
            Assert.AreEqual((uint)value, ((uint)value).GetBytes(buffer.AsSpan()).ToUInt());
            Assert.AreEqual(value, ((long)value).GetBytes(buffer.AsSpan()).ToLong());
            Assert.AreEqual((ulong)value, ((ulong)value).GetBytes(buffer.AsSpan()).ToULong());
            Assert.AreEqual(value, ((float)value).GetBytes(buffer.AsSpan()).ToFloat());
            Assert.AreEqual(value, ((double)value).GetBytes(buffer.AsSpan()).ToDouble());
            Assert.AreEqual(value, ((decimal)value).GetBytes(buffer.AsSpan()).ToDecimal());
        }

        [TestMethod]
        public void BitConverterBufferMemory_Tests()
        {
            int value = 123;
            byte[] buffer = new byte[20];
            Assert.AreEqual((short)value, ((short)value).GetBytes(buffer.AsMemory()).Span.ToShort());
            Assert.AreEqual((ushort)value, ((ushort)value).GetBytes(buffer.AsMemory()).Span.ToUShort());
            Assert.AreEqual(value, value.GetBytes(buffer.AsMemory()).Span.ToInt());
            Assert.AreEqual((uint)value, ((uint)value).GetBytes(buffer.AsMemory()).Span.ToUInt());
            Assert.AreEqual(value, ((long)value).GetBytes(buffer.AsMemory()).Span.ToLong());
            Assert.AreEqual((ulong)value, ((ulong)value).GetBytes(buffer.AsMemory()).Span.ToULong());
            Assert.AreEqual(value, ((float)value).GetBytes(buffer.AsMemory()).Span.ToFloat());
            Assert.AreEqual(value, ((double)value).GetBytes(buffer.AsMemory()).Span.ToDouble());
            Assert.AreEqual(value, ((decimal)value).GetBytes(buffer.AsMemory()).Span.ToDecimal());
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
            char[] carr = new char[] { 'a', 'b', 'c' },
                ctemp = (char[])carr.Clone();
            Assert.IsTrue(carr.SequenceEqual(ctemp));
            ctemp.Clear();
            Assert.IsFalse(carr.SequenceEqual(ctemp));
            Assert.IsTrue(ctemp.SequenceEqual(Enumerable.Repeat((char)0, 3)));
        }

        [TestMethod]
        public void Xor_Tests()
        {
            byte[] a = Enumerable.Range(0, 128).Select(b => (byte)b).ToArray(),
                b = Enumerable.Range(128, 128).Select(b => (byte)b).ToArray(),
                c = Enumerable.Range(0, 128).Select(i => (byte)(a[i] ^ b[i])).ToArray();
            a.Xor(b);
            Assert.IsTrue(a.SequenceEqual(c));
        }

        [TestMethod]
        public void And_Tests()
        {
            byte[] a = Enumerable.Range(0, 128).Select(b => (byte)b).ToArray(),
                b = Enumerable.Range(128, 128).Select(b => (byte)b).ToArray(),
                c = Enumerable.Range(0, 128).Select(i => (byte)(a[i] & b[i])).ToArray();
            a.And(b);
            Assert.IsTrue(a.SequenceEqual(c));
        }

        [TestMethod]
        public void Or_Tests()
        {
            byte[] a = Enumerable.Range(0, 128).Select(b => (byte)b).ToArray(),
                b = Enumerable.Range(128, 128).Select(b => (byte)b).ToArray(),
                c = Enumerable.Range(0, 128).Select(i => (byte)(a[i] | b[i])).ToArray();
            a.Or(b);
            Assert.IsTrue(a.SequenceEqual(c));
        }

        [TestMethod]
        public void Compare_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(1024 * 80 - 1),
                data2 = (byte[])data.Clone();
            data2[^1] = (byte)~data2[^1];
            Assert.IsTrue(data.SlowCompare(data));
            Assert.IsFalse(data.SlowCompare(data2));
            data2[^1] = data[^1];
            data2[0] = (byte)~data2[0];
            Assert.IsFalse(data.SlowCompare(data2));
        }
    }
}
