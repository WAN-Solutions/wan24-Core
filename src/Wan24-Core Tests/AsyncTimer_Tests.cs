using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class AsyncTimer_Tests
    {
        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            int runCount = 0;
            using AsyncTimer timer = new(TimeSpan.FromMilliseconds(50), (timer) =>
            {
                runCount++;
                return Task.CompletedTask;
            });
            await timer.StartAsync();
            await Task.Delay(120);
            Assert.AreEqual(2, runCount);
            Assert.IsTrue(timer.IsRunning);
            timer.AutoReset = false;
            await Task.Delay(100);
            Assert.AreEqual(3, runCount);
            Assert.IsFalse(timer.IsRunning);
        }
    }
}
