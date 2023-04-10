namespace wan24.Core.Caching
{
    /// <summary>
    /// File cache
    /// </summary>
    public class ByteFileCache : AsyncCacheBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="tidyInterval">Tidy interval in ms</param>
        public ByteFileCache(string folder, double tidyInterval = 0) : base(tidyInterval) => Folder = folder;

        /// <summary>
        /// Folder
        /// </summary>
        public string Folder { get; }

        /// <inheritdoc/>
        protected override CacheItemBase CacheItemFactory<T>(string key, Func<ICache, T> factory, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
            => new ByteFileCacheItem(this, key, factory(this)!, timeout, expires, timespan);

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
            return new ByteFileCacheItem(this, key, factory(this)!, timeout, expires, timespan);
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
            ByteFileCacheItem res = new(this, key);
            await res.SetValueAsync((await factory(this, cancellationToken).DynamicContext())!, timeout, expires, timespan, cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Cache item
        /// </summary>
        public class ByteFileCacheItem : AsyncCacheItemBase
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
            public ByteFileCacheItem(IAsyncCache cache, string key, object value, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
                : base(cache, key, value, timeout, expires, timespan)
            {
                if (cache is not ByteFileCache) throw new ArgumentException("Invalid cache type", nameof(cache));
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">Cache</param>
            /// <param name="key">Key</param>
            public ByteFileCacheItem(IAsyncCache cache, string key) : base(cache, key)
            {
                if (cache is not ByteFileCache) throw new ArgumentException("Invalid cache type", nameof(cache));
            }

            /// <summary>
            /// GUID
            /// </summary>
            public string GUID { get; } = Guid.NewGuid().ToString();

            /// <summary>
            /// File cache
            /// </summary>
            public ByteFileCache FileCache => (ByteFileCache)Cache;

            /// <summary>
            /// Filename
            /// </summary>
            public string FileName => Path.Combine(FileCache.Folder, GUID);

            /// <summary>
            /// Number of times the value has been accessed
            /// </summary>
            public long Accessed { get; protected set; }

            /// <inheritdoc/>
            public override object GetValue()
            {
                EnsureUndisposed();
                object res = File.ReadAllBytes(FileName);
                lock (SyncObject)
                {
                    Accessed++;
                    if (CacheTimeout == CacheTimeouts.Sliding) Expires = DateTime.Now + Timeout!.Value;
                }
                return res;
            }

            /// <inheritdoc/>
            public override ICacheItem SetValue(object value, CacheTimeouts? timeout = null, DateTime? expires = null, TimeSpan? timespan = null)
            {
                if (value is not byte[] bytes) throw new ArgumentException("Byte array required", nameof(value));
                base.SetValue(value, timeout, expires, timespan);
                File.WriteAllBytes(FileName, bytes);
                return this;
            }

            /// <inheritdoc/>
            public override async Task<object> GetValueAsync(CancellationToken cancellationToken = default)
            {
                EnsureUndisposed();
                object res = await File.ReadAllBytesAsync(FileName, cancellationToken).DynamicContext();
                lock (SyncObject)
                {
                    Accessed++;
                    if (CacheTimeout == CacheTimeouts.Sliding) Expires = DateTime.Now + Timeout!.Value;
                }
                return res;
            }

            /// <inheritdoc/>
            public override async Task SetValueAsync(
                object value,
                CacheTimeouts? timeout = null,
                DateTime? expires = null,
                TimeSpan? timespan = null,
                CancellationToken cancellationToken = default
                )
            {
                if (value is not byte[] bytes) throw new ArgumentException("Byte array required", nameof(value));
                await base.SetValueAsync(value, timeout, expires, timespan, cancellationToken).DynamicContext();
                await File.WriteAllBytesAsync(FileName, bytes, cancellationToken).DynamicContext();
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (File.Exists(FileName)) File.Delete(FileName);
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                await base.DisposeCore().DynamicContext();
                if (File.Exists(FileName)) File.Delete(FileName);
            }
        }
    }
}
