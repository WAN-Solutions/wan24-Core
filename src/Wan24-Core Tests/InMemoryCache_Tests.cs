using Microsoft.Extensions.Primitives;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class InMemoryCache_Tests
    {
        [TestMethod]
        public void Sync_Tests()
        {
            using InMemoryCache<TestItem> cache = new(new()
            {
                SoftCountLimit = 10
            });
            cache.StartAsync().GetAwaiter().GetResult();

            TestItem item1 = new("1"),
                item2 = new("2"),
                item3 = new("3");
            try
            {
                cache.Add(item1);
                cache.Add(item2);
                cache.Add(item3, new() { ObserveDisposing = true });
                Assert.AreEqual(3, cache.Count);
                Assert.AreEqual(5, cache.Size);

                Assert.AreEqual(item1, cache.Get("1")?.Item);
                Assert.AreEqual(item2, cache.Get("2")?.Item);
                Assert.AreEqual(item3, cache.Get("3")?.Item);

                InMemoryCacheEntry<TestItem>? entry1 = cache.Get("1");
                Assert.IsNotNull(entry1);
                Assert.AreEqual(entry1, cache.TryRemove("1"));
                Assert.AreEqual(2, cache.Count);
                Assert.AreEqual(item2._Size + 1 + item3.Size + 1, cache.Size);
                Assert.IsFalse(item1.IsDisposing);

                Assert.IsNull(cache.TryRemove("1"));
                Assert.IsNull(cache.Get("1"));
                Assert.AreEqual(item2, cache.Get("2")?.Item);

                item3.Dispose();
                Assert.AreEqual(1, cache.Count);// ObserveDisposing in effect

                cache.Dispose();
                Assert.IsFalse(item1.IsDisposing);
                Assert.IsTrue(item2.IsDisposing);
            }
            finally
            {
                item1.Dispose();
                item2.Dispose();
                item3.Dispose();
            }
        }

        [TestMethod]
        public async Task Async_Tests()
        {
            InMemoryCache<TestItem> cache = new(new()
            {
                SoftCountLimit = 10
            });
            await using (cache)
            {
                await cache.StartAsync();

                TestItem item1 = new("1"),
                    item2 = new("2"),
                    item3 = new("3");
                try
                {
                    await cache.AddAsync(item1);
                    await cache.AddAsync(item2);
                    await cache.AddAsync(item3, new() { ObserveDisposing = true });
                    Assert.AreEqual(3, cache.Count);
                    Assert.AreEqual(5, cache.Size);

                    Assert.AreEqual(item1, (await cache.GetAsync("1"))?.Item);
                    Assert.AreEqual(item2, (await cache.GetAsync("2"))?.Item);
                    Assert.AreEqual(item3, (await cache.GetAsync("3"))?.Item);

                    InMemoryCacheEntry<TestItem>? entry1 = await cache.GetAsync("1");
                    Assert.IsNotNull(entry1);
                    Assert.AreEqual(entry1, cache.TryRemove("1"));
                    Assert.AreEqual(2, cache.Count);
                    Assert.AreEqual(item2._Size + 1 + item3.Size + 1, cache.Size);
                    Assert.IsFalse(item1.IsDisposing);

                    Assert.IsNull(cache.TryRemove("1"));
                    Assert.IsNull(await cache.GetAsync("1"));
                    Assert.AreEqual(item2, (await cache.GetAsync("2"))?.Item);

                    await item3.DisposeAsync();
                    Assert.AreEqual(1, cache.Count);// ObserveDisposing in effect

                    await cache.DisposeAsync();
                    Assert.IsFalse(item1.IsDisposing);
                    Assert.IsTrue(item2.IsDisposing);
                }
                finally
                {
                    await item1.DisposeAsync();
                    await item2.DisposeAsync();
                    await item3.DisposeAsync();
                }
            }
        }

        [TestMethod, Timeout(3000)]
        public async Task ReduceCount_Tests()
        {
            TestCache cache = new(new()
            {
                TidyTimeout = TimeSpan.FromMilliseconds(100),
                SoftCountLimit = 2,
                HardCountLimit = 3
            });
            await using (cache)
            {
                await cache.StartAsync();
                cache.StopTidyTimer();

                await cache.AddAsync(new TestItem("1"));
                await cache.AddAsync(new TestItem("2"));
                await cache.ReduceCountAsync(1);
                Assert.AreEqual(1, cache.Count);

                await cache.AddAsync(new TestItem("3"));
                await cache.AddAsync(new TestItem("4"));
                await cache.AddAsync(new TestItem("5"));
                Assert.AreEqual(3, cache.Count);// Hard limit in effect during adding
                cache.StartTidyTimer();
                Assert.AreEqual(3, cache.Count);
                await Task.Delay(500);
                Assert.AreEqual(2, cache.Count);// Soft limit in effect during auto-cleanup
            }
        }

        [TestMethod, Timeout(3000)]
        public async Task ReduceSize_Tests()
        {
            TestCache cache = new(new()
            {
                TidyTimeout = TimeSpan.FromMilliseconds(100),
                SoftSizeLimit = 2,
                HardSizeLimit = 3
            });
            await using (cache)
            {
                await cache.StartAsync();
                cache.StopTidyTimer();

                await cache.AddAsync(new TestItem("1", size: 1));
                await cache.AddAsync(new TestItem("2", size: 1));
                await cache.ReduceCountAsync(1);
                Assert.AreEqual(1, cache.Count);

                await cache.AddAsync(new TestItem("3", size: 1));
                await cache.AddAsync(new TestItem("4", size: 1));
                await cache.AddAsync(new TestItem("5", size: 1));
                Assert.AreEqual(3, cache.Size);// Hard limit in effect during adding
                cache.StartTidyTimer();
                Assert.AreEqual(3, cache.Size);
                await Task.Delay(500);
                Assert.AreEqual(2, cache.Size);// Soft limit in effect during auto-cleanup
            }
        }

        [TestMethod, Timeout(3000)]
        public async Task ReduceOld_Tests()
        {
            TestCache cache = new(new()
            {
                TidyTimeout = TimeSpan.FromMilliseconds(100),
                AgeLimit = TimeSpan.FromMilliseconds(200)
            });
            await using (cache)
            {
                await cache.StartAsync();

                await cache.AddAsync(new TestItem("1"));
                Assert.AreEqual(1, cache.Count);
                await Task.Delay(500);
                Assert.AreEqual(0, cache.Count);
            }
        }

        [TestMethod, Timeout(3000)]
        public async Task ReduceUnpopular_Tests()
        {
            TestCache cache = new(new()
            {
                TidyTimeout = TimeSpan.FromMilliseconds(100),
                IdleLimit = TimeSpan.FromMilliseconds(500)
            });
            await using (cache)
            {
                await cache.StartAsync();

                await cache.AddAsync(new TestItem("1"));
                await cache.AddAsync(new TestItem("2"));
                Assert.AreEqual(2, cache.Count);
                await Task.Delay(200);
                Assert.AreEqual(2, cache.Count);

                InMemoryCacheEntry<TestItem>? entry1 = await cache.GetAsync("1");
                Assert.IsNotNull(entry1);
                entry1.Refresh();
                await Task.Delay(500);
                Assert.AreEqual(1, cache.Count);// Idle limit in effect
            }
        }

        [TestMethod]
        public async Task Persistent_tests()
        {
            TestCache cache = new(new()
            {
                TidyTimeout = TimeSpan.FromMilliseconds(100),
                SoftCountLimit = 1
            });
            await using (cache)
            {
                await cache.StartAsync();
                cache.StopTidyTimer();

                await cache.AddAsync(new TestItem("1") { Options = new() { Type = InMemoryCacheEntryTypes.Persistent } });
                await cache.AddAsync(new TestItem("2"));
                Assert.AreEqual(2, cache.Count);
                cache.StartTidyTimer();
                await Task.Delay(500);
                Assert.AreEqual(1, cache.Count);// Soft limit in effect
                Assert.IsNotNull(await cache.GetAsync("1"));// Persistent item shouldn't be removed automatic
            }
        }

        [TestMethod, Timeout(10000)]
        public async Task Timeout_Tests()
        {
            TestCache cache = new(new()
            {
                TidyTimeout = TimeSpan.FromMilliseconds(100),
                SoftCountLimit = 10
            });
            await using (cache)
            {
                await cache.StartAsync();

                // Absolute timeout
                await cache.AddAsync(new TestItem("1") { Options = new() { AbsoluteTimeout = DateTime.Now + TimeSpan.FromMilliseconds(500) } });
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                await Task.Delay(500);
                Assert.AreEqual(0, cache.Count);// Absolute timeout in effect

                // Relative timeout only (is like an absolute timeout, but can be affected using the Refresh method)
                await cache.AddAsync(new TestItem("1") { Options = new() { Type = InMemoryCacheEntryTypes.Timeout, Timeout = TimeSpan.FromMilliseconds(500) } });
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                await Task.Delay(500);
                Assert.AreEqual(0, cache.Count);// Timeout in effect

                // Sliding timeout
                await cache.AddAsync(new TestItem("1") { Options = new() { Type = InMemoryCacheEntryTypes.Timeout, Timeout = TimeSpan.FromMilliseconds(500), IsSlidingTimeout = true } });
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                InMemoryCacheEntry<TestItem>? entry1 = await cache.GetAsync("1");
                Assert.IsNotNull(entry1);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                await Task.Delay(500);
                Assert.AreEqual(0, cache.Count);// Sliding timeout in effect

                // Sliding and absolute timeout (letting the absolute timeout being hit)
                await cache.AddAsync(new TestItem("1") { Options = new() { Type = InMemoryCacheEntryTypes.Timeout, Timeout = TimeSpan.FromMilliseconds(500), IsSlidingTimeout = true, AbsoluteTimeout = DateTime.Now + TimeSpan.FromSeconds(1) } });
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1 = await cache.GetAsync("1");
                Assert.IsNotNull(entry1);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(0, cache.Count);// Absolute timeout in effect

                // Sliding and absolute timeout (letting the relative timeout being hit)
                await cache.AddAsync(new TestItem("1") { Options = new() { Type = InMemoryCacheEntryTypes.Timeout, Timeout = TimeSpan.FromMilliseconds(500), IsSlidingTimeout = true, AbsoluteTimeout = DateTime.Now + TimeSpan.FromSeconds(3) } });
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1 = await cache.GetAsync("1");
                Assert.IsNotNull(entry1);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                entry1.OnAccess();
                await Task.Delay(200);
                Assert.AreEqual(1, cache.Count);
                await Task.Delay(500);
                Assert.AreEqual(0, cache.Count);// Sliding timeout in effect
            }
        }

        public sealed class TestCache(InMemoryCacheOptions options) : InMemoryCache<TestItem>(options)
        {
            public void StartTidyTimer() => TidyTimer.Start();

            public void StopTidyTimer() => TidyTimer.Stop();
        }

        public static Task<InMemoryCacheEntry<TestItem>> ItemFactory(
            InMemoryCache<TestItem> cache, 
            string key, 
            InMemoryCacheEntryOptions? options,
            CancellationToken cancellationToken
            )
            => Task.FromResult(new InMemoryCacheEntry<TestItem>(key, new(key)) { Cache = cache });

        public sealed class TestItem(string key, int? size = null) : DisposableBase(asyncDisposing: false), IInMemoryCacheItem
        {
            public readonly bool FixedSize = size.HasValue;
            public InMemoryCacheEntryOptions? _Options = null;
            public int _Size = size ?? 0;

            public string Key { get; } = key;

            public int Size => IfUndisposed(() => FixedSize ? _Size : ++_Size);

            public InMemoryCacheEntryOptions? Options
            {
                get => _Options ?? new() { Size = Size };
                set => _Options = value;
            }

            protected override void Dispose(bool disposing) { }
        }
    }
}
