namespace wan24.Core
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
        /// Tidy cache timer
        /// </summary>
        protected readonly Timeout TidyTimer;
        /// <summary>
        /// If cached items are disposable (is the case also, if <c>T</c> isn't sealed)
        /// </summary>
        protected readonly bool IsItemDisposable;
        /// <summary>
        /// If hard limits are configured
        /// </summary>
        protected readonly bool HasHardLimits;

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
            InMemoryCacheTable.Caches.TryRemove(GUID, out _);
            base.Dispose(disposing);
            Clear().Select(e => e.Item).Cast<object>().TryDisposeAll();
            Cache.Dispose();
            TidyTimer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            InMemoryCacheTable.Caches.TryRemove(GUID, out _);
            await base.DisposeCore().DynamicContext();
            await Clear().Select(e => e.Item).Cast<object>().TryDisposeAllAsync().DynamicContext();
            await Cache.DisposeAsync().DynamicContext();
            await TidyTimer.DisposeAsync().DynamicContext();
        }
    }
}
