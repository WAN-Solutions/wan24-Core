using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CollectionExtensions_Tests
    {
        [TestMethod]
        public void AddRange_Tests()
        {
            ICollection<bool> list = new List<bool>();
            list.AddRange(true, false);
            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.First());
            Assert.IsFalse(list.Last());
            list = new List<bool>();
            list.AddRange(new bool[] { true, false }.AsEnumerable());
            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.First());
            Assert.IsFalse(list.Last());
        }

        [TestMethod]
        public async Task AddRangeAsync_Tests()
        {
            ICollection<bool> list = new List<bool>();
            await list.AddRangeAsync(EnumerateAsync(true, false));
            Assert.IsTrue(list.Count == 2);
            Assert.IsTrue(list.First());
            Assert.IsFalse(list.Last());
        }

        private static async IAsyncEnumerable<bool> EnumerateAsync(params bool[] values)
        {
            await Task.Yield();
            foreach (bool value in values) yield return value;
        }
    }
}
