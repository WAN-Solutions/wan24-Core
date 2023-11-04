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
            using CancellationTokenSource tcs2 = new();
            using BoundCancellationTokenSource bcts = new(tcs.Token, tcs2.Token);
            tcs2.Cancel();
            Assert.IsTrue(bcts.IsCancellationRequested);
        }
    }
}
