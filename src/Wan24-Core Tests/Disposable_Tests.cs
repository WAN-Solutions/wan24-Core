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
            Assert.IsTrue(obj.Disposing.IsDisposed);
            Assert.AreNotEqual(byte.MaxValue, obj.Bytes[0]);
            Assert.AreNotEqual('a', obj.Characters[0]);
        }

        [TestMethod]
        public void DisposeGeneric_Tests()
        {
            int disposed = 0,
                disposing = 0;
            using DisposeableTestType3 obj = new();
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
            Assert.IsTrue(obj.Disposing.IsDisposed);
            Assert.AreNotEqual(byte.MaxValue, obj.Bytes[0]);
            Assert.AreNotEqual('a', obj.Characters[0]);
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
            Assert.IsTrue(obj.Disposing.IsDisposed);
            Assert.AreNotEqual(byte.MaxValue, obj.Bytes[0]);
            Assert.AreNotEqual('a', obj.Characters[0]);
        }

        [TestMethod]
        public async Task DisposeGenericAsync_Tests()
        {
            int disposed = 0,
                disposing = 0;
            using DisposeableTestType3 obj = new();
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
            Assert.IsTrue(obj.Disposing.IsDisposed);
            Assert.AreNotEqual(byte.MaxValue, obj.Bytes[0]);
            Assert.AreNotEqual('a', obj.Characters[0]);
        }

        public sealed class DisposeableTestType : DisposableBase
        {
            [Dispose]
            public readonly DisposeableTestType2 Disposing = new();
            [Dispose]
            public readonly byte[] Bytes = new byte[] { byte.MaxValue };
            [Dispose]
            public readonly char[] Characters = new char[] { 'a' };

            public DisposeableTestType() : base() { }

            protected override void Dispose(bool disposing) => DisposeAttributes();
        }

        public sealed class DisposeableTestType2 : DisposableBase
        {
            public DisposeableTestType2() : base() { }

            protected override void Dispose(bool disposing) { }
        }

        public sealed class DisposeableTestType3 : DisposableBase<DisposeableTestType3>
        {
            [Dispose]
            public readonly DisposeableTestType2 Disposing = new();
            [Dispose]
            public readonly byte[] Bytes = new byte[] { byte.MaxValue };
            [Dispose]
            public readonly char[] Characters = new char[] { 'a' };

            public DisposeableTestType3() : base() { }

            protected override void Dispose(bool disposing) => DisposeAttributes();
        }
    }
}
