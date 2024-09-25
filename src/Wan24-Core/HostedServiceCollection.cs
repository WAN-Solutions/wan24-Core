using Microsoft.Extensions.Hosting;

namespace wan24.Core
{
    /// <summary>
    /// Hosted service collection
    /// </summary>
    public class HostedServiceCollection : HostedServiceBase
    {
        /// <summary>
        /// Services
        /// </summary>
        protected readonly List<IHostedService> _Services = [];
        /// <summary>
        /// Services synchronization
        /// </summary>
        protected readonly SemaphoreSync ServicesSync = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public HostedServiceCollection() : base() => CanPause = true;

        /// <summary>
        /// Number of services
        /// </summary>
        public int Count => IfUndisposed(() => _Services.Count);

        /// <summary>
        /// Services
        /// </summary>
        public IHostedService[] Services
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = ServicesSync;
                EnsureUndisposed();
                return [.. _Services];
            }
        }

        /// <summary>
        /// Add a service
        /// </summary>
        /// <param name="service">Service (will be disposed, if not removed until disposing!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public void Add(in IHostedService service, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using Cancellations cancellation = new(cancellationToken, CancelToken);
            using SemaphoreSyncContext ssc = ServicesSync.SyncContext(cancellation);
            EnsureUndisposed();
            cancellation.Cancellation.ThrowIfCancellationRequested();
            _Services.Add(service);
            if (IsRunning && service is IServiceWorker sw && !sw.IsRunning) sw.StartAsync(cancellation).Wait(cancellation);
        }

        /// <summary>
        /// Add a service
        /// </summary>
        /// <param name="service">Service (will be disposed, if not removed until disposing!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task AddAsync(IHostedService service, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await ServicesSync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            using Cancellations cancellation = new(cancellationToken, CancelToken);
            cancellation.Cancellation.ThrowIfCancellationRequested();
            _Services.Add(service);
            if (IsRunning && service is IServiceWorker sw && !sw.IsRunning) await sw.StartAsync(cancellation).DynamicContext();
        }

        /// <summary>
        /// Remove a service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If removed</returns>
        public bool Remove(in IHostedService service, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = ServicesSync.SyncContext(cancellationToken);
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return _Services.Remove(service);
        }

        /// <summary>
        /// Remove a service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If removed</returns>
        public async Task<bool> RemoveAsync(IHostedService service, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await ServicesSync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return _Services.Remove(service);
        }

        /// <inheritdoc/>
        protected override async Task AfterStartAsync(CancellationToken cancellationToken)
        {
            await base.AfterStartAsync(cancellationToken).DynamicContext();
            using SemaphoreSyncContext ssc = await ServicesSync.SyncContextAsync(cancellationToken).DynamicContext();
            await Task.WhenAll(Services.Select(w => w.StartAsync(CancellationToken.None))).WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterPauseAsync(CancellationToken cancellationToken)
        {
            await base.AfterPauseAsync(cancellationToken).DynamicContext();
            using SemaphoreSyncContext ssc = await ServicesSync.SyncContextAsync(cancellationToken).DynamicContext();
            await Task.WhenAll(Services.Select(w => w is IServiceWorker sw ? sw.PauseAsync() : w.StopAsync(CancellationToken.None)))
                .WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterResumeAsync(CancellationToken cancellationToken)
        {
            await base.AfterResumeAsync(cancellationToken).DynamicContext();
            using SemaphoreSyncContext ssc = await ServicesSync.SyncContextAsync(cancellationToken).DynamicContext();
            await Task.WhenAll(Services.Select(w => w is IServiceWorker sw ? sw.ResumeAsync() : w.StartAsync(CancellationToken.None)))
                .WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterStopAsync(CancellationToken cancellationToken)
        {
            await base.AfterStopAsync(cancellationToken).DynamicContext();
            using SemaphoreSyncContext ssc = await ServicesSync.SyncContextAsync(cancellationToken).DynamicContext();
            await Task.WhenAll(Services.Select(w => w.StopAsync(CancellationToken.None))).WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync() => await CancelToken;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ServicesSync.Dispose();
            _Services.TryDisposeAll();
            _Services.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            ServicesSync.Dispose();
            await _Services.TryDisposeAllAsync().DynamicContext();
            _Services.Clear();
        }
    }
}
