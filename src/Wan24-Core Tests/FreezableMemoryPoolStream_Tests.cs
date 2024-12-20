﻿using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class FreezableMemoryPoolStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using FreezableMemoryPoolStream ms = new();
            ms.WriteByte(0);
            Assert.AreEqual(1L, ms.Length);
            Assert.AreEqual(1L, ms.Position);
            Assert.AreEqual(1, ms.BufferCount);
            byte[] data = new byte[200000];
            Random.Shared.NextBytes(data);
            ms.Write(data);
            Assert.AreEqual(200001L, ms.Length);
            Assert.AreEqual(200001L, ms.Position);
            byte[] temp = new byte[50000];
            ms.Position = 50000;
            Assert.AreEqual(temp.Length, ms.Read(temp));
            Assert.AreEqual(100000L, ms.Position);
            Assert.IsTrue(temp.SequenceEqual(data.Skip(49999).Take(50000)));
            ms.WriteByte(0);
            Assert.AreEqual(100001L, ms.Position);
            ms.Freeze();
            Assert.IsFalse(ms.CanWrite);
        }

        [TestMethod]
        public void SetLength_Tests()
        {
            using FreezableMemoryPoolStream ms = new();
            Assert.AreEqual(0L, ms.Length);
            ms.SetLength(1);
            Assert.AreEqual(1L, ms.Length);
            Assert.AreEqual(0L, ms.Position);
            Assert.AreEqual(0, ms.ReadByte());
            ms.SetLength(ms.BufferLength + 1);
            Assert.AreEqual(1L, ms.Position);
            Assert.AreEqual(2, ms.BufferCount);
            byte[] temp = new byte[ms.Length - 1];
            Assert.AreEqual(temp.Length, ms.Read(temp));
            Assert.IsTrue(temp.All(b => b == 0));
            ms.SetLength(1);
            Assert.AreEqual(1L, ms.Length);
            Assert.AreEqual(1L, ms.Position);
        }

        [TestMethod]
        public void WriteByte_Tests()
        {
            using FreezableMemoryPoolStream ms = new(bufferSize: 2);
            ms.WriteByte(1);
            Assert.AreEqual(1L, ms.Length);
            Assert.AreEqual(1L, ms.Position);
            Assert.AreEqual(1, ms.BufferCount);
            long bl = ms.BufferLength;
            ms.SetLength(bl);
            ms.Position = ms.Length;
            ms.WriteByte(1);
            Assert.AreEqual(bl + 1, ms.Length);
            Assert.AreEqual(bl + 1, ms.Position);
            Assert.AreEqual(2, ms.BufferCount);
            ms.WriteByte(1);
            Assert.AreEqual(bl + 2, ms.Length);
            Assert.AreEqual(bl + 2, ms.Position);
            Assert.AreEqual(2, ms.BufferCount);
        }

        [TestMethod]
        public void ReadByte_Tests()
        {
            using FreezableMemoryPoolStream ms = new(bufferSize: 2);
            Assert.AreEqual(-1, ms.ReadByte());
            ms.WriteByte(1);
            ms.Position = 0;
            Assert.AreEqual(1, ms.ReadByte());
            long bl = ms.BufferLength;
            ms.SetLength(bl);
            ms.Position = ms.Length - 1;
            Assert.AreEqual(0, ms.ReadByte());
            Assert.AreEqual(-1, ms.ReadByte());
            ms.SetLength(bl + 1);
            Assert.AreEqual(0, ms.ReadByte());
            Assert.AreEqual(-1, ms.ReadByte());
        }

        [TestMethod]
        public void ToArray_Tests()
        {
            using FreezableMemoryPoolStream ms = new();
            Assert.AreEqual(0, ms.ToArray().Length);
            byte[] data = new byte[200000];
            Random.Shared.NextBytes(data);
            ms.Write(data);
            Assert.IsTrue(data.SequenceEqual(ms.ToArray()));
        }
    }
}
