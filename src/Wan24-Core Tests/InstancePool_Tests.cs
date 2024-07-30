using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class InstancePool_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public async Task General_Tests()
        {
            InstancePool<TestObject> pool = new(capacity: 1, async (pool, ct) => await TestObject.CreateAsync());
            await using (pool)
            {
                await pool.StartAsync();
                await wan24.Core.Timeout.WaitConditionAsync(TimeSpan.FromMilliseconds(50), (ct) => Task.FromResult(pool.Available == 1));
                Assert.IsNotNull(pool.GetOne());
                Assert.AreEqual(0, pool.Available);
                await wan24.Core.Timeout.WaitConditionAsync(TimeSpan.FromMilliseconds(50), (ct) => Task.FromResult(pool.Available == 1));
                DateTime started = DateTime.Now;
                Assert.AreEqual(3, (await pool.GetManyAsync(3).ToListAsync()).Count);
                Assert.IsTrue(pool.CreatedOnDemand != 0);
                Assert.IsTrue(started - DateTime.Now < TimeSpan.FromMilliseconds(140));
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
