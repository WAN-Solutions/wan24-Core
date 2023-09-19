using System.Runtime;
using System.Threading.Channels;

namespace wan24.Core
{
    /// <summary>
    /// Queue worker
    /// </summary>
    public class QueueWorker : HostedServiceBase, IQueueWorker
    {
        /// <summary>
        /// Queue
        /// </summary>
        protected readonly Channel<Task_Delegate> Queue;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueWorker(in int capacity) : base()
            => Queue = Channel.CreateBounded<Task_Delegate>(new BoundedChannelOptions(capacity)
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
                yield return new("GUID", GUID, "Unique ID of the service object");
                yield return new("Last exception", LastException?.Message, "Last exception message");
                yield return new("Queued", Queued, "Number of queued items");
            }
        }

        /// <inheritdoc/>
        public async ValueTask EnqueueAsync(Task_Delegate task, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await RunEvent.WaitAsync(cancellationToken).DynamicContext();
            await Queue.Writer.WriteAsync(task, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public bool TryEnqueue(Task_Delegate task)
        {
            EnsureUndisposed();
            return Queue.Writer.TryWrite(task);
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
