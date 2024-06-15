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
        /// If cached items are disposable (is the case also, if <see cref="object"/> was used as value for <c>T</c>)
        /// </summary>
        protected readonly bool IsItemDisposable;
        /// <summary>
        /// Tidy cache timer
        /// </summary>
        protected readonly Timeout TidyTimer;
        /// <summary>
        /// If hard limits are configured
        /// </summary>
        protected readonly bool HasHardLimits;

        /// <summary>
        /// Create a cache entry
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="item">Item</param>
        /// <param name="options">Options</param>
        /// <returns>Cache entry</returns>
        protected virtual InMemoryCacheEntry<T> CreateCacheEntry(in string key, in T item, in InMemoryCacheEntryOptions options)
            => new(key, item, (item as IInMemoryCacheItem)?.Size ?? options.Size, options.Timeout ?? InMemoryCacheOptions.DefaultEntryTimeout)
            {
                Cache = this,
                ObserveDisposing = options.ObserveDisposing ?? InMemoryCacheOptions.DefaultObserveItemDisposing,
                Type = options.Type ?? InMemoryCacheOptions.DefaultEntryType,
                IsSlidingTimeout = options.IsSlidingTimeout ?? InMemoryCacheOptions.DefaultEntrySlidingTimeout,
                AbsoluteTimeout = options.AbsoluteTimeout
            };

        /// <summary>
        /// Apply hard limits for a new entry
        /// </summary>
        /// <param name="entry">Cache entry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task ApplyHardLimits(InMemoryCacheEntry<T> entry, CancellationToken cancellationToken = default)
        {
            while (Options.HardCountLimit > 0 && Cache.Count >= Options.HardCountLimit)
                await ReduceCountAsync(Options.HardCountLimit - 1, cancellationToken).DynamicContext();
            while (Options.HardSizeLimit > 0 && Size + entry.Size > Options.HardSizeLimit)
                await ReduceSizeAsync(Options.HardSizeLimit - entry.Size, cancellationToken).DynamicContext();
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
        protected override async Task WorkerAsync() => await CancelToken.WaitHandle.WaitAsync().DynamicContext();

        /// <inheritdoc/>
        protected override async Task StartingAsync(CancellationToken cancellationToken)
        {
            TidyTimer.Start();
            await base.StartingAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task StoppingAsync(CancellationToken cancellationToken)
        {
            TidyTimer.Stop();
            await base.StoppingAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Clear().Select(e => e.Item).Cast<object>().TryDisposeAll();
            Cache.Dispose();
            TidyTimer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await Clear().Select(e => e.Item).Cast<object>().TryDisposeAllAsync().DynamicContext();
            await Cache.DisposeAsync().DynamicContext();
            await TidyTimer.DisposeAsync().DynamicContext();
        }
    }
}
