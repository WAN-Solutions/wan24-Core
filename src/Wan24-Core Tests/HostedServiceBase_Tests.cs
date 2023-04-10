﻿using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class HostedServiceBase_Tests
    {
        [TestMethod, Timeout(3000)]
        public async Task General_Tests()
        {
            using TestObject service = new();
            await service.StartAsync(default);
            service.WorkDone.Set();
            Thread.Sleep(20);
            Assert.IsFalse(service.IsRunning);
        }

        public sealed class TestObject : HostedServiceBase
        {
            public readonly ManualResetEventSlim WorkDone = new(initialState: false);

            public TestObject() : base() { }

            protected override async Task WorkerAsync()
            {
                await Task.Yield();
                WorkDone.Wait();
            }

            protected override void Dispose(bool disposing) => WorkDone.Dispose();
        }
    }
}
