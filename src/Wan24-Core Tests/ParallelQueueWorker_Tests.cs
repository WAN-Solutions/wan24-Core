using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelQueueWorker_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            using ParallelQueueWorker worker = new(3, 2);
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
                    await worker.EnqueueAsync(async (ct) =>
                    {
                        await Task.Yield();
                        working++;
                        mres.Wait(ct);
                        worked++;
                    });
                }
                await Task.Delay(100);
                Assert.AreEqual(2, working);
                events[0]!.Set();
                events[1]!.Set();
                await Task.Delay(100);
                Assert.AreEqual(2, worked);
                await Task.Delay(100);
                Assert.AreEqual(3, working);
                events[2]!.Set();
                await Task.Delay(100);
                Assert.AreEqual(3, worked);
            }
            finally
            {
                foreach (ManualResetEventSlim? e in events) e?.Dispose();
            }
        }
    }
}
