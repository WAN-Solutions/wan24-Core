using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class GlobalLock_Tests
    {
        [TestMethod, Timeout(3000)]
        public void Sync_Tests()
        {
            Guid guid = Guid.NewGuid();
            using GlobalLock lock1 = new(guid);
            using GlobalLock lock2 = new(guid, 100);// The same thread can lock the same GUID multiple times
        }

        [TestMethod, Timeout(3000)]
        public async Task Async_Tests()
        {
            // Create a lock asynchronous
            Guid guid = Guid.NewGuid();
            using GlobalLockAsync lock1 = await GlobalLockAsync.CreateAsync(guid).DynamicContext();

            // Test 2nd lock (only the same thread can lock the same GUID multiple times!)
            await Assert.ThrowsExceptionAsync<TimeoutException>(async () =>
            {
                using GlobalLockAsync lock2 = await GlobalLockAsync.CreateAsync(guid, 100).DynamicContext();
            }).DynamicContext();

            // A lock can be disposed from any thread
            Exception? ex = null;
            Thread thread = new(() =>
            {
                try
                {
                    lock1.Dispose();
                }
                catch (Exception e)
                {
                    ex = e;
                }
            });
            thread.Start();
            thread.Join();
            Assert.IsNull(ex);

            // Asynchronous disposer
            GlobalLockAsync lock3 = await GlobalLockAsync.CreateAsync(guid).DynamicContext();
            await using (lock3.DynamicContext()) { }
        }
    }
}
