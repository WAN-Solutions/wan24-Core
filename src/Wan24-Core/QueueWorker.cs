using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace wan24.Core
{
    /// <summary>
    /// Queue worker
    /// </summary>
    public class QueueWorker : BackgroundService
    {
        /// <summary>
        /// Queue
        /// </summary>
        protected readonly TaskQueue Queue;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueWorker(int capacity) : base() => Queue = new(capacity);

        /// <summary>
        /// Last exception
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <summary>
        /// Enqueue
        /// </summary>
        /// <param name="task">Task</param>
        public async ValueTask EnqueueAsync(Func<CancellationToken, ValueTask> task) => await Queue.EnqueueAsync(task).DynamicContext();

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    await (await Queue.DequeueAsync(stoppingToken).DynamicContext())(stoppingToken).DynamicContext();
                }
                catch (OperationCanceledException ex)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    LastException = ex;
                    OnException?.Invoke(this, new());
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    OnException?.Invoke(this, new());
                }
        }

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
        /// Task queue
        /// </summary>
        public class TaskQueue
        {
            /// <summary>
            /// Queue
            /// </summary>
            protected readonly Channel<Func<CancellationToken, ValueTask>> Queue;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="capacity">Capacity</param>
            public TaskQueue(int capacity) => Queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

            /// <summary>
            /// Enqueue
            /// </summary>
            /// <param name="task">Task</param>
            public async ValueTask EnqueueAsync(Func<CancellationToken, ValueTask> task) => await Queue.Writer.WriteAsync(task).DynamicContext();

            /// <summary>
            /// Dequeue
            /// </summary>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Task</returns>
            public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken) => await Queue.Reader.ReadAsync(cancellationToken).DynamicContext();
        }
    }
}
