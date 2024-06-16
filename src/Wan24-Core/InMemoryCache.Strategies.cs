using System.ComponentModel;

namespace wan24.Core
{
    // Strategies
    public partial class InMemoryCache<T>
    {
        /// <summary>
        /// Tidy strategy instance
        /// </summary>
        protected IInMemoryCacheStrategy<T>? TidyStrategyInstance = null;
        /// <summary>
        /// Age strategy instance
        /// </summary>
        protected IInMemoryCacheStrategy<T>? AgeStrategyInstance = null;
        /// <summary>
        /// Access time strategy instance
        /// </summary>
        protected IInMemoryCacheStrategy<T>? AccessTimeStrategyInstance = null;
        /// <summary>
        /// Larger strategy instance
        /// </summary>
        protected IInMemoryCacheStrategy<T>? LargerStrategyInstance = null;
        /// <summary>
        /// Smaller strategy instance
        /// </summary>
        protected IInMemoryCacheStrategy<T>? SmallerStrategyInstance = null;

        /// <inheritdoc/>
        public virtual void ReduceCount(int targetCount, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Count <= targetCount)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry))
                {
                    if (IsItemDisposable)
                        DisposeItem(entry.Item);
                    if (Count <= targetCount)
                        return;
                }
            }
        }

        /// <inheritdoc/>
        [UserAction(), DisplayText("Reduce count"), Description("Reduce the cached entries to a target count")]
        public virtual async Task ReduceCountAsync(
            [DisplayText("Target"), Description("Target count")]
            int targetCount, 
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            if (Count <= targetCount)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry))
                {
                    if (IsItemDisposable)
                        await DisposeItemAsync(entry.Item).DynamicContext();
                    if (Count <= targetCount)
                        return;
                }
            }
        }

        /// <inheritdoc/>
        public virtual void ReduceSize(long targetSize, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Size <= targetSize)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry))
                {
                    if (IsItemDisposable)
                        DisposeItem(entry.Item); ;
                    if (Size <= targetSize)
                        return;
                }
            }
        }

        /// <inheritdoc/>
        [UserAction(), DisplayText("Reduce size"), Description("Reduce the cached entries to a target size")]
        public virtual async Task ReduceSizeAsync(
            [DisplayText("Target"), Description("Target size")]
            long targetSize, 
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            if (Size <= targetSize)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry))
                {
                    if (IsItemDisposable)
                        await DisposeItemAsync(entry.Item).DynamicContext();
                    if (Size <= targetSize)
                        return;
                }
            }
        }

        /// <inheritdoc/>
        public virtual void ReduceOld(TimeSpan maxAge, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in Cache.Values.Where(e => e.Type != InMemoryCacheEntryTypes.Persistent && e.Age > maxAge).OrderByDescending(e => e.Age))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry) && IsItemDisposable)
                    DisposeItem(entry.Item);
            }
        }

        /// <inheritdoc/>
        public virtual async Task ReduceOldAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in Cache.Values.Where(e => e.Type != InMemoryCacheEntryTypes.Persistent && e.Age > maxAge).OrderByDescending(e => e.Age))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry) && IsItemDisposable)
                    await DisposeItemAsync(entry.Item).DynamicContext();
            }
        }

        /// <inheritdoc/>
        public virtual void ReduceUnpopular(TimeSpan maxIdle, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in Cache.Values.Where(e => e.Type != InMemoryCacheEntryTypes.Persistent && e.Idle > maxIdle).OrderByDescending(e => e.Idle))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry) && IsItemDisposable)
                    DisposeItem(entry.Item);
            }
        }

        /// <inheritdoc/>
        public virtual async Task ReduceUnpopularAsync(TimeSpan maxIdle, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in Cache.Values.Where(e => e.Type != InMemoryCacheEntryTypes.Persistent && e.Idle > maxIdle).OrderByDescending(e => e.Idle))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry) && IsItemDisposable)
                    await DisposeItemAsync(entry.Item).DynamicContext();
            }
        }

        /// <inheritdoc/>
        public virtual void ReduceMemory(long targetUsage, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Environment.WorkingSet <= targetUsage)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry))
                {
                    if (IsItemDisposable)
                        DisposeItem(entry.Item);
                    if (Environment.WorkingSet <= targetUsage)
                        return;
                }
            }
        }

        /// <inheritdoc/>
        [UserAction(), DisplayText("Reduce memory usage"), Description("Reduce the cached entries until a target memory usage")]
        public virtual async Task ReduceMemoryAsync(
            [DisplayText("Target"), Description("Target memory usage in bytes")]
            long targetUsage, 
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            if (Environment.WorkingSet <= targetUsage)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry))
                {
                    if (IsItemDisposable)
                        await DisposeItemAsync(entry.Item).DynamicContext();
                    if (Environment.WorkingSet <= targetUsage)
                        return;
                }
            }
        }

        /// <summary>
        /// Reduce the number of cache entries by a custom strategy
        /// </summary>
        /// <param name="strategy">Strategy</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual void ReduceBy(IInMemoryCacheStrategy<T> strategy, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(strategy))
            {
                EnsureUndisposed();
                if (!strategy.IsConditionMet)
                    return;
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry) && IsItemDisposable)
                    DisposeItem(entry.Item);
            }
        }

        /// <summary>
        /// Reduce the number of cache entries by a custom strategy
        /// </summary>
        /// <param name="strategy">Strategy</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ReduceByAsync(IInMemoryCacheStrategy<T> strategy, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(strategy))
            {
                EnsureUndisposed();
                if (!strategy.IsConditionMet)
                    return;
                cancellationToken.ThrowIfCancellationRequested();
                if (Remove(entry) && IsItemDisposable)
                    await DisposeItemAsync(entry.Item).DynamicContext();
            }
        }

        /// <summary>
        /// Tidy the cache
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task TidyCacheAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken.EnsureNotDefault(CancelToken);
            await ReduceByAsync(GetTidyStrategy(), cancellationToken).DynamicContext();
            if (Options.SoftCountLimit > 0)
                await ReduceCountAsync(Options.SoftCountLimit, cancellationToken).DynamicContext();
            if (Options.SoftSizeLimit > 0)
                await ReduceSizeAsync(Options.SoftSizeLimit, cancellationToken).DynamicContext();
            if (Options.MaxMemoryUsage.HasValue)
                await ReduceMemoryAsync(Options.MaxMemoryUsage.Value, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Get the default cache management strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetDefaultStrategy() => Options.DefaultStrategy switch
        {
            InMemoryCacheStrategy.Age => GetAgeStrategy(),
            InMemoryCacheStrategy.AccessTime => GetAccessTimeStrategy(),
            InMemoryCacheStrategy.Largest => GetLargestStrategy(),
            InMemoryCacheStrategy.Smallest => GetSmallestStrategy(),
            InMemoryCacheStrategy.Custom1 => GetCustom1Strategy(),
            InMemoryCacheStrategy.Custom2 => GetCustom2Strategy(),
            InMemoryCacheStrategy.Custom3 => GetCustom3Strategy(),
            _ => throw new InvalidOperationException($"Invalid strategy {Options.DefaultStrategy} in the options")
        };

        /// <summary>
        /// Get the tidy strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetTidyStrategy() => TidyStrategyInstance ??= new TidyStrategy(this);

        /// <summary>
        /// Get the age strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetAgeStrategy() => AgeStrategyInstance ??= new AgeStrategy(this);

        /// <summary>
        /// Get the access time strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetAccessTimeStrategy() => AccessTimeStrategyInstance ??= new AccessTimeStrategy(this);

        /// <summary>
        /// Get the largest entry strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetLargestStrategy() => LargerStrategyInstance ??= new LargerStrategy(this);

        /// <summary>
        /// Get the smallest entry strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetSmallestStrategy() => SmallerStrategyInstance ??= new SmallerStrategy(this);

        /// <summary>
        /// Get the custom strategy 1
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetCustom1Strategy() => throw new NotImplementedException();

        /// <summary>
        /// Get the custom strategy 2
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetCustom2Strategy() => throw new NotImplementedException();

        /// <summary>
        /// Get the custom strategy 3
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetCustom3Strategy() => throw new NotImplementedException();

        /// <summary>
        /// Apply a management strategy
        /// </summary>
        /// <param name="strategy">Strategy</param>
        /// <returns>Filtered cache entries sorted by the lowest priority ascending</returns>
        protected virtual IEnumerable<InMemoryCacheEntry<T>> ApplyStrategy(IInMemoryCacheStrategy<T> strategy)
            => strategy.PreFilterEntries(Cache.Values.Where(e => e.Type != InMemoryCacheEntryTypes.Persistent))
                .Order(strategy);
    }
}
