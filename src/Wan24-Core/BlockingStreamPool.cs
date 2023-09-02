namespace wan24.Core
{
    /// <summary>
    /// Blocking stream pool
    /// </summary>
    /// <typeparam name="T">Stream type</typeparam>
    public class BlockingStreamPool<T> : BlockingObjectPool<T> where T : Stream, IObjectPoolItem, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Available instances</param>
        public BlockingStreamPool(params T[] items) : base(items) => ResetOnRent = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public BlockingStreamPool(in int capacity) : this(capacity, () => new()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Factory</param>
        public BlockingStreamPool(in int capacity, in Func<T> factory) : base(capacity, factory) => ResetOnRent = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Factory</param>
        public BlockingStreamPool(in int capacity, in Func<Task<T>> factory) : base(capacity, factory) => ResetOnRent = false;
    }
}
