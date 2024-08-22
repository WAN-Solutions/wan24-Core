using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CopyStream_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public async Task General_Tests()
        {
            using MemoryPoolStream ms = new();
            using BlockingBufferStream buffer = new(Settings.BufferSize);
            using CopyStream copy = new(buffer, ms);
            await Task.Delay(500);
            Assert.IsFalse(copy.IsCopyCompleted);
            buffer.WriteByte(0);
            await Task.Delay(500);
            Assert.IsFalse(copy.IsCopyCompleted);
            await buffer.SetIsEndOfFileAsync();
            await Task.Delay(500);
            Assert.IsTrue(copy.IsCopyCompleted);
            Assert.AreEqual(1, ms.Length);
        }

        [TestMethod, Timeout(3000)]
        public async Task Cancellation_Tests()
        {
            using MemoryPoolStream ms = new();
            using BlockingBufferStream buffer = new(Settings.BufferSize);
            using CopyStream copy = new(buffer, ms);
            Assert.IsFalse(copy.IsCopyCompleted);
            await copy.CancelCopyAsync();
            await Task.Delay(500);
            Assert.IsFalse(copy.IsCopyCompleted);
            Assert.IsTrue(copy.CopyTask.IsCompleted);
            Assert.IsTrue(copy.LastException is OperationCanceledException);
        }

        [TestMethod]
        public async Task AutoDispose_Tests()
        {
            using MemoryPoolStream ms = new();
            using BlockingBufferStream buffer = new(Settings.BufferSize);
            using CopyStream copy = new(buffer, ms)
            {
                AutoDispose = true
            };
            await copy.CancelCopyAsync();
            await Task.Delay(500);
            Assert.IsTrue(copy.IsDisposed);
        }
    }
}
