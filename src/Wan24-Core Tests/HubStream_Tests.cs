using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class HubStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryPoolStream ms1 = new();
            using MemoryPoolStream ms2 = new();
            using HubStream hub = new(ms1, ms2);
            hub.WriteByte(1);
            Assert.AreEqual(1L, ms1.Length);
            Assert.AreEqual(ms1.Length, ms2.Length);
            Assert.AreEqual(1L, ms1.Position);
            Assert.AreEqual(ms1.Length, ms2.Length);
            hub.SetLength(2);
            Assert.AreEqual(2L, ms1.Length);
            Assert.AreEqual(ms1.Length, ms2.Length);
            Assert.AreEqual(1L, ms1.Position);
            Assert.AreEqual(ms1.Length, ms2.Length);
            byte[] data = RandomNumberGenerator.GetBytes(10);
            hub.Write(data);
            Assert.AreEqual(11L, ms1.Length);
            Assert.AreEqual(ms1.Length, ms2.Length);
            Assert.AreEqual(11L, ms1.Position);
            Assert.AreEqual(ms1.Length, ms2.Length);
            Assert.IsTrue(ms1.ToArray().SequenceEqual(ms2.ToArray()));
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            using MemoryPoolStream ms1 = new();
            using MemoryPoolStream ms2 = new();
            using HubStream hub = new(ms1, ms2);
            byte[] data = RandomNumberGenerator.GetBytes(10);
            await hub.WriteAsync(data);
            Assert.AreEqual(10L, ms1.Length);
            Assert.AreEqual(ms1.Length, ms2.Length);
            Assert.AreEqual(10L, ms1.Position);
            Assert.AreEqual(ms1.Length, ms2.Length);
            Assert.IsTrue(ms1.ToArray().SequenceEqual(ms2.ToArray()));
        }
    }
}
