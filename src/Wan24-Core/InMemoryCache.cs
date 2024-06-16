using System.ComponentModel;
using static wan24.Core.TranslationHelper;

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
    public partial class InMemoryCache<T> : HostedServiceBase, IInMemoryCache
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
            InMemoryCacheTable.Caches[GUID] = this;
        }

        /// <inheritdoc/>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public InMemoryCacheOptions Options { get; }

        /// <inheritdoc/>
        public int Count => Cache.Count;

        /// <inheritdoc/>
        public IEnumerable<string> Keys => Cache.Keys;

        /// <summary>
        /// Cached items
        /// </summary>
        public IEnumerable<T> Items => Cache.Values.Select(e => e.Item);

        /// <inheritdoc/>
        public long Size => Cache.Values.Sum(e => e.Size);

        /// <inheritdoc/>
        public bool IsItemAutoDisposer { get; }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("Type"), GetType(), __("CLR type"));
                yield return new(__("Name"), Name, __("Name"));
                yield return new(__("Item type"), typeof(T), __("Cached item CLR type"));
                yield return new(__("Tidy"), Options.TidyTimeout, __("Tidy timer interval"));
                yield return new(__("Management"), Options.DefaultStrategy, __("Default in-memory cache management strategy"));
                yield return new(__("Soft count limit"), Options.SoftCountLimit, __("Soft cached item count limit"));
                yield return new(__("Hard count limit"), Options.HardCountLimit, __("Hard cached item count limit"));
                yield return new(__("Soft size limit"), Options.SoftSizeLimit, __("Soft cached item size limit"));
                yield return new(__("Hard size limit"), Options.HardSizeLimit, __("Hard cached item size limit"));
                yield return new(__("Size limit"), Options.MaxItemSize, __("Maximum cached item size limit"));
                yield return new(__("Age limit"), Options.AgeLimit, __("Cached entry age limit"));
                yield return new(__("Idle limit"), Options.IdleLimit, __("Cached entry idle time limit"));
                yield return new(__("Memory limit"), Options.MaxMemoryUsage, __("App memory limit in bytes"));
                yield return new(__("Dispose items"), Options.TryDisposeItemsAlways, __("If to dispose cached items on removal always"));
                yield return new(__("Never dispose items"), Options.NeverDisposeItems, __("If to never dispose cached items on removal"));
                yield return new(__("Count"), Count, __("Number of cached items"));
                yield return new(__("Size"), Size, __("Current cached items total size"));
                yield return new(__("Started"), Started, __("Service start time"));
                yield return new(__("Paused"), Paused, __("Service pause time"));
                yield return new(__("Stopped"), Stopped, __("Service stopped time"));
                yield return new(__("Exception"), LastException, __("Last service exception"));
                yield return new(__("Next tidy run"), TidyTimer.RemainingTime, __("Remaining time until the next cache tidy process"));
            }
        }

        /// <inheritdoc/>
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
        /// <param name="disposeItems">Dispose the items?</param>
        /// <returns>Removed cache entries (items are not yet disposed!)</returns>
        public virtual InMemoryCacheEntry<T>[] Clear(in bool disposeItems = false)
        {
            EnsureUndisposed(allowDisposing: true);
            InMemoryCacheEntry<T>[] res = [.. Cache.Values.Select(e => TryRemove(e.Key)).Where(e => e is not null)];
            if (disposeItems)
                foreach (InMemoryCacheEntry<T> entry in res)
                    DisposeItem(entry.Item);
            return res;
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        /// <param name="disposeItems">Dispose the items?</param>
        /// <returns>Removed cache entries (items are not yet disposed!)</returns>
        [UserAction(), DisplayText("Clear"), Description("Clear the cache")]
        public virtual async Task<InMemoryCacheEntry<T>[]> ClearAsync(
            [DisplayText("Dispose items"), Description("If to dispose removed cached items")]
            bool disposeItems = false
            )
        {
            EnsureUndisposed(allowDisposing: true);
            InMemoryCacheEntry<T>[] res = [.. Cache.Values.Select(e => TryRemove(e.Key)).Where(e => e is not null)];
            if (disposeItems)
                foreach (InMemoryCacheEntry<T> entry in res)
                    await DisposeItemAsync(entry.Item).DynamicContext();
            return res;
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
