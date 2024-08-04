using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ChangeTokenCollection_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using ChangeTokenCollection<ChangeToken_Tests.ChangeTokenTest> collection = new();
            ChangeTokenCollectionObserver observer = new();
            using IDisposable subscription = collection.Subscribe(observer);
            collection.Add(new());
            Assert.AreEqual(1, observer.ChangeCount);
            collection[0].InvokeCallbacks();
            Assert.AreEqual(2, observer.ChangeCount);
        }

        public sealed class ChangeTokenCollectionObserver : IObserver<ChangeTokenCollection<ChangeToken_Tests.ChangeTokenTest>>
        {
            public int ChangeCount = 0;

            public ChangeTokenCollectionObserver() { }

            public void OnCompleted() { }

            public void OnError(Exception error) { }

            public void OnNext(ChangeTokenCollection<ChangeToken_Tests.ChangeTokenTest> value) => ChangeCount++;
        }
    }
}
