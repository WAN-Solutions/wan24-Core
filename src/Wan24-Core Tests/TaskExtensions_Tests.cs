using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class TaskExtensions_Tests : TestBase
    {
        [TestMethod("TaskExtensions_Tests.GetResult_Tests"), Timeout(1000)]
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

        [TestMethod("TaskExtensions_Tests.Context_Tests"), Timeout(1000)]
        public async Task Context_Tests()
        {
            Assert.IsTrue(await ResultTask(true).DynamicContext());
            Assert.IsTrue(await ResultTask(true).FixedContext());
        }

        public static async Task VoidTask()
        {
            await Task.Yield();
        }

        public static async Task<T> ResultTask<T>(T result)
        {
            await Task.Yield();
            return result;
        }

        public static async Task LongRunningTask()
        {
            await Task.Delay(int.MaxValue);
            Logging.WriteError("Long running task did finish");
            throw new InvalidProgramException();
        }
    }
}
