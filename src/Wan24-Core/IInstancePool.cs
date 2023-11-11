namespace wan24.Core
{
    /// <summary>
    /// Interface for an instance pool
    /// </summary>
    public interface IInstancePool : IServiceWorkerStatus
    {
        /// <summary>
        /// Capacity
        /// </summary>
        int Capacity { get; }
        /// <summary>
        /// Available instances
        /// </summary>
        int Available { get; }
        /// <summary>
        /// Total number of created instances
        /// </summary>
        int Created { get; }
        /// <summary>
        /// Total number of on-demand created instances
        /// </summary>
        int CreatedOnDemand { get; }
        /// <summary>
        /// Get one instance
        /// </summary>
        /// <returns>Instance</returns>
        object GetOneObject();
        /// <summary>
        /// Get one instance
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance</returns>
        Task<object> GetOneObjectAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Get many instances
        /// </summary>
        /// <param name="count">Number of instances</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instances</returns>
        IEnumerable<object> GetManyObjects(int count, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get many instances
        /// </summary>
        /// <param name="count">Number of instances</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instances</returns>
        IAsyncEnumerable<object> GetManyObjectsAsync(int count, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for an instance pool
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public interface IInstancePool<T> : IInstancePool where T : class
    {
        /// <summary>
        /// Get one instance
        /// </summary>
        /// <returns>Instance</returns>
        T GetOne();
        /// <summary>
        /// Get one instance
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance</returns>
        Task<T> GetOneAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Get many instances
        /// </summary>
        /// <param name="count">Number of instances</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instances</returns>
        IEnumerable<T> GetMany(int count, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get many instances
        /// </summary>
        /// <param name="count">Number of instances</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instances</returns>
        IAsyncEnumerable<T> GetManyAsync(int count, CancellationToken cancellationToken = default);
        /// <summary>
        /// Synchronous instance factory delegate
        /// </summary>
        /// <param name="pool">Instance pool</param>
        /// <returns>Instance</returns>
        public delegate T Instance_Delegate(IInstancePool<T> pool);
        /// <summary>
        /// Asynchronous instance factory delegate
        /// </summary>
        /// <param name="pool">Instance pool</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance</returns>
        public delegate Task<T> InstanceAsync_Delegate(IInstancePool<T> pool, CancellationToken cancellationToken);
    }
}
