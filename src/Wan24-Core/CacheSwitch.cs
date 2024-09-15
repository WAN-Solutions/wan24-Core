using System.Collections.Immutable;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="ICache{T}"/> switch (requires to call methods with <see cref="ICacheEntryOptions"/>)
    /// </summary>
    /// <typeparam name="T">Cache item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="cacheOptions">Cache options</param>
    public class CacheSwitch<T>(params CacheSwitchOptions<T>[] cacheOptions) : DisposableBase(), ICache<T>
    {
        /// <summary>
        /// Number of cache options
        /// </summary>
        protected readonly int CacheOptionsCount = cacheOptions.Length;
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Cache options
        /// </summary>
        public ImmutableArray<CacheSwitchOptions<T>> CacheOptions { get; } = [.. cacheOptions.OrderBy(o => o.MaxItemSize)];

        /// <summary>
        /// If to dispose the caches from the <see cref="CacheOptions"/> when disposing
        /// </summary>
        public bool DisposeCaches { get; init; }

        /// <inheritdoc/>
        public ICacheOptions Options => throw new NotSupportedException();

        /// <summary>
        /// Get the cache options for an item size and a cache type
        /// </summary>
        /// <param name="cache">Cache type ID (see <see cref="CacheSwitchOptions{T}.CacheType"/>) or <c>-1</c> for any</param>
        /// <param name="key">Item key</param>
        /// <param name="itemSize">Item size</param>
        /// <returns>Cache options</returns>
        /// <exception cref="OutOfMemoryException">The item is too large for all managed caches, or no matching cache for the given <c>cache</c> type found</exception>
        public virtual CacheSwitchOptions<T> GetCacheOptions(in int cache, in string key, in int itemSize)
        {
            EnsureUndisposed();
            bool oom = true;
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            for (int i = 0; i < CacheOptionsCount; i++)
                if (cacheOptions[i].MaxItemSize >= itemSize)
                {
                    oom = false;
                    if (cache != -1 && cacheOptions[i].CacheType != cache) continue;
                    return cacheOptions[i];
                }
            throw new OutOfMemoryException(
                oom 
                    ? "The item is too large for all managed caches" 
                    : $"No matching cache for the given cache type {cache} found"
                );
        }

        /// <inheritdoc/>
        public ICacheEntry<T> CreateEntry(in string key, in T item, ICacheEntryOptions? options = null) => throw new NotSupportedException();

        /// <summary>
        /// Add a cache entry
        /// </summary>
        /// <param name="cache">Cache type ID (see <see cref="CacheSwitchOptions{T}.CacheType"/>)</param>
        /// <param name="key">Key</param>
        /// <param name="item">Item</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">If to remove an existing item</param>
        /// <param name="disposeUnused">If to dispose an unused given <c>item</c></param>
        /// <returns>Cache entry</returns>
        public virtual ICacheEntry<T> Add(in int cache, in string key, in T item, ICacheEntryOptions options, in bool removeExisting = true, in bool disposeUnused = true)
        {
            EnsureUndisposed();
            CacheSwitchOptions<T> co = GetCacheOptions(cache, key, options.Size);
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            using SemaphoreSyncContext ssc = Sync;
            for (int i = 0; i < CacheOptionsCount && cacheOptions[i].Cache != co.Cache; cacheOptions[i].Cache.TryRemove(key), i++) ;
            return co.Cache.Add(key, item, options, removeExisting, disposeUnused);
        }

        /// <inheritdoc/>
        public virtual ICacheEntry<T> Add(in string key, in T item, ICacheEntryOptions? options = null, in bool removeExisting = true, in bool disposeUnused = true)
            => Add(cache: 0, key, item, options ?? throw new ArgumentNullException(nameof(options)), removeExisting, disposeUnused);

        /// <summary>
        /// Add a cache entry
        /// </summary>
        /// <param name="cache">Cache type ID (see <see cref="CacheSwitchOptions{T}.CacheType"/>)</param>
        /// <param name="key">Key</param>
        /// <param name="item">Item</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">If to remove an existing item</param>
        /// <param name="disposeUnused">If to dispose an unused given <c>item</c></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        public virtual async Task<ICacheEntry<T>> AddAsync(
            int cache,
            string key,
            T item,
            ICacheEntryOptions options,
            bool removeExisting = true,
            bool disposeUnused = true,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            CacheSwitchOptions<T> co = GetCacheOptions(cache, key, options.Size);
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            for (int i = 0; i < CacheOptionsCount && cacheOptions[i].Cache != co.Cache; cacheOptions[i].Cache.TryRemove(key), i++) ;
            return await co.Cache.AddAsync(key, item, options, removeExisting, disposeUnused, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public virtual Task<ICacheEntry<T>> AddAsync(
            string key,
            T item,
            ICacheEntryOptions? options = null,
            bool removeExisting = true,
            bool disposeUnused = true,
            CancellationToken cancellationToken = default
            )
            => AddAsync(cache: 0, key, item, options ?? throw new ArgumentNullException(nameof(options)), removeExisting, disposeUnused, cancellationToken);

        /// <summary>
        /// Get an entry
        /// </summary>
        /// <param name="cache">Cache type ID (see <see cref="CacheSwitchOptions{T}.CacheType"/>) or <c>-1</c> for any</param>
        /// <param name="key">Entry key</param>
        /// <param name="entryFactory">Entry factory</param>
        /// <param name="options">Options</param>
        /// <returns>Cache entry</returns>
        public virtual ICacheEntry<T>? Get(in int cache, in string key, in ICache<T>.CacheEntryFactory_Delegate? entryFactory = null, in ICacheEntryOptions? options = null)
        {
            EnsureUndisposed();
            if (entryFactory is not null) ArgumentNullException.ThrowIfNull(options);
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            for (int i = 0; i < CacheOptionsCount; i++)
            {
                if ((cache != -1 && cacheOptions[i].CacheType != cache) || (options is not null && options.Size > cacheOptions[i].MaxItemSize))
                    continue;
                if (cacheOptions[i].Cache.Get(key, entryFactory, options) is ICacheEntry<T> res)
                    return res;
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual ICacheEntry<T>? Get(in string key, in ICache<T>.CacheEntryFactory_Delegate? entryFactory = null, in ICacheEntryOptions? options = null)
            => Get(cache: 0, key, entryFactory, options);

        /// <summary>
        /// Get an entry
        /// </summary>
        /// <param name="cache">Cache type ID (see <see cref="CacheSwitchOptions{T}.CacheType"/>) or <c>-1</c> for any</param>
        /// <param name="key">Entry key</param>
        /// <param name="entryFactory">Entry factory</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        public virtual async Task<ICacheEntry<T>?> GetAsync(
            int cache,
            string key,
            ICache<T>.CacheEntryFactory_Delegate? entryFactory = null,
            ICacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            if (entryFactory is not null) ArgumentNullException.ThrowIfNull(options);
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            for (int i = 0; i < CacheOptionsCount; i++)
            {
                if ((cache != -1 && cacheOptions[i].CacheType != cache) || (options is not null && options.Size > cacheOptions[i].MaxItemSize))
                    continue;
                if (await cacheOptions[i].Cache.GetAsync(key, entryFactory, options, cancellationToken).DynamicContext() is ICacheEntry<T> res)
                    return res;
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual Task<ICacheEntry<T>?> GetAsync(
            string key,
            ICache<T>.CacheEntryFactory_Delegate? entryFactory = null,
            ICacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
            )
            => GetAsync(cache: 0, key, entryFactory, options, cancellationToken);

        /// <inheritdoc/>
        public bool Remove(in ICacheEntry<T> entry)
        {
            EnsureUndisposed();
            return entry.Cache.Remove(entry);
        }

        /// <inheritdoc/>
        public ICacheEntry<T>? TryRemove(in string key)
        {
            EnsureUndisposed();
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            for (int i = 0; i < CacheOptionsCount; i++)
                if (cacheOptions[i].Cache.TryRemove(key) is ICacheEntry<T> res)
                    return res;
            return null;
        }

        /// <inheritdoc/>
        public void RemoveBy(in Func<ICacheEntry<T>, bool> filter)
        {
            EnsureUndisposed();
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            using SemaphoreSyncContext ssc = Sync;
            for (int i = 0; i < CacheOptionsCount; cacheOptions[i].Cache.RemoveBy(filter), i++) ;
        }

        /// <inheritdoc/>
        public async Task RemoveByAsync(Func<ICacheEntry<T>, CancellationToken, Task<bool>> filter, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            for (int i = 0; i < CacheOptionsCount; await cacheOptions[i].Cache.RemoveByAsync(filter, cancellationToken).DynamicContext(), i++) ;
        }

        /// <inheritdoc/>
        public ICacheEntry<T>[] Clear(in bool disposeItems = false)
        {
            EnsureUndisposed();
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            List<ICacheEntry<T>> res = [];
            using SemaphoreSyncContext ssc = Sync;
            for (int i = 0; i < CacheOptionsCount; res.AddRange(cacheOptions[i].Cache.Clear(disposeItems)), i++) ;
            return [.. res];
        }

        /// <inheritdoc/>
        public async Task<ICacheEntry<T>[]> ClearAsync(bool disposeItems = false)
        {
            EnsureUndisposed();
            ImmutableArray<CacheSwitchOptions<T>> cacheOptions = CacheOptions;
            List<ICacheEntry<T>> res = [];
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext();
            for (int i = 0; i < CacheOptionsCount; res.AddRange(await cacheOptions[i].Cache.ClearAsync(disposeItems).DynamicContext()), i++) ;
            return [.. res];
        }

        /// <inheritdoc/>
        public event ICache<T>.CacheEntryEvent_Delegate? OnCacheEntryAdded
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public event ICache<T>.CacheEntryEvent_Delegate? OnCacheEntryRemoved
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Sync.Dispose();
            if (DisposeCaches) CacheOptions.Select(o => o.Cache).TryDisposeAll();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Sync.DisposeAsync().DynamicContext();
            if (DisposeCaches) await CacheOptions.Select(o => o.Cache).TryDisposeAllAsync().DynamicContext();
        }
    }
}
