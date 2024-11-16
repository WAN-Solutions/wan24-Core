using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a parallel item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="threads">Number of threads</param>
    public abstract class ParallelConcurrentItemQueueWorkerBase<T>(in int threads) : ParallelConcurrentQueueWorker(threads), IConcurrentItemQueueWorker<T>
    {
        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void Enqueue(T item, CancellationToken cancellationToken = default)
            => Enqueue(async (ct) => await ProcessItem(item, ct).DynamicContext(), cancellationToken);

        /// <inheritdoc/>
        public virtual int EnqueueRange(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            foreach (T item in items)
            {
                Enqueue(item, cancellationToken);
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        public virtual async ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            await foreach (T item in items.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
            {
                Enqueue(item, cancellationToken);
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
