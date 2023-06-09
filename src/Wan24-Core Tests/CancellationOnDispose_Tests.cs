using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CancellationOnDispose_Tests
    {
        [TestMethod]
        public void Dispose_Tests()
        {
            using Disposable_Tests.DisposeableTestType obj = new();
            using CancellationOnDispose cod = new(obj);
            int canceled = 0;
            cod.Cancellation.Register(() => canceled++);
            Assert.AreEqual(0, canceled);
            Assert.IsFalse(cod.Cancellation.IsCancellationRequested);
            obj.Dispose();
            Assert.AreEqual(1, canceled);
            Assert.IsTrue(cod.Cancellation.IsCancellationRequested);
        }

        [TestMethod]
        public void Token_Tests()
        {
            using Disposable_Tests.DisposeableTestType obj = new();
            using CancellationTokenSource cts = new();
            using CancellationOnDispose cod = new(obj, cts.Token);
            int canceled = 0;
            cod.Cancellation.Register(() => canceled++);
            Assert.AreEqual(0, canceled);
            Assert.IsFalse(cod.Cancellation.IsCancellationRequested);
            cts.Cancel();
            Assert.AreEqual(1, canceled);
            Assert.IsTrue(cod.Cancellation.IsCancellationRequested);
            obj.Dispose();
            Assert.AreEqual(1, canceled);
        }

        [TestMethod]
        public void DisposeBeforeToken_Tests()
        {
            using Disposable_Tests.DisposeableTestType obj = new();
            using CancellationTokenSource cts = new();
            using CancellationOnDispose cod = new(obj, cts.Token);
            int canceled = 0;
            cod.Cancellation.Register(() => canceled++);
            Assert.AreEqual(0, canceled);
            Assert.IsFalse(cod.Cancellation.IsCancellationRequested);
            obj.Dispose();
            Assert.AreEqual(1, canceled);
            Assert.IsTrue(cod.Cancellation.IsCancellationRequested);
            cts.Cancel();
            Assert.AreEqual(1, canceled);
        }

        [TestMethod]
        public void Cancelled_Tests()
        {
            // Object canceled
            {
                using Disposable_Tests.DisposeableTestType obj = new();
                obj.Dispose();
                using CancellationOnDispose cod = new(obj);
                Assert.IsTrue(cod.IsDisposed);
                Assert.IsTrue(cod.Cancellation.IsCancellationRequested);
            }
            // Token canceled
            {
                using Disposable_Tests.DisposeableTestType obj = new();
                using CancellationTokenSource cts = new();
                CancellationToken token = cts.Token;
                cts.Cancel();
                using CancellationOnDispose cod = new(obj, token);
                Assert.IsTrue(cod.IsDisposed);
                Assert.IsTrue(cod.Cancellation.IsCancellationRequested);
            }
        }
    }
}
