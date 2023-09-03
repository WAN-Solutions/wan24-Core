using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Rented object (returns the rented object to the pool when diposing)
    /// </summary>
    /// <typeparam name="T">Rented object type</typeparam>
    public sealed class RentedObject<T> : DisposableBase, IRentedObject<T>
    {
        /// <summary>
        /// Rented object
        /// </summary>
        private readonly T _Object;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        public RentedObject(in IObjectPool<T> pool) : base()
        {
            Pool = pool;
            _Object = pool.Rent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="obj">Rented object</param>
        public RentedObject(in IObjectPool<T> pool, in T obj) : base()
        {
            Pool = pool;
            _Object = obj;
        }

        /// <inheritdoc/>
        public IObjectPool<T> Pool { get; }

        /// <inheritdoc/>
        public T Object => IfUndisposed(_Object);

        /// <inheritdoc/>
        public bool Reset { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Pool.Return(_Object, Reset);

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if(Pool is IAsyncObjectPool<T> asyncPool)
            {
                await asyncPool.ReturnAsync(_Object, Reset).DynamicContext();
            }
            else
            {
                Pool.Return(_Object, Reset);
            }
        }

        /// <summary>
        /// Cast as rented object
        /// </summary>
        /// <param name="rented">Rented object</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(RentedObject<T> rented) => rented.Object;

        /// <summary>
        /// Create an instance asynchronous
        /// </summary>
        /// <param name="pool">Object pool</param>
        /// <returns>Rented object</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<RentedObject<T>> CreateAsync(IAsyncObjectPool<T> pool) => new(pool, await pool.RentAsync().DynamicContext());
    }
}
