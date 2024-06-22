using Microsoft.Extensions.Primitives;
using System.Collections;
using System.ComponentModel;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an in-memory cache
    /// </summary>
    public interface IInMemoryCache : IWillDispose, IServiceWorkerStatus, IChangeToken, INotifyPropertyChanged, IExportUserActions, IEnumerable
    {
        /// <summary>
        /// GUID
        /// </summary>
        string GUID { get; }
        /// <summary>
        /// Options
        /// </summary>
        InMemoryCacheOptions Options { get; }
        /// <summary>
        /// Number of currently cached items
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Cache entry keys
        /// </summary>
        IEnumerable<string> Keys { get; }
        /// <summary>
        /// Total size of all cache entries
        /// </summary>
        long Size { get; }
        /// <summary>
        /// If the item is an <see cref="AutoDisposer{T}"/>
        /// </summary>
        bool IsItemAutoDisposer { get; }
        /// <summary>
        /// Ensure non-<see langword="null"/> cache entry options
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Options</returns>
        InMemoryCacheEntryOptions EnsureEntryOptions(InMemoryCacheEntryOptions? options);
        /// <summary>
        /// Determine if a cache entry key is contained
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>If contained</returns>
        bool ContainsKey(in string key);
        /// <summary>
        /// Reduce the number of cache entries
        /// </summary>
        /// <param name="targetCount">Target count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceCount(int targetCount, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries
        /// </summary>
        /// <param name="targetCount">Target count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceCountAsync(int targetCount, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the size of the cache
        /// </summary>
        /// <param name="targetSize">Target size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceSize(long targetSize, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the size of the cache
        /// </summary>
        /// <param name="targetSize">Target size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceSizeAsync(long targetSize, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the oldest entries
        /// </summary>
        /// <param name="maxAge">Max. cache entry age</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceOld(TimeSpan maxAge, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the oldest entries
        /// </summary>
        /// <param name="maxAge">Max. cache entry age</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceOldAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the least accessed entries
        /// </summary>
        /// <param name="maxIdle">Max. cache entry idle time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceUnpopular(TimeSpan maxIdle, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the least accessed entries
        /// </summary>
        /// <param name="maxIdle">Max. cache entry idle time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceUnpopularAsync(TimeSpan maxIdle, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the cache memory usage by removing entries until a maximum memory usage does match
        /// </summary>
        /// <param name="targetUsage">Target max. memory usage in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceMemory(long targetUsage, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the cache memory usage by removing entries until a maximum memory usage does match
        /// </summary>
        /// <param name="targetUsage">Target max. memory usage in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceMemoryAsync(long targetUsage, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for an in-memory cache
    /// </summary>
    /// <typeparam name="T">Cached item type</typeparam>
    public interface IInMemoryCache<T> 
        : IInMemoryCache, 
            IObservable<ConcurrentChangeTokenDictionary<string, InMemoryCacheEntry<T>>>, 
            IEnumerable<KeyValuePair<string, InMemoryCacheEntry<T>>>, 
            IEnumerable<T>,
            IDictionary<string, InMemoryCacheEntry<T>>
    {
        /// <summary>
        /// Create a cache entry
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="item">Cached item</param>
        /// <param name="options">Options</param>
        /// <returns>Cache entry</returns>
        InMemoryCacheEntry<T> CreateEntry(in string key, in T item, InMemoryCacheEntryOptions? options = null);
        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned and <c>disposeUnused</c> isn't <see langword="false"/>)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove an existing entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is disposable</exception>
        InMemoryCacheEntry<T> Add(
                    in string key,
                    in T item,
                    InMemoryCacheEntryOptions? options = null,
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
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is disposable</exception>
        Task<InMemoryCacheEntry<T>> AddAsync(
                    string key,
                    T item,
                    InMemoryCacheEntryOptions? options = null,
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
        InMemoryCacheEntry<T>? Get(
                    in string key,
                    in CacheEntryFactory_Delegate? entryFactory = null,
                    in InMemoryCacheEntryOptions? options = null
                    );
        /// <summary>
        /// Get a cache entry
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="entryFactory">Cache entry factory (used to create a new item from the given <c>key</c>, if no existing item was found)</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        Task<InMemoryCacheEntry<T>?> GetAsync(
                    string key,
                    CacheEntryFactory_Delegate? entryFactory = null,
                    InMemoryCacheEntryOptions? options = null,
                    CancellationToken cancellationToken = default
                    );
        /// <summary>
        /// Try removing an entry by its key
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <returns>Removed entry (item is not yet disposed!)</returns>
        InMemoryCacheEntry<T>? TryRemove(in string key);
        /// <summary>
        /// Remove an entry
        /// </summary>
        /// <param name="entry">Entry to remove (item won't be disposed!)</param>
        /// <returns>If removed</returns>
        bool Remove(in InMemoryCacheEntry<T> entry);
        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <param name="disposeItems">Dispose the items?</param>
        /// <returns>Removed cache entries</returns>
        InMemoryCacheEntry<T>[] Clear(in bool disposeItems = false);
        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <param name="disposeItems">Dispose the items?</param>
        /// <returns>Removed cache entries</returns>
        Task<InMemoryCacheEntry<T>[]> ClearAsync(bool disposeItems = false);
        /// <summary>
        /// Delegate for a cache entry factory
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry (or <see langword="null"/>, if no item could be created for the given key)</returns>
        public delegate Task<InMemoryCacheEntry<T>?> CacheEntryFactory_Delegate(
            InMemoryCache<T> cache,
            string key,
            InMemoryCacheEntryOptions? options,
            CancellationToken cancellationToken
            );
    }
}
