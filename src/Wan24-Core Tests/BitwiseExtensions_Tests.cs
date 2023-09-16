using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BitwiseExtensions_Tests : TestBase
    {
        [TestMethod]
        public void UInt8_Tests()
        {
            Assert.AreEqual((byte)2, ((byte)1).ShiftLeft(1));
            Assert.AreEqual((byte)1, ((byte)2).ShiftRight(1));
            Assert.IsTrue(((byte)1).HasFlags(1));
            Assert.IsFalse(((byte)2).HasFlags(1));
            Assert.AreEqual((byte)1, ((byte)0).AddFlags(1));
            Assert.AreEqual((byte)0, ((byte)1).RemoveFlags(1));
            Assert.AreEqual((sbyte)1, ((byte)1).ToSByte());
            Assert.AreEqual((short)1, ((byte)1).ToShort());
            Assert.AreEqual((ushort)1, ((byte)1).ToUShort());
            Assert.AreEqual(1, ((byte)1).ToInt());
            Assert.AreEqual((uint)1, ((byte)1).ToUInt());
            Assert.AreEqual(1, ((byte)1).ToLong());
            Assert.AreEqual((ulong)1, ((byte)1).ToULong());
        }

        [TestMethod]
        public void Int8_Tests()
        {
            Assert.AreEqual((sbyte)2, ((sbyte)1).ShiftLeft(1));
            Assert.AreEqual((sbyte)1, ((sbyte)2).ShiftRight(1));
            Assert.IsTrue(((sbyte)1).HasFlags(1));
            Assert.IsFalse(((sbyte)2).HasFlags(1));
            Assert.AreEqual((sbyte)1, ((sbyte)0).AddFlags(1));
            Assert.AreEqual((sbyte)0, ((sbyte)1).RemoveFlags(1));
            Assert.AreEqual((byte)1, ((sbyte)1).ToByte());
            Assert.AreEqual((short)1, ((sbyte)1).ToShort());
            Assert.AreEqual((ushort)1, ((sbyte)1).ToUShort());
            Assert.AreEqual(1, ((sbyte)1).ToInt());
            Assert.AreEqual((uint)1, ((sbyte)1).ToUInt());
            Assert.AreEqual(1, ((sbyte)1).ToLong());
            Assert.AreEqual((ulong)1, ((sbyte)1).ToULong());
        }

        [TestMethod]
        public void UInt16_Tests()
        {
            Assert.AreEqual((ushort)2, ((ushort)1).ShiftLeft(1));
            Assert.AreEqual((ushort)1, ((ushort)2).ShiftRight(1));
            Assert.IsTrue(((ushort)1).HasFlags(1));
            Assert.IsFalse(((ushort)2).HasFlags(1));
            Assert.AreEqual((ushort)1, ((ushort)0).AddFlags(1));
            Assert.AreEqual((ushort)0, ((ushort)1).RemoveFlags(1));
            Assert.AreEqual((byte)1, ((ushort)1).ToByte());
            Assert.AreEqual((sbyte)1, ((ushort)1).ToSByte());
            Assert.AreEqual((short)1, ((ushort)1).ToShort());
            Assert.AreEqual(1, ((ushort)1).ToInt());
            Assert.AreEqual((uint)1, ((ushort)1).ToUInt());
            Assert.AreEqual(1, ((ushort)1).ToLong());
            Assert.AreEqual((ulong)1, ((ushort)1).ToULong());
        }

        [TestMethod]
        public void Int16_Tests()
        {
            Assert.AreEqual((short)2, ((short)1).ShiftLeft(1));
            Assert.AreEqual((short)1, ((short)2).ShiftRight(1));
            Assert.IsTrue(((short)1).HasFlags(1));
            Assert.IsFalse(((short)2).HasFlags(1));
            Assert.AreEqual((short)1, ((short)0).AddFlags(1));
            Assert.AreEqual((short)0, ((short)1).RemoveFlags(1));
            Assert.AreEqual((sbyte)1, ((short)1).ToSByte());
            Assert.AreEqual((byte)1, ((short)1).ToByte());
            Assert.AreEqual((ushort)1, ((short)1).ToUShort());
            Assert.AreEqual(1, ((short)1).ToInt());
            Assert.AreEqual((uint)1, ((short)1).ToUInt());
            Assert.AreEqual(1, ((short)1).ToLong());
            Assert.AreEqual((ulong)1, ((short)1).ToULong());
        }

        [TestMethod]
        public void UInt32_Tests()
        {
            Assert.AreEqual((uint)2, ((uint)1).ShiftLeft(1));
            Assert.AreEqual((uint)1, ((uint)2).ShiftRight(1));
            Assert.IsTrue(((uint)1).HasFlags(1));
            Assert.IsFalse(((uint)2).HasFlags(1));
            Assert.AreEqual((uint)1, ((uint)0).AddFlags(1));
            Assert.AreEqual((uint)0, ((uint)1).RemoveFlags(1));
            Assert.AreEqual((byte)1, ((uint)1).ToByte());
            Assert.AreEqual((sbyte)1, ((uint)1).ToSByte());
            Assert.AreEqual((short)1, ((uint)1).ToShort());
            Assert.AreEqual((ushort)1, ((uint)1).ToUShort());
            Assert.AreEqual(1, ((uint)1).ToInt());
            Assert.AreEqual(1, ((uint)1).ToLong());
            Assert.AreEqual((ulong)1, ((uint)1).ToULong());
        }

        [TestMethod]
        public void Int32_Tests()
        {
            Assert.AreEqual(2, 1.ShiftLeft(1));
            Assert.AreEqual(1, 2.ShiftRight(1));
            Assert.IsTrue(1.HasFlags(1));
            Assert.IsFalse(2.HasFlags(1));
            Assert.AreEqual(1, 0.AddFlags(1));
            Assert.AreEqual(0, 1.RemoveFlags(1));
            Assert.AreEqual((sbyte)1, 1.ToSByte());
            Assert.AreEqual((byte)1, 1.ToByte());
            Assert.AreEqual((short)1, 1.ToShort());
            Assert.AreEqual((ushort)1, 1.ToUShort());
            Assert.AreEqual((uint)1, 1.ToUInt());
            Assert.AreEqual(1, 1.ToLong());
            Assert.AreEqual((ulong)1, 1.ToULong());
        }

        [TestMethod]
        public void UInt64_Tests()
        {
            Assert.AreEqual((ulong)2, ((ulong)1).ShiftLeft(1));
            Assert.AreEqual((ulong)1, ((ulong)2).ShiftRight(1));
            Assert.IsTrue(((ulong)1).HasFlags(1));
            Assert.IsFalse(((ulong)2).HasFlags(1));
            Assert.AreEqual((ulong)1, ((ulong)0).AddFlags(1));
            Assert.AreEqual((ulong)0, ((ulong)1).RemoveFlags(1));
            Assert.AreEqual((sbyte)1, ((ulong)1).ToSByte());
            Assert.AreEqual((byte)1, ((ulong)1).ToByte());
            Assert.AreEqual((short)1, ((ulong)1).ToShort());
            Assert.AreEqual((ushort)1, ((ulong)1).ToUShort());
            Assert.AreEqual(1, ((ulong)1).ToInt());
            Assert.AreEqual((uint)1, ((ulong)1).ToUInt());
            Assert.AreEqual(1, ((ulong)1).ToLong());
        }

        [TestMethod]
        public void Int64_Tests()
        {
            Assert.AreEqual(2, ((long)1).ShiftLeft(1));
            Assert.AreEqual(1, ((long)2).ShiftRight(1));
            Assert.IsTrue(((long)1).HasFlags(1));
            Assert.IsFalse(((long)2).HasFlags(1));
            Assert.AreEqual(1, ((long)0).AddFlags(1));
            Assert.AreEqual(0, ((long)1).RemoveFlags(1));
            Assert.AreEqual((byte)1, ((long)1).ToByte());
            Assert.AreEqual((byte)1, ((long)1).ToByte());
            Assert.AreEqual((short)1, ((long)1).ToShort());
            Assert.AreEqual((ushort)1, ((long)1).ToUShort());
            Assert.AreEqual(1, ((long)1).ToInt());
            Assert.AreEqual((uint)1, ((long)1).ToUInt());
            Assert.AreEqual((ulong)1, ((long)1).ToULong());
        }
    }
}
