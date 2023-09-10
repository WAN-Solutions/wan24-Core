using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ProcessingProgress_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            using ProcessingProgress collection = new();
            int countProgress = 0,
                countDone = 0;
            collection.OnProgress += (s, e) => countProgress++;
            collection.OnDone += (s, e) => countDone++;
            using ProcessingProgress progress = new()
            {
                Total = 10
            };
            collection.AddSubProgress(progress);
            for (int i = 0; i < 10; i++) progress.Update();
            await Task.Delay(100);
            Assert.AreEqual(11, countProgress);
            Assert.AreEqual(1, countDone);
            Assert.IsTrue(progress.IsDisposed);
            Assert.IsTrue(collection.IsDone);
        }
    }
}
