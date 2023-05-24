namespace wan24.Core
{
    /// <summary>
    /// Base class for a parallel item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public abstract class ParallelItemQueueWorker<T> : ParallelQueueWorker, IItemQueueWorker<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="threads">Number of threads</param>
        protected ParallelItemQueueWorker(int capacity, int threads) : base(capacity, threads) { }

        /// <summary>
        /// Enqueue an item to process
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default)
            => EnqueueAsync(async (ct) => await ProcessItem(item, ct).DynamicContext(), cancellationToken);

        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async ValueTask EnqueueRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            foreach (T item in items) await EnqueueAsync(item, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async ValueTask EnqueueRangeAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            await foreach (T item in items.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                await EnqueueAsync(item, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Process one item
        /// </summary>
        /// <param name="item">Item to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task ProcessItem(T item, CancellationToken cancellationToken);
    }
}
