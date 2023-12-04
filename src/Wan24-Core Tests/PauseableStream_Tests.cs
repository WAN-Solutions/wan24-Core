using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class PauseableStream_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using PausableStream stream = new(new MemoryPoolStream());
            stream.WriteByte(1);
            stream.Pause = true;
            Task task = Task.Run(() => stream.WriteByte(1));
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(1, stream.Length);
            stream.Pause = false;
            Thread.Sleep(50);
            Assert.IsTrue(task.IsCompleted);
            stream.Position = 0;
            stream.Pause = true;
            task = Task.Run(() => Assert.AreEqual(1, stream.ReadByte()));
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(0, stream.Position);
            stream.Pause = false;
            Thread.Sleep(50);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(1, stream.ReadByte());
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            byte[] buffer = new byte[] { 1 };
            using PausableStream stream = new(new MemoryPoolStream());
            await stream.WriteAsync(buffer);
            stream.Pause = true;
            Task task = Task.Run(async () => await stream.WriteAsync(buffer));
            await Task.Delay(50);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(1, stream.Length);
            stream.Pause = false;
            await Task.Delay(50);
            Assert.IsTrue(task.IsCompleted);
            stream.Position = 0;
            stream.Pause = true;
            task = Task.Run(async () =>
            {
                Assert.AreEqual(1, await stream.ReadAsync(buffer));
                Assert.AreEqual(1, buffer[0]);
            });
            await Task.Delay(50);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(0, stream.Position);
            stream.Pause = false;
            await Task.Delay(50);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(1, await stream.ReadAsync(buffer));
            Assert.AreEqual(1, buffer[0]);
        }
    }
}
