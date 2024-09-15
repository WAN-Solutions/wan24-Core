namespace wan24.Core
{
    /// <summary>
    /// In-memory cache entry options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class InMemoryCacheEntryOptions() : ICacheEntryOptions
    {
        /// <inheritdoc/>
        public int Size { get; set; } = 1;

        /// <summary>
        /// Type
        /// </summary>
        public InMemoryCacheEntryTypes? Type { get; set; }

        /// <summary>
        /// Timeout (if <see cref="Type"/> is <see cref="InMemoryCacheEntryTypes.Timeout"/>)
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Is the <see cref="Timeout"/> a sliding timeout? (has only an effect if <see cref="Type"/> is <see cref="InMemoryCacheEntryTypes.Timeout"/>)
        /// </summary>
        public bool? IsSlidingTimeout { get; set; }

        /// <summary>
        /// Absolute timeout
        /// </summary>
        public DateTime? AbsoluteTimeout { get; set; }

        /// <summary>
        /// Observe item disposing (works only for <see cref="IDisposableObject"/> types; disposing items will be removed from the cache automatic)
        /// </summary>
        public bool? ObserveDisposing { get; set; }
    }
}
