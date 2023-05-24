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
        public ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default)
            => EnqueueAsync(async (ct) => await ProcessItem(item, ct).DynamicContext(), cancellationToken);

        /// <inheritdoc/>
        public async ValueTask EnqueueRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            foreach (T item in items) await EnqueueAsync(item, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public async ValueTask EnqueueRangeAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)
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
