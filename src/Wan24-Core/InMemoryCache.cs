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
            Type type = typeof(T);
            IsItemTypeDisposable = !type.IsSealed ||
                typeof(IDisposable).IsAssignableFrom(type) ||
                typeof(IAsyncDisposable).IsAssignableFrom(type);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        /// <param name="cache">Cache dictionary to use</param>
        public InMemoryCache(in InMemoryCacheOptions options, in ConcurrentChangeTokenDictionary<string, InMemoryCacheEntry<T>>? cache = null) : base()
        {
            // Validate options
            if (options.SoftCountLimit < 1 && options.SoftSizeLimit < 1 && options.AgeLimit <= TimeSpan.Zero && options.IdleLimit <= TimeSpan.Zero)
                throw new ArgumentException("Any limit (SoftCountLimit, SoftSizeLimit, AgeLimit, IdleLimit) is required", nameof(options));
            if (options.ConcurrencyLevel.HasValue && options.SoftCountLimit < 1 && options.HardCountLimit < 1)
                throw new ArgumentException("SoftCountLimit or HardCountLimit required, if ConcurrencyLevel was set", nameof(options));
            // Initialize this instance
            Cache = cache ?? (options.ConcurrencyLevel.HasValue
                ? new(options.ConcurrencyLevel.Value, Math.Max(options.SoftCountLimit, options.HardCountLimit))
                : new());
            Cache.Tag = this;
            IsItemAutoDisposer = typeof(T).HasBaseType(typeof(AutoDisposer<>));
            HasHardLimits = options.HardCountLimit > 0 || options.HardSizeLimit > 0;
            IsItemDisposable = !options.NeverDisposeItems && (options.TryDisposeItemsAlways || IsItemTypeDisposable);
            Options = options;
            TidyTimer = new(Options.TidyTimeout);
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
                    ErrorHandling.Handle(new("Tidy in-memory cache failed", ex, ErrorSource, this));
                }
                finally
                {
                    try
                    {
                        using SemaphoreSyncContext ssc = Sync;
                        if (!IsDisposing && IsRunning)
                            TidyTimer.Start();
                    }
                    catch
                    {
                        // Sync/TidyTimer may be disposed from another thread - just ignore that
                    }
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
        /// Ensure cache entry options
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Options</returns>
        public virtual InMemoryCacheEntryOptions EnsureEntryOptions(InMemoryCacheEntryOptions? options)
        {
            EnsureUndisposed();
            return options ?? (Options.DefaultEntryOptions is null ? new() : Options.DefaultEntryOptions with { });
        }

        /// <summary>
        /// Create a cache entry
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="item">Item</param>
        /// <param name="options">Options</param>
        /// <returns>Cache entry</returns>
        public virtual InMemoryCacheEntry<T> CreateEntry(in string key, in T item, InMemoryCacheEntryOptions? options = null)
        {
            EnsureUndisposed();
            options = EnsureEntryOptions(options);
            return new(key, item, (item as IInMemoryCacheItem)?.Size ?? options.Size)
            {
                Cache = this,
                ObserveDisposing = options.ObserveDisposing ?? InMemoryCacheOptions.DefaultObserveItemDisposing,
                Type = options.Type ?? InMemoryCacheOptions.DefaultEntryType,
                Timeout = options.Timeout ?? InMemoryCacheOptions.DefaultEntryTimeout,
                IsSlidingTimeout = options.IsSlidingTimeout ?? InMemoryCacheOptions.DefaultEntrySlidingTimeout,
                AbsoluteTimeout = options.AbsoluteTimeout
            };
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
        /// Remove an entry
        /// </summary>
        /// <param name="entry">Entry to remove</param>
        /// <returns>If removed</returns>
        public virtual bool Remove(in InMemoryCacheEntry<T> entry)
        {
            EnsureUndisposed(allowDisposing: true);
            if (entry.Cache != this)
                throw new ArgumentException("Foreign cache entry", nameof(entry));
            if (Cache.Remove(new KeyValuePair<string, InMemoryCacheEntry<T>>(entry.Key, entry)))
            {
                entry.OnRemoved();
                return true;
            }
            return false;
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
