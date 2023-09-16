using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class AsyncDisposableExtensions_Tests : TestBase
    {
        [TestMethod]
        public async Task Context_Tests()
        {
            Disposable_Tests.DisposeableTestType obj = new();
            await using (obj.DynamicContext()) { }
            Assert.IsTrue(obj.IsDisposed);
            obj = new();
            await using (obj.FixedContext()) { }
            Assert.IsTrue(obj.IsDisposed);
        }

        [TestMethod]
        public async Task DisposeAll_Tests()
        {
            Disposable_Tests.DisposeableTestType[] objs = new Disposable_Tests.DisposeableTestType[]
            {
                new(),
                new()
            };
            await objs.DisposeAllAsync();
            Assert.IsTrue(objs[0].IsDisposed);
            Assert.IsTrue(objs[1].IsDisposed);
        }

        [TestMethod]
        public async Task DisposeAllParallel_Tests()
        {
            Disposable_Tests.DisposeableTestType[] objs = new Disposable_Tests.DisposeableTestType[]
            {
                new(),
                new()
            };
            await objs.DisposeAllAsync(parallel: true);
            Assert.IsTrue(objs[0].IsDisposed);
            Assert.IsTrue(objs[1].IsDisposed);
        }
    }
}
