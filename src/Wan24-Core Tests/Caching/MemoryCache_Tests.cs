using wan24.Core.Caching;

namespace Wan24_Core_Tests.Caching
{
    [TestClass]
    public class MemoryCache_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryCache cache = new(20);
            int added = 0,
                removed = 0;
            cache.OnAdded += (s, e) => added++;
            cache.OnRemoved += (s, e) => removed++;
            TestObject neverExpires = new(),
                expires = new(),
                sliding = new();
            cache.GetOrAdd(neverExpires.GUID, (cache) => neverExpires, CacheTimeouts.None);
            cache.GetOrAdd(expires.GUID, (cache) => expires, CacheTimeouts.Fixed, DateTime.Now + TimeSpan.FromMilliseconds(50));
            cache.GetOrAdd(sliding.GUID, (cache) => sliding, CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(3, (int)cache);
            Assert.IsNotNull(cache.GetOrAdd<TestObject>(sliding.GUID, (cache) => null!, CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(3, (int)cache);
            Assert.AreEqual(neverExpires, cache.GetOrAdd<TestObject>(neverExpires.GUID, (cache) => null!, CacheTimeouts.None));
            Thread.Sleep(200);
            Assert.AreEqual(1, (int)cache);
            Assert.AreNotEqual(expires, cache.GetOrAdd(expires.GUID, (cache) => new TestObject(), CacheTimeouts.Fixed, DateTime.Now + TimeSpan.FromMilliseconds(50)));
            Assert.AreEqual(2, (int)cache);
            Assert.IsNull(cache.GetOrAdd<TestObject>(sliding.GUID, (cache) => null!, CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(3, (int)cache);
            Assert.AreEqual(neverExpires, cache.GetOrAdd<TestObject>(neverExpires.GUID, (cache) => null!, CacheTimeouts.None));
            Assert.AreEqual(5, added);
            Assert.AreEqual(2, removed);
        }

        public sealed class TestObject
        {
            public TestObject() { }

            public string GUID { get; set; } = Guid.NewGuid().ToString();
        }
    }
}
