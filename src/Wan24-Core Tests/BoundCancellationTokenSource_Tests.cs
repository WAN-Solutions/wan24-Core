using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BoundCancellationTokenSource_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using CancellationTokenSource tcs = new();
            using BoundCancellationTokenSource tcs2 = new(tcs.Token);
            tcs.Cancel();
            Assert.IsTrue(tcs2.IsCancellationRequested);
        }
    }
}
