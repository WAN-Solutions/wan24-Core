using System.Collections.Concurrent;
using System.Runtime;

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
        public ObjectPool(in int capacity, in Func<T> factory)
        {
            PoolTable.Pools[GUID] = this;
            Capacity = capacity;
            Factory = factory;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ObjectPool() => PoolTable.Pools.Remove(GUID, out _);

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public virtual string? Name { get; set; }

        /// <inheritdoc/>
        public Type ItemType => typeof(T);

        /// <summary>
        /// Capacity
        /// </summary>
        public int Capacity { get; }

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
            if (!Pool.TryTake(out T? res))
            {
                res = Factory();
            }
            else if (ResetOnRent && res is IObjectPoolItem item)
            {
                item.Reset();
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual void Return(in T item, in bool reset = false)
        {
            if (Pool.Count < Capacity)
            {
                if ((reset || ForceResetOnReturn) && item is IObjectPoolItem opItem) opItem.Reset();
                Pool.Add(item);
            }
            else if (!reset && !ForceResetOnReturn && item is IObjectPoolItem opItem)
            {
                opItem.Reset();
            }
        }

        /// <summary>
        /// Cast as rented object
        /// </summary>
        /// <param name="pool">Pool</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(in ObjectPool<T> pool) => pool.Rent();
    }
}
