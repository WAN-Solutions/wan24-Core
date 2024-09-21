namespace wan24.Core
{
    /// <summary>
    /// Base class for a distributed read-write-lock
    /// </summary>
    public abstract partial class DistributedReadWriteLockBase : HostedServiceBase
    {
        /// <summary>
        /// Active global lock contexts
        /// </summary>
        protected readonly ConcurrentHashSet<ContextBase> GlobalLockContexts = [];
        /// <summary>
        /// Local locks (key is the global key)
        /// </summary>
        protected readonly InMemoryCache<AutoDisposer<ReadWriteLock>> LocalLocks;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Local locks cache options</param>
        protected DistributedReadWriteLockBase(in InMemoryCacheOptions options) : base()
        {
            CanPause = true;
            LocalLocks = new(options);
        }

        /// <summary>
        /// Create a lock for reading
        /// </summary>
        /// <param name="key">Global key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        public async Task<ContextBase> ReadAsync(string key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await RunEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            AutoDisposer<ReadWriteLock> localLocks = await LocalLocks.GetAsync(
                key,
                (cache, key, options, ct) => Task.FromResult(
                    (ICacheEntry<AutoDisposer<ReadWriteLock>>?)new InMemoryCacheEntry<AutoDisposer<ReadWriteLock>>(
                        key,
                        new AutoDisposer<ReadWriteLock>(CreateLocalLock(key))
                        )
                    {
                        Cache = LocalLocks
                    }
                    ),
                cancellationToken: cancellationToken
                ).DynamicContext()
                ?? throw new InvalidProgramException();
            AutoDisposer<ReadWriteLock>.Context localUsage = await localLocks.UseObjectAsync("Distributed read-lock").DynamicContext();
            ReadWriteLock.Context? localContext = null;
            try
            {
                localContext = await localUsage.Object.ReadAsync(cancellationToken).DynamicContext();
                ContextBase res = await CreateGlobalReadLockAsync(key, localUsage, localContext, cancellationToken).DynamicContext();
                GlobalLockContexts.Add(res);
                return res;
            }
            catch
            {
                if (localContext is not null) await localContext.DisposeAsync().DynamicContext();
                await localUsage.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <summary>
        /// Create an exclusive lock for writing
        /// </summary>
        /// <param name="key">Global key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        public async Task<ContextBase> WriteAsync(string key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await RunEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            AutoDisposer<ReadWriteLock> localLocks = await LocalLocks.GetAsync(
                key,
                (cache, key, options, ct) => Task.FromResult(
                    (ICacheEntry<AutoDisposer<ReadWriteLock>>?)new InMemoryCacheEntry<AutoDisposer<ReadWriteLock>>(
                        key,
                        new AutoDisposer<ReadWriteLock>(CreateLocalLock(key))
                        )
                    {
                        Cache = LocalLocks
                    }
                    ),
                cancellationToken: cancellationToken
                ).DynamicContext()
                ?? throw new InvalidProgramException();
            AutoDisposer<ReadWriteLock>.Context localUsage = await localLocks.UseObjectAsync("Distributed write-lock").DynamicContext();
            ReadWriteLock.Context? localContext = null;
            try
            {
                localContext = await localUsage.Object.WriteAsync(cancellationToken).DynamicContext();
                ContextBase res = await CreateGlobalWriteLockAsync(key, localUsage, localContext, cancellationToken).DynamicContext();
                GlobalLockContexts.Add(res);
                return res;
            }
            catch
            {
                if (localContext is not null) await localContext.DisposeAsync().DynamicContext();
                await localUsage.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <summary>
        /// Create a local read-write-lock
        /// </summary>
        /// <param name="key">Global key</param>
        /// <returns>Read-write-lock</returns>
        protected virtual ReadWriteLock CreateLocalLock(string key) => new();

        /// <summary>
        /// Create a global read lock
        /// </summary>
        /// <param name="key">Global key</param>
        /// <param name="localUsage">Local usage context</param>
        /// <param name="localContext">Local read-lock-context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context</returns>
        protected abstract Task<ContextBase> CreateGlobalReadLockAsync(
            string key,
            AutoDisposer<ReadWriteLock>.Context localUsage,
            ReadWriteLock.Context localContext, 
            CancellationToken cancellationToken
            );

        /// <summary>
        /// Create a global write lock
        /// </summary>
        /// <param name="key">Global key</param>
        /// <param name="localUsage">Local usage context</param>
        /// <param name="localContext">Local write-lock-context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context</returns>
        protected abstract Task<ContextBase> CreateGlobalWriteLockAsync(
            string key,
            AutoDisposer<ReadWriteLock>.Context localUsage,
            ReadWriteLock.Context localContext, 
            CancellationToken cancellationToken
            );

        /// <inheritdoc/>
        protected override async Task StartingAsync(CancellationToken cancellationToken) => await LocalLocks.StartAsync(CancellationToken.None).DynamicContext();

        /// <inheritdoc/>
        protected override async Task WorkerAsync() => await CancelToken.WaitHandle.WaitAsync(CancelToken).DynamicContext();

        /// <inheritdoc/>
        protected override async Task StoppingAsync(CancellationToken cancellationToken) => await LocalLocks.StopAsync(CancellationToken.None).DynamicContext();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            GlobalLockContexts.DisposeAll();
            LocalLocks.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await GlobalLockContexts.DisposeAllAsync().DynamicContext();
            await LocalLocks.DisposeAsync().DynamicContext();
        }
    }
}
