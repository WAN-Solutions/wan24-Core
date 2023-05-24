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
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        ValueTask EnqueueRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        ValueTask EnqueueRangeAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken = default);
    }
}
