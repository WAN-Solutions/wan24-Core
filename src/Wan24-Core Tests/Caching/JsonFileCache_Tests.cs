using wan24.Core.Caching;

namespace Wan24_Core_Tests.Caching
{
    [TestClass]
    public class JsonFileCache_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            string folder = Path.Combine(Path.GetFullPath("./"), Guid.NewGuid().ToString());
            Directory.CreateDirectory(folder);
            try
            {
                using JsonFileCache cache = new(folder, 20);
                int added = 0,
                    removed = 0;
                cache.OnAdded += (s, e) => added++;
                cache.OnRemoved += (s, e) => removed++;
                MemoryCache_Tests.TestObject neverExpires = new(),
                    expires = new(),
                    sliding = new();
                cache.GetOrAdd(neverExpires.GUID, (cache) => neverExpires, CacheTimeouts.None);
                cache.GetOrAdd(expires.GUID, (cache) => expires, CacheTimeouts.Fixed, DateTime.Now + TimeSpan.FromMilliseconds(50));
                cache.GetOrAdd(sliding.GUID, (cache) => sliding, CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100));
                Assert.AreEqual(3, cache.Count);
                Assert.IsNotNull(cache.GetOrAdd<MemoryCache_Tests.TestObject>(sliding.GUID, (cache) => null!, CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100)));
                Assert.AreEqual(3, cache.Count);
                Assert.AreEqual(neverExpires.GUID, cache.GetOrAdd<MemoryCache_Tests.TestObject>(neverExpires.GUID, (cache) => null!, CacheTimeouts.None).GUID);
                Thread.Sleep(200);
                Assert.AreEqual(1, cache.Count);
                Assert.AreNotEqual(expires.GUID, cache.GetOrAdd(expires.GUID, (cache) => new MemoryCache_Tests.TestObject(), CacheTimeouts.Fixed, DateTime.Now + TimeSpan.FromMilliseconds(50)).GUID);
                Assert.AreEqual(2, cache.Count);
                Assert.AreNotEqual(sliding.GUID, cache.GetOrAdd(sliding.GUID, (cache) => new MemoryCache_Tests.TestObject(), CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100)).GUID);
                Assert.AreEqual(3, cache.Count);
                Assert.AreEqual(neverExpires.GUID, cache.GetOrAdd<MemoryCache_Tests.TestObject>(neverExpires.GUID, (cache) => null!, CacheTimeouts.None).GUID);
                Assert.AreEqual(5, added);
                Assert.AreEqual(2, removed);
            }
            finally
            {
                Directory.Delete(folder, recursive: true);
            }
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            string folder = Path.Combine(Path.GetFullPath("./"), Guid.NewGuid().ToString());
            Directory.CreateDirectory(folder);
            try
            {
                JsonFileCache cache = new(folder, 20);
                await using (cache)
                {
                    int added = 0,
                        removed = 0;
                    cache.OnAdded += (s, e) => added++;
                    cache.OnRemoved += (s, e) => removed++;
                    MemoryCache_Tests.TestObject neverExpires = new(),
                        expires = new(),
                        sliding = new();
                    await cache.GetOrAddAsync(neverExpires.GUID, (cache, ct) => Task.FromResult(neverExpires), CacheTimeouts.None);
                    await cache.GetOrAddAsync(expires.GUID, (cache) => expires, CacheTimeouts.Fixed, DateTime.Now + TimeSpan.FromMilliseconds(50));
                    await cache.GetOrAddAsync(sliding.GUID, (cache, ct) => Task.FromResult(sliding), CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100));
                    Assert.AreEqual(3, cache.Count);
                    Assert.IsNotNull(await cache.GetOrAddAsync<MemoryCache_Tests.TestObject>(sliding.GUID, (cache) => null!, CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100)));
                    Assert.AreEqual(3, cache.Count);
                    Assert.AreEqual(neverExpires.GUID, (await cache.GetOrAddAsync<MemoryCache_Tests.TestObject>(neverExpires.GUID, (cache) => null!, CacheTimeouts.None)).GUID);
                    Thread.Sleep(200);
                    Assert.AreEqual(1, cache.Count);
                    Assert.AreNotEqual(expires.GUID, (await cache.GetOrAddAsync(expires.GUID, (cache) => new MemoryCache_Tests.TestObject(), CacheTimeouts.Fixed, DateTime.Now + TimeSpan.FromMilliseconds(50))).GUID);
                    Assert.AreEqual(2, cache.Count);
                    Assert.AreNotEqual(sliding.GUID, (await cache.GetOrAddAsync(sliding.GUID, (cache) => new MemoryCache_Tests.TestObject(), CacheTimeouts.Sliding, timespan: TimeSpan.FromMilliseconds(100))).GUID);
                    Assert.AreEqual(3, cache.Count);
                    Assert.AreEqual(neverExpires.GUID, (await cache.GetOrAddAsync<MemoryCache_Tests.TestObject>(neverExpires.GUID, (cache) => null!, CacheTimeouts.None)).GUID);
                    Assert.AreEqual(5, added);
                    Assert.AreEqual(2, removed);
                }
            }
            finally
            {
                Directory.Delete(folder, recursive: true);
            }
        }
    }
}
