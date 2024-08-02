using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class FlushStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryStream ms = new();
            using FlushStream flushStream = new(ms, leaveOpen: true);

            flushStream.WriteByte(0);
            Assert.AreEqual(1, flushStream.BufferSize);
            Assert.AreEqual(0, ms.Length);
            flushStream.Flush();
            Assert.AreEqual(0, flushStream.BufferSize);
            Assert.AreEqual(1, ms.Length);

            flushStream.FlushOnWrite = true;
            flushStream.WriteByte(0);
            flushStream.WriteByte(0);
            Assert.AreEqual(1, flushStream.BufferSize);
            Assert.AreEqual(2, ms.Length);
            flushStream.Flush();
            Assert.AreEqual(0, flushStream.BufferSize);
            Assert.AreEqual(3, ms.Length);

            flushStream.MaxBuffer = 1;
            flushStream.FlushOnWrite = false;
            flushStream.WriteByte(0);
            Assert.ThrowsException<OutOfMemoryException>(() => flushStream.WriteByte(0));

            flushStream.Dispose();
            Assert.AreEqual(3, ms.Length);
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            byte[] data = [0];
            using MemoryStream ms = new();
            using FlushStream flushStream = new(ms, leaveOpen: true);

            await flushStream.WriteAsync(data);
            Assert.AreEqual(1, flushStream.BufferSize);
            Assert.AreEqual(0, ms.Length);
            await flushStream.FlushAsync();
            Assert.AreEqual(0, flushStream.BufferSize);
            Assert.AreEqual(1, ms.Length);

            flushStream.FlushOnWrite = true;
            await flushStream.WriteAsync(data);
            await flushStream.WriteAsync(data);
            Assert.AreEqual(1, flushStream.BufferSize);
            Assert.AreEqual(2, ms.Length);
            await flushStream.FlushAsync();
            Assert.AreEqual(0, flushStream.BufferSize);
            Assert.AreEqual(3, ms.Length);

            flushStream.MaxBuffer = 1;
            flushStream.FlushOnWrite = false;
            await flushStream.WriteAsync(data);
            await Assert.ThrowsExceptionAsync<OutOfMemoryException>(async () => await flushStream.WriteAsync(data));

            await flushStream.DisposeAsync();
            Assert.AreEqual(3, ms.Length);
        }
    }
}
