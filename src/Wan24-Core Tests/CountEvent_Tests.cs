using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CountEvent_Tests
    {
        [TestMethod, Timeout(3000)]
        public async Task General_TestsAsync()
        {
            using CountEvent e = new();

            Task task = e.WaitAsync(1);
            await Task.Delay(200);
            Assert.IsFalse(task.IsCompleted);
            e.Raise();
            await Task.Delay(200);
            Assert.IsTrue(task.IsCompletedSuccessfully);

            task = e.WaitTotalCountAsync(2);
            await Task.Delay(200);
            Assert.IsFalse(task.IsCompleted);
            e.Raise();
            await Task.Delay(200);
            Assert.IsTrue(task.IsCompletedSuccessfully);
        }
    }
}
