using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Disposable object pool (disposes trashed items)
    /// </summary>
    /// <typeparam name="T">Disposable item type</typeparam>
    public class DisposableObjectPool<T> : DisposableBase, IObjectPool<T> where T : IDisposable
    {
        /// <summary>
        /// Pool
        /// </summary>
        protected readonly ConcurrentBag<T> Pool = new();
        /// <summary>
        /// Factory
        /// </summary>
        protected readonly Func<T> Factory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity (may overflow a bit)</param>
        /// <param name="factory">Item factory</param>
        public DisposableObjectPool(int capacity, Func<T> factory) : base()
        {
            Capacity = capacity;
            Factory = factory;
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
            if (!Pool.TryTake(out T? res))
            {
                res = Factory();
            }
            else if (res is IObjectPoolItem item)
            {
                item.Reset();
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual void Return(T item, bool reset = false)
        {
            if (Pool.Count >= Capacity || !EnsureUndisposed(throwException: false))
            {
                item.Dispose();
            }
            else
            {
                if (item is IObjectPoolItem opItem) opItem.Reset();
                Pool.Add(item);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            foreach (T item in Pool) item.Dispose();
            Pool.Clear();
        }
    }
}
