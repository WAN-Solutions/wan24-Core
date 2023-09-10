using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DisposableLazyValue_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using DisposableLazyValue<Disposable_Tests.DisposeableTestType> value = new(() => new());
            using Disposable_Tests.DisposeableTestType obj = value.Value;
            Assert.IsNotNull(obj);
            Assert.IsFalse(obj.IsDisposed);
            Assert.IsFalse(obj.IsDisposing);
            value.Dispose();
            Assert.IsTrue(obj.IsDisposed);
        }
    }
}
