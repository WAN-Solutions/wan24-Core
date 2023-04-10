namespace wan24.Core.Caching
{
    /// <summary>
    /// Interface for a cache which supports asynchronous access
    /// </summary>
    public interface IAsyncCache : ICache
    {
        /// <summary>
        /// Get or add an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="factory">Object factory</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="expires">Expires</param>
        /// <param name="timespan">Timespan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        Task<T> GetOrAddAsync<T>(
            string key, 
            Func<IAsyncCache, T> factory, 
            CacheTimeouts timeout, 
            DateTime? expires = null, 
            TimeSpan? timespan = null, 
            CancellationToken cancellationToken = default
            );
        /// <summary>
        /// Get or add an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="factory">Object factory</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="expires">Expires</param>
        /// <param name="timespan">Timespan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        Task<T> GetOrAddAsync<T>(
            string key, 
            Func<IAsyncCache, CancellationToken, Task<T>> factory, 
            CacheTimeouts timeout, 
            DateTime? expires = null, 
            TimeSpan? timespan = null, 
            CancellationToken cancellationToken = default
            );
        /// <summary>
        /// Remove an object
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        /// <summary>
        /// Remove an object
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Removed object</returns>
        Task<object?> GetAndRemoveAsync(string key, CancellationToken cancellationToken = default);
        /// <summary>
        /// Remove an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Removed object</returns>
        Task<T?> GetAndRemoveAsync<T>(string key, CancellationToken cancellationToken = default);
    }
}
