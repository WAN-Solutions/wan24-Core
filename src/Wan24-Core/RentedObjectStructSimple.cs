using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Rented object (returns the rented object to the pool when diposing; not thread-safe)
    /// </summary>
    /// <typeparam name="T">Rented object type</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public record struct RentedObjectStructSimple<T> : IRentedObject<T> where T : struct
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        public RentedObjectStructSimple(in IObjectPool<T> pool)
        {
            Pool = pool;
            Object = pool.Rent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="obj">Rented object</param>
        public RentedObjectStructSimple(in IObjectPool<T> pool, in T obj)
        {
            Pool = pool;
            Object = obj;
        }

        /// <inheritdoc/>
        public IObjectPool<T> Pool { get; }

        /// <inheritdoc/>
        public T Object { get; }

        /// <inheritdoc/>
        public bool Reset { get; set; } = false;

        /// <inheritdoc/>
        public readonly void Dispose() => Pool.Return(Object, Reset);

        /// <summary>
        /// Cast as rented object
        /// </summary>
        /// <param name="rented">Rented object</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(in RentedObjectStructSimple<T> rented) => rented.Object;

        /// <summary>
        /// Create an instance asynchronous
        /// </summary>
        /// <param name="pool">Object pool</param>
        /// <returns>Rented object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<RentedObjectStructSimple<T>> CreateAsync(IAsyncObjectPool<T> pool) => new(pool, await pool.RentAsync().DynamicContext());
    }
}
