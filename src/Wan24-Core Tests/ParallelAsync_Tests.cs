using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelAsync_Tests
    {
        [TestMethod("ParallelAsync_Tests.ForEach_Tests"), Timeout(1000)]
        public async Task ForEach_Tests()
        {
            int processed = 0;
            await ParallelAsync.ForEachAsync(new bool[] { true, true, true }, async (item, ct) =>
            {
                await Task.Yield();
                processed++;
            }, 3, 2);
            Assert.AreEqual(3, processed);
        }

        [TestMethod("ParallelAsync_Tests.FilterAsync_Tests"), Timeout(1000)]
        public async Task FilterAsync_Tests()
        {
            List<int> filtered = await ParallelAsync.FilterAsync(Enumerable.Range(1, 3), async (item, ct) =>
            {
                await Task.Delay(Random.Shared.Next(10, 30), ct);
                return (item.HasFlags(1), item);
            }, 3, 2).ToListAsync();
            Assert.AreEqual(2, filtered.Count);
            Assert.IsTrue(filtered.Contains(1));
            Assert.IsTrue(filtered.Contains(3));
        }

        [TestMethod("ParallelAsync_Tests.Filter_Tests"), Timeout(1000)]
        public void Filter_Tests()
        {
            List<int> filtered = ParallelAsync.Filter(Enumerable.Range(1, 3), (item, state, ct) =>
            {
                Thread.Sleep(Random.Shared.Next(10, 30));
                return (item.HasFlags(1), item);
            }, new() { MaxDegreeOfParallelism = 2 }).ToList();
            Assert.AreEqual(2, filtered.Count);
            Assert.IsTrue(filtered.Contains(1));
            Assert.IsTrue(filtered.Contains(3));
        }
    }
}
