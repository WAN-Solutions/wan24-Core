namespace wan24.Core
{
    /// <summary>
    /// Blocking stream pool
    /// </summary>
    /// <typeparam name="T">Stream type</typeparam>
    public class BlockingStreamPool<T> : BlockingObjectPool<T> where T : Stream, IObjectPoolItem, new()
    {
        /// <summary>
        /// Is a <see cref="StreamBase"/> stream?
        /// </summary>
        protected static readonly bool IsStreamBase;

        /// <summary>
        /// Static constructor
        /// </summary>
        static BlockingStreamPool() => IsStreamBase = typeof(StreamBase).IsAssignableFrom(typeof(T));

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

        /// <inheritdoc/>
        public override void Return(in T item, in bool reset = false)
        {
            if (IsStreamBase && (item as StreamBase)!.IsClosed)
            {
                item.Dispose();
                return;
            }
            base.Return(item, reset);
        }

        /// <inheritdoc/>
        public override async Task ReturnAsync(T item, bool reset = false)
        {
            if (IsStreamBase && (item as StreamBase)!.IsClosed)
            {
                await item.DisposeAsync().DynamicContext();
                return;
            }
            await base.ReturnAsync(item, reset).DynamicContext();
        }
    }
}
