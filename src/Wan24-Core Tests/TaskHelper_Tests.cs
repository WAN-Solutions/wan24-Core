using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class TaskHelper_Tests : TestBase
    {
        [TestMethod("TaskExtensions_Tests.GetResult_Tests"), Timeout(1000)]
        public async Task GetResult_Tests()
        {
            Task<bool> task = ResultTask(true);
            await task;
            Assert.IsTrue(TaskHelper.GetResult<bool>(task));
            Assert.IsTrue((bool)TaskHelper.GetResult(task, typeof(bool)));
            Task<bool?> task2 = ResultTask((bool?)null);
            await task2;
            Assert.IsNull(TaskHelper.GetResultNullable<bool?>(task2));
            Assert.IsNull((bool?)TaskHelper.GetResultNullable(task2, typeof(bool?)));
            {
                Task[] tasks = new Task[] { VoidTask(), VoidTask(), VoidTask() };
                await tasks.WaitAll();
            }
            {
                Task<bool>[] tasks = new Task<bool>[] { ResultTask(true), ResultTask(true), ResultTask(true) };
                foreach (bool result in await tasks.WaitAll()) Assert.IsTrue(result);
            }
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
