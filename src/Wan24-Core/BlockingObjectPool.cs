using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Disposable blocking object pool (disposes items when disposing, if <see cref="IDisposable"/>/<see cref="IAsyncDisposable"/>)
    /// </summary>
    /// <typeparam name="T">Item type (<see cref="IDisposable"/>/<see cref="IAsyncDisposable"/> will be disposed when disposing)</typeparam>
    public class BlockingObjectPool<T> : DisposableBase, IAsyncObjectPool<T>
    {
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly BlockingCollection<T> Pool;
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Factory
        /// </summary>
        protected Func<T>? Factory;
        /// <summary>
        /// Factory
        /// </summary>
        protected Func<Task<T>>? AsyncFactory;
        /// <summary>
        /// Number of lazy initialized objects
        /// </summary>
        protected volatile int _Initialized = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Pooled items</param>
        public BlockingObjectPool(params T[] items) : base()
        {
            if (items.Length < 1) throw new ArgumentException("Items required", nameof(items));
            Pool = new(items.Length);
            for (int i = 0; i < items.Length; Pool.Add(items[i]), i++) ;
            Factory = null;
            AsyncFactory = null;
        }

        /// <summary>
        /// COnstructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Factory</param>
        public BlockingObjectPool(int capacity, Func<T> factory) : base()
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            Factory = factory;
            AsyncFactory = null;
            Pool = new(capacity);
        }

        /// <summary>
        /// COnstructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Factory</param>
        public BlockingObjectPool(int capacity, Func<Task<T>> factory) : base()
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            Factory = null;
            AsyncFactory = factory;
            Pool = new(capacity);
        }

        /// <summary>
        /// Display name
        /// </summary>
        public virtual string? Name { get; set; }

        /// <inheritdoc/>
        Type IPool.ItemType => typeof(T);

        /// <summary>
        /// Capacity
        /// </summary>
        public int Capacity => Pool.BoundedCapacity;

        /// <summary>
        /// Number of items in the pool
        /// </summary>
        public int Available => Pool.Count;

        /// <summary>
        /// Number of initialized items
        /// </summary>
        public int Initialized => _Initialized;

        /// <summary>
        /// Reset rented objects (if they don't come from the factory)
        /// </summary>
        public bool ResetOnRent { get; set; } = true;

        /// <summary>
        /// Force resetting returned items (if they're going back to the pool)? (trashed items will be reset anyway)
        /// </summary>
        public bool ForceResetOnReturn { get; set; }

        /// <inheritdoc/>
        public virtual T Rent()
        {
            EnsureUndisposed();
            if (AsyncFactory != null) throw new InvalidOperationException("No synchronous object factory");
            T? res;
            bool synced = false;
            if (Factory != null)
            {
                Sync.Sync();
                synced = true;
            }
            try
            {
                if (Factory == null || _Initialized >= Capacity)
                {
                    res = Pool.Take();
                    if (ResetOnRent && res is IObjectPoolItem item) item.Reset();
                }
                else if (Pool.TryTake(out res))
                {
                    if (ResetOnRent && res is IObjectPoolItem item) item.Reset();
                }
                else
                {
                    res = Factory();
                    if (++_Initialized >= Capacity) Factory = null;
                }
            }
            finally
            {
                if (synced) Sync.Release();
            }
            return res;
        }

        /// <summary>
        /// Try renting an item (non-blocking)
        /// </summary>
        /// <param name="item">Item (<see cref="IObjectPoolItem"/> will be reset before returning)</param>
        /// <returns>Succeed?</returns>
        public virtual bool TryRent([MaybeNullWhen(returnValue: false)] out T? item)
        {
            item = default;
            EnsureUndisposed();
            if (AsyncFactory != null) throw new InvalidOperationException("No synchronous object factory");
            bool synced = false;
            if (Factory != null)
            {
                Sync.Sync();
                synced = true;
            }
            try
            {
                if (Factory == null || _Initialized >= Capacity)
                {
                    if (!Pool.TryTake(out item)) return false;
                    if (ResetOnRent && item is IObjectPoolItem poolItem) poolItem.Reset();
                }
                else if (Pool.TryTake(out item))
                {
                    if (ResetOnRent && item is IObjectPoolItem poolItem) poolItem.Reset();
                }
                else
                {
                    item = Factory();
                    if (++_Initialized >= Capacity) Factory = null;
                }
            }
            finally
            {
                if (synced) Sync.Release();
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<T> RentAsync()
        {
            EnsureUndisposed();
            T? res;
            bool synced = false;
            if (AsyncFactory != null || Factory != null)
            {
                await Sync.SyncAsync().DynamicContext(); ;
                synced = true;
            }
            try
            {
                if ((AsyncFactory == null && Factory == null) || _Initialized >= Capacity)
                {
                    res = Pool.Take();
                    if (ResetOnRent && res is IObjectPoolItem item) item.Reset();
                }
                else if (Pool.TryTake(out res))
                {
                    if (ResetOnRent && res is IObjectPoolItem item) item.Reset();
                }
                else
                {
                    res = AsyncFactory == null ? Factory!() : await AsyncFactory().DynamicContext();
                    if (++_Initialized >= Capacity)
                    {
                        Factory = null;
                        AsyncFactory = null;
                    }
                }
            }
            finally
            {
                if (synced) Sync.Release();
            }
            return res;
        }

        /// <summary>
        /// Try renting an item
        /// </summary>
        /// <returns>If succeed, and the rented item</returns>
        public virtual async Task<(bool Succeed, T? Item)> TryRentAsync()
        {
            EnsureUndisposed();
            T? res;
            bool synced = false;
            if (AsyncFactory != null || Factory != null)
            {
                await Sync.SyncAsync().DynamicContext(); ;
                synced = true;
            }
            try
            {
                if ((AsyncFactory == null && Factory == null) || _Initialized >= Capacity)
                {
                    if (!Pool.TryTake(out res)) return (Succeed: false, Item: default);
                    if (ResetOnRent && res is IObjectPoolItem item) item.Reset();
                }
                else if (Pool.TryTake(out res))
                {
                    if (ResetOnRent && res is IObjectPoolItem item) item.Reset();
                }
                else
                {
                    res = AsyncFactory == null ? Factory!() : await AsyncFactory().DynamicContext();
                    if (++_Initialized >= Capacity)
                    {
                        Factory = null;
                        AsyncFactory = null;
                    }
                }
            }
            finally
            {
                if (synced) Sync.Release();
            }
            return (Succeed: true, Item: res);
        }

        /// <inheritdoc/>
        public virtual void Return(T item, bool reset = false)
        {
            if (!EnsureUndisposed(throwException: false))
            {
                if (item is IObjectPoolItem opItem) opItem.Reset();
                if (item is IDisposable disposable) disposable.Dispose();
                return;
            }
            else if ((reset || ForceResetOnReturn) && item is IObjectPoolItem opItem)
            {
                opItem.Reset();
            }
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            if (Pool.Count >= Capacity) throw new OverflowException();
            Pool.Add(item);
            if ((Factory != null || AsyncFactory != null) && _Initialized < Capacity && Pool.Count > _Initialized)
            {
                _Initialized = Pool.Count;
                if (_Initialized >= Capacity)
                {
                    Factory = null;
                    AsyncFactory = null;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task ReturnAsync(T item, bool reset = false)
        {
            if (!EnsureUndisposed(throwException: false))
            {
                if (item is IObjectPoolItem opItem) opItem.Reset();
                if (item is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().DynamicContext();
                }
                else if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return;
            }
            else if ((reset || ForceResetOnReturn) && item is IObjectPoolItem opItem)
            {
                opItem.Reset();
            }
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext();
            if (Pool.Count >= Capacity) throw new OverflowException();
            Pool.Add(item);
            if ((AsyncFactory != null || Factory != null) && _Initialized < Capacity && Pool.Count > _Initialized)
            {
                _Initialized = Pool.Count;
                if (_Initialized >= Capacity)
                {
                    Factory = null;
                    AsyncFactory = null;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                while (Pool.TryTake(out T? item))
                {
                    if (item is IObjectPoolItem opItem) opItem.Reset();
                    ((IDisposable)item!).Dispose();
                }
            Pool.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Task.Yield();
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                while (Pool.TryTake(out T? item))
                {
                    if (item is IObjectPoolItem opItem) opItem.Reset();
                    if (item is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().DynamicContext();
                    }
                    else
                    {
                        ((IDisposable)item!).Dispose();
                    }
                }
            Pool.Dispose();
        }

        /// <summary>
        /// Cast as rented object
        /// </summary>
        /// <param name="pool">Pool</param>
        public static implicit operator T(BlockingObjectPool<T> pool) => pool.Rent();

        /// <summary>
        /// Cast as available item count
        /// </summary>
        /// <param name="pool">Pool</param>
        public static implicit operator int(BlockingObjectPool<T> pool) => pool.Available;

        /// <summary>
        /// Cast as available-flag
        /// </summary>
        /// <param name="pool">Pool</param>
        public static implicit operator bool(BlockingObjectPool<T> pool) => pool.Available != 0;
    }
}
