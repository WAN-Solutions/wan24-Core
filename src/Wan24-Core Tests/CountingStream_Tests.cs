using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CountingStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryPoolStream ms = new();
            using CountingStream counter = new(ms);
            byte[] data = RandomNumberGenerator.GetBytes(10);
            counter.Write(data);
            Assert.AreEqual(data.LongLength, ms.Length);
            Assert.AreEqual(ms.Length, counter.Length);
            Assert.AreEqual(data.LongLength, counter.Written);
            Assert.AreEqual(0, counter.Red);
            counter.Position = 0;
            Assert.AreEqual(ms.Position, counter.Position);
            byte[] temp = new byte[data.Length];
            Assert.AreEqual(temp.Length, counter.Read(temp));
            Assert.IsTrue(data.SequenceEqual(temp));
            Assert.AreEqual(temp.LongLength, ms.Length);
            Assert.AreEqual(ms.Position, counter.Position);
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            using MemoryPoolStream ms = new();
            using CountingStream counter = new(ms);
            byte[] data = RandomNumberGenerator.GetBytes(10);
            await counter.WriteAsync(data);
            Assert.AreEqual(data.LongLength, ms.Length);
            Assert.AreEqual(ms.Length, counter.Length);
            Assert.AreEqual(data.LongLength, counter.Written);
            Assert.AreEqual(0, counter.Red);
            counter.Position = 0;
            Assert.AreEqual(ms.Position, counter.Position);
            byte[] temp = new byte[data.Length];
            Assert.AreEqual(temp.Length, await counter.ReadAsync(temp));
            Assert.IsTrue(data.SequenceEqual(temp));
            Assert.AreEqual(temp.LongLength, ms.Length);
            Assert.AreEqual(ms.Position, counter.Position);
        }
    }
}
