using Microsoft.Extensions.Hosting;
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
        public QueueWorker(int capacity) : base() => Queue = Channel.CreateBounded<Task_Delegate>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        /// <inheritdoc/>
        public int Queued => Queue.Reader.Count;

        /// <summary>
        /// Last exception
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <inheritdoc/>
        public async ValueTask EnqueueAsync(Task_Delegate task, CancellationToken cancellationToken = default) => await Queue.Writer.WriteAsync(task, cancellationToken).DynamicContext();

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
        protected void RaiseOnException() => OnException?.Invoke(this, new());
    }
}
