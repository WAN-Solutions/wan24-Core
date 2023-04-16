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
        public async ValueTask EnqueueAsync(T item)
            => await EnqueueAsync(async (ct) => await ProcessItem(item, ct).DynamicContext()).DynamicContext();

        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        public virtual async ValueTask EnqueueRangeAsync(IEnumerable<T> items)
        {
            foreach (T item in items) await EnqueueAsync(item).DynamicContext();
        }

        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        public virtual async ValueTask EnqueueRangeAsync(IAsyncEnumerable<T> items)
        {
            await foreach (T item in items.DynamicContext()) await EnqueueAsync(item).DynamicContext();
        }

        /// <summary>
        /// Process one item
        /// </summary>
        /// <param name="item">Item to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task ProcessItem(T item, CancellationToken cancellationToken);
    }
}
