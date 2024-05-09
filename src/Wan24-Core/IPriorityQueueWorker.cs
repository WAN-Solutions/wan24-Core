namespace wan24.Core
{
    /// <summary>
    /// Interface for a priority queue worker
    /// </summary>
    /// <typeparam name="T">Priority</typeparam>
    public interface IPriorityQueueWorker<T> : IServiceWorkerStatus
    {
        /// <summary>
        /// Number of queued items
        /// </summary>
        int Queued { get; }
        /// <summary>
        /// Enqueue
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="priority">Priority</param>
        /// <param name="cancellationToken">Cancellation token</param>
        ValueTask EnqueueAsync(PriorityQueueWorker<T>.Task_Delegate task, T priority, CancellationToken cancellationToken = default);
        /// <summary>
        /// Try enqueue
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="priority">Priority</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enqueued?</returns>
        bool TryEnqueue(PriorityQueueWorker<T>.Task_Delegate task, T priority, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many tasks for processing
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <param name="priority">Priority</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued tasks</returns>
        ValueTask<int> EnqueueRangeAsync(IEnumerable<PriorityQueueWorker<T>.Task_Delegate> tasks, T priority, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many tasks for processing
        /// </summary>
        /// <param name="tasks">Tasks</param>
        /// <param name="priority">Priority</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued tasks</returns>
        ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<PriorityQueueWorker<T>.Task_Delegate> tasks, T priority, CancellationToken cancellationToken = default);
    }
}
