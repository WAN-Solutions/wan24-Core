using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class RentedThread_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            int worker = 0;
            DisposableObjectPool<RentedThread> pool = new(capacity: 1, () => new());
            await using (pool)
            {
                using RentedObject<RentedThread> thread = new(pool);
                await thread.Object.WorkAsync((t, ct) => worker++);
                Assert.AreEqual(1, worker);

                using RentedObject<RentedThread> thread2 = new(pool);
                await thread2.Object.WorkAsync((t, ct) => worker++);
                Assert.AreEqual(2, worker);

                RentedThread rentedThread = thread2.Object;
                await thread2.DisposeAsync();
                Assert.IsFalse(rentedThread.IsDisposing);

                rentedThread = thread.Object;
                await thread.DisposeAsync();
                Assert.IsTrue(rentedThread.IsDisposing);// Over pool capacity
            }
        }

        [TestMethod, Timeout(3000)]
        public async Task GenericGeneralAsync_Tests()
        {
            int worker = 0;
            DisposableObjectPool<RentedThread<int>> pool = new(capacity: 1, () => new());
            await using (pool)
            {
                using RentedObject<RentedThread<int>> thread = new(pool);
                Assert.AreEqual(1, await thread.Object.WorkAsync((t, ct) => ++worker));
                Assert.AreEqual(1, worker);

                using RentedObject<RentedThread<int>> thread2 = new(pool);
                Assert.AreEqual(2, await thread2.Object.WorkAsync((t, ct) => ++worker));
                Assert.AreEqual(2, worker);

                RentedThread<int> rentedThread = thread2.Object;
                await thread2.DisposeAsync();
                Assert.IsFalse(rentedThread.IsDisposing);

                rentedThread = thread.Object;
                await thread.DisposeAsync();
                Assert.IsTrue(rentedThread.IsDisposing);// Over pool capacity
            }
        }
    }
}
