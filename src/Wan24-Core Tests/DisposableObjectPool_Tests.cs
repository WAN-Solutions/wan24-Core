using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DisposableObjectPool_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using DisposableObjectPool<Disposable_Tests.DisposeableTestType> pool = new(1, () => new());
            using Disposable_Tests.DisposeableTestType obj = pool.Rent();
            Assert.IsNotNull(obj);
            pool.Return(obj);
            Assert.IsFalse(obj.IsDisposed);
            using Disposable_Tests.DisposeableTestType obj2 = new();
            pool.Return(obj2);
            Assert.IsTrue(obj2.IsDisposed);
            pool.Dispose();
            Assert.IsTrue(obj.IsDisposed);
        }
    }
}
