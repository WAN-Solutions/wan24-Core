using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="capacity">Capacity</param>
    public abstract class ItemQueueWorkerBase<T>(in int capacity) : QueueWorker(capacity), IItemQueueWorker<T>
    {
        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default)
            => EnqueueAsync(async (ct) => await ProcessItem(item, ct).DynamicContext(), cancellationToken);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool TryEnqueue(T item) => TryEnqueue(async (ct) => await ProcessItem(item, ct).DynamicContext());

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
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
        [TargetedPatchingOptOut("Tiny method")]
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
