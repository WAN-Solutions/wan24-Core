using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ParallelAsync_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            int processed = 0;
            await ParallelAsync.ForEachAsync(new bool[] { true, true, true }, async (item, ct) =>
            {
                await Task.Yield();
                processed++;
            }, 2);
            Assert.AreEqual(3, processed);
        }
    }
}
