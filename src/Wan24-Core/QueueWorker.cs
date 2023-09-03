using Microsoft.Extensions.Hosting;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace wan24.Core
{
    /// <summary>
    /// Queue worker
    /// </summary>
    public class QueueWorker : BackgroundService, IQueueWorker
    {
        /// <summary>
        /// Queue
        /// </summary>
        protected readonly Channel<Task_Delegate> Queue;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueWorker(in int capacity) : base()
        {
            ServiceWorkerTable.ServiceWorkers[GUID] = this;
            Queue = Channel.CreateBounded<Task_Delegate>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public virtual string? Name { get; set; }

        /// <inheritdoc/>
        public bool IsRunning { get; protected set; }

        /// <inheritdoc/>
        public DateTime Started { get; protected set; }

        /// <inheritdoc/>
        public int Queued => Queue.Reader.Count;

        /// <summary>
        /// Last exception
        /// </summary>
        public Exception? LastException { get; protected set; }

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
        Task IServiceWorker.StartAsync() => StartAsync(default);

        /// <inheritdoc/>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            IsRunning = true;
            try
            {
                await base.StartAsync(cancellationToken).DynamicContext();
                Started = DateTime.Now;
            }
            catch
            {
                IsRunning = false;
                throw;
            }
        }

        /// <inheritdoc/>
        Task IServiceWorker.StopAsync() => StopAsync(default);

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken).DynamicContext();
            IsRunning = false;
        }

        /// <inheritdoc/>
        async Task IServiceWorker.RestartAsync()
        {
            await StopAsync(default).DynamicContext();
            await StartAsync(default).DynamicContext();
        }

        /// <inheritdoc/>
        public ValueTask EnqueueAsync(Task_Delegate task, CancellationToken cancellationToken = default)
            => Queue.Writer.WriteAsync(task, cancellationToken);

        /// <inheritdoc/>
        public bool TryEnqueue(Task_Delegate task) => Queue.Writer.TryWrite(task);

        /// <inheritdoc/>
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
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    await (await Queue.Reader.ReadAsync(stoppingToken).DynamicContext())(stoppingToken).DynamicContext();
                }
                catch (OperationCanceledException ex)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    LastException = ex;
                    RaiseOnException();
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    RaiseOnException();
                }
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Call CG (willbe called from the parent)
        public override void Dispose()
        {
            ServiceWorkerTable.ServiceWorkers.Remove(GUID, out _);
            base.Dispose();
        }
#pragma warning restore CA1816 // Call CG (willbe called from the parent)

        /// <summary>
        /// Delegate for a task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate ValueTask Task_Delegate(CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a queue worker event
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        public delegate void QueueWorker_Delegate(QueueWorker worker, EventArgs e);

        /// <summary>
        /// Raised on exception
        /// </summary>
        public event QueueWorker_Delegate? OnException;
        /// <summary>
        /// Raise the <see cref="OnException"/> event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RaiseOnException() => OnException?.Invoke(this, new());

        /// <summary>
        /// Cast as queued item count
        /// </summary>
        /// <param name="worker">Worker</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in QueueWorker worker) => worker.Queued;
    }
}
