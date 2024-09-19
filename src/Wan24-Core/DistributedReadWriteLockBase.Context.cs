namespace wan24.Core
{
    // Context
    public abstract partial class DistributedReadWriteLockBase
    {
        /// <summary>
        /// Base class for a distributed global lock context
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="lockManager">Lock manager</param>
        /// <param name="key">Global key</param>
        /// <param name="localUsage">Local usage context (will be disposed)</param>
        /// <param name="localContext">Local lock context (will be disposed)</param>
        /// <param name="guid">GUID</param>
        public abstract class ContextBase(
            in DistributedReadWriteLockBase lockManager,
            in string key,
            in AutoDisposer<ReadWriteLock>.Context localUsage,
            in ReadWriteLock.Context localContext,
            in UidExt guid
            )
            : DisposableBase()
        {
            /// <summary>
            /// Local usage context (will be disposed)
            /// </summary>
            protected readonly AutoDisposer<ReadWriteLock>.Context LocalUsage = localUsage;
            /// <summary>
            /// Local lock context (will be disposed)
            /// </summary>
            protected readonly ReadWriteLock.Context LocalContext = localContext;

            /// <summary>
            /// If to release the local and the global lock in parallel (if <see langword="true"/>, <see cref="ReleaseAsync"/> should yield a task)
            /// </summary>
            protected abstract bool ReleaseParallel { get; }

            /// <summary>
            /// GUID
            /// </summary>
            public UidExt GUID { get; } = guid;

            /// <summary>
            /// Lock manager
            /// </summary>
            public DistributedReadWriteLockBase LockManager { get; } = lockManager;

            /// <summary>
            /// Global key
            /// </summary>
            public string Key { get; } = key;

            /// <summary>
            /// If read-only
            /// </summary>
            public bool Reading => LocalContext.Reading;

            /// <summary>
            /// Validate the lock
            /// </summary>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>If validated (if <see langword="false"/>, this instance is disposed!)</returns>
            public virtual async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
            {
                if (!EnsureUndisposed(throwException: false)) return false;
                if (await ValidateIntAsync(cancellationToken).DynamicContext()) return true;
                await DisposeAsync().DynamicContext();
                return false;
            }

            /// <summary>
            /// Validate the lock
            /// </summary>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>If validated (if <see langword="false"/>, this instance will be disposed!)</returns>
            protected virtual Task<bool> ValidateIntAsync(CancellationToken cancellationToken) => Task.FromResult(true);

            /// <summary>
            /// Release the global lock (if <see cref="ReleaseParallel"/> is <see langword="true"/>, a task should be yielded)
            /// </summary>
            protected abstract Task ReleaseAsync();

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                LockManager.GlobalLockContexts.Remove(this);
                Task? releaseTask = ReleaseParallel ? ReleaseAsync() : null;
                LocalContext.Dispose();
                LocalUsage.Dispose();
                try
                {
                    releaseTask?.GetAwaiter().GetResult();
                }
                catch(Exception ex)
                {
                    ErrorHandling.Handle(new($"Distributed global read-write-lock context failed to release the global lock", ex, tag: this));
                }
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                LockManager.GlobalLockContexts.Remove(this);
                Task? releaseTask = ReleaseParallel ? ReleaseAsync() : null;
                await LocalContext.DisposeAsync().DynamicContext();
                await LocalUsage.DisposeAsync().DynamicContext();
                if(releaseTask is not null)
                    try
                    {
                        await releaseTask.DynamicContext();
                    }
                    catch(Exception ex)
                    {
                        ErrorHandling.Handle(new($"Distributed global read-write-lock context failed to release the global lock", ex, tag: this));
                    }
            }
        }
    }
}
