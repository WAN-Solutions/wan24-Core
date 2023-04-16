namespace wan24.Core
{
    /// <summary>
    /// Base class for an item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public abstract class ItemQueueWorkerBase<T> : QueueWorker, IItemQueueWorker<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        protected ItemQueueWorkerBase(int capacity) : base(capacity) { }

        /// <inheritdoc/>
        public async ValueTask EnqueueAsync(T item)
            => await EnqueueAsync(async (ct) => await ProcessItem(item, ct).DynamicContext()).DynamicContext();

        /// <inheritdoc/>
        public async ValueTask EnqueueRangeAsync(IEnumerable<T> items)
        {
            foreach (T item in items) await EnqueueAsync(item).DynamicContext();
        }

        /// <inheritdoc/>
        public async ValueTask EnqueueRangeAsync(IAsyncEnumerable<T> items)
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
