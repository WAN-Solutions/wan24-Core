using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ThrottledStream_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(250);
            using MemoryPoolStream ms = new();
            using ThrottledStream stream = new(ms, readCount: 100, TimeSpan.FromMilliseconds(200), writeCount: 50, TimeSpan.FromMilliseconds(100));
            DateTime start = DateTime.Now;
            stream.Write(data);
            Logging.WriteInfo($"Write time: {(DateTime.Now - start).TotalMilliseconds}");
            Assert.IsTrue(DateTime.Now - start >= TimeSpan.FromMilliseconds(375));
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(500));
            stream.Position = 0;
            start = DateTime.Now;
            Assert.AreEqual(data.Length, stream.Read(data));
            Logging.WriteInfo($"Read time: {(DateTime.Now - start).TotalMilliseconds}");
            Assert.IsTrue(DateTime.Now - start >= TimeSpan.FromMilliseconds(375));
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(600));
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(250);
            using MemoryPoolStream ms = new();
            using ThrottledStream stream = new(ms, readCount: 100, TimeSpan.FromMilliseconds(200), writeCount: 50, TimeSpan.FromMilliseconds(100));
            DateTime start = DateTime.Now;
            await stream.WriteAsync(data);
            Logging.WriteInfo($"Write time: {(DateTime.Now - start).TotalMilliseconds}");
            Assert.IsTrue(DateTime.Now - start >= TimeSpan.FromMilliseconds(375));
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(500));
            stream.Position = 0;
            start = DateTime.Now;
            Assert.AreEqual(data.Length, await stream.ReadAsync(data));
            Logging.WriteInfo($"Read time: {(DateTime.Now - start).TotalMilliseconds}");
            Assert.IsTrue(DateTime.Now - start >= TimeSpan.FromMilliseconds(375));
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(600));
        }
    }
}
