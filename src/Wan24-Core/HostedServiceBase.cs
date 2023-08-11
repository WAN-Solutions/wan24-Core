using Microsoft.Extensions.Hosting;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a hosted service
    /// </summary>
    public abstract class HostedServiceBase : DisposableBase, IHostedService
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Stop task
        /// </summary>
        protected volatile TaskCompletionSource? StopTask = null;
        /// <summary>
        /// Cancellation token
        /// </summary>
        protected CancellationTokenSource? Cancellation = null;
        /// <summary>
        /// Service task
        /// </summary>
        protected Task? ServiceTask = null;

        /// <summary>
        /// Constructor
        /// </summary>
        protected HostedServiceBase() : base() { }

        /// <summary>
        /// Is running?
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Is stopping?
        /// </summary>
        public bool IsStopping => StopTask != null;

        /// <summary>
        /// Last exception
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncAsync(cancellationToken).DynamicContext();
            if (IsRunning) return;
            IsRunning = true;
            Cancellation = new();
            ServiceTask = ((Func<Task>)RunServiceAsync).StartLongRunningTask(cancellationToken: CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            Task stopTask;
            using (SemaphoreSyncContext ssc = await Sync.SyncAsync(cancellationToken).DynamicContext())
            {
                if (!IsRunning) return;
                if (StopTask == null)
                {
                    StopTask = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    Cancellation!.Cancel();
                    stopTask = StopTask.Task;
                }
                else
                {
                    stopTask = StopTask.Task;
                }
            }
            await stopTask.DynamicContext();
        }

        /// <summary>
        /// Service handler
        /// </summary>
        protected async Task RunServiceAsync()
        {
            try
            {
                await WorkerAsync().DynamicContext();
            }
            catch (OperationCanceledException ex)
            {
                if (!Cancellation!.IsCancellationRequested)
                {
                    LastException = ex;
                    OnException?.Invoke(this, new());
                    throw;
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
                OnException?.Invoke(this, new());
            }
            finally
            {
                using (SemaphoreSyncContext ssc = await Sync.SyncAsync().DynamicContext())
                    StopTask ??= new(TaskCreationOptions.RunContinuationsAsynchronously);
                Cancellation!.Dispose();
                Cancellation = null;
                ServiceTask = null;
                IsRunning = false;
                using SemaphoreSyncContext ssc2 = await Sync.SyncAsync().DynamicContext();
                StopTask.SetResult();
                StopTask = null;
            }
        }

        /// <summary>
        /// Service worker
        /// </summary>
        protected abstract Task WorkerAsync();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            StopAsync().Wait();
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await StopAsync().DynamicContext();
            Sync.Dispose();
        }

        /// <summary>
        /// Delegate for a hosted service event
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="e">Event arguments</param>
        public delegate void HostedService_Delegate(HostedServiceBase service, EventArgs e);

        /// <summary>
        /// Raised on exception
        /// </summary>
        public event HostedService_Delegate? OnException;

        /// <summary>
        /// Cast as running-flag
        /// </summary>
        /// <param name="service">Service</param>
        public static implicit operator bool(HostedServiceBase service) => service.IsRunning;
    }
}
