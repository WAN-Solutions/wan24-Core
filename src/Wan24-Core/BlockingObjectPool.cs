using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Disposable blocking object pool (disposes trashed items, if <see cref="IDisposable"/>)
    /// </summary>
    /// <typeparam name="T">Disposable item type</typeparam>
    public class BlockingObjectPool<T> : DisposableBase, IObjectPool<T>
    {
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly BlockingCollection<T> Pool = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Pooled items</param>
        public BlockingObjectPool(T[] items) : base()
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
        public int Pooled => Pool.Count;

        /// <inheritdoc/>
        public virtual T Rent()
        {
            EnsureUndisposed();
            T res = Pool.Take();
            if (res is IObjectPoolItem item) item.Reset();
            return res;
        }

        /// <inheritdoc/>
        public virtual void Return(T item, bool reset = false)
        {
            if (!EnsureUndisposed(throwException: false))
            {
                if (item is IDisposable disposable) disposable.Dispose();
                return;
            }
            if (reset && item is IObjectPoolItem opItem) opItem.Reset();
            if (Pool.Count >= Capacity) throw new OverflowException();
            Pool.Add(item);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                foreach (IDisposable disposable in Pool.Cast<IDisposable>())
                    disposable.Dispose();
            Pool.Dispose();
        }
    }
}
