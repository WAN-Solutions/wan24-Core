namespace wan24.Core
{
    /// <summary>
    /// In-memory cache options
    /// </summary>
    public record class InMemoryCacheOptions()
    {
        /// <summary>
        /// Default cache entry type
        /// </summary>
        public static InMemoryCacheEntryTypes DefaultEntryType { get; set; } = InMemoryCacheEntryTypes.Variable;

        /// <summary>
        /// Default cache entry timeout
        /// </summary>
        public static TimeSpan DefaultEntryTimeout { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Default cache entry timeout is sliding
        /// </summary>
        public static bool DefaultEntrySlidingTimeout { get; set; }

        /// <summary>
        /// Default cached item disposing observation (works only for <see cref="IDisposableObject"/> types; disposing items will be removed)
        /// </summary>
        public static bool DefaultObserveItemDisposing { get; set; }

        /// <summary>
        /// Cache management default strategy
        /// </summary>
        public InMemoryCacheStrategy DefaultStrategy { get; init; } = InMemoryCacheStrategy.Age;

        /// <summary>
        /// Default cache entry options
        /// </summary>
        public InMemoryCacheEntryOptions? DefaultEntryOptions { get; init; }

        /// <summary>
        /// Cache tidy timeout
        /// </summary>
        public TimeSpan TidyTimeout { get; init; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Soft cache entry count limit (applied in the background; <c>0</c> to disable)
        /// </summary>
        public int SoftCountLimit { get; init; }

        /// <summary>
        /// Hard cache entry count limit (may overflow a bit; <c>0</c> to disable)
        /// </summary>
        public int HardCountLimit { get; init; }

        /// <summary>
        /// Soft size limit (applied in the background; <c>0</c> to disable)
        /// </summary>
        public long SoftSizeLimit { get; init; }

        /// <summary>
        /// Hard size limit (may overflow a bit; <c>0</c> to disable)
        /// </summary>
        public long HardSizeLimit { get; init; }

        /// <summary>
        /// Max. cacheable item size
        /// </summary>
        public int MaxItemSize { get; init; }

        /// <summary>
        /// Age limit (applied in the background; <see cref="TimeSpan.Zero"/> to disable)
        /// </summary>
        public TimeSpan AgeLimit { get; init; }

        /// <summary>
        /// Idle limit (applied in the background; <see cref="TimeSpan.Zero"/> to disable)
        /// </summary>
        public TimeSpan IdleLimit { get; init; }

        /// <summary>
        /// Try disposing cached items when auto-removing always?
        /// </summary>
        public bool TryDisposeItemsAlways { get; init; }

        /// <summary>
        /// Cache I/O concurrency level (if set, <see cref="SoftCountLimit"/> or <see cref="HardCountLimit"/> is required to be set, too!)
        /// </summary>
        public int? ConcurrencyLevel { get; init; }
    }
}
