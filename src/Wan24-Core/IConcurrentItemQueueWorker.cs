namespace wan24.Core
{
    /// <summary>
    /// Interface for an item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface IConcurrentItemQueueWorker<T> : IConcurrentQueueWorker
    {
        /// <summary>
        /// Enqueue an item to process
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void Enqueue(T item, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued items</returns>
        int EnqueueRange(IEnumerable<T> items, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of enqueued items</returns>
        ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken = default);
    }
}
