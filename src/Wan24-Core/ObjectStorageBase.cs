using System.Collections.Concurrent;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an object storage
    /// </summary>
    /// <typeparam name="tKey">Object key type</typeparam>
    /// <typeparam name="tObj">Object type</typeparam>
    public abstract class ObjectStorageBase<tKey, tObj> : HostedServiceBase, IObjectStorage<tKey, tObj>
        where tKey : notnull
        where tObj : class, IStoredObject<tKey>
    {
        /// <summary>
        /// Can dispose objects?
        /// </summary>
        protected static readonly bool CanDispose;

        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync WorkerSync = new();
        /// <summary>
        /// Clean event (raised when need to dispose some objects)
        /// </summary>
        protected readonly ResetEvent CleanEvent = new(initialState: false);
        /// <summary>
        /// Storage
        /// </summary>
        protected readonly ConcurrentDictionary<tKey, StoredObject> Storage = new();
        /// <summary>
        /// Cancel registration
        /// </summary>
        protected CancellationTokenRegistration? CancelRegistration = null;
        /// <summary>
        /// Max. number of stored objects
        /// </summary>
        protected volatile int _StoredPeak = 0;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ObjectStorageBase() => CanDispose = typeof(IDisposable).IsAssignableFrom(typeof(tObj)) || typeof(IAsyncDisposable).IsAssignableFrom(typeof(tObj));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inMemoryLimit">Max. number of objects to hold in memory</param>
        public ObjectStorageBase(in int inMemoryLimit) : base()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(inMemoryLimit, 1);
            InMemoryLimit = inMemoryLimit;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public int InMemoryLimit { get; }

        /// <inheritdoc/>
        public int Stored => Storage.Count;

        /// <inheritdoc/>
        public int StoredPeak => _StoredPeak;

        /// <inheritdoc/>
        public long ObjectReferences => IfUndisposed(() => Storage.Values.Sum(o => (long)o.UsageCount));

        /// <inheritdoc/>
        public IEnumerable<Status> State
        {
            get
            {
                yield return new(__("GUID"), GUID, __("Unique ID of the service object"));
                yield return new(__("Last exception"), LastException?.Message, __("Last exception message"));
                yield return new(__("In-memory limit"), InMemoryLimit, __("Maximum number of objects hold in memory"));
                yield return new(__("Stored"), Stored, __("Number of objects hold in memory"));
                yield return new(__("Peak"), StoredPeak, __("Maximum number of objects hold in memory counted"));
                yield return new(__("References"), ObjectReferences, __("Number of currently used object references"));
            }
        }

        /// <inheritdoc/>
        public virtual StoredObject<tKey, tObj>? GetObject(in tKey key)
        {
            EnsureUndisposed();
            if (Storage.TryGetValue(key, out StoredObject? res)) return new(this, res.Object);
            RunEvent.Wait();
            using SemaphoreSyncContext ssc = WorkerSync.SyncContext();
            if (Storage.TryGetValue(key, out res)) return new(this, res.Object);
            tObj? obj = CreateObject(key);
            if (obj is null) return null;
            Storage[key] = new(obj);
            if (Storage.Count > _StoredPeak) _StoredPeak = Storage.Count;
            if (Storage.Count > InMemoryLimit) CleanEvent.Set();
            return new(this, obj);
        }

        /// <inheritdoc/>
        public virtual async Task<StoredObject<tKey, tObj>?> GetObjectAsync(tKey key, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Storage.TryGetValue(key, out StoredObject? res)) return new(this, res.Object);
            await RunEvent.WaitAsync(cancellationToken).DynamicContext();
            using SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(cancellationToken).DynamicContext();
            if (Storage.TryGetValue(key, out res)) return new(this, res.Object);
            tObj? obj = await CreateObjectAsync(key, cancellationToken).DynamicContext();
            if (obj is null) return null;
            Storage[key] = new(obj);
            if (Storage.Count > _StoredPeak) _StoredPeak = Storage.Count;
            if (Storage.Count > InMemoryLimit) await CleanEvent.SetAsync(CancellationToken.None).DynamicContext();
            return new(this, obj);
        }

        /// <inheritdoc/>
        public virtual void Release(in tObj obj)
        {
            EnsureUndisposed();
            if (!Storage.TryGetValue(obj.ObjectKey, out StoredObject? stored)) return;
            stored.Release();
            if (Storage.Count > InMemoryLimit) CleanEvent.Set();
        }

        /// <inheritdoc/>
        public virtual tObj Remove(in tObj obj)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = WorkerSync.SyncContext();
            Storage.TryRemove(obj.ObjectKey, out StoredObject? so);
            if (Storage.Count > InMemoryLimit) CleanEvent.Set();
            return obj;
        }

        /// <summary>
        /// Create an object from a key (and store it)
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Object</returns>
        protected abstract tObj? CreateObject(in tKey key);

        /// <summary>
        /// Create an object from a key (and store it)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        protected abstract Task<tObj?> CreateObjectAsync(in tKey key, in CancellationToken cancellationToken);

        /// <summary>
        /// Remove a stored object (and dispose it, if possible)
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="sync">Synchronize threads?</param>
        protected virtual async Task RemoveAsync(StoredObject obj, bool sync = true)
        {
            using SemaphoreSyncContext? ssc = sync ? await WorkerSync.SyncContextAsync().DynamicContext() : null;
            Storage.TryRemove(obj.Key, out StoredObject? so);
            if(CanDispose) await obj.TryDisposeAsync().DynamicContext();
        }

        /// <inheritdoc/>
        protected override Task AfterStartAsync(CancellationToken cancellationToken)
        {
            CancelRegistration = Cancellation!.Token.Register(() => CleanEvent.Set());
            return base.AfterStartAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override Task AfterStopAsync(CancellationToken cancellationToken)
        {
            if (CancelRegistration.HasValue)
            {
                CancelRegistration.Value.Dispose();
                CancelRegistration = null;
            }
            return base.AfterStopAsync(cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            for (; !Cancellation!.IsCancellationRequested;)
            {
                await CleanEvent.WaitAndResetAsync(Cancellation!.Token).DynamicContext();
                if (Cancellation!.IsCancellationRequested) return;
                using SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(Cancellation!.Token).DynamicContext();
                if (Storage.Count - InMemoryLimit < 1) continue;
                foreach (StoredObject obj in (from obj in Storage.Values
                                              where obj.UsageCount == 0
                                              orderby obj.Accessed
                                              select obj)
                                      .Take(Math.Max(0, Storage.Count - InMemoryLimit))
                                      .ToArray())
                    await RemoveAsync(obj, sync: false).DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WorkerSync.Dispose();
            CleanEvent.Dispose();
            if (CanDispose) foreach (StoredObject obj in Storage.Values) obj.Object.TryDispose();
            Storage.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            base.DisposeAttributes();
            WorkerSync.Dispose();
            await CleanEvent.DisposeAsync().DynamicContext();
            if (CanDispose) foreach (StoredObject obj in Storage.Values) await obj.Object.TryDisposeAsync().DynamicContext();
            Storage.Clear();
        }

        /// <summary>
        /// Stored object
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="obj">Object</param>
        protected sealed class StoredObject(tObj obj)
        {
            /// <summary>
            /// An object for thread synchronization
            /// </summary>
            private readonly object SyncObject = new();
            /// <summary>
            /// Object
            /// </summary>
            private readonly tObj _Object = obj;
            /// <summary>
            /// Usage counter
            /// </summary>
            private volatile int _UsageCount = 1;

            /// <summary>
            /// Object (when accessing the getter, the <see cref="UsageCount"/> will be increased by one)
            /// </summary>
            public tObj Object
            {
                get
                {
                    lock (SyncObject) Accessed = DateTime.Now;
                    _UsageCount++;
                    return _Object;
                }
            }

            /// <summary>
            /// Object key
            /// </summary>
            public tKey Key { get; } = obj.ObjectKey;

            /// <summary>
            /// Stored time
            /// </summary>
            public DateTime Stored { get; } = DateTime.Now;

            /// <summary>
            /// Last access time
            /// </summary>
            public DateTime Accessed { get; private set; } = DateTime.Now;

            /// <summary>
            /// Usage counter
            /// </summary>
            public int UsageCount => _UsageCount;

            /// <summary>
            /// Release an object usage
            /// </summary>
            /// <exception cref="OverflowException">Negative usage count</exception>
            public void Release()
            {
                if (--_UsageCount < 0)
                {
                    _UsageCount = 0;
                    throw new OverflowException("Negative usage count");
                }
            }
        }
    }
}
