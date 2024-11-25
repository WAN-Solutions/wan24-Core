using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StreamExtensions_Tests : TestBase
    {
        [TestMethod]
        public void GetRemainingBytes_Tests()
        {
            using MemoryPoolStream ms = new();
            ms.Write(RandomNumberGenerator.GetBytes(10));
            ms.Position = 3;
            Assert.AreEqual(7L, ms.GetRemainingBytes());
        }

        [TestMethod]
        public void CopyPartialTo_Tests()
        {
            using MemoryPoolStream ms1 = new();
            ms1.Write(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            Assert.AreEqual(0L, ms1.CopyPartialTo(ms2, count: 50));
            Assert.AreEqual(75L, ms1.Position);
            Assert.AreEqual(50L, ms2.Position);
            Assert.AreEqual(25L, ms1.CopyPartialTo(ms2, count: 50));
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public async Task CopyPartialToAsync_Tests()
        {
            using MemoryPoolStream ms1 = new();
            await ms1.WriteAsync(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            Assert.AreEqual(0L, await ms1.CopyPartialToAsync(ms2, count: 50));
            Assert.AreEqual(75L, ms1.Position);
            Assert.AreEqual(50L, ms2.Position);
            Assert.AreEqual(25L, await ms1.CopyPartialToAsync(ms2, count: 50));
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public void GenericSeek_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(100);
            using MemoryPoolStream ms = new();
            ms.Write(data);
            ms.GenericSeek(50, SeekOrigin.Begin);
            Assert.AreEqual(50L, ms.Position);
            Assert.AreEqual(data[50], (byte)ms.ReadByte());
            ms.GenericSeek(25, SeekOrigin.Current);
            Assert.AreEqual(76L, ms.Position);
            Assert.AreEqual(data[76], (byte)ms.ReadByte());
            ms.GenericSeek(-75, SeekOrigin.End);
            Assert.AreEqual(25L, ms.Position);
            Assert.AreEqual(data[25], (byte)ms.ReadByte());
        }

        [TestMethod]
        public void GenericCopyTo_Tests()
        {
            using MemoryPoolStream ms1 = new();
            ms1.Write(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            ms1.GenericCopyTo(ms2);
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public async Task GenericCopyToAsync_Tests()
        {
            using MemoryPoolStream ms1 = new();
            await ms1.WriteAsync(RandomNumberGenerator.GetBytes(100));
            ms1.Position = 25;
            using MemoryPoolStream ms2 = new();
            await ms1.GenericCopyToAsync(ms2);
            Assert.AreEqual(100L, ms1.Position);
            Assert.AreEqual(75L, ms2.Position);
        }

        [TestMethod]
        public void Serialization_Tests()
        {

        }
    }
}
