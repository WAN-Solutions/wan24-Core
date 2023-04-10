namespace wan24.Core.Caching
{
    /// <summary>
    /// Interface for a cache
    /// </summary>
    public interface ICache : IDisposableObject
    {
        /// <summary>
        /// Number of cached items
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Cached item keys
        /// </summary>
        string[] Keys { get; }
        /// <summary>
        /// Determine if a key exists
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Exists?</returns>
        bool Exists(string key);
        /// <summary>
        /// Get or add an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="factory">Object factory</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="expires">Expires</param>
        /// <param name="timespan">Timespan</param>
        /// <returns>Object</returns>
        T GetOrAdd<T>(string key, Func<ICache, T> factory, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null);
        /// <summary>
        /// Remove an object
        /// </summary>
        /// <param name="key">Key</param>
        ICache Remove(string key);
        /// <summary>
        /// Get and remove an object
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed object</returns>
        object? GetAndRemove(string key);
        /// <summary>
        /// Get and remove an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Removed object</returns>
        T? GetAndRemove<T>(string key);
        /// <summary>
        /// Try getting a cache item
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="item">Cache item</param>
        /// <returns>Succeed?</returns>
        bool TryGetItem(string key, out ICacheItem? item);
        /// <summary>
        /// Try removing an item
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="item">Item</param>
        /// <returns>Removed?</returns>
        bool TryRemoveItem(string key, out ICacheItem? item);
        /// <summary>
        /// Delegate for cache item events
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="e">Event arguments</param>
        public delegate void CacheItem_Delegate(ICache cache, CacheItemEventArgs e);
        /// <summary>
        /// Raised when an item was added
        /// </summary>
        event CacheItem_Delegate? OnAdded;
        /// <summary>
        /// Raised when an item was removed
        /// </summary>
        event CacheItem_Delegate? OnRemoved;
    }
}
