using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Rented object (returns the rented object to the pool when disposing)
    /// </summary>
    /// <typeparam name="T">Rented object type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public record struct RentedObjectStruct<T> : IRentedObject<T> where T:struct
    {
        /// <summary>
        /// Rented object
        /// </summary>
        private readonly T _Object;
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Is disposed?
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        public RentedObjectStruct(in IObjectPool<T> pool)
        {
            Pool = pool;
            _Object = pool.Rent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="obj">Rented object</param>
        public RentedObjectStruct(in IObjectPool<T> pool, in T obj)
        {
            Pool = pool;
            _Object = obj;
        }

        /// <inheritdoc/>
        public IObjectPool<T> Pool { get; }

        /// <inheritdoc/>
        public readonly T Object => IfUndisposed(_Object);

        /// <inheritdoc/>
        public bool Reset { get; set; } = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            Pool.Return(_Object, Reset);
        }

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private readonly void EnsureUndisposed()
        {
            lock (SyncObject) if (!IsDisposed) return;
            throw new ObjectDisposedException(ToString());
        }

        /// <summary>
        /// Return a value if not disposing/disposed
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private readonly tValue IfUndisposed<tValue>(tValue value)
        {
            EnsureUndisposed();
            return value;
        }

        /// <summary>
        /// Cast as rented object
        /// </summary>
        /// <param name="rented">Rented object</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(in RentedObjectStruct<T> rented) => rented.Object;

        /// <summary>
        /// Create an instance asynchronous
        /// </summary>
        /// <param name="pool">Object pool</param>
        /// <returns>Rented object</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<RentedObjectStruct<T>> CreateAsync(IAsyncObjectPool<T> pool) => new(pool, await pool.RentAsync().DynamicContext());
    }
}
