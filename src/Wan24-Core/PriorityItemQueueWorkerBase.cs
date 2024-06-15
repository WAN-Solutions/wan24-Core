using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an item queue worker
    /// </summary>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <typeparam name="tPriority">Priority type (may be a <see cref="IQueueItemPriority"/> also, for example)</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="capacity">Capacity</param>
    /// <param name="comparer">Comparer (may be a <see cref="QueueItemPriorityComparer"/>, if <c>tPriority</c> is an <see cref="IQueueItemPriority"/>)</param>
    public abstract class PriorityItemQueueWorkerBase<tItem, tPriority>(in int capacity, in IComparer<tPriority>? comparer = null)
        : PriorityQueueWorker<tPriority>(capacity, comparer), IPriorityItemQueueWorker<tItem, tPriority>
    {
        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public ValueTask EnqueueAsync(tItem item, tPriority priority, CancellationToken cancellationToken = default)
            => EnqueueAsync(async (ct) => await ProcessItem(item, ct).DynamicContext(), priority, cancellationToken);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool TryEnqueue(tItem item, tPriority priority) => TryEnqueue(async (ct) => await ProcessItem(item, ct).DynamicContext(), priority);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public async ValueTask<int> EnqueueRangeAsync(IEnumerable<tItem> items, tPriority priority, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            foreach (tItem item in items)
            {
                await EnqueueAsync(item, priority, cancellationToken).DynamicContext();
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public async ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<tItem> items, tPriority priority, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            await foreach (tItem item in items.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
            {
                await EnqueueAsync(item, priority, cancellationToken).DynamicContext();
                enqueued++;
            }
            return enqueued;
        }

        /// <summary>
        /// Process one item
        /// </summary>
        /// <param name="item">Item to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task ProcessItem(tItem item, CancellationToken cancellationToken);
    }
}
