using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Queue worker
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class ConcurrentQueueWorker() : HostedServiceBase(), IConcurrentQueueWorker
    {
        /// <summary>
        /// Queue
        /// </summary>
        protected readonly ConcurrentQueue<Task_Delegate> Queue = new();
        /// <summary>
        /// Item event (raised when there are queued items)
        /// </summary>
        protected readonly ResetEvent ItemEvent = new();

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public int Queued => Queue.Count;

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("GUID"), GUID, __("Unique ID of the service object"));
                yield return new(__("Last exception"), LastException?.Message ?? LastException?.GetType().ToString(), __("Last exception message"));
                yield return new(__("Queued"), Queued, __("Number of queued items"));
            }
        }

        /// <inheritdoc/>
        public int Count => Queued;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Enqueue(Task_Delegate task, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            RunEvent.Wait(cancellationToken);
            Queue.Enqueue(task);
            ItemEvent.Set(CancellationToken.None);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public int EnqueueRange(IEnumerable<Task_Delegate> tasks, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            foreach (Task_Delegate task in tasks)
            {
                Enqueue(task, cancellationToken);
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public async ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<Task_Delegate> tasks, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            await foreach (Task_Delegate task in tasks.DynamicContext().WithCancellation(cancellationToken))
            {
                Enqueue(task, cancellationToken);
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        public bool TryAdd(Task_Delegate item)
        {
            Enqueue(item);
            return true;
        }

        /// <inheritdoc/>
        public bool TryTake([MaybeNullWhen(false)] out Task_Delegate item)
        {
            EnsureUndisposed();
            return Queue.TryDequeue(out item);
        }

        /// <inheritdoc/>
        public void CopyTo(Task_Delegate[] array, int index) => throw new NotSupportedException();

        /// <inheritdoc/>
        public Task_Delegate[] ToArray() => throw new NotSupportedException();

        /// <inheritdoc/>
        public IEnumerator<Task_Delegate> GetEnumerator() => throw new NotSupportedException();

        /// <inheritdoc/>
        public void CopyTo(Array array, int index) => throw new NotSupportedException();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            while (!CancelToken.IsCancellationRequested)
                if (Queue.TryDequeue(out Task_Delegate? task))
                {
                    await task(CancelToken).DynamicContext();
                }
                else
                {
                    await ItemEvent.WaitAndResetAsync(CancelToken).DynamicContext();
                }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ItemEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await ItemEvent.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for a task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate ValueTask Task_Delegate(CancellationToken cancellationToken);

        /// <summary>
        /// Cast as queued item count
        /// </summary>
        /// <param name="worker">Worker</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in ConcurrentQueueWorker worker) => worker.Queued;
    }
}
