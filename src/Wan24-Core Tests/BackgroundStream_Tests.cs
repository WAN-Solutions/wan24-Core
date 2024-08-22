using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BackgroundStream_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using ZeroStream zero = new();
            using ThrottledStream throttle = new(zero, writeCount: 100, writeTime: TimeSpan.FromMilliseconds(400));
            using CountingStream counter = new(throttle);
            using BackgroundStream stream = new(counter, maxMemory: 100, queueSize: 2);
            byte[] data = new byte[100];

            // Write
            Logging.WriteInfo("Write");
            stream.Write(data);
            stream.WaitWritten();
            Assert.AreEqual(100, counter.Written);

            // Queue
            Logging.WriteInfo("Queue");
            Thread.Sleep(200);
            stream.Write(data);
            stream.Write(data);
            Thread.Sleep(200);
            Assert.AreEqual(200, counter.Written);
            Assert.AreEqual(100, stream.CurrentMemory);
            Logging.WriteInfo("Wait written");
            stream.WaitWritten();
            Assert.AreEqual(300, counter.Written);
            Assert.AreEqual(0, stream.CurrentMemory);
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            using ZeroStream zero = new();
            using ThrottledStream throttle = new(zero, writeCount: 100, writeTime: TimeSpan.FromMilliseconds(400));
            using CountingStream counter = new(throttle);
            using BackgroundStream stream = new(counter, maxMemory: 100, queueSize: 2);
            byte[] data = new byte[100];

            // Write
            Logging.WriteInfo("Write");
            await stream.WriteAsync(data);
            await stream.WaitWrittenAsync();
            Assert.AreEqual(100, counter.Written);

            // Queue
            Logging.WriteInfo("Queue");
            await Task.Delay(200);
            await stream.WriteAsync(data);
            await stream.WriteAsync(data);
            await Task.Delay(200);
            Logging.WriteInfo("Wait written");
            await stream.WaitWrittenAsync();
            Assert.AreEqual(300, counter.Written);
            Assert.AreEqual(0, stream.CurrentMemory);
            Logging.WriteInfo("Dispose");
            await stream.DisposeAsync().DynamicContext();
        }
    }
}
