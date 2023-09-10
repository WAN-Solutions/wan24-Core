using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DisposableExtensions_Tests : TestBase
    {
        [TestMethod]
        public void DisposeAll_Tests()
        {
            Disposable_Tests.DisposeableTestType[] objs = new Disposable_Tests.DisposeableTestType[]
            {
                new(),
                new()
            };
            objs.DisposeAll();
            Assert.IsTrue(objs[0].IsDisposed);
            Assert.IsTrue(objs[1].IsDisposed);
        }
    }
}
