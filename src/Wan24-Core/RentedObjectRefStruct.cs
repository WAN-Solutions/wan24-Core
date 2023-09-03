using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Rented object (returns the rented object to the pool when diposing)
    /// </summary>
    /// <typeparam name="T">Rented object type</typeparam>
    [StructLayout(LayoutKind.Auto)]
    public ref struct RentedObjectRefStruct<T> where T : struct
    {
        /// <summary>
        /// Object pool
        /// </summary>
        public readonly IObjectPool<T> Pool;
        /// <summary>
        /// Rented object
        /// </summary>
        public readonly T Object;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        public RentedObjectRefStruct(in IObjectPool<T> pool)
        {
            Pool = pool;
            Object = pool.Rent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="obj">Rented object</param>
        public RentedObjectRefStruct(in IObjectPool<T> pool, in T obj)
        {
            Pool = pool;
            Object = obj;
        }

        /// <summary>
        /// Reset the <see cref="IObjectPoolItem"/> object when returning?
        /// </summary>
        public bool Reset { get; set; } = false;

        /// <summary>
        /// Dispose
        /// </summary>
        public readonly void Dispose() => Pool.Return(Object, Reset);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly bool Equals(object? obj) => Object.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly int GetHashCode() => Object.GetHashCode();

        /// <summary>
        /// Cast as rented object
        /// </summary>
        /// <param name="rented">Rented object</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(RentedObjectRefStruct<T> rented) => rented.Object;
    }
}
