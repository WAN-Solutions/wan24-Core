﻿using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CancellationAwaiter_Tests : TestBase
    {
        [TestMethod("CancellationAwaiter_Tests.General_Tests"), Timeout(3000)]
        public async Task General_Tests()
        {
            using CancellationTokenSource cts = new();
            DateTime start = DateTime.Now;
            _ = DelayCancellation(cts);
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await cts.Token);
            TimeSpan runTime = DateTime.Now - start;
            Logging.WriteInfo($"Runtime {runTime}");
            Assert.IsTrue(runTime.TotalMilliseconds >= 80);// Task.Delay isn't very accurate :(
        }

        private static async Task DelayCancellation(CancellationTokenSource cts)
        {
            await Task.Delay(100);
            cts.Cancel();
        }
    }
}
