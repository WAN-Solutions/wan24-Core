using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ChangeToken_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            bool changed = false;
            ChangeToken test = new(() => changed);
            Assert.IsFalse(test.HasChanged);
            changed = true;
            Assert.IsTrue(test.HasChanged);
            int changeCount = 0;
            using IDisposable changeCallback = test.RegisterChangeCallback((state) => changeCount++);
            test.InvokeCallbacks();
            Assert.AreEqual(1, changeCount);
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);
            changeCallback.Dispose();
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);
        }

        [TestMethod]
        public void Disposable_Tests()
        {
            bool changed = false;
            using DisposableChangeToken test = new(() => changed);
            Assert.IsFalse(test.HasChanged);
            changed = true;
            Assert.IsTrue(test.HasChanged);
            int changeCount = 0;
            using IDisposable changeCallback = test.RegisterChangeCallback((state) => changeCount++);
            test.InvokeCallbacks();
            Assert.AreEqual(1, changeCount);
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);
            test.Dispose();
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);
        }
    }
}
