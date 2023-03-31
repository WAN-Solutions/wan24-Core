using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Disposable_Tests
    {
        [TestMethod]
        public void Dispose_Tests()
        {
            int disposed = 0,
                disposing = 0;
            using DisposeableTestType obj = new();
            Assert.IsFalse(obj.IsDisposing);
            Assert.IsFalse(obj.IsDisposed);
            obj.OnDisposing += (s, e) =>
            {
                disposing++;
                Assert.IsTrue(obj.IsDisposing);
                Assert.IsFalse(obj.IsDisposed);
                Assert.AreEqual(1, disposing);
                Assert.AreEqual(0, disposed);
            };
            obj.OnDisposed += (s, e) =>
            {
                disposed++;
                Assert.IsTrue(obj.IsDisposing);
                Assert.IsTrue(obj.IsDisposed);
                Assert.AreEqual(1, disposing);
                Assert.AreEqual(1, disposed);
            };
            obj.Dispose();
            Assert.AreEqual(1, disposing);
            Assert.AreEqual(1, disposed);
            Assert.IsTrue(obj.IsDisposing);
            Assert.IsTrue(obj.IsDisposed);
        }

        [TestMethod]
        public async Task DisposeAsync_Tests()
        {
            int disposed = 0,
                disposing = 0;
            using DisposeableTestType obj = new();
            Assert.IsFalse(obj.IsDisposing);
            Assert.IsFalse(obj.IsDisposed);
            obj.OnDisposing += (s, e) =>
            {
                disposing++;
                Assert.IsTrue(obj.IsDisposing);
                Assert.IsFalse(obj.IsDisposed);
                Assert.AreEqual(1, disposing);
                Assert.AreEqual(0, disposed);
            };
            obj.OnDisposed += (s, e) =>
            {
                disposed++;
                Assert.IsTrue(obj.IsDisposing);
                Assert.IsTrue(obj.IsDisposed);
                Assert.AreEqual(1, disposing);
                Assert.AreEqual(1, disposed);
            };
            await obj.DisposeAsync();
            Assert.AreEqual(1, disposing);
            Assert.AreEqual(1, disposed);
            Assert.IsTrue(obj.IsDisposing);
            Assert.IsTrue(obj.IsDisposed);
        }

        public sealed class DisposeableTestType : DisposableBase
        {
            public DisposeableTestType() : base() { }

            protected override void Dispose(bool disposing) { }
        }
    }
}
