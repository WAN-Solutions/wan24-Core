using Microsoft.Extensions.Hosting;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a hosted service
    /// </summary>
    public abstract class HostedServiceBase : DisposableBase, IHostedService
    {
        /// <summary>
        /// Stopping event (raised when stopped)
        /// </summary>
        protected readonly ManualResetEventSlim Stopping = new(initialState: true);
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
        public bool IsStopping => !Stopping.IsSet;

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
                ServiceTask = ServiceAsync();
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            bool waitStop = false;
            lock (SyncObject)
            {
                if (!IsRunning) return;
                if (!Stopping.IsSet)
                {
                    // Another thread is stopping the service - wait until that tread did stop the service
                    waitStop = true;
                }
                else
                {
                    // We're going to stop the service
                    Stopping.Reset();
                }
            }
            if (waitStop)
            {
                // Another thread is stopping the service - wait until that tread did stop the service
                Stopping.Wait();
                return;
            }
            // Stop the service
            Task serviceTask = ServiceTask!;
            Cancellation!.Cancel();
            await serviceTask!.DynamicContext();
            Stopping.Set();
        }

        /// <summary>
        /// Service handler
        /// </summary>
        protected async Task ServiceAsync()
        {
            try
            {
                // Wait for the worker to finish
                await WorkerAsync().DynamicContext();
            }
            catch(Exception ex)
            {
                // Handle a worker exception
                LastException = ex;
                OnException?.Invoke(this, new());
            }
            finally
            {
                // Stop the service
                Cancellation!.Dispose();
                Cancellation = null;
                ServiceTask = null;
                IsRunning = false;
            }
        }

        /// <summary>
        /// Service worker
        /// </summary>
        protected abstract Task WorkerAsync();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            StopAsync(default).Wait();
            Stopping.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await StopAsync(default).DynamicContext();
            Stopping.Dispose();
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
    }
}
