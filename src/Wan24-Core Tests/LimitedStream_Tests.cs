using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class LimitedStream_Tests : TestBase
    {
        [TestMethod]
        public void Write_Tests()
        {
            using MemoryPoolStream ms = new();
            using LimitedStream limited = new(ms, canRead: false, canWrite: true, canSeek: false);
            Assert.IsFalse(limited.CanRead);
            Assert.IsFalse(limited.CanSeek);
            Assert.IsTrue(limited.CanWrite);
            byte[] data = RandomNumberGenerator.GetBytes(10);
            limited.Write(data);
            Assert.IsTrue(data.SequenceEqual(ms.ToArray()));
            Assert.ThrowsException<NotSupportedException>(() => limited.Read(data));
            Assert.ThrowsException<NotSupportedException>(() => limited.Position = 0);
        }

        [TestMethod]
        public async Task WriteAsync_Tests()
        {
            using MemoryPoolStream ms = new();
            using LimitedStream limited = new(ms, canRead: false, canWrite: true, canSeek: false);
            Assert.IsFalse(limited.CanRead);
            Assert.IsFalse(limited.CanSeek);
            Assert.IsTrue(limited.CanWrite);
            byte[] data = RandomNumberGenerator.GetBytes(10);
            await limited.WriteAsync(data);
            Assert.IsTrue(data.SequenceEqual(ms.ToArray()));
            await Assert.ThrowsExceptionAsync<NotSupportedException>(async () => await limited.ReadAsync(data));
        }

        [TestMethod]
        public void Read_Tests()
        {
            using MemoryPoolStream ms = new(RandomNumberGenerator.GetBytes(10));
            using LimitedStream limited = new(ms, canRead: true, canWrite: false, canSeek: true);
            Assert.IsTrue(limited.CanRead);
            Assert.IsTrue(limited.CanSeek);
            Assert.IsFalse(limited.CanWrite);
            byte[] temp = new byte[ms.Length];
            Assert.AreEqual(temp.Length, limited.Read(temp));
            Assert.IsTrue(ms.ToArray().SequenceEqual(temp));
            limited.Position = 0;
            Assert.ThrowsException<NotSupportedException>(() => limited.Write(temp));
        }

        [TestMethod]
        public async Task ReadAsync_Tests()
        {
            using MemoryPoolStream ms = new(RandomNumberGenerator.GetBytes(10));
            using LimitedStream limited = new(ms, canRead: true, canWrite: false, canSeek: true);
            Assert.IsTrue(limited.CanRead);
            Assert.IsTrue(limited.CanSeek);
            Assert.IsFalse(limited.CanWrite);
            byte[] temp = new byte[ms.Length];
            Assert.AreEqual(temp.Length, await limited.ReadAsync(temp));
            Assert.IsTrue(ms.ToArray().SequenceEqual(temp));
            limited.Position = 0;
            await Assert.ThrowsExceptionAsync<NotSupportedException>(async () => await limited.WriteAsync(temp));
        }
    }
}
