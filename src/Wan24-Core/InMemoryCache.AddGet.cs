namespace wan24.Core
{
    // Add/get
    public partial class InMemoryCache<T>
    {
        /// <inheritdoc/>
        public virtual InMemoryCacheEntry<T> Add(
            in string key,
            in T item,
            InMemoryCacheEntryOptions? options = null,
            in bool removeExisting = true,
            in bool disposeUnused = true
            )
        {
            EnsureUndisposed();
            ArgumentNullException.ThrowIfNull(item, nameof(item));
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
                HandleEntryRemoved(existing);
                RaiseOnEntryRemoved(existing);
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
                        removeEntry = !isOverSize;
                        if (!isOverSize)
                            _Count++;
                        if (removeEntry)
                        {
                            entry.OnAdded();
                            HandleEntryAdded(entry);
                            RaiseOnEntryAdded(entry);
                        }
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
                    if (removeEntry && !RemoveInt(entry, CacheEventReasons.Automatic))
                        entry.OnRemoved();
                    if (disposeUnused && IsItemDisposable)
                        item.TryDispose();
                }
            }
        }

        /// <inheritdoc/>
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
            ArgumentNullException.ThrowIfNull(item, nameof(item));
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
                await HandleEntryRemovedAsync(existing, cancellationToken).DynamicContext();
                RaiseOnEntryRemoved(existing);
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
                        removeEntry = !isOverSize;
                        if (!isOverSize)
                            _Count++;
                        if (removeEntry)
                        {
                            entry.OnAdded();
                            await HandleEntryAddedAsync(entry, cancellationToken).DynamicContext();
                            RaiseOnEntryAdded(entry);
                        }
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
                    if (removeEntry && !RemoveInt(entry, CacheEventReasons.Automatic))
                        entry.OnRemoved();
                    if (disposeUnused && IsItemDisposable)
                        await item.TryDisposeAsync().DynamicContext();
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(in string key)
        {
            EnsureUndisposed();
            return Cache.ContainsKey(key);
        }

        /// <inheritdoc/>
        public virtual InMemoryCacheEntry<T>? Get(
            in string key,
            in IInMemoryCache<T>.CacheEntryFactory_Delegate? entryFactory = null,
            in InMemoryCacheEntryOptions? options = null
            )
        {
            EnsureUndisposed();
            // Use the existing item, if possible
            if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? existing) || (existing = GetEntry(key)) is not null)
                if (!existing.CanUse)
                {
                    if (RemoveInt(existing, CacheEventReasons.Automatic) && IsItemDisposable)
                        DisposeItem(existing.Item);
                }
                else
                {
                    existing.OnAccess();
                    return existing;
                }
            if (entryFactory is null) return null;
            // Create the item
            if (entryFactory(this, key, options, CancelToken).GetAwaiter().GetResult() is not InMemoryCacheEntry<T> newEntry)
                return null;
            if (newEntry.Item is null)
                throw new InvalidProgramException("Entry factory created an entry with a NULL item");
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
                        removeEntry = !isOverSize;
                        if (!isOverSize)
                            _Count++;
                        if (removeEntry)
                        {
                            newEntry.OnAdded();
                            HandleEntryAdded(newEntry);
                            RaiseOnEntryAdded(newEntry);
                        }
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
                    if (removeEntry && !RemoveInt(newEntry, CacheEventReasons.Automatic))
                        newEntry.OnRemoved();
                    if (IsItemDisposable)
                        newEntry.Item.TryDispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<InMemoryCacheEntry<T>?> GetAsync(
            string key,
            IInMemoryCache<T>.CacheEntryFactory_Delegate? entryFactory = null,
            InMemoryCacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            // Use the existing item, if possible
            if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? existing) || (existing = await GetEntryAsync(key, cancellationToken).DynamicContext()) is not null)
                if (!existing.CanUse)
                {
                    if (RemoveInt(existing, CacheEventReasons.Automatic) && IsItemDisposable)
                        await DisposeItemAsync(existing.Item).DynamicContext();
                }
                else
                {
                    existing.OnAccess();
                    return existing;
                }
            if (entryFactory is null) return null;
            // Create the item
            if (await entryFactory(this, key, options, cancellationToken).DynamicContext() is not InMemoryCacheEntry<T> newEntry)
                return null;
            if (newEntry.Item is null)
                throw new InvalidProgramException("Entry factory created an entry with a NULL item");
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
                        removeEntry = !isOverSize;
                        if (!isOverSize)
                            _Count++;
                        if (removeEntry)
                        {
                            newEntry.OnAdded();
                            await HandleEntryAddedAsync(newEntry, cancellationToken).DynamicContext();
                            RaiseOnEntryAdded(newEntry);
                        }
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
                    if (removeEntry && !RemoveInt(newEntry, CacheEventReasons.Automatic))
                        newEntry.OnRemoved();
                    if (IsItemDisposable)
                        await newEntry.Item.TryDisposeAsync().DynamicContext();
                }
            }
        }
    }
}
