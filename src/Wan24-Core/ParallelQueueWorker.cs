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
        protected readonly ResetEvent Busy = new(initialState: true);
        /// <summary>
        /// Processing event (raised when there's space for more processes)
        /// </summary>
        protected readonly ResetEvent Processing = new(initialState: true);
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync WorkerSync = new();
        /// <summary>
        /// Processing workers
        /// </summary>
        protected int ProcessCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="threads">Number of threads</param>
        public ParallelQueueWorker(in int capacity, in int threads) : base(capacity)
        {
            if (threads < 1 || capacity < threads) throw new ArgumentOutOfRangeException(nameof(threads));
            Threads = threads;
        }

        /// <inheritdoc/>
        public int Threads { get; }

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status state in base.State) yield return state;
                yield return new("Threads", Threads, "Number of threads");
                yield return new("Active threads", ProcessCount, "Number of currently active threads");
            }
        }

        /// <inheritdoc/>
        public bool WaitBoring(TimeSpan timeout)
        {
            DateTime started = DateTime.Now;
            while (ServiceTask is not null && (Queued != 0 || !Busy.IsSet))
                try
                {
                    Busy.Wait(timeout);
                    if (!TimeSpanHelper.UpdateTimeout(ref started, ref timeout)) break;
                }
                catch (TimeoutException)
                {
                    break;
                }
            return Busy.IsSet && Queued == 0;
        }

        /// <inheritdoc/>
        public async Task<bool> WaitBoringAsync(TimeSpan timeout)
        {
            DateTime started = DateTime.Now;
            while (ServiceTask is not null && (Queued != 0 || !Busy.IsSet))
                try
                {
                    await Busy.WaitAsync(timeout).DynamicContext();
                    if (!TimeSpanHelper.UpdateTimeout(ref started, ref timeout)) break;
                }
                catch (TimeoutException)
                {
                    break;
                }
            return Busy.IsSet && Queued == 0;
        }

        /// <inheritdoc/>
        public bool WaitBoring(CancellationToken cancellationToken = default)
        {
            while (ServiceTask is not null && (Queued != 0 || !Busy.IsSet))
                try
                {
                    Busy.Wait(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            return Busy.IsSet && Queued == 0;
        }

        /// <inheritdoc/>
        public async Task<bool> WaitBoringAsync(CancellationToken cancellationToken = default)
        {
            while (ServiceTask is not null && (Queued != 0 || !Busy.IsSet))
                try
                {
                    await Busy.WaitAsync(cancellationToken).DynamicContext();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            return Busy.IsSet && Queued == 0;
        }

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await base.StopAsync(cancellationToken).DynamicContext();
            await Busy.WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            while (!Cancellation!.IsCancellationRequested)
            {
                await Processing.WaitAsync(Cancellation!.Token).DynamicContext();
                Task_Delegate task = await Queue.Reader.ReadAsync(Cancellation!.Token).DynamicContext();
                using (SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(Cancellation!.Token).DynamicContext())
                {
                    ProcessCount++;
                    if (ProcessCount >= Threads) await Processing.ResetAsync().DynamicContext();
                    await Busy.ResetAsync().DynamicContext();
                }
                _ = Process(task, Cancellation!.Token);
            }
        }

        /// <summary>
        /// Process a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected async Task Process(Task_Delegate task, CancellationToken cancellationToken)
        {
            try
            {
                await task(cancellationToken).DynamicContext();
            }
            catch (OperationCanceledException ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    LastException = ex;
                    RaiseOnException();
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
                RaiseOnException();
            }
            finally
            {
                using SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(CancellationToken.None).DynamicContext();
                ProcessCount--;
                if (ProcessCount == Threads - 1) await Processing.SetAsync().DynamicContext();
                if (ProcessCount == 0) await Busy.SetAsync().DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Busy.Dispose();
            Processing.Dispose();
            WorkerSync.Dispose();
        }
    }
}
