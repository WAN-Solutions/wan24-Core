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
        /// <param name="serviceProvider">Service provider</param>
        public BlockingInstancePool(in int capacity, in IAsyncServiceProvider? serviceProvider = null) : base(capacity, serviceProvider) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        /// <param name="serviceProvider">Service provider</param>
        public BlockingInstancePool(in int capacity, in IInstancePool<T>.Instance_Delegate factory, in IAsyncServiceProvider? serviceProvider = null)
            : base(capacity, factory, serviceProvider)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        /// <param name="serviceProvider">Service provider</param>
        public BlockingInstancePool(in int capacity, in IInstancePool<T>.InstanceAsync_Delegate factory, in IAsyncServiceProvider? serviceProvider = null)
            : base(capacity, factory, serviceProvider)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="intern">Intern construction</param>
        /// <param name="capacity">Capacity</param>
        /// <param name="serviceProvider">Service provider</param>
        protected BlockingInstancePool(in bool intern, in int capacity, in IAsyncServiceProvider? serviceProvider = null) : base(intern, capacity, serviceProvider) { }

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
