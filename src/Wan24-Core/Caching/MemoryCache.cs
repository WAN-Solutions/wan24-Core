namespace wan24.Core.Caching
{
    /// <summary>
    /// Memory cache
    /// </summary>
    public class MemoryCache : CacheBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tidyInterval">Tidy interval in ms</param>
        public MemoryCache(double tidyInterval = 0) : base(tidyInterval) { }

        /// <inheritdoc/>
        protected override CacheItemBase CacheItemFactory<T>(string key, Func<ICache, T> factory, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
            => new MemoryCacheItem(this, key, factory(this)!, timeout, expires, timespan);

        /// <summary>
        /// Cache item
        /// </summary>
        public class MemoryCacheItem : CacheItemBase
        {
            /// <summary>
            /// Value
            /// </summary>
            protected object _Value = null!;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">Cache</param>
            /// <param name="key">Key</param>
            /// <param name="value">Cached value</param>
            /// <param name="timeout">Timeout</param>
            /// <param name="expires">Expires</param>
            /// <param name="timespan">Timespan</param>
            public MemoryCacheItem(ICache cache, string key, object value, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
                : base(cache, key, value, timeout, expires, timespan)
            { }

            /// <summary>
            /// Cached value
            /// </summary>
            public virtual object Value
            {
                get
                {
                    lock (SyncObject)
                    {
                        Accessed++;
                        if (CacheTimeout == CacheTimeouts.Sliding) Expires = DateTime.Now + Timeout!.Value;
                    }
                    return _Value;
                }
            }

            /// <summary>
            /// Number of times the value has been accessed
            /// </summary>
            public long Accessed { get; protected set; }

            /// <inheritdoc/>
            public override object GetValue() => Value;

            /// <inheritdoc/>
            public override ICacheItem SetValue(object value, CacheTimeouts? timeout = null, DateTime? expires = null, TimeSpan? timespan = null)
            {
                lock (SyncObject)
                {
                    base.SetValue(value, timeout, expires, timespan);
                    _Value = value;
                }
                return this;
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (Value is IDisposable disposable) disposable.Dispose();
                else if (Value is IAsyncDisposable asyncDisposable) asyncDisposable.DisposeAsync().AsTask().Wait();
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                await base.DisposeCore().DynamicContext();
                if (Value is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync().DynamicContext();
                else if (Value is IDisposable disposable) disposable.Dispose();
            }
        }
    }
}
