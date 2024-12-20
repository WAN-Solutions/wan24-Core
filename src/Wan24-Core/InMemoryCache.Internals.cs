﻿namespace wan24.Core
{
    // Internals
    public partial class InMemoryCache<T>
    {
        /// <summary>
        /// If the cached items type is disposable (is the case also, if <see cref="object"/> was used as value for <c>T</c>)
        /// </summary>
        protected static readonly bool IsItemTypeDisposable;

        /// <summary>
        /// Cache (key is the entry key)
        /// </summary>
        protected readonly ConcurrentChangeTokenDictionary<string, InMemoryCacheEntry<T>> Cache;
        /// <summary>
        /// If cached items are disposable (is the case also, if <c>T</c> isn't sealed)
        /// </summary>
        protected readonly bool IsItemDisposable;
        /// <summary>
        /// If hard limits are configured
        /// </summary>
        protected readonly bool HasHardLimits;
        /// <summary>
        /// Exported user actions
        /// </summary>
        protected readonly UserActionInfo[] UserActions;
        /// <summary>
        /// Number of cache entries
        /// </summary>
        protected volatile int _Count = 0;

        /// <summary>
        /// Apply hard limits for a new entry
        /// </summary>
        /// <param name="entry">Cache entry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task ApplyHardLimits(InMemoryCacheEntry<T> entry, CancellationToken cancellationToken = default)
        {
            if (!HasHardLimits)
                return;
            long size = Options.HardSizeLimit > 0 ? Size : 0;
            for (
                int count = Options.HardCountLimit > 0 ? Count : 0;
                (Options.HardCountLimit > 0 && count >= Options.HardCountLimit) || (Options.HardSizeLimit > 0 && size + entry.Size > Options.HardSizeLimit);
                count = Options.HardCountLimit > 0 ? Count : 0, size = Options.HardSizeLimit > 0 ? Size : 0
                )
            {
                CancelToken.ThrowIfCancellationRequested();
                if (Options.HardCountLimit > 0 && count >= Options.HardCountLimit)
                    await ReduceCountAsync(Options.HardCountLimit - 1, cancellationToken).DynamicContext();
                if (Options.HardSizeLimit > 0 && Size + entry.Size > Options.HardSizeLimit)
                    await ReduceSizeAsync(Options.HardSizeLimit - entry.Size, cancellationToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected virtual InMemoryCacheEntry<T>? TryRemoveInt(in string key, in CacheEventReasons reason)
        {
            if (!Cache.TryRemove(key, out InMemoryCacheEntry<T>? res))
                return null;
            _Count--;
            res.OnRemoved(reason);
            HandleEntryRemoved(res);
            RaiseOnEntryRemoved(res, reason);
            return res;
        }

        /// <inheritdoc/>
        protected virtual bool RemoveInt(in InMemoryCacheEntry<T> entry, in CacheEventReasons reason)
        {
            if (entry.Cache != this)
                throw new ArgumentException("Foreign cache entry", nameof(entry));
            if (Cache.Remove(new KeyValuePair<string, InMemoryCacheEntry<T>>(entry.Key, entry)))
            {
                _Count--;
                entry.OnRemoved(reason);
                HandleEntryRemoved(entry);
                RaiseOnEntryRemoved(entry, reason);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dispose an item
        /// </summary>
        /// <param name="item">Item to dispose</param>
        protected virtual void DisposeItem(T item)
        {
            if (item is AutoDisposer<T> disposer)
            {
                disposer.ShouldDispose = true;
                return;
            }
            item.TryDispose();
        }

        /// <summary>
        /// Dispose an item
        /// </summary>
        /// <param name="item">Item to dispose</param>
        protected virtual async Task DisposeItemAsync(T item)
        {
            if (item is AutoDisposer<T> disposer)
            {
                await disposer.SetShouldDisposeAsync().DynamicContext();
                return;
            }
            await item.TryDisposeAsync().DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            while(EnsureNotCanceled(throwOnCancellation: false))
            {
                await Task.Delay((int)Options.TidyTimeout.TotalMilliseconds, CancelToken).DynamicContext();
                await TidyCacheAsync().DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ServiceWorkerTable.ServiceWorkers.TryRemove(GUID, out _);
            InMemoryCacheTable.Caches.TryRemove(GUID, out _);
            base.Dispose(disposing);
            Clear(disposeItems: true);
            Cache.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            ServiceWorkerTable.ServiceWorkers.TryRemove(GUID, out _);
            InMemoryCacheTable.Caches.TryRemove(GUID, out _);
            await base.DisposeCore().DynamicContext();
            await ClearAsync(disposeItems: true).DynamicContext();
            await Cache.DisposeAsync().DynamicContext();
        }
    }
}
