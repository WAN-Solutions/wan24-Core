using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelQueueWorker_Tests
    {
        [TestMethod("ParallelQueueWorker_Tests.General_Tests"), Timeout(3000)]
        public async Task General_Tests()
        {
            using ParallelQueueWorker worker = new(3, 2);
            Logging.WriteInfo("Starting worker");
            await worker.StartAsync(default);
            int working = 0,
                worked = 0;
            DateTime start = DateTime.Now;
            ManualResetEventSlim?[] events = new ManualResetEventSlim?[3];
            try
            {
                for (int i = 0; i < events.Length; i++)
                {
                    ManualResetEventSlim mres = new(initialState: false);
                    events[i] = mres;
                    Logging.WriteInfo($"Enqueuing {i} ({DateTime.Now - start})");
                    await worker.EnqueueAsync(async (ct) =>
                    {
                        await Task.Yield();
                        working++;
                        mres.Wait(ct);
                        worked++;
                    });
                }
                Logging.WriteInfo($"Waiting workers ({DateTime.Now - start})");
                await Task.Delay(200);
                Assert.AreEqual(2, working);
                Logging.WriteInfo($"Setting event #0 ({DateTime.Now - start})");
                events[0]!.Set();
                Logging.WriteInfo($"Setting event #1v");
                events[1]!.Set();
                await Task.Delay(200);
                Assert.AreEqual(2, worked);
                await Task.Delay(200);
                Assert.AreEqual(3, working);
                Logging.WriteInfo($"Setting event #2 ({DateTime.Now - start})");
                events[2]!.Set();
                await Task.Delay(200);
                Assert.AreEqual(3, worked);
                Logging.WriteInfo($"All workers done ({DateTime.Now - start})");
            }
            finally
            {
                Logging.WriteInfo($"Disposing events ({DateTime.Now - start})");
                foreach (ManualResetEventSlim? e in events) e?.Dispose();
                Logging.WriteInfo($"Tests done ({DateTime.Now - start})");
            }
        }
    }
}
