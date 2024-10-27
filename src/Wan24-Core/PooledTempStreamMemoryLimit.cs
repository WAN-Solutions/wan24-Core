using System.ComponentModel;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="PooledTempStream"/> memory limit
    /// </summary>
    public sealed class PooledTempStreamMemoryLimit : BasicDisposableBase, IStatusProvider, IExportUserActions
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        private readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Exported user actions
        /// </summary>
        private readonly UserActionInfo[] UserActions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limit">Memory limit in bytes (zero for no limit)</param>
        public PooledTempStreamMemoryLimit(in long limit = 0) : base()
        {
            ArgumentOutOfRangeException.ThrowIfNegative(limit, nameof(limit));
            MemoryLimit = limit;
            UserActions = [.. this.GetUserActionInfos(GUID)];
            PooledTempStreamMemoryLimitTable.Limits[GUID] = this;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Memory limit in bytes (zero for no limit)
        /// </summary>
        public long MemoryLimit { get; set; }

        /// <summary>
        /// Used memory in bytes
        /// </summary>
        public long MemoryUsed { get; private set; }

        /// <summary>
        /// Number of active memory consuming contexts
        /// </summary>
        public int ContextCount { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<Status> State
        {
            get
            {
                yield return new(UserActionInfo.STATE_KEY, UserActions);
                yield return new(__("GUID"), GUID, __("Global unique ID of this pooled temp stream memory limit instance"));
                yield return new(__("Name"), Name, __("Name of this pooled temp stream memory limit instance"));
                yield return new(__("Limit"), MemoryLimit, __("Current memory limit in bytes (zero disables the limit)"));
                yield return new(__("Usage"), MemoryUsed, __("Currently used memory on bytes"));
                yield return new(__("Contexts"), ContextCount, __("Number of active memory consuming contexts"));
            }
        }

        /// <summary>
        /// Use memory
        /// </summary>
        /// <param name="size">Memory size in bytes</param>
        /// <param name="used">Finally useable memory size in bytes</param>
        /// <returns>Context (dispose to signal that memory has been freed)</returns>
        public Context UseMemory(int size, out int used)
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = Sync)
            {
                if (size > long.MaxValue - MemoryUsed) throw new OutOfMemoryException();
                if (MemoryLimit > 0 && MemoryUsed + size > MemoryLimit)
                {
                    used = size = (int)(MemoryLimit - MemoryUsed);
                }
                else
                {
                    used = size;
                }
                MemoryUsed += size;
                ContextCount++;
            }
            return new(this, size);
        }

        /// <summary>
        /// Use memory
        /// </summary>
        /// <param name="size">Memory size in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Finally useable memory size in bytes and the context (dispose to signal that memory has been freed)</returns>
        public async Task<(int Used, Context Context)> UseMemoryAsync(int size, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                if (size > long.MaxValue - MemoryUsed) throw new OutOfMemoryException();
                if (MemoryLimit > 0 && MemoryUsed + size > MemoryLimit)
                    size = (int)(MemoryLimit - MemoryUsed);
                MemoryUsed += size;
                ContextCount++;
            }
            return (size, new(this, size));
        }

        /// <summary>
        /// Set the <see cref="MemoryLimit"/>
        /// </summary>
        /// <param name="limit">New memory limit in bytes (or zero to disable)</param>
        [UserAction(), DisplayText("Change memory limit"), Description("Set a new memory limit")]
        public void SetMemoryLimit(in long limit)
        {
            EnsureUndisposed();
            MemoryLimit = limit;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            PooledTempStreamMemoryLimitTable.Limits.TryRemove(GUID, out _);
            Sync.Dispose();
        }

        /// <summary>
        /// Context
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="limit">Limit</param>
        /// <param name="size">Used memory size in bytes</param>
        public sealed class Context(in PooledTempStreamMemoryLimit limit, in int size) : BasicAllDisposableBase()
        {
            /// <summary>
            /// Limit
            /// </summary>
            public PooledTempStreamMemoryLimit Limit { get; } = limit;

            /// <summary>
            /// Used memory in bytes
            /// </summary>
            public int Size { get; private set; } = size;

            /// <summary>
            /// Increase the memory usage
            /// </summary>
            /// <param name="size">Additional used memory in bytes</param>
            /// <returns>If increasing was possible</returns>
            public bool Increase(in int size)
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(size, nameof(size));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(size, int.MaxValue - Size, nameof(size));
                if (size < 1) return true;
                using SemaphoreSyncContext ssc = Limit.Sync;
                if (size > long.MaxValue - Limit.MemoryUsed) throw new OutOfMemoryException();
                if (Limit.MemoryLimit > 0 && Limit.MemoryUsed + size > Limit.MemoryLimit)
                    return false;
                Limit.MemoryUsed += size;
                Size += size;
                return true;
            }

            /// <summary>
            /// Increase the memory usage
            /// </summary>
            /// <param name="size">Additional used memory in bytes</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>If increasing was possible</returns>
            public async Task<bool> IncreaseAsync(int size, CancellationToken cancellationToken = default)
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(size, nameof(size));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(size, int.MaxValue - Size, nameof(size));
                if (size < 1) return true;
                using SemaphoreSyncContext ssc = await Limit.Sync.SyncContextAsync(cancellationToken).DynamicContext();
                if (size > long.MaxValue - Limit.MemoryUsed) throw new OutOfMemoryException();
                if (Limit.MemoryLimit > 0 && Limit.MemoryUsed + size > Limit.MemoryLimit)
                    return false;
                Limit.MemoryUsed += size;
                Size += size;
                return true;
            }

            /// <summary>
            /// Decrease the memory usage
            /// </summary>
            /// <param name="size">Lesser used memory in bytes</param>
            public void Decrease(in int size)
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(size, nameof(size));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(size, Size, nameof(size));
                using SemaphoreSyncContext ssc = Limit.Sync;
                Limit.MemoryUsed -= size;
                Size -= size;
            }

            /// <summary>
            /// Decrease the memory usage
            /// </summary>
            /// <param name="size">Lesser used memory in bytes</param>
            /// <param name="cancellationToken">Cancellation token</param>
            public async Task DecreaseAsync(int size, CancellationToken cancellationToken = default)
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(size, nameof(size));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(size, Size, nameof(size));
                using SemaphoreSyncContext ssc = await Limit.Sync.SyncContextAsync(cancellationToken).DynamicContext();
                Limit.MemoryUsed -= size;
                Size -= size;
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                if (Size > 0 && !Limit.IsDisposing)
                    try
                    {
                        using SemaphoreSyncContext ssc = Limit.Sync;
                        Limit.MemoryUsed -= Size;
                        Limit.ContextCount--;
                    }
                    catch (ObjectDisposedException)
                    {
                    }
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                if (Size > 0 && !Limit.IsDisposing)
                    try
                    {
                        using SemaphoreSyncContext ssc = await Limit.Sync.SyncContextAsync().DynamicContext();
                        Limit.MemoryUsed -= Size;
                        Limit.ContextCount--;
                    }
                    catch (ObjectDisposedException)
                    {
                    }
            }
        }
    }
}
