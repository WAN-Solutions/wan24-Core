using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Cancellations_Tests
    {
        [TestMethod]
        public void Cancelling_Tests()
        {
            using CancellationTokenSource cts1 = new();
            using CancellationTokenSource cts2 = new();
            using Cancellations test = new(cts1.Token, cts2.Token);
            int canceled = 0;
            test.Cancellation.Register(() => canceled++);
            Assert.IsFalse(test.IsDisposed);
            Assert.IsFalse(test.Cancellation.IsCancellationRequested);
            Assert.AreEqual(0, canceled);
            cts2.Cancel();
            Assert.AreEqual(1, canceled);
            Assert.IsTrue(test.IsDisposed);
            Assert.IsTrue(test.Cancellation.IsCancellationRequested);
            cts1.Cancel();
            Assert.AreEqual(1, canceled);
        }

        [TestMethod]
        public void Cancelled_Tests()
        {
            using CancellationTokenSource cts1 = new();
            using CancellationTokenSource cts2 = new();
            CancellationToken ct1 = cts1.Token;
            CancellationToken ct2 = cts2.Token;
            cts1.Cancel();
            using Cancellations test = new(ct1, ct2);
            Assert.IsTrue(test.IsDisposed);
            Assert.IsTrue(test.Cancellation.IsCancellationRequested);
        }
    }
}
