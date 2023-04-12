using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ProcessThrottle_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            using TestObject throttle = new();
            // Timeout
            Assert.AreEqual(1, await throttle.ProcessAsync(2, TimeSpan.FromMilliseconds(20)));
            await Task.Delay(20);
            Assert.AreEqual(1, throttle.Processed);
            // Cancellation token
            using CancellationTokenSource cancellation = new();
            int processed = 0;
            Task task = Task.Run(async () => processed = await throttle.ProcessAsync(2, cancellation.Token));
            await Task.Delay(20);
            Assert.AreEqual(2, throttle.Processed);
            cancellation.Cancel();
            await task;
            Assert.AreEqual(2, throttle.Processed);
            Assert.AreEqual(1, processed);
            // Throttle
            processed = 0;
            task = Task.Run(async () => processed = await throttle.ProcessAsync(2)); ;
            await Task.Delay(90);
            await task;
            Assert.AreEqual(4, throttle.Processed);
            Assert.AreEqual(2, processed);
        }

        public sealed class TestObject : ProcessThrottle
        {
            public int Processed = 0;

            public TestObject() : base(1, 50) { }

            public async Task<int> ProcessAsync(int count, TimeSpan timeout)
                => await ProcessAsync(count, (count) =>
                {
                    Processed += count;
                    return Task.CompletedTask;
                }, timeout);

            public async Task<int> ProcessAsync(int count, CancellationToken cancellationToken = default)
                => await ProcessAsync(count, (count) =>
                {
                    Processed += count;
                    return Task.CompletedTask;
                }, cancellationToken);
        }
    }
}
