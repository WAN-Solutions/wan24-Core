using System.Runtime;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Priority queue worker
    /// </summary>
    /// <typeparam name="T">Priority type</typeparam>
    public class PriorityQueueWorker<T> : HostedServiceBase, IPriorityQueueWorker<T>
    {
        /// <summary>
        /// Queue
        /// </summary>
        protected readonly PriorityQueue<Task_Delegate, T> Queue;
        /// <summary>
        /// Queue thread synchronization
        /// </summary>
        protected readonly SemaphoreSync QueueSync = new();
        /// <summary>
        /// Queue event (raised when having queued items)
        /// </summary>
        protected readonly ResetEvent QueueEvent = new();
        /// <summary>
        /// Space event (raised when the queue has space)
        /// </summary>
        protected readonly ResetEvent SpaceEvent = new(initialState: true);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="comparer">Comparer</param>
        public PriorityQueueWorker(in int capacity, in IComparer<T>? comparer = null) : base()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
            Queue = new(capacity, comparer);
            Capacity = capacity;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Capacity
        /// </summary>
        public int Capacity { get; }

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
        public async ValueTask EnqueueAsync(Task_Delegate task, T priority, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await RunEvent.WaitAsync(cancellationToken).DynamicContext();
            while (true)
            {
                await SpaceEvent.WaitAsync(cancellationToken).DynamicContext();
                using SemaphoreSyncContext ssc = await QueueSync.SyncContextAsync(cancellationToken).DynamicContext();
                if (!SpaceEvent.IsSet)
                    continue;
                Queue.Enqueue(task, priority);
                if (Queue.Count >= Capacity)
                    await SpaceEvent.ResetAsync(CancellationToken.None).DynamicContext();
                await QueueEvent.SetAsync(CancellationToken.None).DynamicContext();
                return;
            }
        }

        /// <inheritdoc/>
        public bool TryEnqueue(Task_Delegate task, T priority, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            RunEvent.Wait(cancellationToken);
            using SemaphoreSyncContext ssc = QueueSync.SyncContext(cancellationToken);
            if (!SpaceEvent.IsSet || !RunEvent.IsSet)
                return false;
            Queue.Enqueue(task, priority);
            if (Queue.Count >= Capacity)
                SpaceEvent.Reset();
            QueueEvent.Set();
            return true;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public async ValueTask<int> EnqueueRangeAsync(IEnumerable<Task_Delegate> tasks, T priority, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            foreach (Task_Delegate task in tasks)
            {
                await EnqueueAsync(task, priority, cancellationToken).DynamicContext();
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public async ValueTask<int> EnqueueRangeAsync(IAsyncEnumerable<Task_Delegate> tasks, T priority, CancellationToken cancellationToken = default)
        {
            int enqueued = 0;
            await foreach (Task_Delegate task in tasks.DynamicContext().WithCancellation(cancellationToken))
            {
                await EnqueueAsync(task, priority, cancellationToken).DynamicContext();
                enqueued++;
            }
            return enqueued;
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            Task_Delegate? task;
            while (!Cancellation!.IsCancellationRequested)
            {
                await QueueEvent.WaitAsync(CancelToken).DynamicContext();
                using(SemaphoreSyncContext ssc = await QueueSync.SyncContextAsync(CancelToken).DynamicContext())
                {
                    if (Queue.Count == 0)
                        continue;
                    task = Queue.Dequeue();
                    if (Queue.Count < Capacity)
                        await SpaceEvent.SetAsync(CancellationToken.None).DynamicContext();
                    if(Queue.Count==0)
                        await QueueEvent.ResetAsync(CancellationToken.None).DynamicContext();
                }
                await task(CancelToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            QueueSync.Dispose();
            QueueEvent.Dispose();
            SpaceEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await QueueSync.DisposeAsync().DynamicContext();
            await QueueEvent.DisposeAsync().DynamicContext();
            await SpaceEvent.DisposeAsync().DynamicContext();
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
        public static implicit operator int(in PriorityQueueWorker<T> worker) => worker.Queued;
    }
}
