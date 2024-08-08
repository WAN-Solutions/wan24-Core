using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class TaskExtensions_Tests : TestBase
    {
        [TestMethod("TaskExtensions_Tests.Context_Tests"), Timeout(1000)]
        public async Task Context_Tests()
        {
            Assert.IsTrue(await TaskHelper_Tests.ResultTask(true).DynamicContext());
            Assert.IsTrue(await TaskHelper_Tests.ResultTask(true).FixedContext());
        }
    }
}
