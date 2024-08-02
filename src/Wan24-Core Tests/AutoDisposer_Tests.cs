using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class AutoDisposer_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            Test test = new();
            using (AutoDisposer<Test> disposer = new(test))
            {
                Assert.IsFalse(test.IsDisposing);
                using (AutoDisposer<Test>.Context context = disposer.UseObject())
                {
                    Assert.IsFalse(test.IsDisposed);
                    Assert.AreEqual(1, disposer.UsageCount);
                }
                Assert.IsFalse(test.IsDisposed);
                Assert.AreEqual(0, disposer.UsageCount);
            }
            Assert.IsTrue(test.IsDisposing);
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            Test test = new();
            AutoDisposer<Test> disposer = new(test);
            await using (disposer)
            {
                Assert.IsFalse(test.IsDisposing);
                AutoDisposer<Test>.Context context = await disposer.UseObjectAsync();
                await using (context)
                {
                    Assert.IsFalse(test.IsDisposed);
                    Assert.AreEqual(1, disposer.UsageCount);
                }
                Assert.IsFalse(test.IsDisposed);
                Assert.AreEqual(0, disposer.UsageCount);
            }
            Assert.IsTrue(test.IsDisposing);
        }

        [TestMethod]
        public void ShouldDispose_Tests()
        {
            Test test = new();
            using AutoDisposer<Test> disposer = new(test);
            Assert.IsFalse(test.IsDisposing);
            using (AutoDisposer<Test>.Context context = disposer.UseObject())
            {
                Assert.IsFalse(test.IsDisposed);
                Assert.AreEqual(1, disposer.UsageCount);
            }
            Assert.IsFalse(test.IsDisposed);
            Assert.AreEqual(0, disposer.UsageCount);
            using (AutoDisposer<Test>.Context context2 = disposer.UseObject())
            {
                Assert.IsFalse(test.IsDisposed);
                Assert.AreEqual(1, disposer.UsageCount);
                disposer.ShouldDispose = true;
                Assert.IsFalse(test.IsDisposed);
                Assert.AreEqual(1, disposer.UsageCount);
            }
            Assert.IsTrue(test.IsDisposing);
            Assert.IsTrue(disposer.IsDisposing);
        }

        [TestMethod]
        public async Task ShouldDisposeAsync_Tests()
        {
            Test test = new();
            AutoDisposer<Test> disposer = new(test);
            await using (disposer)
            {
                Assert.IsFalse(test.IsDisposing);
                AutoDisposer<Test>.Context context = disposer.UseObject();
                await using (context)
                {
                    Assert.IsFalse(test.IsDisposed);
                    Assert.AreEqual(1, disposer.UsageCount);
                }
                Assert.IsFalse(test.IsDisposed);
                Assert.AreEqual(0, disposer.UsageCount);
                AutoDisposer<Test>.Context context2 = disposer.UseObject();
                await using (context2)
                {
                    Assert.IsFalse(test.IsDisposed);
                    Assert.AreEqual(1, disposer.UsageCount);
                    await disposer.SetShouldDisposeAsync();
                    Assert.IsFalse(test.IsDisposed);
                    Assert.AreEqual(1, disposer.UsageCount);
                }
                Assert.IsTrue(test.IsDisposing);
                Assert.IsTrue(disposer.IsDisposing);
            }
        }

        public sealed class Test() : DisposableBase()
        {
            protected override void Dispose(bool disposing) { }
        }
    }
}
