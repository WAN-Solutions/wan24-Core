using System.Threading;

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
        protected readonly SemaphoreSlim Sync = new(1, 1);
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

        /// <inheritdoc/>
        public int Threads { get; }

        /// <inheritdoc/>
        public bool WaitBoring(TimeSpan timeout)
        {
            DateTime started = DateTime.Now;
            while (ExecuteTask != null && (Queued != 0 || !Busy.IsSet))
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
            while (ExecuteTask != null && (Queued != 0 || !Busy.IsSet))
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
            while (ExecuteTask != null && (Queued != 0 || !Busy.IsSet))
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
            while (ExecuteTask != null && (Queued != 0 || !Busy.IsSet))
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
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken).DynamicContext();
            await Busy.WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    await Processing.WaitAsync(stoppingToken).DynamicContext();
                    Task_Delegate task = await Queue.Reader.ReadAsync(stoppingToken).DynamicContext();
                    await Sync.WaitAsync(stoppingToken).DynamicContext();
                    try
                    {
                        ProcessCount++;
                        if (ProcessCount >= Threads) await Processing.ResetAsync().DynamicContext();
                        await Busy.ResetAsync().DynamicContext();
                    }
                    finally
                    {
                        Sync.Release();
                    }
                    _ = Process(task, stoppingToken);
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
                await Sync.WaitAsync(CancellationToken.None).DynamicContext();
                try
                {
                    ProcessCount--;
                    if (ProcessCount < Threads) await Processing.SetAsync().DynamicContext();
                    if (ProcessCount == 0) await Busy.SetAsync().DynamicContext();
                }
                finally
                {
                    Sync.Release();
                }
            }
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Call GC
        public override void Dispose()
        {
            base.Dispose();
            Busy.Dispose();
            Processing.Dispose();
            Sync.Dispose();
        }
#pragma warning restore CA1816 // Call GC
    }
}
