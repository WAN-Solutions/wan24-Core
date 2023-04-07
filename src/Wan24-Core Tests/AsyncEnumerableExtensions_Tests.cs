using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class AsyncEnumerableExtensions_Tests
    {
        [TestMethod]
        public async Task AsyncEnumerable_Tests()
        {
            bool[] data = new bool[] { false, true };
            bool[] arr = await AsyncEnumerable(data).ToArrayAsync();
            Assert.IsTrue(data.SequenceEqual(arr));
            List<bool> list = await AsyncEnumerable(data).ToListAsync();
            Assert.IsTrue(data.SequenceEqual(list));
        }

        [TestMethod]
        public async Task Context_Tests()
        {
            bool[] data = new bool[] { false, true };
            await foreach (bool item in AsyncEnumerable(data).DynamicContext()) ;
            await foreach (bool item in AsyncEnumerable(data).FixedContext()) ;
        }

        [TestMethod]
        public async Task Combine_Tests()
        {
            {
                bool[] test = await AsyncEnumerable(new bool[][] { new bool[] { true }, new bool[] { false, true } }).CombineAsync().ToArrayAsync();
                Assert.IsTrue(test[0]);
                Assert.IsFalse(test[1]);
                Assert.IsTrue(test[2]);
            }
            {
                bool[] test = await AsyncEnumerable(new IAsyncEnumerable<bool>[] { AsyncEnumerable(new bool[] { true }), AsyncEnumerable(new bool[] { false, true }) }).CombineAsync().ToArrayAsync();
                Assert.IsTrue(test[0]);
                Assert.IsFalse(test[1]);
                Assert.IsTrue(test[2]);
            }
        }

        [TestMethod]
        public async Task ChunkEnum_Tests()
        {
            bool[][] test = await AsyncEnumerable(new bool[] { true, true, false, false, true }).ChunkEnumAsync(2).ToArrayAsync();
            Assert.AreEqual(3, test.Length);
            Assert.AreEqual(2, test[0].Length);
            Assert.AreEqual(2, test[1].Length);
            Assert.AreEqual(1, test[2].Length);
            Assert.IsTrue(test[0][0]);
            Assert.IsTrue(test[0][1]);
            Assert.IsFalse(test[1][0]);
            Assert.IsFalse(test[1][1]);
            Assert.IsTrue(test[2][0]);
        }

        public async IAsyncEnumerable<T> AsyncEnumerable<T>(T[] data)
        {
            await Task.Yield();
            foreach (T item in data) yield return item;
        }
    }
}
