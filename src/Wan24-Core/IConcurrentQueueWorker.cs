using System.Collections.Concurrent;
using static wan24.Core.ConcurrentQueueWorker;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a queue worker
    /// </summary>
    public interface IConcurrentQueueWorker : IServiceWorkerStatus, IProducerConsumerCollection<ConcurrentQueueWorker.Task_Delegate>
    {
        /// <summary>
        /// Number of queued items
        /// </summary>
        int Queued { get; }
        /// <summary>
        /// Enqueue
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void Enqueue(Task_Delegate task, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many tasks for processing
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued tasks</returns>
        int EnqueueRange(IEnumerable<Task_Delegate> tasks, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many tasks for processing
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued tasks</returns>
        ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<Task_Delegate> tasks, CancellationToken cancellationToken = default);
    }
}
