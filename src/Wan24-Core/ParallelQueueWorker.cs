namespace wan24.Core
{
    /// <summary>
    /// Parallel queue worker
    /// </summary>
    public class ParallelQueueWorker : QueueWorker, IParallelQueueWorker
    {
        /// <summary>
        /// Busy event (raised when not busy)
        /// </summary>
        protected readonly ManualResetEventSlim Busy = new(initialState: true);
        /// <summary>
        /// Processing event (raised when there's space for more processes)
        /// </summary>
        protected readonly ManualResetEventSlim Processing = new(initialState: true);
        /// <summary>
        /// Processing workers
        /// </summary>
        protected int ProcessCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="threads">Number of threads</param>
        public ParallelQueueWorker(int capacity, int threads) : base(capacity)
        {
            if (threads < 1) throw new ArgumentOutOfRangeException(nameof(threads));
            Threads = threads;
        }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <inheritdoc/>
        public int Threads { get; }

        /// <summary>
        /// Wait until all queued work was done
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>All done?</returns>
        public bool WaitBoring(TimeSpan timeout)
        {
            DateTime started = DateTime.Now;
            while (Queued > 0 || !Busy.IsSet)
            {
                if (!Busy.Wait(timeout)) return Busy.IsSet && Queued == 0;
                DateTime now = DateTime.Now;
                if (now - started > timeout) return Busy.IsSet && Queued == 0;
                timeout -= now - started;
                started = now;
            }
            return Busy.IsSet && Queued == 0;
        }

        /// <summary>
        /// Wait until all queued work was done
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void WaitBoring(CancellationToken cancellationToken = default)
        {
            while (Queued > 0 || !Busy.IsSet) Busy.Wait(cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken).DynamicContext();
            Busy.Wait(cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    Processing.Wait(stoppingToken);
                    Task_Delegate task = await Queue.Reader.ReadAsync(stoppingToken).DynamicContext();
                    lock (SyncObject)
                    {
                        ProcessCount++;
                        if (ProcessCount >= Threads) Processing.Reset();
                        Busy.Reset();
                    }
                    Process(task, stoppingToken);
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
        /// Process a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected async void Process(Task_Delegate task, CancellationToken cancellationToken)
        {
            try
            {
                await task(cancellationToken).DynamicContext();
            }
            finally
            {
                lock (SyncObject)
                {
                    ProcessCount--;
                    if (ProcessCount < Threads) Processing.Set();
                    if (ProcessCount == 0) Busy.Set();
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Busy.Dispose();
            Processing.Dispose();
        }
    }
}
