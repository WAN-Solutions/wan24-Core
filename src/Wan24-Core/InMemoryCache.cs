namespace wan24.Core
{
    /// <summary>
    /// In-memory cache
    /// </summary>
    /// <param name="options">Options</param>
    public class InMemoryCache(in InMemoryCacheOptions options) : InMemoryCache<object>(options) { }

    /// <summary>
    /// In-memory cache
    /// </summary>
    /// <typeparam name="T">Cached item type</typeparam>
    public partial class InMemoryCache<T> : HostedServiceBase
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static InMemoryCache()
        {
            IsItemTypeDisposable = typeof(T) == typeof(object) ||
                typeof(IDisposable).IsAssignableFrom(typeof(T)) ||
                typeof(IAsyncDisposable).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        public InMemoryCache(in InMemoryCacheOptions options) : base()
        {
            // Validate options
            if (options.SoftCountLimit < 1 && options.SoftSizeLimit < 1 && options.AgeLimit <= TimeSpan.Zero && options.IdleLimit <= TimeSpan.Zero)
                throw new ArgumentException("Any limit (SoftCountLimit, SoftSizeLimit, AgeLimit, IdleLimit) is required", nameof(options));
            if (options.ConcurrencyLevel.HasValue && options.SoftCountLimit < 1 && options.HardCountLimit < 1)
                throw new ArgumentException("SoftCountLimit or HardCountLimit required, if ConcurrencyLevel was set", nameof(options));
            // Initialize this instance
            Cache = options.ConcurrencyLevel.HasValue
                ? new(options.ConcurrencyLevel.Value, Math.Max(options.SoftCountLimit, options.HardCountLimit))
                {
                    Tag = this
                }
                : new()
                {
                    Tag = this
                };
            IsItemAutoDisposer = typeof(T).HasBaseType(typeof(AutoDisposer<>));
            HasHardLimits = options.HardCountLimit > 0 || options.HardSizeLimit > 0;
            IsItemDisposable = options.TryDisposeItemsAlways || IsItemTypeDisposable;
            Options = options;
            TidyTimer = new(Options.TidyTimeout);
            TidyStrategyInstance = new TidyStrategy(this);
            AgeStrategyInstance = new AgeStrategy(this);
            AccessTimeStrategyInstance = new AccessTimeStrategy(this);
            LargerStrategyInstance = new LargerStrategy(this);
            SmallerStrategyInstance = new SmallerStrategy(this);
            // Timed auto-cleanup processing
            TidyTimer.OnTimeout += async (s, e) =>
            {
                try
                {
                    await TidyCacheAsync(CancelToken).DynamicContext();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new("Tidy in-memory cache failed", ex, ErrorHandling.SERVICE_ERROR, this));
                }
                finally
                {
                    if (!IsDisposing && IsRunning)
                        TidyTimer.Start();
                }
            };
        }

        /// <summary>
        /// Options
        /// </summary>
        public InMemoryCacheOptions Options { get; }

        /// <summary>
        /// Number of currently cached items
        /// </summary>
        public int Count => Cache.Count;

        /// <summary>
        /// Cache entry keys
        /// </summary>
        public IEnumerable<string> Keys => Cache.Keys;

        /// <summary>
        /// Cached items
        /// </summary>
        public IEnumerable<T> Items => Cache.Values.Select(e => e.Item);

        /// <summary>
        /// Size of all cache entries
        /// </summary>
        public long Size => Cache.Values.Sum(e => e.Size);

        /// <summary>
        /// If the item is an <see cref="AutoDisposer{T}"/>
        /// </summary>
        public bool IsItemAutoDisposer { get; }

        /// <summary>
        /// Try adding an item
        /// </summary>
        /// <param name="key">Entry key</param>
        /// <param name="item">Cached item (will be disposed, if a newer revision can be returned)</param>
        /// <param name="options">Options</param>
        /// <param name="removeExisting">Remove the existign entry?</param>
        /// <param name="disposeUnused">Dispose the given <c>item</c>, if a newer item was found?</param>
        /// <returns>Cache entry (may be another revision, if not removing or a newer item revision has been cached during processing)</returns>
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is <see cref="AutoDisposer{T}"/></exception>
        public virtual InMemoryCacheEntry<T> Add(
            in string key,
            in T item,
            InMemoryCacheEntryOptions? options = null,
            in bool removeExisting = true,
            in bool disposeUnused = true
            )
        {
            EnsureUndisposed();
            options ??= Options.DefaultEntryOptions ?? new();
            // Check item size
            bool isOverSize = Options.MaxItemSize > 0 && options.Size > Options.MaxItemSize;
            if (isOverSize && IsItemAutoDisposer)
                throw new OutOfMemoryException();
            // Don't overwrite the existing item
            if (!removeExisting && Cache.TryGetValue(key, out InMemoryCacheEntry<T>? older))
            {
                if (disposeUnused && IsItemDisposable)
                    item.TryDispose();
                return older;
            }
            // Overwrite the existing item
            if (removeExisting && Cache.TryRemove(key, out InMemoryCacheEntry<T>? existing) && IsItemDisposable)
                DisposeItem(existing.Item);
            // Add a new item
            InMemoryCacheEntry<T> entry = CreateCacheEntry(key, item, options);
            while (true)
            {
                CancelToken.ThrowIfCancellationRequested();
                // Use the newer item
                if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? newer))
                {
                    if (disposeUnused && IsItemDisposable)
                        item.TryDispose();
                    return newer;
                }
                // Respect hard limits
                if (!isOverSize && HasHardLimits)
                    ApplyHardLimits(entry, CancelToken).GetAwaiter().GetResult();
                // Add and return
                if (isOverSize || Cache.TryAdd(key, entry))
                {
                    if (!isOverSize)
                        try
                        {
                            entry.OnAdded();
                        }
                        catch
                        {
                            if (TryRemove(key) is InMemoryCacheEntry<T> faulted && faulted != entry && IsItemDisposable)
                                DisposeItem(faulted.Item);
                            entry.OnRemoved();
                            if (disposeUnused && IsItemDisposable)
                                entry.Item.TryDispose();
                            throw;
                        }
                    return entry;
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
        /// <exception cref="OutOfMemoryException">Item exceeds the <see cref="InMemoryCacheOptions.MaxItemSize"/>, and type is <see cref="AutoDisposer{T}"/></exception>
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
            options ??= Options.DefaultEntryOptions ?? new();
            // Check item size
            bool isOverSize = Options.MaxItemSize > 0 && options.Size > Options.MaxItemSize;
            if (isOverSize && IsItemAutoDisposer)
                throw new OutOfMemoryException();
            // Don't overwrite the existing item
            if (!removeExisting && Cache.TryGetValue(key, out InMemoryCacheEntry<T>? older))
            {
                if (disposeUnused && IsItemDisposable)
                    await item.TryDisposeAsync().DynamicContext();
                return older;
            }
            // Overwrite the existing item
            if (removeExisting && Cache.TryRemove(key, out InMemoryCacheEntry<T>? existing) && IsItemDisposable)
                await DisposeItemAsync(existing.Item).DynamicContext();
            // Add a new item
            InMemoryCacheEntry<T> entry = CreateCacheEntry(key, item, options);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Use the newer item
                if (Cache.TryGetValue(key, out InMemoryCacheEntry<T>? newer))
                {
                    if (disposeUnused && IsItemDisposable)
                        await item.TryDisposeAsync().DynamicContext();
                    return newer;
                }
                // Respect hard limits
                if (!isOverSize && HasHardLimits)
                    await ApplyHardLimits(entry, cancellationToken).DynamicContext();
                // Add and return
                if (isOverSize || Cache.TryAdd(key, entry))
                {
                    if (!isOverSize)
                        try
                        {
                            entry.OnAdded();
                        }
                        catch
                        {
                            if (TryRemove(key) is InMemoryCacheEntry<T> faulted && faulted != entry && IsItemDisposable)
                                await DisposeItemAsync(faulted.Item).DynamicContext();
                            entry.OnRemoved();
                            if (disposeUnused && IsItemDisposable)
                                await entry.Item.TryDisposeAsync().DynamicContext();
                            throw;
                        }
                    return entry;
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
                    if (TryRemove(key) is InMemoryCacheEntry<T> removed && IsItemDisposable)
                        DisposeItem(removed.Item);
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
            bool disposeItem = false;
            try
            {
                // Check item size
                bool isOverSize = Options.MaxItemSize > 0 && newEntry.Size > Options.MaxItemSize;
                if (isOverSize && IsItemAutoDisposer)
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
                    if (isOverSize || Cache.TryAdd(key, newEntry)) return newEntry;
                }
            }
            finally
            {
                // If the created item wasn't used, finally, dispose it
                if (disposeItem && IsItemDisposable)
                    newEntry.Item.TryDispose();
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
                    if (TryRemove(key) is InMemoryCacheEntry<T> removed && IsItemDisposable)
                        await DisposeItemAsync(removed.Item).DynamicContext();
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
            bool disposeItem = false;
            try
            {
                // Check item size
                bool isOverSize = Options.MaxItemSize > 0 && newEntry.Size > Options.MaxItemSize;
                if (isOverSize && IsItemAutoDisposer)
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
                        if (!isOverSize)
                            newEntry.OnAdded();
                        return newEntry;
                    }
                }
            }
            finally
            {
                // If the created item wasn't used, finally, dispose it
                if (disposeItem && IsItemDisposable)
                    await newEntry.Item.TryDisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Try removing an entry
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed entry (items are not yet disposed!)</returns>
        public virtual InMemoryCacheEntry<T>? TryRemove(in string key)
        {
            EnsureUndisposed(allowDisposing: true);
            if (!Cache.TryRemove(key, out InMemoryCacheEntry<T>? res))
                return null;
            res.OnRemoved();
            return res;
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <returns>Removed cache entries (items are not yet disposed!)</returns>
        public virtual InMemoryCacheEntry<T>[] Clear()
        {
            EnsureUndisposed(allowDisposing: true);
            return [.. Cache.Values.Select(e => TryRemove(e.Key)).Where(e => e is not null)];
        }

        /// <summary>
        /// Delegate for a cache entry factory
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="key">Entry key</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        public delegate Task<InMemoryCacheEntry<T>?> CacheEntryFactory_Delegate(
            InMemoryCache<T> cache, 
            string key, 
            InMemoryCacheEntryOptions? options, 
            CancellationToken cancellationToken
            );
    }
}
