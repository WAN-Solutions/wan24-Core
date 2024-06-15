namespace wan24.Core
{
    /// <summary>
    /// In-memory cache entry options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class InMemoryCacheEntryOptions()
    {
        /// <summary>
        /// Item size
        /// </summary>
        public int Size { get; set; } = 1;

        /// <summary>
        /// Type
        /// </summary>
        public InMemoryCacheEntryTypes? Type { get; set; }

        /// <summary>
        /// Timeout
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// If <see cref="Timeout"/> is sliding
        /// </summary>
        public bool? IsSlidingTimeout { get; set; }

        /// <summary>
        /// Absolute timeout
        /// </summary>
        public DateTime? AbsoluteTimeout { get; set; }

        /// <summary>
        /// Observe cached item disposing (works only for <see cref="IDisposableObject"/> types; disposing items will be removed)
        /// </summary>
        public bool? ObserveDisposing { get; set; }
    }
}
