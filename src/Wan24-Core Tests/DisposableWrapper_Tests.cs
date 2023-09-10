using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DisposableWrapper_Tests : TestBase
    {
        [TestMethod]
        public async Task General_Tests()
        {
            // Synchronous dispose synchronous dispose action
            int count = 0;
            using (DisposableWrapper<bool> wrapper = new(true, (disposing) =>
            {
                count++;
            }))
            {
                Assert.AreEqual(0, count);
                Assert.IsTrue(wrapper.Object);
                wrapper.Dispose();
                Assert.AreEqual(1, count);
                Assert.ThrowsException<ObjectDisposedException>(() => Assert.IsTrue(wrapper.Object));
            }
            // Asynchronous dispose synchronous dispose action
            count = 0;
            using (DisposableWrapper<bool> wrapper = new(true, (disposing) =>
            {
                count++;
            }))
            {
                Assert.AreEqual(0, count);
                Assert.IsTrue(wrapper.Object);
                await wrapper.DisposeAsync();
                Assert.AreEqual(1, count);
                Assert.ThrowsException<ObjectDisposedException>(() => Assert.IsTrue(wrapper.Object));
            }
            // Asynchronous dispose asynchronous dispose action
            count = 0;
            using (DisposableWrapper<bool> wrapper = new(true, async () =>
            {
                await Task.Yield();
                count++;
            }))
            {
                Assert.AreEqual(0, count);
                Assert.IsTrue(wrapper.Object);
                await wrapper.DisposeAsync();
                Assert.AreEqual(1, count);
                Assert.ThrowsException<ObjectDisposedException>(() => Assert.IsTrue(wrapper.Object));
            }
            // Synchronous dispose asynchronous dispose action
            count = 0;
            using (DisposableWrapper<bool> wrapper = new(true, async () =>
            {
                await Task.Yield();
                count++;
            }))
            {
                Assert.AreEqual(0, count);
                Assert.IsTrue(wrapper.Object);
                wrapper.Dispose();
                Assert.AreEqual(1, count);
                Assert.ThrowsException<ObjectDisposedException>(() => Assert.IsTrue(wrapper.Object));
            }
            // Synchronous dispose, two dispose actions
            count = 0;
            int countAsync = 0;
            using (DisposableWrapper<bool> wrapper = new(true, (disposing) =>
            {
                count++;
            }, async () =>
            {
                await Task.Yield();
                countAsync++;
            }))
            {
                Assert.AreEqual(0, count);
                Assert.AreEqual(0, countAsync);
                Assert.IsTrue(wrapper.Object);
                wrapper.Dispose();
                Assert.AreEqual(1, count);
                Assert.AreEqual(0, countAsync);
                Assert.ThrowsException<ObjectDisposedException>(() => Assert.IsTrue(wrapper.Object));
            }
            // Asynchronous dispose, two dispose actions
            count = 0;
            countAsync = 0;
            using (DisposableWrapper<bool> wrapper = new(true, (disposing) =>
            {
                count++;
            }, async () =>
            {
                await Task.Yield();
                countAsync++;
            }))
            {
                Assert.AreEqual(0, count);
                Assert.AreEqual(0, countAsync);
                Assert.IsTrue(wrapper.Object);
                await wrapper.DisposeAsync();
                Assert.AreEqual(0, count);
                Assert.AreEqual(1, countAsync);
                Assert.ThrowsException<ObjectDisposedException>(() => Assert.IsTrue(wrapper.Object));
            }
        }
    }
}
