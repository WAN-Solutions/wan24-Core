using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectLockManager_Tests : TestBase
    {
        [TestMethod("ObjectLockManager_Tests.General_Tests"), Timeout(3000)]
        public async Task General_Tests()
        {
            ObjectLock? ol2 = null;
            Task ol2Getter;
            Logging.WriteInfo("Locking object");
            ObjectLock ol = await ObjectLockManager<TestObject>.Shared.LockAsync(new TestObject(1));
            await using (ol)
            {
                Logging.WriteInfo("Running second locking as background task");
                ol2Getter = Task.Run(async () => ol2 = await ObjectLockManager<TestObject>.Shared.LockAsync(new TestObject(1)));
                await Task.Delay(50);
                Assert.IsFalse(ol2Getter.IsCompleted);
                Assert.IsNotNull(ObjectLockManager<TestObject>.Shared.GetActiveLock(1));
                Assert.IsFalse(ol.Task.IsCompleted);
                Logging.WriteInfo("Sending completed task");
                await ol.RunTaskAsync(Task.CompletedTask);
                Assert.IsTrue(ol.IsDisposed);
                Assert.IsTrue(ol.Task.IsCompleted);
                Logging.WriteInfo("Disposing lock for object");
            }
            Logging.WriteInfo("Waiting for the second lock");
            await ol2Getter;
            Assert.IsNotNull(ol2);
            await using (ol2)
            {
                Assert.IsFalse(ol2.Task.IsCompleted);
                Logging.WriteInfo("Sending completed task");
                await ol2.RunTaskAsync(Task.CompletedTask);
                Logging.WriteInfo("Awaiting second lock");
                await ol2.Task;
                Assert.IsTrue(ol2.IsDisposed);
                Assert.IsTrue(ol2.Task.IsCompleted);
                Assert.IsNull(ObjectLockManager<TestObject>.Shared.GetActiveLock(1));
                Logging.WriteInfo("Disposing lock for object");
            }
            Logging.WriteInfo("Tests done");
        }

        public sealed class TestObject : IObjectKey
        {
            public TestObject(object key) => Key = key;

            public object Key { get; }
        }
    }
}
