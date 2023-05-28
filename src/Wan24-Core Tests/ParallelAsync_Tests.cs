using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelAsync_Tests
    {
        [TestMethod]
        public async Task ForEach_Tests()
        {
            int processed = 0;
            await ParallelAsync.ForEachAsync(new bool[] { true, true, true }, async (item, ct) =>
            {
                await Task.Yield();
                processed++;
            }, 2);
            Assert.AreEqual(3, processed);
        }

        [TestMethod]
        public async Task Filter_Tests()
        {
            List<int> filtered = await ParallelAsync.FilterAsync(Enumerable.Range(1, 3), async (item, ct) =>
            {
                await Task.Delay(Random.Shared.Next(10, 30), ct);
                return (item.HasFlags(1), item);
            }, 2).ToListAsync();
            Assert.AreEqual(2, filtered.Count);
            Assert.IsTrue(filtered.Contains(1));
            Assert.IsTrue(filtered.Contains(3));
        }
    }
}
