using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime;
using System.Runtime.CompilerServices;
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
    public partial class InMemoryCache<T> : HostedServiceBase, IInMemoryCache<T>
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static InMemoryCache() => IsItemTypeDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T)) ||
            typeof(IAsyncDisposable).IsAssignableFrom(typeof(T));

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
            if (cache is not null) _Count = cache.Count;
            IsItemAutoDisposer = typeof(T).HasBaseType(typeof(AutoDisposer<>));
            HasHardLimits = options.HardCountLimit > 0 || options.HardSizeLimit > 0;
            IsItemDisposable = !options.NeverDisposeItems && 
                (
                    (options.TryDisposeItemsAlways && !typeof(T).IsSealed) || 
                    typeof(IDisposable).IsAssignableFrom(typeof(T)) || 
                    typeof(IAsyncDisposable).IsAssignableFrom(typeof(T))
                );
            Options = options;
            UserActions = [.. this.GetUserActionInfos(GUID)];
            ServiceWorkerTable.ServiceWorkers[GUID] = this;
            InMemoryCacheTable.Caches[GUID] = this;
        }

        /// <inheritdoc/>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public InMemoryCacheOptions Options { get; }

        /// <inheritdoc/>
        public int Count
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                EnsureUndisposed();
                Contract.Assert(_Count >= 0);
                return _Count;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> Keys
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                EnsureUndisposed();
                return Cache.Keys;
            }
        }

        /// <summary>
        /// Cached items
        /// </summary>
        public IEnumerable<T> Items
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                EnsureUndisposed();
                return Cache.Values.Select(e => e.Item);
            }
        }

        /// <inheritdoc/>
        public long Size
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                EnsureUndisposed();
                return Cache.Values.Sum(e => e.Size);
            }
        }

        /// <inheritdoc/>
        public bool IsItemAutoDisposer { get; }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(UserActionInfo.STATE_KEY, UserActions);
                yield return new(__("Type"), GetType(), __("CLR type"));
                yield return new(__("Name"), Name, __("Name"));
                yield return new(__("Item type"), typeof(T), __("Cached item CLR type"));
                yield return new(__("Type disposable"), IsItemTypeDisposable, __("If the cached item type is or may be disposable"));
                yield return new(__("Disposing"), IsItemDisposable, __("If cached items will be disposed on removal, if possible"));
                yield return new(__("Pause"), CanPause, __("If the service worker can pause"));
                yield return new(__("Paused"), IsPaused, __("If the service worker is paused"));
                yield return new(__("Running"), IsRunning, __("If the service worker is running"));
                yield return new(__("Started"), Started == DateTime.MinValue ? __("never") : Started.ToString(), __("Last service start time"));
                yield return new(__("Stopped"), Stopped == DateTime.MinValue ? __("never") : Stopped.ToString(), __("Last service stop time"));
                yield return new(__("Management"), Options.DefaultStrategy, __("Default in-memory cache management strategy"));
                yield return new(__("Soft count limit"), Options.SoftCountLimit, __("Soft cached item count limit"));
                yield return new(__("Hard count limit"), Options.HardCountLimit, __("Hard cached item count limit"));
                yield return new(__("Soft size limit"), Options.SoftSizeLimit, __("Soft cached item total size limit"));
                yield return new(__("Hard size limit"), Options.HardSizeLimit, __("Hard cached item total size limit"));
                yield return new(__("Size limit"), Options.MaxItemSize, __("Maximum cached item size limit"));
                yield return new(__("Age limit"), Options.AgeLimit, __("Cached entry age limit"));
                yield return new(__("Idle limit"), Options.IdleLimit, __("Cached entry idle time limit"));
                yield return new(__("Memory limit"), Options.MaxMemoryUsage, __("App memory limit in bytes"));
                yield return new(__("Count"), Count, __("Number of cached items"));
                yield return new(__("Size"), Size, __("Current cached items total size"));
                yield return new(__("Tidy"), Options.TidyTimeout, __("Tidy interval"));
            }
        }

        /// <inheritdoc/>
        ICacheOptions ICache<T>.Options => Options;

        /// <inheritdoc/>
        public virtual InMemoryCacheEntryOptions EnsureEntryOptions(InMemoryCacheEntryOptions? options)
        {
            EnsureUndisposed();
            return options ?? (Options.DefaultEntryOptions is null ? new() : Options.DefaultEntryOptions with { });
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual InMemoryCacheEntry<T>? TryRemove(in string key)
        {
            EnsureUndisposed(allowDisposing: true);
            return TryRemoveInt(key, CacheEventReasons.UserAction);
        }

        /// <inheritdoc/>
        public virtual bool Remove(in InMemoryCacheEntry<T> entry)
        {
            EnsureUndisposed(allowDisposing: true);
            return RemoveInt(entry, CacheEventReasons.UserAction);
        }

        /// <inheritdoc/>
        public virtual InMemoryCacheEntry<T>[] Clear(in bool disposeItems = false)
        {
            EnsureUndisposed(allowDisposing: true);
            InMemoryCacheEntry<T>[] res = [.. Cache.Values.Select(e => TryRemoveInt(e.Key, CacheEventReasons.UserAction)).Where(e => e is not null)];
            if (disposeItems)
                foreach (InMemoryCacheEntry<T> entry in res)
                    DisposeItem(entry.Item);
            return res;
        }

        /// <inheritdoc/>
        [UserAction(), DisplayText("Clear"), Description("Clear the cache")]
        public virtual async Task<InMemoryCacheEntry<T>[]> ClearAsync(
            [DisplayText("Dispose items"), Description("If to dispose removed cached items")]
            bool disposeItems = false
            )
        {
            EnsureUndisposed(allowDisposing: true);
            InMemoryCacheEntry<T>[] res = [.. Cache.Values.Select(e => TryRemoveInt(e.Key, CacheEventReasons.UserAction)).Where(e => e is not null)];
            if (disposeItems)
                foreach (InMemoryCacheEntry<T> entry in res)
                    await DisposeItemAsync(entry.Item).DynamicContext();
            return res;
        }
    }
}
