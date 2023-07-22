using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StreamExtensions_Tests
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
    }
}
