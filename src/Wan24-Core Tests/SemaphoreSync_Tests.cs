using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class SemaphoreSync_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using SemaphoreSync sync = new();
            Assert.IsFalse(sync.IsSynchronized);
            using SemaphoreSyncContext ssc = sync;
            Assert.IsTrue(sync.IsSynchronized);
            bool synced = false;
            Task task = Task.Run(() =>
            {
                using SemaphoreSyncContext ssc = sync;
                synced = true;
            });
            Thread.Sleep(200);
            Assert.IsFalse(synced);
            ssc.Dispose();
            task.Wait();
            Assert.IsTrue(synced);
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            using SemaphoreSync sync = new();
            Assert.IsFalse(sync.IsSynchronized);
            using SemaphoreSyncContext ssc = await sync.SyncContextAsync();
            Assert.IsTrue(sync.IsSynchronized);
            bool synced = false;
            Task task = Task.Run(async () =>
            {
                using SemaphoreSyncContext ssc = await sync.SyncContextAsync();
                synced = true;
            });
            await Task.Delay(200);
            Assert.IsFalse(synced);
            ssc.Dispose();
            await task;
            Assert.IsTrue(synced);
        }

        [TestMethod, Timeout(3000)]
        public void Object_Tests()
        {
            object obj = new();
            using SemaphoreSync sync = new(obj);
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(1, SemaphoreSync.GetSynchronizationInstanceCount(obj));
            using SemaphoreSyncContext ssc = sync;
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(1, SemaphoreSync.GetSynchronizationInstanceCount(obj));

            using SemaphoreSync sync2 = new(obj);
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(2, SemaphoreSync.GetSynchronizationInstanceCount(obj));
            bool synced = false;
            Task task = Task.Run(() =>
            {
                using SemaphoreSyncContext ssc = sync2;
                synced = true;
            });
            Thread.Sleep(200);
            Assert.IsFalse(synced);
            ssc.Dispose();
            task.Wait();
            Assert.IsTrue(synced);

            sync2.Dispose();
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(1, SemaphoreSync.GetSynchronizationInstanceCount(obj));

            sync.Dispose();
            Assert.AreEqual(0, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(0, SemaphoreSync.GetSynchronizationInstanceCount(obj));
        }

        [TestMethod, Timeout(3000)]
        public async Task ObjectAsync_Tests()
        {
            object obj = new();
            using SemaphoreSync sync = new(obj);
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(1, SemaphoreSync.GetSynchronizationInstanceCount(obj));
            using SemaphoreSyncContext ssc = await sync.SyncContextAsync();
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(1, SemaphoreSync.GetSynchronizationInstanceCount(obj));

            using SemaphoreSync sync2 = new(obj);
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(2, SemaphoreSync.GetSynchronizationInstanceCount(obj));
            bool synced = false;
            Task task = Task.Run(async () =>
            {
                using SemaphoreSyncContext ssc = await sync2.SyncContextAsync();
                synced = true;
            });
            await Task.Delay(200);
            Assert.IsFalse(synced);
            ssc.Dispose();
            await task;
            Assert.IsTrue(synced);
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(2, SemaphoreSync.GetSynchronizationInstanceCount(obj));

            sync2.Dispose();
            Assert.AreEqual(1, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(1, SemaphoreSync.GetSynchronizationInstanceCount(obj));

            sync.Dispose();
            Assert.AreEqual(0, SemaphoreSync.SynchronizedObjectCount);
            Assert.AreEqual(0, SemaphoreSync.GetSynchronizationInstanceCount(obj));
        }
    }
}
