namespace wan24.Core
{
    /// <summary>
    /// Interface for an item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface IItemQueueWorker<T> : IQueueWorker
    {
        /// <summary>
        /// Enqueue an item to process
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Try enqueueing an item to process
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued?</returns>
        bool TryEnqueue(T item);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued items</returns>
        ValueTask<int> EnqueueRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued items</returns>
        ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken = default);
    }
}
