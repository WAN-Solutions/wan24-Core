namespace wan24.Core
{
    /// <summary>
    /// Stream pool
    /// </summary>
    /// <typeparam name="T">Stream type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="capacity">Capacity</param>
    /// <param name="factory">Stream factory</param>
    public class StreamPool<T>(in int capacity, in Func<T> factory) : DisposableObjectPool<T>(capacity, factory) where T : Stream, IObjectPoolItem, new()
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

        /// <inheritdoc/>
        public override void Return(in T item, in bool reset = false)
        {
            if(IsStreamBase && (item as StreamBase)!.IsClosed)
            {
                Logging.WriteWarning($"Returned stream to {GetType()} was closed");
                item.Dispose();
                return;
            }
            base.Return(item, reset);
        }
    }
}
