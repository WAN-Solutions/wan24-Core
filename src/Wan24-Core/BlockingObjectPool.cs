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
        /// Factory
        /// </summary>
        protected readonly Func<T> Factory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity (may overflow a bit)</param>
        /// <param name="factory">Item factory</param>
        public BlockingObjectPool(int capacity, Func<T> factory) : base()
        {
            Capacity = capacity;
            Factory = factory;
            Pool = new(capacity);
            for (int i = 0; i < capacity; Pool.Add(factory()), i++) ;
        }

        /// <summary>
        /// Capacity
        /// </summary>
        public int Capacity { get; }

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
