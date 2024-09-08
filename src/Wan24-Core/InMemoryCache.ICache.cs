namespace wan24.Core
{
    // ICache implementation
    public partial class InMemoryCache<T>
    {
        /// <inheritdoc/>
        ICacheEntry<T> ICache<T>.CreateEntry(in string key, in T item, ICacheEntryOptions? options)
            => CreateEntry(
                key,
                item,
                options is null
                    ? null
                    : options as InMemoryCacheEntryOptions ?? throw new ArgumentException($"{typeof(InMemoryCacheEntryOptions)} required", nameof(options))
                );

        /// <inheritdoc/>
        ICacheEntry<T> ICache<T>.Add(in string key, in T item, ICacheEntryOptions? options, in bool removeExisting, in bool disposeUnused)
            => Add(
                key,
                item,
                options is null
                    ? null
                    : options as InMemoryCacheEntryOptions ?? throw new ArgumentException($"{typeof(InMemoryCacheEntryOptions)} required", nameof(options)),
                removeExisting,
                disposeUnused
                );

        /// <inheritdoc/>
        async Task<ICacheEntry<T>> ICache<T>.AddAsync(string key, T item, ICacheEntryOptions? options, bool removeExisting, bool disposeUnused, CancellationToken cancellationToken)
            => await AddAsync(
                key,
                item,
                options is null
                    ? null
                    : options as InMemoryCacheEntryOptions ?? throw new ArgumentException($"{typeof(InMemoryCacheEntryOptions)} required", nameof(options)),
                removeExisting,
                disposeUnused,
                cancellationToken
                ).DynamicContext();

        /// <inheritdoc/>
        ICacheEntry<T>? ICache<T>.Get(in string key, in ICache<T>.CacheEntryFactory_Delegate? entryFactory, in ICacheEntryOptions? options)
            => Get(
                key,
                entryFactory,
                options is null
                    ? null
                    : options as InMemoryCacheEntryOptions ?? throw new ArgumentException($"{typeof(InMemoryCacheEntryOptions)} required", nameof(options))
                );

        /// <inheritdoc/>
        async Task<ICacheEntry<T>?> ICache<T>.GetAsync(string key, ICache<T>.CacheEntryFactory_Delegate? entryFactory, ICacheEntryOptions? options, CancellationToken cancellationToken)
            => await GetAsync(
                key,
                entryFactory,
                options is null
                    ? null
                    : options as InMemoryCacheEntryOptions ?? throw new ArgumentException($"{typeof(InMemoryCacheEntryOptions)} required", nameof(options)),
                cancellationToken
                ).DynamicContext();

        /// <inheritdoc/>
        ICacheEntry<T>? ICache<T>.TryRemove(in string key) => TryRemove(key);

        /// <inheritdoc/>
        bool ICache<T>.Remove(in ICacheEntry<T> entry)
            => Remove(entry as InMemoryCacheEntry<T> ?? throw new ArgumentException($"{typeof(InMemoryCacheEntry<T>)} required", nameof(entry)));

        /// <inheritdoc/>
        ICacheEntry<T>[] ICache<T>.Clear(in bool disposeItems) => Clear();

        /// <inheritdoc/>
        async Task<ICacheEntry<T>[]> ICache<T>.ClearAsync(bool disposeItems) => await ClearAsync(disposeItems).DynamicContext();

        /// <inheritdoc/>
        public event ICache<T>.CacheEntryEvent_Delegate? OnCacheEntryAdded;

        /// <inheritdoc/>
        public event ICache<T>.CacheEntryEvent_Delegate? OnCacheEntryRemoved;
    }
}
