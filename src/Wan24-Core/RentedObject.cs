﻿namespace wan24.Core
{
    /// <summary>
    /// Rented object (returns the rented object to the pool when diposing)
    /// </summary>
    /// <typeparam name="T">Rented object type</typeparam>
    public sealed class RentedObject<T> : DisposableBase
    {
        /// <summary>
        /// Rented object
        /// </summary>
        private readonly T _Object;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        public RentedObject(IObjectPool<T> pool) : base()
        {
            Pool = pool;
            _Object = pool.Rent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pool">Pool</param>
        /// <param name="obj">Rented object</param>
        public RentedObject(IObjectPool<T> pool, T obj)
        {
            Pool = pool;
            _Object = obj;
        }

        /// <summary>
        /// Object pool
        /// </summary>
        public IObjectPool<T> Pool { get; }

        /// <summary>
        /// Rented object
        /// </summary>
        public T Object => IfUndisposed(_Object);

        /// <summary>
        /// Reset the <see cref="IObjectPoolItem"/> object when returning?
        /// </summary>
        public bool Reset { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Pool.Return(_Object, Reset);
    }
}
