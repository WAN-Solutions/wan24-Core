using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Object pool
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ObjectPool<T>
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
        /// <param name="capacity">Capacity</param>
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
        /// Rent an item
        /// </summary>
        /// <returns>Item</returns>
        public virtual T Rent() => Pool.TryTake(out T? res) ? res : Factory();

        /// <summary>
        /// Return an item
        /// </summary>
        /// <param name="item">Item</param>
        public virtual void Return(T item)
        {
            if (Pool.Count < Capacity) Pool.Add(item);
        }
    }
}
