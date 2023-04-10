using System.Text;

namespace wan24.Core.Caching
{
    /// <summary>
    /// JSON file cache
    /// </summary>
    public class JsonFileCache : ByteFileCache
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="tidyInterval">Tidy interval in ms</param>
        public JsonFileCache(string folder, double tidyInterval = 0) : base(folder, tidyInterval) { }

        /// <inheritdoc/>
        protected override CacheItemBase CacheItemFactory<T>(string key, Func<ICache, T> factory, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
            => new JsonFileCacheItem(this, key, factory(this)!, timeout, expires, timespan);

        /// <inheritdoc/>
        protected override async Task<AsyncCacheItemBase> CacheItemFactoryAsync<T>(
            string key,
            Func<IAsyncCache, T> factory,
            CacheTimeouts timeout,
            DateTime? expires = null,
            TimeSpan? timespan = null,
            CancellationToken cancellationToken = default
            )
        {
            await Task.Yield();
            return new JsonFileCacheItem(this, key, factory(this)!, timeout, expires, timespan);
        }


        /// <inheritdoc/>
        protected override async Task<AsyncCacheItemBase> CacheItemFactoryAsync<T>(
            string key,
            Func<IAsyncCache, CancellationToken, Task<T>> factory,
            CacheTimeouts timeout,
            DateTime? expires = null,
            TimeSpan? timespan = null,
            CancellationToken cancellationToken = default
            )
        {
            JsonFileCacheItem res = new(this, key);
            await res.SetValueAsync((await factory(this, cancellationToken).DynamicContext())!, timeout, expires, timespan, cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Cache item
        /// </summary>
        public class JsonFileCacheItem : ByteFileCacheItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">Cache</param>
            /// <param name="key">Key</param>
            /// <param name="value">Cached value</param>
            /// <param name="timeout">Timeout</param>
            /// <param name="expires">Expires</param>
            /// <param name="timespan">Timespan</param>
            public JsonFileCacheItem(IAsyncCache cache, string key, object value, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
                : base(cache, key, value, timeout, expires, timespan)
            {
                if (cache is not JsonFileCache) throw new ArgumentException("Invalid cache type", nameof(cache));
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">Cache</param>
            /// <param name="key">Key</param>
            public JsonFileCacheItem(IAsyncCache cache, string key) : base(cache, key)
            {
                if (cache is not JsonFileCache) throw new ArgumentException("Invalid cache type", nameof(cache));
            }

            /// <summary>
            /// Value type
            /// </summary>
            public Type ValueType { get; protected set; } = null!;

            /// <inheritdoc/>
            public override object GetValue()
                => JsonHelper.DecodeObject(ValueType, Encoding.UTF8.GetString((byte[])base.GetValue()))
                    ?? throw new InvalidDataException($"Failed to decode {ValueType} from {FileName}");

            /// <inheritdoc/>
            public override ICacheItem SetValue(object value, CacheTimeouts? timeout = null, DateTime? expires = null, TimeSpan? timespan = null)
            {
                base.SetValue(Encoding.UTF8.GetBytes(JsonHelper.Encode(value)), timeout, expires, timespan);
                ValueType = value.GetType();
                return this;
            }

            /// <inheritdoc/>
            public override async Task<object> GetValueAsync(CancellationToken cancellationToken = default)
                => JsonHelper.DecodeObject(ValueType, Encoding.UTF8.GetString((byte[])await base.GetValueAsync(cancellationToken).DynamicContext()))
                    ?? throw new InvalidDataException($"Failed to decode {ValueType} from {FileName}");

            /// <inheritdoc/>
            public override async Task SetValueAsync(
                object value, 
                CacheTimeouts? timeout = null, 
                DateTime? expires = null, 
                TimeSpan? timespan = null, 
                CancellationToken cancellationToken = default
                )
            {
                await base.SetValueAsync(Encoding.UTF8.GetBytes(JsonHelper.Encode(value)), timeout, expires, timespan, cancellationToken).DynamicContext();
                ValueType = value.GetType();
            }
        }
    }
}
