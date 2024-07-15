using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Throttle_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using Throttle throttle = new(2, TimeSpan.FromMilliseconds(200));

            DateTime start = DateTime.Now;
            throttle.CountOne();
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(50));
            Assert.IsFalse(throttle.IsThrottling);
            Assert.IsFalse(throttle.WillThrottle);

            start = DateTime.Now;
            throttle.CountOne();
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(50));
            Assert.AreEqual(2, throttle.CurrentCount);
            Assert.IsTrue(throttle.IsThrottling);
            Assert.IsTrue(throttle.WillThrottle);

            start = DateTime.Now;
            throttle.Count(count: 4);
            Logging.WriteInfo((DateTime.Now - start).ToString());
            Assert.IsTrue(DateTime.Now - start > TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(4, throttle.CurrentCount);
            Assert.IsTrue(throttle.IsThrottling);
            Assert.IsTrue(throttle.WillThrottle);

            start = DateTime.Now;
            throttle.CountOne();
            Logging.WriteInfo((DateTime.Now - start).ToString());
            Assert.IsTrue(DateTime.Now - start > TimeSpan.FromMilliseconds(300));
            Assert.AreEqual(1, throttle.CurrentCount);
            Assert.IsFalse(throttle.IsThrottling);
            Assert.IsFalse(throttle.WillThrottle);
        }

        [TestMethod, Timeout(3000)]
        public async Task General_TestsAsync()
        {
            using Throttle throttle = new(2, TimeSpan.FromMilliseconds(200));

            DateTime start = DateTime.Now;
            await throttle.CountOneAsync();
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(50));
            Assert.IsFalse(throttle.IsThrottling);
            Assert.IsFalse(throttle.WillThrottle);

            start = DateTime.Now;
            await throttle.CountOneAsync();
            Assert.IsTrue(DateTime.Now - start < TimeSpan.FromMilliseconds(50));
            Assert.AreEqual(2, throttle.CurrentCount);
            Assert.IsTrue(throttle.IsThrottling);
            Assert.IsTrue(throttle.WillThrottle);

            start = DateTime.Now;
            await throttle.CountAsync(count: 4);
            Logging.WriteInfo((DateTime.Now - start).ToString());
            Assert.IsTrue(DateTime.Now - start > TimeSpan.FromMilliseconds(100));
            Assert.AreEqual(4, throttle.CurrentCount);
            Assert.IsTrue(throttle.IsThrottling);
            Assert.IsTrue(throttle.WillThrottle);

            start = DateTime.Now;
            await throttle.CountOneAsync();
            Logging.WriteInfo((DateTime.Now - start).ToString());
            Assert.IsTrue(DateTime.Now - start > TimeSpan.FromMilliseconds(300));
            Assert.AreEqual(1, throttle.CurrentCount);
            Assert.IsFalse(throttle.IsThrottling);
            Assert.IsFalse(throttle.WillThrottle);
        }
    }
}
