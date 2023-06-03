using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class HostedServiceBase_Tests
    {
        [TestMethod("HostedServiceBase_Tests.General_Tests"), Timeout(3000)]
        public async Task General_Tests()
        {
            using TestObject service = new();
            await service.StartAsync(default);
            service.WorkDone.Set();
            await Task.Delay(100);
            Assert.AreEqual(1, service.Worked);
            Assert.IsFalse(service.IsRunning);
        }

        public sealed class TestObject : HostedServiceBase
        {
            public readonly ManualResetEventSlim WorkDone = new(initialState: false);

            public int Worked = 0;

            public TestObject() : base() { }

            protected override async Task WorkerAsync()
            {
                await Task.Yield();
                WorkDone.Wait();
                Worked++;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                WorkDone.Dispose();
            }

            protected override async Task DisposeCore()
            {
                await base.DisposeCore().DynamicContext();
                WorkDone.Dispose();
            }
        }
    }
}
