namespace wan24.Core.Caching
{
    /// <summary>
    /// Base class for an asynchronous cache
    /// </summary>
    public abstract class AsyncCacheBase : CacheBase, IAsyncCache
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tidyInterval">Tidy interval in ms</param>
        protected AsyncCacheBase(double tidyInterval = 0) : base(tidyInterval) { }

        /// <inheritdoc/>
        public virtual async Task<T> GetOrAddAsync<T>(
            string key, 
            Func<IAsyncCache, T> factory, 
            CacheTimeouts timeout, 
            DateTime? expires = null, 
            TimeSpan? timespan = null, 
            CancellationToken cancellationToken = default
            )
        {
            while (true)
            {
                if (Items.TryGetValue(key, out CacheItemBase? item))
                    return (T)await ((AsyncCacheItemBase)item).GetValueAsync(cancellationToken).DynamicContext();
                item = await CacheItemFactoryAsync(key, factory, timeout, expires, timespan, cancellationToken).DynamicContext();
                if (Items.TryAdd(key, item))
                {
                    RaiseOnAdded(item);
                    return (T)await ((AsyncCacheItemBase)item).GetValueAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    await item.DisposeAsync().DynamicContext();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetOrAddAsync<T>(
            string key, 
            Func<IAsyncCache, CancellationToken, Task<T>> factory, 
            CacheTimeouts timeout, 
            DateTime? expires = null, 
            TimeSpan? timespan = null, 
            CancellationToken cancellationToken = default
            )
        {
            while (true)
            {
                if (Items.TryGetValue(key, out CacheItemBase? item))
                    return (T)await((AsyncCacheItemBase)item).GetValueAsync(cancellationToken).DynamicContext();
                item = await CacheItemFactoryAsync(key, factory, timeout, expires, timespan, cancellationToken).DynamicContext();
                if (Items.TryAdd(key, item))
                {
                    RaiseOnAdded(item);
                    return (T)await((AsyncCacheItemBase)item).GetValueAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    await item.DisposeAsync().DynamicContext();
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!Items.TryRemove(key, out CacheItemBase? item)) return;
            RaiseOnRemoved(item);
            await ((AsyncCacheItemBase)item).DisposeAsync().DynamicContext();
        }

        /// <inheritdoc/>
        public virtual async Task<object?> GetAndRemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!Items.TryRemove(key, out CacheItemBase? item)) return null;
            object res = await ((AsyncCacheItemBase)item).GetValueAsync(cancellationToken).DynamicContext();
            RaiseOnRemoved(item);
            await ((AsyncCacheItemBase)item).DisposeAsync().DynamicContext();
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<T?> GetAndRemoveAsync<T>(string key, CancellationToken cancellationToken = default) => (T?)await GetAndRemoveAsync(key, cancellationToken).DynamicContext();

        /// <summary>
        /// Cache item factory
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="factory">Object factory</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="expires">Expires</param>
        /// <param name="timespan">Timespan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache item</returns>
        protected abstract Task<AsyncCacheItemBase> CacheItemFactoryAsync<T>(
            string key, 
            Func<IAsyncCache, T> factory, 
            CacheTimeouts timeout, 
            DateTime? expires = null, 
            TimeSpan? timespan = null, 
            CancellationToken cancellationToken = default
            );

        /// <summary>
        /// Cache item factory
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="factory">Object factory</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="expires">Expires</param>
        /// <param name="timespan">Timespan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache item</returns>
        protected abstract Task<AsyncCacheItemBase> CacheItemFactoryAsync<T>(
            string key,
            Func<IAsyncCache, CancellationToken, Task<T>> factory,
            CacheTimeouts timeout,
            DateTime? expires = null,
            TimeSpan? timespan = null,
            CancellationToken cancellationToken = default
            );

        /// <inheritdoc/>
        protected override async void Tidy()
        {
            await Task.Yield();
            foreach (string key in (from item in Items.Values
                                    where item.CacheTimeout != CacheTimeouts.None &&
                                        item.Expires <= DateTime.Now
                                    select item.Key).ToArray())
                await RemoveAsync(key).DynamicContext();
            lock (DisposeSyncObject)
                lock (SyncObject)
                    if (!IsDisposing && TidyTimer.Interval > 0)
                        TidyTimer.Start();
        }

        /// <summary>
        /// Base class for an asynchronous cache item
        /// </summary>
        public abstract class AsyncCacheItemBase : CacheItemBase
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
            protected AsyncCacheItemBase(ICache cache, string key, object value, CacheTimeouts timeout, DateTime? expires = null, TimeSpan? timespan = null)
                : base(cache,key,value,timeout,expires,timespan)
            { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cache">Cache</param>
            /// <param name="key">Key</param>
            protected AsyncCacheItemBase(ICache cache, string key) : base(cache, key) { }

            /// <summary>
            /// Get the value
            /// </summary>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Value</returns>
            public abstract Task<object> GetValueAsync(CancellationToken cancellationToken = default);

            /// <summary>
            /// Set the value
            /// </summary>
            /// <param name="value">Value</param>
            /// <param name="timeout">Timeout</param>
            /// <param name="expires">Expires</param>
            /// <param name="timespan">Timespan</param>
            /// <param name="cancellationToken">Cancellation token</param>
            public virtual Task SetValueAsync(object value, CacheTimeouts? timeout = null, DateTime? expires = null, TimeSpan? timespan = null, CancellationToken cancellationToken = default)
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
                return Task.CompletedTask;
            }
        }
    }
}
