using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelItemQueueWorker_Tests
    {
        [TestMethod("ParallelItemQueueWorker_Tests.General_Tests"), Timeout(1000)]
        public async Task General_Tests()
        {
            using TestObject worker = new();
            Logging.WriteInfo("Starting worker");
            await worker.StartAsync(default);
            ManualResetEventSlim?[] events = new ManualResetEventSlim?[3];
            try
            {
                for (int i = 0; i < events.Length; await worker.EnqueueAsync(events[i] = new(initialState: false)), i++) Logging.WriteInfo($"Enqueuing {i}");
                Logging.WriteInfo("Waiting workers");
                await Task.Delay(100);
                Assert.AreEqual(2, worker.Working);
                Logging.WriteInfo("Setting event #0");
                events[0]!.Set();
                Logging.WriteInfo("Setting event #1");
                events[1]!.Set();
                await Task.Delay(100);
                Assert.AreEqual(2, worker.Worked);
                await Task.Delay(100);
                Assert.AreEqual(3, worker.Working);
                Logging.WriteInfo("Setting event #2");
                events[2]!.Set();
                await Task.Delay(100);
                Assert.AreEqual(3, worker.Worked);
                Logging.WriteInfo("All workers done");
            }
            finally
            {
                Logging.WriteInfo("Disposing events");
                foreach (ManualResetEventSlim? e in events) e?.Dispose();
                Logging.WriteInfo("Tests done");
            }
        }

        public sealed class TestObject : ParallelItemQueueWorkerBase<ManualResetEventSlim>
        {
            public int Working = 0;
            public int Worked = 0;

            public TestObject() : base(3, 2) { }

            protected override async Task ProcessItem(ManualResetEventSlim item, CancellationToken cancellationToken)
            {
                await Task.Yield();
                Working++;
                item.Wait(cancellationToken);
                Worked++;
            }
        }
    }
}
