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
                if (Stopping.IsSet)
                {
                    waitStop = true;
                }
                else
                {
                    Stopping.Reset();
                }
            if (waitStop)
            {
                Stopping.Wait();
                return;
            }
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
                await WorkerAsync().DynamicContext();
            }
            catch(Exception ex)
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
            }
        }

        /// <summary>
        /// Service worker
        /// </summary>
        protected abstract Task WorkerAsync();

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
