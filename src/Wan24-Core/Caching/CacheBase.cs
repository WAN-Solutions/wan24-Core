using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core.Caching
{
    /// <summary>
    /// Memory cache
    /// </summary>
    public abstract class CacheBase : DisposableBase, ICache
    {
        /// <summary>
        /// Cached items
        /// </summary>
        protected readonly ConcurrentDictionary<string, CacheItemBase> Items = new();
        /// <summary>
        /// Tidy timer
        /// </summary>
        protected readonly System.Timers.Timer TidyTimer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tidyInterval">Tidy interval in ms</param>
        protected CacheBase(double tidyInterval = 0) : base()
        {
            TidyTimer = new()
            {
                AutoReset = false
            };
            TidyTimer.Elapsed += (s, e) => Tidy();
            TidyInterval = tidyInterval;
            if (tidyInterval > 0) TidyTimer.Start();
        }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Tidy interval in ms (zero to disable)
        /// </summary>
        public double TidyInterval
        {
            get => IfUndisposed(TidyTimer.Interval);
            set
            {
                lock (DisposeSyncObject)
                {
                    EnsureUndisposed();
                    lock (SyncObject)
                    {
                        bool running = TidyTimer.Enabled;
                        TidyTimer.Stop();
                        TidyTimer.Interval = value;
                        if (running && value > 0) TidyTimer.Start();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public int Count => Items.Count;

        /// <inheritdoc/>
        public string[] Keys => Items.Keys.ToArray();

        /// <inheritdoc/>
        public bool Exists(string key) => IfUndisposed(() => Items.ContainsKey(key));

        /// <inheritdoc/>
        public virtual T GetOrAdd<T>(string key, Func<ICache, T> factory, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
            => (T)IfUndisposed(() => Items.GetOrAdd(key, (key) =>
            {
                CacheItemBase item = CacheItemFactory(key, factory, timeout, expires, timespan);
                RaiseOnAdded(item);
                return item;
            }).GetValue());

        /// <inheritdoc/>
        public virtual ICache Remove(string key)
        {
            EnsureUndisposed(allowDisposing: true);
            Items.Remove(key, out CacheItemBase? item);
            if (item != null) RaiseOnRemoved(item);
            return this;
        }

        /// <inheritdoc/>
        public virtual object? GetAndRemove(string key)
        {
            EnsureUndisposed(allowDisposing: true);
            Items.Remove(key, out CacheItemBase? item);
            if (item != null) RaiseOnRemoved(item);
            return item?.GetValue();
        }

        /// <inheritdoc/>
        public virtual T? GetAndRemove<T>(string key) => (T?)Remove(key);

        /// <inheritdoc/>
        public bool TryGetItem(string key, [MaybeNullWhen(false)] out ICacheItem? item)
        {
            bool res = Items.TryGetValue(key, out CacheItemBase? i);
            item = i;
            return res;
        }

        /// <inheritdoc/>
        public virtual bool TryRemoveItem(string key, [MaybeNullWhen(false)] out ICacheItem? item)
        {
            bool res = Items.TryRemove(key, out CacheItemBase? i);
            item = i;
            if (i != null) RaiseOnRemoved(i);
            return res;
        }

        /// <summary>
        /// Cache item factory
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="factory">Object factory</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="expires">Expires</param>
        /// <param name="timespan">Timespan</param>
        /// <returns>Cache item</returns>
        protected abstract CacheItemBase CacheItemFactory<T>(string key, Func<ICache, T> factory, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null);

        /// <summary>
        /// Tidy the cache
        /// </summary>
        protected virtual void Tidy()
        {
            foreach (string key in (from item in Items.Values
                                    where item.CacheTimeout != CacheTimeouts.None &&
                                        item.Expires <= DateTime.Now
                                    select item.Key).ToArray())
                Remove(key);
            lock (DisposeSyncObject)
                lock (SyncObject)
                    if (!IsDisposing && TidyTimer.Interval > 0)
                        TidyTimer.Start();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (SyncObject)
            {
                TidyTimer.Stop();
                TidyTimer.Dispose();
            }
            Items.Values.DisposeAll();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            lock (SyncObject)
            {
                TidyTimer.Stop();
                TidyTimer.Dispose();
            }
            await Items.Values.DisposeAllAsync().DynamicContext();
        }

        /// <inheritdoc/>
        public event ICache.CacheItem_Delegate? OnAdded;
        /// <summary>
        /// Raise the <see cref="OnAdded"/> event
        /// </summary>
        /// <param name="item">Item</param>
        protected virtual void RaiseOnAdded(ICacheItem item) => OnAdded?.Invoke(this, new(item));

        /// <inheritdoc/>
        public event ICache.CacheItem_Delegate? OnRemoved;
        /// <summary>
        /// Raise the <see cref="OnRemoved"/> event
        /// </summary>
        /// <param name="item">Item</param>
        protected virtual void RaiseOnRemoved(ICacheItem item) => OnRemoved?.Invoke(this, new(item));

        /// <summary>
        /// Base class for a cache item
        /// </summary>
        public abstract class CacheItemBase : DisposableBase, ICacheItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">Cache</param>
            /// <param name="key">Key</param>
            protected CacheItemBase(ICache cache, string key) : base()
            {
                Cache = cache;
                Key = key;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">Cache</param>
            /// <param name="key">Key</param>
            /// <param name="value">Cached value</param>
            /// <param name="timeout">Timeout</param>
            /// <param name="expires">Expires</param>
            /// <param name="timespan">Timespan</param>
            protected CacheItemBase(ICache cache, string key, object value, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null) : this(cache, key)
                => SetValue(value, timeout, expires, timespan);

            /// <summary>
            /// An object for thread synchronization
            /// </summary>
            public object SyncObject { get; } = new();

            /// <inheritdoc/>
            public ICache Cache { get; }

            /// <inheritdoc/>
            public string Key { get; }

            /// <inheritdoc/>
            public DateTime Added { get; } = DateTime.Now;

            /// <inheritdoc/>
            public DateTime Updated { get; protected set; }

            /// <inheritdoc/>
            public CacheTimeouts CacheTimeout { get; protected set; }

            /// <inheritdoc/>
            public DateTime Expires { get; protected set; }

            /// <inheritdoc/>
            public TimeSpan? Timeout { get; protected set; }

            /// <summary>
            /// Get the value
            /// </summary>
            /// <returns>Value</returns>
            public abstract object GetValue();

            /// <summary>
            /// Set the value
            /// </summary>
            /// <param name="value">Value</param>
            /// <param name="timeout">Timeout</param>
            /// <param name="expires">Expires</param>
            /// <param name="timespan">Timespan</param>
            /// <returns>This</returns>
            public virtual ICacheItem SetValue(object value, CacheTimeouts? timeout = null, DateTime? expires = null, TimeSpan? timespan = null)
            {
                EnsureUndisposed();
                if (timeout != null)
                {
                    switch (timeout.Value)
                    {
                        case CacheTimeouts.Fixed:
                            if (expires == null) throw new ArgumentNullException(nameof(expires));
                            Expires = expires.Value;
                            break;
                        case CacheTimeouts.Sliding:
                            if (timespan == null) throw new ArgumentNullException(nameof(timespan));
                            Timeout = timespan.Value;
                            break;
                    }
                    CacheTimeout = timeout.Value;
                }
                if (CacheTimeout == CacheTimeouts.Sliding) Expires = DateTime.Now + Timeout!.Value;
                return this;
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing) => Cache.Remove(Key);

            /// <inheritdoc/>
            protected override Task DisposeCore()
            {
                Cache.Remove(Key);
                return Task.CompletedTask;
            }
        }
    }
}
