using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Disposable object pool (disposes trashed items)
    /// </summary>
    /// <typeparam name="T">Disposable item type</typeparam>
    public class DisposableObjectPool<T> : DisposableBase where T : IDisposable
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
        /// Rent an item
        /// </summary>
        /// <returns>Item</returns>
        public virtual T Rent() => IfUndisposed(() => Pool.TryTake(out T? res) ? res : Factory());

        /// <summary>
        /// Return an item
        /// </summary>
        /// <param name="item">Item</param>
        public virtual void Return(T item)
        {
            if (Pool.Count >= Capacity || !EnsureUndisposed(throwException: false))
            {
                item.Dispose();
            }
            else
            {
                Pool.Add(item);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            foreach (T item in Pool) item.Dispose();
        }
    }
}
