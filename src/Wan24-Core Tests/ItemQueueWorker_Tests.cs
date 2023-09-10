using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ItemQueueWorker_Tests : TestBase
    {
        [TestMethod("ItemQueueWorker_Tests.General_Tests"), Timeout(3000)]
        public async Task General_Tests()
        {
            using TestObject worker = new();
            await worker.StartAsync(default);
            using ManualResetEventSlim mre = new(initialState: false);
            await worker.EnqueueAsync(mre);
            await Task.Delay(20);
            Assert.AreEqual(0, worker.Worked);
            mre.Set();
            await Task.Delay(100);
            Assert.AreEqual(1, worker.Worked);
        }

        public sealed class TestObject : ItemQueueWorkerBase<ManualResetEventSlim>
        {
            public int Worked = 0;

            public TestObject() : base(1) { }

            protected override async Task ProcessItem(ManualResetEventSlim item, CancellationToken cancellationToken)
            {
                await Task.Yield();
                item.Wait(cancellationToken);
                Worked++;
            }
        }
    }
}
