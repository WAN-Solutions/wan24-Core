using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class SynchronizedStream_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            PauseableStream pauseStream = new(new MemoryPoolStream());
            using SynchronizedStream stream = new(pauseStream);
            pauseStream.Pause = true;
            bool running = false;
            Task writeTask = Task.Run(() =>
                {
                    running = true;
                    stream.Write(RandomNumberGenerator.GetBytes(100));
                    running = false;
                }),
                readTask = Task.Run(() =>
                {
                    Thread.Sleep(100);
                    running = true;
                    Assert.AreEqual(100, stream.ReadAt(0, new byte[100]));
                });
            Thread.Sleep(50);
            Assert.IsTrue(running);
            pauseStream.Pause = false;
            writeTask.Wait();
            Assert.IsFalse(running);
            readTask.Wait();
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            PauseableStream pauseStream = new(new MemoryPoolStream());
            using SynchronizedStream stream = new(pauseStream);
            pauseStream.Pause = true;
            bool running = false;
            Task writeTask = Task.Run(async () =>
            {
                running = true;
                await stream.WriteAsync(RandomNumberGenerator.GetBytes(100));
                running = false;
            }),
                readTask = Task.Run(async () =>
                {
                    await Task.Delay(100);
                    running = true;
                    Assert.AreEqual(100, await stream.ReadAtAsync(0, new byte[100]));
                });
            await Task.Delay(50);
            Assert.IsTrue(running);
            pauseStream.Pause = false;
            await writeTask;
            Assert.IsFalse(running);
            await readTask;
        }
    }
}
