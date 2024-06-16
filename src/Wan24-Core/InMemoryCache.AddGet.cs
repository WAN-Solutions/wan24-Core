namespace wan24.Core
{
    // Add/get
    public partial class InMemoryCache<T>
    {
        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="key">Entry key</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove the existign entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is disposable</exception>
        public virtual InMemoryCacheEntry<T> Add(
            in string key,
            in T item,
            InMemoryCacheEntryOptions? options = null,
            in bool removeExisting = true,
            in bool disposeUnused = true
            )
        {
            EnsureUndisposed();
            options = EnsureEntryOptions(options);
            // Check item size
            bool isOverSize = Options.MaxItemSize > 0 && options.Size > Options.MaxItemSize;
            if (isOverSize && IsItemDisposable)
            {
                if (disposeUnused)
                    item.TryDispose();
                throw new OutOfMemoryException();
            }
            // Don't overwrite the existing item
            if (!removeExisting && Cache.TryGetValue(key, out InMemoryCacheEntry<T>? older))
            {
                if (disposeUnused && IsItemDisposable)
                    item.TryDispose();
                older.OnAccess();
                return older;
            }
            // Overwrite the existing item
            if (removeExisting && Cache.TryRemove(key, out InMemoryCacheEntry<T>? existing))
            {
                if (IsItemDisposable)
                    DisposeItem(existing.Item);
                existing.OnRemoved();
            }
            // Add a new item
            InMemoryCacheEntry<T> entry = CreateEntry(key, item, options);
            bool disposeItem = false,
                removeEntry = false;
            try
            {
                while (true)
                {
                    CancelToken.ThrowIfCancellationRequested();
                    // Use the newer item
                    if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? newer))
                    {
                        disposeItem = true;
                        newer.OnAccess();
                        return newer;
                    }
                    // Respect hard limits
                    if (!isOverSize && HasHardLimits)
                        ApplyHardLimits(entry, CancelToken).GetAwaiter().GetResult();
                    // Add and return
                    if (isOverSize || Cache.TryAdd(key, entry))
                    {
                        removeEntry = true;
                        if (!isOverSize)
                            entry.OnAdded();
                        return entry;
                    }
                }
            }
            catch
            {
                disposeItem = true;
                throw;
            }
            finally
            {
                if (disposeItem)
                {
                    if (removeEntry && !Remove(entry))
                        entry.OnRemoved();
                    if (disposeUnused && IsItemDisposable)
                        item.TryDispose();
                }
            }
        }

        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="key">Entry key</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove the existign entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is disposable</exception>
        public virtual async Task<InMemoryCacheEntry<T>> AddAsync(
            string key,
            T item,
            InMemoryCacheEntryOptions? options = null,
            bool removeExisting = true,
            bool disposeUnused = true,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            options = EnsureEntryOptions(options);
            // Check item size
            bool isOverSize = Options.MaxItemSize > 0 && options.Size > Options.MaxItemSize;
            if (isOverSize && IsItemDisposable)
            {
                if (disposeUnused)
                    await item.TryDisposeAsync().DynamicContext();
                throw new OutOfMemoryException();
            }
            // Don't overwrite the existing item
            if (!removeExisting && Cache.TryGetValue(key, out InMemoryCacheEntry<T>? older))
            {
                if (disposeUnused && IsItemDisposable)
                    await item.TryDisposeAsync().DynamicContext();
                older.OnAccess();
                return older;
            }
            // Overwrite the existing item
            if (removeExisting && Cache.TryRemove(key, out InMemoryCacheEntry<T>? existing))
            {
                if (IsItemDisposable)
                    await DisposeItemAsync(existing.Item).DynamicContext();
                existing.OnRemoved();
            }
            // Add a new item
            InMemoryCacheEntry<T> entry = CreateEntry(key, item, options);
            bool disposeItem = false,
                removeEntry = false;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Use the newer item
                    if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? newer))
                    {
                        disposeItem = disposeUnused;
                        newer.OnAccess();
                        return newer;
                    }
                    // Respect hard limits
                    if (!isOverSize && HasHardLimits)
                        await ApplyHardLimits(entry, cancellationToken).DynamicContext();
                    // Add and return
                    if (isOverSize || Cache.TryAdd(key, entry))
                    {
                        removeEntry = true;
                        if (!isOverSize)
                            entry.OnAdded();
                        return entry;
                    }
                }
            }
            catch
            {
                disposeItem = true;
                throw;
            }
            finally
            {
                if (disposeItem)
                {
                    if (removeEntry && !Remove(entry))
                        entry.OnRemoved();
                    if (disposeUnused && IsItemDisposable)
                        await item.TryDisposeAsync().DynamicContext();
                }
            }
        }

        /// <summary>
        /// Determine if a cache entry key is contained
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>If contained</returns>
        public virtual bool ContainsKey(in string key)
        {
            EnsureUndisposed();
            return Cache.ContainsKey(key);
        }

        /// <summary>
        /// Get a cache entry
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entryFactory">Cache entry factory</param>
        /// <param name="options">Options</param>
        /// <returns>Cache entry</returns>
        public virtual InMemoryCacheEntry<T>? Get(
            in string key,
            in CacheEntryFactory_Delegate? entryFactory = null,
            in InMemoryCacheEntryOptions? options = null
            )
        {
            EnsureUndisposed();
            // Use the existing item, if possible
            if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? existing))
                if (!existing.CanUse)
                {
                    if (Remove(existing) && IsItemDisposable)
                        DisposeItem(existing.Item);
                }
                else
                {
                    existing.OnAccess();
                    return existing;
                }
            if (entryFactory is null) return null;
            // Create the item
            InMemoryCacheEntry<T>? newEntry = entryFactory(this, key, options, CancelToken).GetAwaiter().GetResult();
            if (newEntry is null) return null;
            bool disposeItem = false,
                removeEntry = false;
            try
            {
                // Check item size
                bool isOverSize = Options.MaxItemSize > 0 && newEntry.Size > Options.MaxItemSize;
                if (isOverSize && IsItemDisposable)
                {
                    disposeItem = true;
                    return null;
                }
                // Add the new item
                while (true)
                {
                    CancelToken.ThrowIfCancellationRequested();
                    // Use the newer item
                    if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? newer))
                    {
                        disposeItem = true;
                        newer.OnAccess();
                        return newer;
                    }
                    // Respect hard limits
                    if (!isOverSize && HasHardLimits)
                        ApplyHardLimits(newEntry, CancelToken).GetAwaiter().GetResult();
                    // Add and return
                    if (isOverSize || Cache.TryAdd(key, newEntry))
                    {
                        removeEntry = true;
                        if (!isOverSize)
                            newEntry.OnAdded();
                        return newEntry;
                    }
                }
            }
            catch
            {
                disposeItem = true;
                throw;
            }
            finally
            {
                // If the created item wasn't used, finally, dispose it
                if (disposeItem)
                {
                    if (removeEntry && !Remove(newEntry))
                        newEntry.OnRemoved();
                    if (IsItemDisposable)
                        newEntry.Item.TryDispose();
                }
            }
        }

        /// <summary>
        /// Get a cache entry
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="entryFactory">Cache entry factory</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        public virtual async Task<InMemoryCacheEntry<T>?> GetAsync(
            string key,
            CacheEntryFactory_Delegate? entryFactory = null,
            InMemoryCacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            // Use the existing item, if possible
            if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? existing))
                if (!existing.CanUse)
                {
                    if (Remove(existing) && IsItemDisposable)
                        await DisposeItemAsync(existing.Item).DynamicContext();
                }
                else
                {
                    existing.OnAccess();
                    return existing;
                }
            if (entryFactory is null) return null;
            // Create the item
            InMemoryCacheEntry<T>? newEntry = await entryFactory(this, key, options, cancellationToken).DynamicContext();
            if (newEntry is null) return null;
            bool disposeItem = false,
                removeEntry = false;
            try
            {
                // Check item size
                bool isOverSize = Options.MaxItemSize > 0 && newEntry.Size > Options.MaxItemSize;
                if (isOverSize && IsItemDisposable)
                {
                    disposeItem = true;
                    return null;
                }
                // Add the new item
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Use the newer item
                    if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? newer))
                    {
                        disposeItem = true;
                        newer.OnAccess();
                        return newer;
                    }
                    // Respect hard limits
                    if (!isOverSize && HasHardLimits)
                        await ApplyHardLimits(newEntry, cancellationToken).DynamicContext();
                    // Add and return
                    if (isOverSize || Cache.TryAdd(key, newEntry))
                    {
                        removeEntry = true;
                        if (!isOverSize)
                            newEntry.OnAdded();
                        return newEntry;
                    }
                }
            }
            catch
            {
                disposeItem = true;
                throw;
            }
            finally
            {
                // If the created item wasn't used, finally, dispose it
                if (disposeItem && IsItemDisposable)
                {
                    if (removeEntry && !Remove(newEntry))
                        newEntry.OnRemoved();
                    if (IsItemDisposable)
                        await newEntry.Item.TryDisposeAsync().DynamicContext();
                }
            }
        }
    }
}
