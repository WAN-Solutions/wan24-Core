using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Disposable blocking object pool (disposes items when disposing, if <see cref="IDisposable"/>)
    /// </summary>
    /// <typeparam name="T">Item type (<see cref="IDisposable"/> will be disposed when disposing)</typeparam>
    public class BlockingObjectPool<T> : DisposableBase, IObjectPool<T>
    {
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly BlockingCollection<T> Pool;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Pooled items</param>
        public BlockingObjectPool(params T[] items) : base()
        {
            if (items.Length < 1) throw new ArgumentException("Items required", nameof(items));
            Pool = new(items.Length);
            for (int i = 0; i < items.Length; Pool.Add(items[i]), i++) ;
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
            T res = Pool.Take();
            if (ResetOnRent && res is IObjectPoolItem item) item.Reset();
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
            if (Pool.Count >= Capacity) throw new OverflowException();
            Pool.Add(item);
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
    }
}
