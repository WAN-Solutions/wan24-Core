namespace wan24.Core
{
    /// <summary>
    /// <see cref="InMemoryCache"/> extensions
    /// </summary>
    public static class InMemoryCacheExtensions
    {
        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove the existign entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is <see cref="AutoDisposer{T}"/></exception>
        public static InMemoryCacheEntry<tEntry> Add<tEntry, tItem>(
            this InMemoryCache<tEntry> cache,
            in tItem item,
            InMemoryCacheEntryOptions? options = null,
            in bool removeExisting = true,
            in bool disposeUnused = true
            )
            where tItem : tEntry, IInMemoryCacheItem
        {
            options ??= item.Options ?? cache.EnsureEntryOptions(options);
            return cache.Add(item.Key, item, options, removeExisting, disposeUnused);
        }

        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove the existign entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is <see cref="AutoDisposer{T}"/></exception>
        public static async Task<InMemoryCacheEntry<tEntry>> AddAsync<tEntry, tItem>(
            this InMemoryCache<tEntry> cache,
            tItem item,
            InMemoryCacheEntryOptions? options = null,
            bool removeExisting = true,
            bool disposeUnused = true,
            CancellationToken cancellationToken = default
            )
            where tItem : tEntry, IInMemoryCacheItem
        {
            options ??= item.Options ?? cache.EnsureEntryOptions(options);
            return await cache.AddAsync(item.Key, item, options, removeExisting, disposeUnused, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Get an item usage context for a cached item
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="cache">Cache</param>
        /// <param name="key">Key</param>
        /// <param name="entryFactory">Cache entry factory</param>
        /// <param name="options">Options</param>
        /// <returns>Item context</returns>
        public static AutoDisposer<T>.Context? GetItemContext<T>(
            this InMemoryCache<AutoDisposer<T>> cache,
            string key,
            InMemoryCache<AutoDisposer<T>>.CacheEntryFactory_Delegate? entryFactory = null,
            InMemoryCacheEntryOptions? options = null
            )
        {
            if (!cache.IsItemAutoDisposer) throw new InvalidOperationException();
            while (true)
            {
                if (cache.Get(key, entryFactory, options) is AutoDisposer<T> disposer)
                    try
                    {
                        return disposer.UseObject();
                    }
                    catch (ObjectDisposedException) when (!cache.IsDisposing && (disposer.ShouldDispose || disposer.IsDisposing))
                    {
                        continue;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                return null;
            }
        }

        /// <summary>
        /// Get an item usage context for a cached item
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="cache">Cache</param>
        /// <param name="key">Key</param>
        /// <param name="entryFactory">Cache entry factory</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Item context</returns>
        public static async Task<AutoDisposer<T>.Context?> GetItemContextAsync<T>(
            this InMemoryCache<AutoDisposer<T>> cache,
            string key,
            InMemoryCache<AutoDisposer<T>>.CacheEntryFactory_Delegate? entryFactory = null,
            InMemoryCacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
            )
        {
            if (!cache.IsItemAutoDisposer) throw new InvalidOperationException();
            while (true)
            {
                if (await cache.GetAsync(key, entryFactory, options, cancellationToken).DynamicContext() is AutoDisposer<T> disposer)
                    try
                    {
                        return await disposer.UseObjectAsync().DynamicContext();
                    }
                    catch (ObjectDisposedException) when (!cache.IsDisposing && (disposer.ShouldDispose || disposer.IsDisposing))
                    {
                        continue;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                return null;
            }
        }
    }
}
