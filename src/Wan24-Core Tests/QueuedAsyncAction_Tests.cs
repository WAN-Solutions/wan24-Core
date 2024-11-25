using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class QueuedAsyncAction_Tests
    {
        [TestMethod, Timeout(3000)]
        public async Task Void_TestsAsync()
        {
            using VoidQueue queue = new();
            await queue.StartAsync();

            bool executed = false;
            using QueuedAsyncVoidAction action = new((action, ct) =>
            {
                executed = true;
                return Task.CompletedTask;
            });
            await queue.EnqueueAsync(action);

            await action.Task;
            Assert.IsTrue(executed);
        }

        [TestMethod, Timeout(3000)]
        public async Task Value_TestsAsync()
        {
            using ValueQueue queue = new();
            await queue.StartAsync();

            using QueuedAsyncValueAction<bool> action = new((action, ct) => Task.FromResult(true));
            await queue.EnqueueAsync(action);

            Assert.IsTrue(await action.Task);
        }

        private sealed class VoidQueue() : ItemQueueWorkerBase<QueuedAsyncVoidAction>(capacity: 10)
        {
            protected override async Task ProcessItem(QueuedAsyncVoidAction item, CancellationToken cancellationToken)
                => await item.ExecuteAsync(cancellationToken).DynamicContext();
        }

        private sealed class ValueQueue() : ItemQueueWorkerBase<QueuedAsyncValueAction<bool>>(capacity: 10)
        {
            protected override async Task ProcessItem(QueuedAsyncValueAction<bool> item, CancellationToken cancellationToken)
                => await item.ExecuteAsync(cancellationToken).DynamicContext();
        }
    }
}
