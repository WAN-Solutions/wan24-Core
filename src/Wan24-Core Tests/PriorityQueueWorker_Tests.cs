using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class PriorityQueueWorker_Tests
    {
        [TestMethod, Timeout(10000)]
        public async Task General_Tests()
        {
            using ResetEvent runEvent = new();
            using PriorityQueueWorker<int> worker = new(3);
            await worker.StartAsync();
            List<int> processed = [];
            await worker.EnqueueAsync(async (ct) =>
            {
                await runEvent.WaitAsync();
                processed.Add(3);
            }, 3);
            await Task.Delay(500);
            await worker.EnqueueAsync(async (ct) =>
            {
                await runEvent.WaitAsync();
                processed.Add(2);
            }, 2);
            await worker.EnqueueAsync(async (ct) =>
            {
                await runEvent.WaitAsync();
                processed.Add(1);
            }, 1);
            runEvent.Set();
            await Task.Delay(1500);
            Assert.AreEqual(0, worker.Queued);
            Assert.AreEqual(3, processed.Count);
            Assert.AreEqual(3, processed[0]);
            Assert.AreEqual(1, processed[1]);
            Assert.AreEqual(2, processed[2]);
        }
    }
}
