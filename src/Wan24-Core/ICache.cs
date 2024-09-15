namespace wan24.Core
{
    /// <summary>
    /// Interface for a cache
    /// </summary>
    public interface ICache<T>
    {
        /// <summary>
        /// Options
        /// </summary>
        ICacheOptions Options { get; }
        /// <summary>
        /// Create a cache entry
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="item">Cached item</param>
        /// <param name="options">Options</param>
        /// <returns>Cache entry</returns>
        ICacheEntry<T> CreateEntry(in string key, in T item, ICacheEntryOptions? options = null);
        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned and <c>disposeUnused</c> isn't <see langword="false"/>)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove an existing entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        ICacheEntry<T> Add(
                    in string key,
                    in T item,
                    ICacheEntryOptions? options = null,
                    in bool removeExisting = true,
                    in bool disposeUnused = true
                    );
        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned and <c>disposeUnused</c> isn't <see langword="false"/>)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove an existing entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        Task<ICacheEntry<T>> AddAsync(
                    string key,
                    T item,
                    ICacheEntryOptions? options = null,
                    bool removeExisting = true,
                    bool disposeUnused = true,
                    CancellationToken cancellationToken = default
                    );
        /// <summary>
        /// Get a cache entry
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="entryFactory">Cache entry factory (used to create a new item from the given <c>key</c>, if no existing item was found)</param>
        /// <param name="options">Options</param>
        /// <returns>Cache entry</returns>
        ICacheEntry<T>? Get(
                    in string key,
                    in CacheEntryFactory_Delegate? entryFactory = null,
                    in ICacheEntryOptions? options = null
                    );
        /// <summary>
        /// Get a cache entry
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="entryFactory">Cache entry factory (used to create a new item from the given <c>key</c>, if no existing item was found)</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        Task<ICacheEntry<T>?> GetAsync(
                    string key,
                    CacheEntryFactory_Delegate? entryFactory = null,
                    ICacheEntryOptions? options = null,
                    CancellationToken cancellationToken = default
                    );
        /// <summary>
        /// Try removing an entry by its key
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <returns>Removed entry (item is not yet disposed!)</returns>
        ICacheEntry<T>? TryRemove(in string key);
        /// <summary>
        /// Remove an entry
        /// </summary>
        /// <param name="entry">Entry to remove (item won't be disposed!)</param>
        /// <returns>If removed</returns>
        bool Remove(in ICacheEntry<T> entry);
        /// <summary>
        /// Remove entries by a filter
        /// </summary>
        /// <param name="filter">Filter (needs to return if to remove the entry)</param>
        /// <returns>Removed entries</returns>
        void RemoveBy(in Func<ICacheEntry<T>, bool> filter);
        /// <summary>
        /// Remove entries by a filter
        /// </summary>
        /// <param name="filter">Filter (needs to return if to remove the entry)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Removed entries</returns>
        Task RemoveByAsync(Func<ICacheEntry<T>, CancellationToken, Task<bool>> filter, CancellationToken cancellationToken = default);
        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <param name="disposeItems">Dispose the items?</param>
        /// <returns>Removed cache entries</returns>
        ICacheEntry<T>[] Clear(in bool disposeItems = false);
        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <param name="disposeItems">Dispose the items?</param>
        /// <returns>Removed cache entries</returns>
        Task<ICacheEntry<T>[]> ClearAsync(bool disposeItems = false);
        /// <summary>
        /// Delegate for a cache entry factory
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry (or <see langword="null"/>, if no item could be created for the given key)</returns>
        public delegate Task<ICacheEntry<T>?> CacheEntryFactory_Delegate(
            ICache<T> cache,
            string key,
            ICacheEntryOptions? options,
            CancellationToken cancellationToken
            );
        /// <summary>
        /// Delegate for a cache entry event handler
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="e">Arguments</param>
        public delegate void CacheEntryEvent_Delegate(ICache<T> cache, CacheEntryEventArgs e);
        /// <summary>
        /// Raised when an entry was added
        /// </summary>
        event CacheEntryEvent_Delegate? OnCacheEntryAdded;
        /// <summary>
        /// Raised when an entry was removed
        /// </summary>
        event CacheEntryEvent_Delegate? OnCacheEntryRemoved;
        /// <summary>
        /// Cache entry event arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="entry">Entry</param>
        /// <param name="reason">Reason</param>
        public class CacheEntryEventArgs(in ICacheEntry<T> entry, in CacheEventReasons reason) : EventArgs()
        {
            /// <summary>
            /// Entry
            /// </summary>
            public ICacheEntry<T> Entry { get; } = entry;

            /// <summary>
            /// Reason
            /// </summary>
            public CacheEventReasons Reason { get; } = reason;
        }
    }
}
