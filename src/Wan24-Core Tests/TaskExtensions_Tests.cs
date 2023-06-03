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

        [TestMethod]
        public async Task WithCancellation_Tests()
        {
            using CancellationTokenSource cts = new();
            Task task = LongRunningTask().WithCancellation(cts.Token);
            await Task.Delay(50);
            Assert.IsFalse(task.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await task);
        }

        [TestMethod]
        public async Task WithTimeout_Tests()
        {
            Task task = LongRunningTask().WithTimeout(TimeSpan.FromMilliseconds(70));
            await Task.Delay(50);
            Assert.IsFalse(task.IsCompleted);
            await Assert.ThrowsExceptionAsync<TimeoutException>(async () => await task);
        }

        [TestMethod]
        public async Task WithTimeoutAndCancellation_Tests()
        {
            using CancellationTokenSource cts = new();
            Task task = LongRunningTask().WithTimeoutAndCancellation(TimeSpan.FromMilliseconds(70), cts.Token);
            await Task.Delay(50);
            Assert.IsFalse(task.IsCompleted);
            await Assert.ThrowsExceptionAsync<TimeoutException>(async () => await task);
            task = LongRunningTask().WithTimeoutAndCancellation(TimeSpan.FromMilliseconds(70), cts.Token);
            await Task.Delay(50);
            Assert.IsFalse(task.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await task);
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
        }
    }
}
