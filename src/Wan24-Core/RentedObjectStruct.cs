namespace wan24.Core
{
    /// <summary>
    /// Rented object (returns the rented object to the pool when diposing)
    /// </summary>
    /// <typeparam name="T">Rented object type</typeparam>
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
        public RentedObjectStruct(IObjectPool<T> pool)
        {
            Pool = pool;
            _Object = pool.Rent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="obj">Rented object</param>
        public RentedObjectStruct(IObjectPool<T> pool, T obj)
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
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
        private readonly tValue IfUndisposed<tValue>(tValue value)
        {
            EnsureUndisposed();
            return value;
        }

        /// <summary>
        /// Create an instance asynchronous
        /// </summary>
        /// <param name="pool">Object pool</param>
        /// <returns>Rented object</returns>
        public static async Task<RentedObjectStruct<T>> CreateAsync(IAsyncObjectPool<T> pool) => new RentedObjectStruct<T>(pool, await pool.RentAsync().DynamicContext());
    }
}
