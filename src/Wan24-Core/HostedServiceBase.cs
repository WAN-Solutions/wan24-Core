using Microsoft.Extensions.Hosting;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a hosted service
    /// </summary>
    public abstract class HostedServiceBase : DisposableBase, IHostedService
    {
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
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

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
        public Task StartAsync(CancellationToken cancellationToken)
        {
            lock (SyncObject)
            {
                if (IsRunning) return Task.CompletedTask;
                IsRunning = true;
                Cancellation = new();
                ServiceTask = RunServiceAsync();
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            lock (SyncObject)
            {
                if (!IsRunning) return Task.CompletedTask;
                if (StopTask != null) return StopTask.Task;
                StopTask = new();
            }
            Cancellation!.Cancel();
            return StopTask?.Task ?? Task.CompletedTask;
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
                Cancellation!.Dispose();
                Cancellation = null;
                ServiceTask = null;
                IsRunning = false;
                lock (SyncObject)
                    if (StopTask != null)
                    {
                        StopTask?.SetResult();
                        StopTask = null;
                    }
            }
        }

        /// <summary>
        /// Service worker
        /// </summary>
        protected abstract Task WorkerAsync();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => StopAsync(default).Wait();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await StopAsync(default).DynamicContext();

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
    }
}
