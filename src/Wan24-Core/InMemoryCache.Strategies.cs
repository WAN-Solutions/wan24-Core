namespace wan24.Core
{
    // Strategies
    public partial class InMemoryCache<T>
    {
        /// <summary>
        /// Tidy strategy instance
        /// </summary>
        protected readonly IInMemoryCacheStrategy<T> TidyStrategyInstance;
        /// <summary>
        /// Age strategy instance
        /// </summary>
        protected readonly IInMemoryCacheStrategy<T> AgeStrategyInstance;
        /// <summary>
        /// Access time strategy instance
        /// </summary>
        protected readonly IInMemoryCacheStrategy<T> AccessTimeStrategyInstance;
        /// <summary>
        /// Larger strategy instance
        /// </summary>
        protected readonly IInMemoryCacheStrategy<T> LargerStrategyInstance;
        /// <summary>
        /// Smaller strategy instance
        /// </summary>
        protected readonly IInMemoryCacheStrategy<T> SmallerStrategyInstance;

        /// <summary>
        /// Reduce the number of cache entries
        /// </summary>
        /// <param name="targetCount">Target count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ReduceCountAsync(int targetCount, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Count <= targetCount)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (TryRemove(entry.Key) is InMemoryCacheEntry<T> removed)
                {
                    if (IsItemDisposable)
                        await DisposeItemAsync(removed.Item).DynamicContext();
                    if (Count <= targetCount)
                        return;
                }
            }
        }

        /// <summary>
        /// Reduce the size of the cache
        /// </summary>
        /// <param name="targetSize">Target size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ReduceSizeAsync(long targetSize, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Size <= targetSize)
                return;
            foreach (InMemoryCacheEntry<T> entry in ApplyStrategy(GetDefaultStrategy()))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (TryRemove(entry.Key) is InMemoryCacheEntry<T> removed)
                {
                    if (IsItemDisposable)
                        await DisposeItemAsync(removed.Item).DynamicContext();
                    if (Size <= targetSize)
                        return;
                }
            }
        }

        /// <summary>
        /// Reduce the number of cache entries by removing the oldest entries
        /// </summary>
        /// <param name="maxAge">Max. cache entry age</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ReduceOldAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in Cache.Values.Where(e => e.Type != InMemoryCacheEntryTypes.Persistent && e.Age > maxAge))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (TryRemove(entry.Key) is InMemoryCacheEntry<T> removed && IsItemDisposable)
                    await DisposeItemAsync(removed.Item).DynamicContext();
            }
        }

        /// <summary>
        /// Reduce the number of cache entries by removing the least accessed entries
        /// </summary>
        /// <param name="maxIdle">Max. cache entry idle time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ReduceUnpopularAsync(TimeSpan maxIdle, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            foreach (InMemoryCacheEntry<T> entry in Cache.Values.Where(e => e.Type != InMemoryCacheEntryTypes.Persistent && e.Idle > maxIdle))
            {
                EnsureUndisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (TryRemove(entry.Key) is InMemoryCacheEntry<T> removed && IsItemDisposable)
                    await DisposeItemAsync(removed.Item).DynamicContext();
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
                cancellationToken.ThrowIfCancellationRequested();
                if (TryRemove(entry.Key) is InMemoryCacheEntry<T> removed && IsItemDisposable)
                    await DisposeItemAsync(removed.Item).DynamicContext();
            }
        }

        /// <summary>
        /// Tidy the cache
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task TidyCacheAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken = cancellationToken.EnsureNotDefault(CancelToken);
            await ReduceByAsync(GetTidyStrategy(), CancelToken).DynamicContext();
            if (Options.SoftCountLimit > 0)
                await ReduceCountAsync(Options.SoftCountLimit, cancellationToken).DynamicContext();
            if (Options.SoftSizeLimit > 0)
                await ReduceSizeAsync(Options.SoftSizeLimit, cancellationToken).DynamicContext();
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
        protected virtual IInMemoryCacheStrategy<T> GetTidyStrategy() => TidyStrategyInstance;

        /// <summary>
        /// Get the age strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetAgeStrategy() => AgeStrategyInstance;

        /// <summary>
        /// Get the access time strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetAccessTimeStrategy() => AccessTimeStrategyInstance;

        /// <summary>
        /// Get the largest entry strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetLargestStrategy() => LargerStrategyInstance;

        /// <summary>
        /// Get the smallest entry strategy
        /// </summary>
        /// <returns>Strategy</returns>
        protected virtual IInMemoryCacheStrategy<T> GetSmallestStrategy() => SmallerStrategyInstance;

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
                .OrderBy(e => !e.CanUse)
                .Order(strategy);

        /// <summary>
        /// Tidy strategy
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class TidyStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if ((x is not null && y is not null) || (x is null && y is null)) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                return 0;
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries)
                => entries.Where(
                        e => !e.CanUse ||
                            (Cache.Options.AgeLimit > TimeSpan.Zero && e.Age > Cache.Options.AgeLimit) ||
                            (Cache.Options.IdleLimit > TimeSpan.Zero && e.Idle > Cache.Options.IdleLimit)
                    );
        }

        /// <summary>
        /// Age strategy (higher age has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class AgeStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Age.CompareTo(y.Age) switch
                {
                    0 => 0,
                    -1 => 1,
                    1 => -1,
                    _ => throw new InvalidProgramException()
                };
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }

        /// <summary>
        /// Access time strategy (higher idle time has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class AccessTimeStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Idle.CompareTo(y.Idle) switch
                {
                    0 => 0,
                    -1 => 1,
                    1 => -1,
                    _ => throw new InvalidProgramException()
                };
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }

        /// <summary>
        /// Larger strategy (larger size has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class LargerStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Size.CompareTo(y.Size) switch
                {
                    0 => 0,
                    -1 => 1,
                    1 => -1,
                    _ => throw new InvalidProgramException()
                };
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }

        /// <summary>
        /// Smaller strategy (smaller size has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class SmallerStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Size.CompareTo(y.Size);
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }
    }
}
