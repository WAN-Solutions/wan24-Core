using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Object pool
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ObjectPool<T> : IObjectPool<T>
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
        public ObjectPool(int capacity, Func<T> factory)
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
            if (Pool.Count < Capacity)
            {
                if (reset && item is IObjectPoolItem opItem) opItem.Reset();
                Pool.Add(item);
            }
            else if (item is IObjectPoolItem opItem)
            {
                opItem.Reset();
            }
        }
    }
}
