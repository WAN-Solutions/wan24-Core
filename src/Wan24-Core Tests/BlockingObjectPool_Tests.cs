using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BlockingObjectPool_Tests
    {
        [TestMethod("BlockingObjectPool_Tests.General_Tests"), Timeout(1000)]
        public async Task General_Tests()
        {
            Logging.WriteInfo("BlockingObjectPool tests");
            // Initial
            Logging.WriteInfo("Initial");
            using BlockingObjectPool<bool> pool = new(2, () => true);
            Assert.AreEqual(2, pool.Capacity);
            Assert.AreEqual(0, pool.Initialized);
            Assert.AreEqual(0, pool.Available);
            // Factory
            Logging.WriteInfo("Factory");
            Assert.IsTrue(pool.Rent());
            Assert.AreEqual(2, pool.Capacity);
            Assert.AreEqual(1, pool.Initialized);
            Assert.AreEqual(0, pool.Available);
            // Returning while the pool wasn't fully initialized
            Logging.WriteInfo("Returning");
            pool.Return(false);
            Assert.AreEqual(2, pool.Capacity);
            Assert.AreEqual(1, pool.Initialized);
            Assert.AreEqual(1, pool.Available);
            // Renting the previously returned value
            Logging.WriteInfo("Renting");
            Assert.IsFalse(pool.Rent());
            // Renting a factory created value
            Logging.WriteInfo("Renting factory");
            Assert.IsTrue(pool.Rent());
            Assert.AreEqual(2, pool.Capacity);
            Assert.AreEqual(2, pool.Initialized);
            Assert.AreEqual(0, pool.Available);
            // Renting block
            Logging.WriteInfo("Renting block");
            bool value = true;
            Task rentTask = Task.Run(() => value = pool.Rent());
            Assert.IsTrue(value);
            pool.Return(false);
            await rentTask;
            Assert.IsFalse(value);
            // Exception when returning too many values
            Logging.WriteInfo("Returning too many");
            pool.Return(false);
            pool.Return(false);
            Assert.ThrowsException<OverflowException>(() => pool.Return(false));
        }

        [TestMethod("BlockingObjectPool_Tests.GeneralAsync_Tests"), Timeout(1000)]
        public async Task GeneralAsync_Tests()
        {
            Logging.WriteInfo("BlockingObjectPool async tests");
            // Initial
            Logging.WriteInfo("Initial");
            BlockingObjectPool<bool> pool = new(2, () => true);
            await using (pool.DynamicContext())
            {
                Assert.AreEqual(2, pool.Capacity);
                Assert.AreEqual(0, pool.Initialized);
                Assert.AreEqual(0, pool.Available);
                // Factory
                Logging.WriteInfo("Factory");
                Assert.IsTrue(await pool.RentAsync());
                Assert.AreEqual(2, pool.Capacity);
                Assert.AreEqual(1, pool.Initialized);
                Assert.AreEqual(0, pool.Available);
                // Returning while the pool wasn't fully initialized
                Logging.WriteInfo("Returning");
                await pool.ReturnAsync(false);
                Assert.AreEqual(2, pool.Capacity);
                Assert.AreEqual(1, pool.Initialized);
                Assert.AreEqual(1, pool.Available);
                // Renting the previously returned value
                Logging.WriteInfo("Renting");
                Assert.IsFalse(await pool.RentAsync());
                // Renting a factory created value
                Logging.WriteInfo("Renting factory");
                Assert.IsTrue(await pool.RentAsync());
                Assert.AreEqual(2, pool.Capacity);
                Assert.AreEqual(2, pool.Initialized);
                Assert.AreEqual(0, pool.Available);
                // Renting block
                Logging.WriteInfo("Renting block");
                bool value = true;
                Task rentTask = Task.Run(async () => value = await pool.RentAsync());
                Assert.IsTrue(value);
                await pool.ReturnAsync(false);
                await rentTask;
                Assert.IsFalse(value);
                // Exception when returning too many values
                Logging.WriteInfo("Returning too many");
                await pool.ReturnAsync(false);
                await pool.ReturnAsync(false);
                await Assert.ThrowsExceptionAsync<OverflowException>(async () => await pool.ReturnAsync(false));
            }
        }
    }
}
