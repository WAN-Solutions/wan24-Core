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

        public async IAsyncEnumerable<T> AsyncEnumerable<T>(T[] data)
        {
            await Task.Yield();
            foreach (T item in data) yield return item;
        }
    }
}
