namespace wan24.Core
{
    /// <summary>
    /// Stream pool
    /// </summary>
    /// <typeparam name="T">Stream type</typeparam>
    public class StreamPool<T> : DisposableObjectPool<T> where T : Stream, IObjectPoolItem, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public StreamPool(in int capacity) : this(capacity, () => new()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Stream factory</param>
        public StreamPool(in int capacity, in Func<T> factory) : base(capacity, factory) => ResetOnRent = false;
    }
}
