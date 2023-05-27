using System.Collections.Concurrent;

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
        protected readonly SemaphoreSlim Sync = new(1, 1);
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
                Sync.Wait();
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

        /// <inheritdoc/>
        public virtual async Task<T> RentAsync()
        {
            EnsureUndisposed();
            T? res;
            bool synced = false;
            if (AsyncFactory != null || Factory != null)
            {
                await Sync.WaitAsync().DynamicContext(); ;
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
                    res = AsyncFactory == null ? Factory!() : await AsyncFactory.Invoke().DynamicContext();
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
            Sync.Wait();
            try
            {
                if (Pool.Count >= Capacity)
                {
                    Sync.Release();
                    throw new OverflowException();
                }
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
                Sync.Release();
            }
            catch (OverflowException)
            {
                throw;
            }
            catch
            {
                Sync.Release();
                throw;
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
            await Sync.WaitAsync().DynamicContext();
            try
            {
                if (Pool.Count >= Capacity)
                {
                    Sync.Release();
                    throw new OverflowException();
                }
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
                Sync.Release();
            }
            catch (OverflowException)
            {
                throw;
            }
            catch
            {
                Sync.Release();
                throw;
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
    }
}
