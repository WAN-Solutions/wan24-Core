using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class TaskExtensions_Tests
    {
        [TestMethod]
        public async Task GetResult_Tests()
        {
            Task task = ResultTask(true);
            await task;
            Assert.IsTrue(task.GetResult<bool>());
            Assert.IsTrue((bool)task.GetResult(typeof(bool)));
            task = ResultTask((bool?)null);
            await task;
            Assert.IsNull(task.GetResultNullable<bool?>());
            Assert.IsNull((bool?)task.GetResultNullable(typeof(bool?)));
            {
                Task[] tasks = new Task[] { VoidTask(), VoidTask(), VoidTask() };
                await tasks.WaitAll();
            }
            {
                Task<bool>[] tasks = new Task<bool>[] { ResultTask(true), ResultTask(true), ResultTask(true) };
                foreach (bool result in await tasks.WaitAll()) Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task Context_Tests()
        {
            Assert.IsTrue(await ResultTask(true).DynamicContext());
            Assert.IsTrue(await ResultTask(true).FixedContext());
        }

        public async Task VoidTask()
        {
            await Task.Yield();
        }

        public async Task<T> ResultTask<T>(T result)
        {
            await Task.Yield();
            return result;
        }
    }
}
