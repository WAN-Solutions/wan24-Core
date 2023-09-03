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
        protected ItemQueueWorkerBase(in int capacity) : base(capacity) { }

        /// <inheritdoc/>
        public ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default)
            => EnqueueAsync(async (ct) => await ProcessItem(item, ct).DynamicContext(), cancellationToken);

        /// <inheritdoc/>
        public bool TryEnqueue(T item) => TryEnqueue(async (ct) => await ProcessItem(item, ct).DynamicContext());

        /// <inheritdoc/>
        public async ValueTask<int> EnqueueRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            foreach (T item in items)
            {
                await EnqueueAsync(item, cancellationToken).DynamicContext();
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        public async ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            await foreach (T item in items.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
            {
                await EnqueueAsync(item, cancellationToken).DynamicContext();
                enqueued++;
            }
            return enqueued;
        }

        /// <summary>
        /// Process one item
        /// </summary>
        /// <param name="item">Item to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task ProcessItem(T item, CancellationToken cancellationToken);
    }
}
