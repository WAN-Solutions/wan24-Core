namespace wan24.Core
{
    /// <summary>
    /// Stream pool
    /// </summary>
    /// <typeparam name="T">Stream type</typeparam>
    public class StreamPool<T> : DisposableObjectPool<T> where T : Stream, IObjectPoolItem, new()
    {
        /// <summary>
        /// Is a <see cref="StreamBase"/> stream?
        /// </summary>
        protected static readonly bool IsStreamBase;

        /// <summary>
        /// Static constructor
        /// </summary>
        static StreamPool() => IsStreamBase = typeof(StreamBase).IsAssignableFrom(typeof(T));

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

        /// <inheritdoc/>
        public override void Return(in T item, in bool reset = false)
        {
            if(IsStreamBase && (item as StreamBase)!.IsClosed)
            {
                item.Dispose();
                return;
            }
            base.Return(item, reset);
        }
    }
}
