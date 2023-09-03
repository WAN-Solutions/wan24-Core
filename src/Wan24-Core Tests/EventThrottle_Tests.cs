using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class EventThrottle_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            using TestObject throttle = new();
            throttle.Raise();
            throttle.Raise();
            throttle.Raise();
            Assert.AreEqual(1, throttle.Handled);
            await Task.Delay(50);
            Assert.AreEqual(2, throttle.Handled);
            await Task.Delay(50);
            Assert.AreEqual(2, throttle.Handled);
            throttle.Raise();
            Assert.AreEqual(3, throttle.Handled);
            throttle.Raise();
            Assert.AreEqual(3, throttle.Handled);
            await Task.Delay(50);
            Assert.AreEqual(4, throttle.Handled);
        }

        public sealed class TestObject : EventThrottle
        {
            public int Handled = 0;

            public TestObject() : base(20) { }

            protected override void HandleEvent(in DateTime raised, in int raisedCount)
            {
                Handled++;
            }
        }
    }
}
