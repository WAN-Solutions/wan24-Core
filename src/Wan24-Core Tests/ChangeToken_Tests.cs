using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ChangeToken_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            ChangeToken test = new();
            Assert.IsFalse(test.HasChanged);
            test.HasChanged = true;
            Assert.IsFalse(test.HasChanged);
            int changeCount = 0;
            using IDisposable changeCallback = test.RegisterChangeCallback((state) => changeCount++);
            test.InvokeCallbacks();
            Assert.AreEqual(1, changeCount);
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);
            changeCallback.Dispose();
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);

            ChangeTokenTest test2 = new();
            Assert.IsFalse(test2.HasChanged);
            test2.HasChanged = true;
            Assert.IsFalse(test2.HasChanged);
            ChangeTokenObserver observer = new();
            using IDisposable subscription = test2.Subscribe(observer);
            test2.InvokeCallbacks();
            Assert.AreEqual(1, observer.ChangeCount);
            test2.InvokeCallbacks();
            Assert.AreEqual(2, observer.ChangeCount);
            subscription.Dispose();
            test2.InvokeCallbacks();
            Assert.AreEqual(2, observer.ChangeCount);
        }

        [TestMethod]
        public void Disposable_Tests()
        {
            using DisposableChangeToken test = new();
            Assert.IsFalse(test.HasChanged);
            test.HasChanged = true;
            Assert.IsFalse(test.HasChanged);
            int changeCount = 0;
            using IDisposable changeCallback = test.RegisterChangeCallback((state) => changeCount++);
            test.InvokeCallbacks();
            Assert.AreEqual(1, changeCount);
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);
            test.Dispose();
            test.InvokeCallbacks();
            Assert.AreEqual(2, changeCount);

            using DisposableChangeTokenTest test2 = new();
            Assert.IsFalse(test2.HasChanged);
            test2.HasChanged = true;
            Assert.IsFalse(test2.HasChanged);
            DisposableChangeTokenObserver observer = new();
            using IDisposable subscription = test2.Subscribe(observer);
            test2.InvokeCallbacks();
            Assert.AreEqual(1, observer.ChangeCount);
            test2.InvokeCallbacks();
            Assert.AreEqual(2, observer.ChangeCount);
            test2.Dispose();
            Assert.IsTrue(observer.Completed);
            Assert.AreEqual(2, observer.ChangeCount);
        }

        public sealed class ChangeTokenTest : ChangeToken<ChangeTokenTest>
        {
            public ChangeTokenTest() : base() { }

            public int Value
            {
                set => HasChanged = true;
            }
        }

        public sealed class ChangeTokenObserver : IObserver<ChangeTokenTest>
        {
            public int ChangeCount = 0;

            public ChangeTokenObserver() { }

            public void OnCompleted() { }

            public void OnError(Exception error) { }

            public void OnNext(ChangeTokenTest value) => ChangeCount++;
        }

        public sealed class DisposableChangeTokenTest : DisposableChangeToken<DisposableChangeTokenTest>
        {
            public DisposableChangeTokenTest() : base() { }

            public int Value
            {
                set => HasChanged = true;
            }
        }

        public sealed class DisposableChangeTokenObserver : IObserver<DisposableChangeTokenTest>
        {
            public int ChangeCount = 0;
            public bool Completed = false;

            public DisposableChangeTokenObserver() { }

            public void OnCompleted() => Completed = true;

            public void OnError(Exception error) { }

            public void OnNext(DisposableChangeTokenTest value) => ChangeCount++;
        }
    }
}
