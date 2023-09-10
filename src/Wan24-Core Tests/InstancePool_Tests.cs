using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class InstancePool_Tests
    {
        [TestMethod, Timeout(3000)]
        public async Task General_Tests()
        {
            InstancePool<TestObject> pool = new(capacity: 1, async (pool, ct) => await TestObject.CreateAsync());
            await using (pool)
            {
                await pool.StartAsync();
                await Task.Delay(100);
                Assert.AreEqual(1, pool.Available);
                Assert.IsNotNull(pool.GetOne());
                Assert.AreEqual(0, pool.Available);
                await Task.Delay(100);
                Assert.AreEqual(1, pool.Available);
                DateTime started = DateTime.Now;
                Assert.AreEqual(2, (await pool.GetManyAsync(2).ToListAsync()).Count);
                Assert.IsTrue(started - DateTime.Now < TimeSpan.FromMilliseconds(100));
            }
        }

        public sealed class TestObject
        {
            private TestObject() { }

            public static async Task<TestObject> CreateAsync()
            {
                await Task.Delay(50);
                return new();
            }
        }
    }
}
