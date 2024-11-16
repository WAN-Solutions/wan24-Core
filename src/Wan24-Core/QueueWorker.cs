using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Threading.Channels;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Queue worker
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="capacity">Capacity</param>
    public class QueueWorker(in int capacity) : HostedServiceBase(), IQueueWorker
    {
        /// <summary>
        /// Queue
        /// </summary>
        protected readonly Channel<Task_Delegate> Queue = Channel.CreateBounded<Task_Delegate>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public int Queued => Queue.Reader.Count;

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
        public async ValueTask EnqueueAsync(Task_Delegate task, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await RunEvent.WaitAsync(cancellationToken).DynamicContext();
            await Queue.Writer.WriteAsync(task, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public bool TryEnqueue(Task_Delegate task, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            RunEvent.Wait(cancellationToken);
            return RunEvent.IsSet && Queue.Writer.TryWrite(task);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public async ValueTask<int> EnqueueRangeAsync(IEnumerable<Task_Delegate> tasks, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            foreach (Task_Delegate task in tasks)
            {
                await EnqueueAsync(task, cancellationToken).DynamicContext();
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
                await EnqueueAsync(task, cancellationToken).DynamicContext();
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        public bool TryAdd(Task_Delegate item) => TryEnqueue(item);

        /// <inheritdoc/>
        public bool TryTake([MaybeNullWhen(false)] out Task_Delegate item)
        {
            EnsureUndisposed();
            return Queue.Reader.TryRead(out item);
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
            while (!Cancellation!.IsCancellationRequested)
                await (await Queue.Reader.ReadAsync(Cancellation!.Token).DynamicContext())(Cancellation!.Token).DynamicContext();
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
        public static implicit operator int(in QueueWorker worker) => worker.Queued;
    }
}
