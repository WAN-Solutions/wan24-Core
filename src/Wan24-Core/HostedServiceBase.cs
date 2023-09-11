using Microsoft.Extensions.Hosting;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a hosted service
    /// </summary>
    public abstract class HostedServiceBase : DisposableBase, IServiceWorker, IHostedService
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
        /// Last exception
        /// </summary>
        protected Exception? _LastException = null;

        /// <summary>
        /// Constructor
        /// </summary>
        protected HostedServiceBase() : base() { }

        /// <summary>
        /// Is running?
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Start time
        /// </summary>
        public DateTime Started { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Is stopping?
        /// </summary>
        public bool IsStopping => StopTask is not null;

        /// <summary>
        /// Last exception
        /// </summary>
        public virtual Exception? LastException
        {
            get => _LastException;
            protected set
            {
                _LastException = value;
                if (value is not null) ErrorHandling.Handle(new($"{this} stopped exceptional", value, this));
            }
        }

        /// <summary>
        /// Stopped exceptional?
        /// </summary>
        public bool StoppedExceptional { get; protected set; }

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public virtual async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (IsRunning) return;
            IsRunning = true;
            await BeforeStartAsync(cancellationToken).DynamicContext();
            Cancellation = new();
            ServiceTask = ((Func<Task>)RunServiceAsync).StartLongRunningTask(cancellationToken: CancellationToken.None);
            await AfterStartAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public virtual async Task StopAsync(CancellationToken cancellationToken = default)
        {
            Task stopTask;
            bool isStopping = false;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                if (!IsRunning) return;
                if (StopTask is null)
                {
                    isStopping = true;
                    await BeforeStopAsync(cancellationToken).DynamicContext();
                    StopTask = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    Cancellation!.Cancel();
                }
                stopTask = StopTask.Task;
            }
            await stopTask.WaitAsync(cancellationToken).DynamicContext();
            if (isStopping) await AfterStopAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override string ToString() => $"Service \"{Name ?? "(unnamed)"}\" ({GetType()}, started {Started})";

        /// <inheritdoc/>
        Task IServiceWorker.StartAsync() => StartAsync();

        /// <inheritdoc/>
        Task IServiceWorker.StopAsync() => StopAsync();

        /// <inheritdoc/>
        async Task IServiceWorker.RestartAsync()
        {
            await StopAsync().DynamicContext();
            await StartAsync().DynamicContext();
        }

        /// <summary>
        /// Before starting
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task BeforeStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// After started
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task AfterStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Before stopping
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task BeforeStopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// After stopped
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task AfterStopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Service handler
        /// </summary>
        protected async Task RunServiceAsync()
        {
            StoppedExceptional = false;
            try
            {
                await WorkerAsync().DynamicContext();
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken != Cancellation!.Token)
                {
                    LastException = ex;
                    RaiseOnException();
                }
            }
            catch (Exception ex)
            {
                StoppedExceptional = true;
                LastException = ex;
                RaiseOnException();
            }
            finally
            {
                bool isStopping = false;
                using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext())
                    if (StopTask is null)
                    {
                        isStopping = true;
                        await BeforeStopAsync(CancellationToken.None).DynamicContext();
                        StopTask = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    }
                Cancellation!.Dispose();
                Cancellation = null;
                ServiceTask = null;
                IsRunning = false;
                if (isStopping) await AfterStopAsync(CancellationToken.None).DynamicContext();
                using SemaphoreSyncContext ssc2 = await Sync.SyncContextAsync().DynamicContext();
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
        /// Raise the <see cref="OnException"/> event
        /// </summary>
        protected virtual void RaiseOnException() => OnException?.Invoke(this, new());

        /// <summary>
        /// Cast as running-flag
        /// </summary>
        /// <param name="service">Service</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in HostedServiceBase service) => service.IsRunning;
    }
}
