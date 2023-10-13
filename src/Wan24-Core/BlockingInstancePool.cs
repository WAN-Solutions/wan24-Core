namespace wan24.Core
{
    /// <summary>
    /// Blocking instance pool
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public class BlockingInstancePool<T> : InstancePool<T> where T : class
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public BlockingInstancePool(in int capacity) : base(capacity, async (pool, ct) => (T)(await typeof(T).ConstructAutoAsync(DiHelper.Instance).DynamicContext()).Object) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        public BlockingInstancePool(in int capacity, in IInstancePool<T>.Instance_Delegate factory) : base(capacity, factory) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        public BlockingInstancePool(in int capacity, in IInstancePool<T>.InstanceAsync_Delegate factory) : base(capacity, factory) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="intern">Intern construction</param>
        protected BlockingInstancePool(in int capacity, in bool intern) : base(capacity, intern) { }

        /// <inheritdoc/>
        public sealed override T GetOne() => throw new NotSupportedException();

        /// <inheritdoc/>
        public override async Task<T> GetOneAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return await Instances.Reader.ReadAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override IEnumerable<T> GetMany(int count, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
