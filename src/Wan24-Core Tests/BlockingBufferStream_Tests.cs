using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BlockingBufferStream_Tests : TestBase
    {
        [TestMethod, Timeout(5000)]
        public void General_Tests()
        {
            using BlockingBufferStream stream = new(bufferSize: 3);
            Assert.IsTrue(stream.IsReadBlocked);
            stream.WriteByte(1);
            stream.WriteByte(1);
            stream.WriteByte(1);
            Task task = Task.Run(() => stream.WriteByte(1));
            Thread.Sleep(500);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(3L, stream.Length);
            Assert.AreEqual(3, stream.Available);
            Assert.IsTrue(stream.IsWriteBlocked);
            Assert.IsFalse(stream.IsReadBlocked);
            Assert.AreEqual(1, stream.ReadByte());
            Assert.AreEqual(1, stream.ReadByte());
            Assert.AreEqual(1, stream.ReadByte());
            Thread.Sleep(500);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(4L, stream.Length);
            Assert.AreEqual(1, stream.Available);
            Assert.IsFalse(stream.IsWriteBlocked);
            Assert.IsFalse(stream.IsReadBlocked);
            Assert.AreEqual(1, stream.ReadByte());
            Assert.IsTrue(stream.IsReadBlocked);
            Assert.AreEqual(0, stream.Available);
            Assert.AreEqual(stream.BufferSize, stream.SpaceLeft);
            task = Task.Run(() => Assert.AreEqual(1, stream.ReadByte()));
            Thread.Sleep(500);
            Assert.IsFalse(task.IsCompleted);
            stream.WriteByte(1);
            Thread.Sleep(500);
            Assert.IsTrue(task.IsCompleted);
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            byte[] writeBuffer = new byte[] { 1 },
                readBuffer = new byte[1];
            using BlockingBufferStream stream = new(bufferSize: 3);
            Assert.IsTrue(stream.IsReadBlocked);
            await stream.WriteAsync(writeBuffer);
            await stream.WriteAsync(writeBuffer);
            await stream.WriteAsync(writeBuffer);
            Task task = Task.Run(async () => await stream.WriteAsync(writeBuffer));
            await Task.Delay(500);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(3L, stream.Length);
            Assert.AreEqual(3, stream.Available);
            Assert.IsTrue(stream.IsWriteBlocked);
            Assert.IsFalse(stream.IsReadBlocked);
            Assert.AreEqual(1, await stream.ReadAsync(readBuffer));
            Assert.AreEqual(1, readBuffer[0]);
            Assert.AreEqual(1, await stream.ReadAsync(readBuffer));
            Assert.AreEqual(1, readBuffer[0]);
            Assert.AreEqual(1, await stream.ReadAsync(readBuffer));
            Assert.AreEqual(1, readBuffer[0]);
            await task;
            Assert.AreEqual(4L, stream.Length);
            Assert.AreEqual(1, stream.Available);
            Assert.IsFalse(stream.IsWriteBlocked);
            Assert.IsFalse(stream.IsReadBlocked);
            Assert.AreEqual(1, await stream.ReadAsync(readBuffer));
            Assert.AreEqual(1, readBuffer[0]);
            Assert.IsTrue(stream.IsReadBlocked);
            Assert.AreEqual(0, stream.Available);
            Assert.AreEqual(stream.BufferSize, stream.SpaceLeft);
            task = Task.Run(async () =>
            {
                Assert.AreEqual(1, await stream.ReadAsync(readBuffer));
                Assert.AreEqual(1, readBuffer[0]);
            });
            await Task.Delay(500);
            Assert.IsFalse(task.IsCompleted);
            await stream.WriteAsync(writeBuffer);
            await task;
        }

        [TestMethod, Timeout(3000)]
        public async Task Cancellation_Test()
        {
            using BlockingBufferStream stream = new(bufferSize: 3);
            using CancellationTokenSource cts = new();
            Task task = Task.Run(async () => await stream.WriteAsync(new byte[] { 1, 2, 3, 4 }, cts.Token).DynamicContext());
            await Task.Delay(200);
            cts.Cancel();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await task);
        }
    }
}
