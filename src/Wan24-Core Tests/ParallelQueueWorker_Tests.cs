using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelQueueWorker_Tests
    {
        [TestMethod("ParallelQueueWorker_Tests.General_Tests"), Timeout(1000)]
        public async Task General_Tests()
        {
            using ParallelQueueWorker worker = new(3, 2);
            Logging.WriteInfo("Starting worker");
            await worker.StartAsync(default);
            int working = 0,
                worked = 0;
            ManualResetEventSlim?[] events = new ManualResetEventSlim?[3];
            try
            {
                for (int i = 0; i < events.Length; i++)
                {
                    ManualResetEventSlim mres = new(initialState: false);
                    events[i] = mres;
                    Logging.WriteInfo($"Enqueuing {i}");
                    await worker.EnqueueAsync(async (ct) =>
                    {
                        await Task.Yield();
                        working++;
                        mres.Wait(ct);
                        worked++;
                    });
                }
                Logging.WriteInfo("Waiting workers");
                await Task.Delay(100);
                Assert.AreEqual(2, working);
                Logging.WriteInfo("Setting event #0");
                events[0]!.Set();
                Logging.WriteInfo("Setting event #1");
                events[1]!.Set();
                await Task.Delay(100);
                Assert.AreEqual(2, worked);
                await Task.Delay(100);
                Assert.AreEqual(3, working);
                Logging.WriteInfo("Setting event #2");
                events[2]!.Set();
                await Task.Delay(100);
                Assert.AreEqual(3, worked);
                Logging.WriteInfo("All workers done");
            }
            finally
            {
                Logging.WriteInfo("Disposing events");
                foreach (ManualResetEventSlim? e in events) e?.Dispose();
                Logging.WriteInfo("Tests done");
            }
        }
    }
}
