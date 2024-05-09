namespace wan24.Core
{
    /// <summary>
    /// Interface for a priority item queue worker
    /// </summary>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <typeparam name="tPriority">Priority type</typeparam>
    public interface IPriorityItemQueueWorker<tItem, tPriority> : IPriorityQueueWorker<tPriority>
    {
        /// <summary>
        /// Enqueue an item to process
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="priority">Priority</param>
        /// <param name="cancellationToken">Cancellation token</param>
        ValueTask EnqueueAsync(tItem item, tPriority priority, CancellationToken cancellationToken = default);
        /// <summary>
        /// Try enqueueing an item to process
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="priority">Priority</param>
        /// <returns>Enqueued?</returns>
        bool TryEnqueue(tItem item, tPriority priority);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="priority">Priority</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued items</returns>
        ValueTask<int> EnqueueRangeAsync(IEnumerable<tItem> items, tPriority priority, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="priority">Priority</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued items</returns>
        ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<tItem> items, tPriority priority, CancellationToken cancellationToken = default);
    }
}
