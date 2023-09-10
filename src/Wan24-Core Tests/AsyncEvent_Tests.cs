using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class AsyncEvent_Tests : TestBase
    {
        [TestMethod]
        public async Task General_Tests()
        {
            AsyncEvent<object, EventArgs> ae = new(new());
            IAsyncEvent<object, EventArgs> pae = ae;
            int count = 0,
                asyncCount = 0;
            void syncHandler(object sender, EventArgs e, CancellationToken cancellationToken) => count++;
            async Task asyncHandler(object sender, EventArgs e, CancellationToken cancellationToken)
            {
                await Task.Yield();
                asyncCount++;
            }
            // Add listeners
            ae += syncHandler;
            ae += asyncHandler;
            Assert.IsTrue(ae);
            // Raise
            await pae.RaiseEventAsync();
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, asyncCount);
            // Remove listener and raise
            ae -= syncHandler;
            Assert.IsTrue(ae);
            await pae.RaiseEventAsync();
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, asyncCount);
            // Remove all listener and raise
            ae -= asyncHandler;
            Assert.IsFalse(ae);
            await pae.RaiseEventAsync();
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, asyncCount);
            Assert.AreEqual(3, ae.RaiseCount);
        }

        [TestMethod]
        public async Task Object_Tests()
        {
            TestObject obj = new();
            int count = 0,
                asyncCount = 0;
            void syncHandler(object sender, EventArgs e, CancellationToken cancellationToken) => count++;
            async Task asyncHandler(object sender, EventArgs e, CancellationToken cancellationToken)
            {
                await Task.Yield();
                asyncCount++;
            }
            // Add listeners
            obj.OnEvent.Listen(syncHandler);
            obj.OnEvent.Listen(asyncHandler);
            Assert.IsTrue(obj.OnEvent);
            // Raise
            await obj.PrivateOnEvent.RaiseEventAsync();
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, asyncCount);
            // Remove listener and raise
            obj.OnEvent.Detach(syncHandler);
            Assert.IsTrue(obj.OnEvent);
            await obj.PrivateOnEvent.RaiseEventAsync();
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, asyncCount);
            // Remove all listener and raise
            obj.OnEvent.Detach(asyncHandler);
            Assert.IsFalse(obj.OnEvent);
            await obj.PrivateOnEvent.RaiseEventAsync();
            Assert.AreEqual(1, count);
            Assert.AreEqual(2, asyncCount);
            Assert.AreEqual(3, obj.OnEvent.RaiseCount);
        }

        private sealed class TestObject
        {
            public readonly AsyncEvent<TestObject, EventArgs> OnEvent;

            public TestObject() => OnEvent = new(this);

            public IAsyncEvent<TestObject, EventArgs> PrivateOnEvent => OnEvent;
        }
    }
}
