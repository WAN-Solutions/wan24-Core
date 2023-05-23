using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CancellationAwaiter_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            using CancellationTokenSource cts = new();
            DateTime start = DateTime.Now;
            _ = DelayCancellation(cts);
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await cts.Token);
            Assert.IsTrue((DateTime.Now - start).TotalMilliseconds >= 100);
        }

        private static async Task DelayCancellation(CancellationTokenSource cts)
        {
            await Task.Delay(100);
            cts.Cancel();
        }
    }
}
