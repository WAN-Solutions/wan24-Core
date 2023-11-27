using System.Collections.Concurrent;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Semaphore synchronization
    /// </summary>
    public sealed class SemaphoreSync : DisposableBase
    {
        /// <summary>
        /// Object synchronization instances
        /// </summary>
        internal static readonly ConcurrentDictionary<int, ObjectInfo> Instances = new();

        /// <summary>
        /// Object synchronization information
        /// </summary>
        internal readonly ObjectInfo? Object;

        /// <summary>
        /// Constructor
        /// </summary>
        public SemaphoreSync() : base(asyncDisposing: false)
        {
            Object = null;
            Semaphore = new(1, 1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Synchronized object</param>
        public SemaphoreSync(in object obj) : base(asyncDisposing: false)
        {
            int hashCode = obj.GetType().GetHashCode() ^ obj.GetHashCode();
            ObjectInfo? objInfo;
            while (!Instances.TryGetValue(hashCode, out objInfo) || !objInfo.AddInstance())
            {
                objInfo = new(obj, hashCode);
                if (Instances.TryAdd(hashCode, objInfo)) break;
                objInfo.Dispose();
            }
            Object = objInfo;
            Semaphore = objInfo.Semaphore;
        }

        /// <summary>
        /// Number of managed object synchronizing instances
        /// </summary>
        public static int SynchronizedObjectCount => Instances.Count;

        /// <summary>
        /// Semaphore
        /// </summary>
        public SemaphoreSlim Semaphore { get; }

        /// <summary>
        /// Is synchronized?
        /// </summary>
        public bool IsSynchronized => Semaphore.CurrentCount == 0;

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public SemaphoreSyncContext SyncContext(in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Semaphore.Wait(timeout, cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public SemaphoreSyncContext SyncContext(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Semaphore.Wait(cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<SemaphoreSyncContext> SyncContextAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Semaphore.WaitAsync(timeout, cancellationToken).DynamicContext();
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<SemaphoreSyncContext> SyncContextAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Semaphore.WaitAsync(cancellationToken);
            return new(Semaphore);
        }

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void Sync(TimeSpan timeout, CancellationToken cancellationToken = default) => IfUndisposed(() => Semaphore.Wait(timeout, cancellationToken));

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void Sync(CancellationToken cancellationToken = default) => IfUndisposed(() => Semaphore.Wait(cancellationToken));

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public async Task SyncAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
            => await IfUndisposed(async () => await Semaphore.WaitAsync(timeout, cancellationToken).DynamicContext()).DynamicContext();

        /// <summary>
        /// Synchronize
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public async Task SyncAsync(CancellationToken cancellationToken = default) => await IfUndisposed(async () => await Semaphore.WaitAsync(cancellationToken)).DynamicContext();

        /// <summary>
        /// Release the synchronization lock
        /// </summary>
        [TargetedPatchingOptOut("Just a method adapter")]
        public void Release()
        {
            EnsureUndisposed(allowDisposing: true);
            lock (Semaphore) if (IsSynchronized) Semaphore.Release();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public void Execute(in Action action, in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(timeout, cancellationToken);
            action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public void Execute(in Action action, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(cancellationToken);
            action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public T Execute<T>(in Func<T> action, in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(timeout, cancellationToken);
            return action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public T Execute<T>(in Func<T> action, in CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = SyncContext(cancellationToken);
            return action();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task ExecuteAsync(Func<Task> action, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(timeout, cancellationToken).DynamicContext();
            await action().DynamicContext();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(cancellationToken).DynamicContext();
            await action().DynamicContext();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(timeout, cancellationToken).DynamicContext();
            return await action().DynamicContext();
        }

        /// <summary>
        /// Execute an action while synchronized
        /// </summary>
        /// <typeparam name="T">Action return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Action return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncContextAsync(cancellationToken).DynamicContext();
            return await action().DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (Object is null)
            {
                Semaphore.Dispose();
            }
            else
            {
                Object.RemoveInstance();
            }
        }

        /// <summary>
        /// Cast as synchronization context
        /// </summary>
        /// <param name="sync">Synchronization</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SemaphoreSyncContext(in SemaphoreSync sync) => sync.SyncContext();

        /// <summary>
        /// Get the number of object synchronizing instances
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Number of object synchronizing instances</returns>
        public static int GetSynchronizationInstanceCount(in object obj)
            => Instances.TryGetValue(obj.GetType().GetHashCode() ^ obj.GetHashCode(), out ObjectInfo? objInfo) ? objInfo.InstanceCount : 0;

        /// <summary>
        /// Synchronized object information
        /// </summary>
        internal sealed class ObjectInfo : DisposableBase
        {
            /// <summary>
            /// Synchronization instance count
            /// </summary>
            private volatile int _InstanceCount = 1;
            /// <summary>
            /// Semaphore
            /// </summary>
            private SemaphoreSlim? _Semaphore = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="obj">Object</param>
            /// <param name="hashCode">Hash code</param>
            public ObjectInfo(in object obj, in int hashCode) : base()
            {
                Object = obj;
                HashCode = hashCode;
            }

            /// <summary>
            /// Object
            /// </summary>
            public object Object { get; }

            /// <summary>
            /// Hash code
            /// </summary>
            public int HashCode { get; }

            /// <summary>
            /// Synchronization count
            /// </summary>
            public int InstanceCount => _InstanceCount;

            /// <summary>
            /// Semaphore
            /// </summary>
            public SemaphoreSlim Semaphore => IfUndisposed(() => _Semaphore ??= new(1, 1));

            /// <summary>
            /// Add a synchronization instance
            /// </summary>
            /// <returns>Instance added?</returns>
            public bool AddInstance()
            {
                if (!EnsureUndisposed(throwException: false)) return false;
                try
                {
                    DisposeSyncObject.Wait();
                    try
                    {
                        if (!EnsureUndisposed(throwException: false)) return false;
                        _InstanceCount++;
                        return true;
                    }
                    finally
                    {
                        DisposeSyncObject.Release();
                    }
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Remove a synchronization instance
            /// </summary>
            public void RemoveInstance()
            {
                if (!EnsureUndisposed(throwException: false)) return;
                try
                {
                    DisposeSyncObject.Wait();
                    try
                    {
                        if (!EnsureUndisposed(throwException: false)) return;
                        _InstanceCount--;
                        if (_InstanceCount > 0) return;
                        Instances.TryRemove(HashCode, out _);
                        _ = DisposeAsync().DynamicContext();
                    }
                    finally
                    {
                        DisposeSyncObject.Release();
                    }
                }
                catch
                {
                }
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                _Semaphore?.Dispose();
                if (_InstanceCount != 0)
                    Logging.WriteWarning($"{GetType()} counts {_InstanceCount} synchronization instances for {Object.GetType()} when disposed");
            }

            /// <inheritdoc/>
            protected override Task DisposeCore()
            {
                Dispose(disposing: true);
                return Task.CompletedTask;
            }
        }
    }
}
