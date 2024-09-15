namespace wan24.Core
{
    /// <summary>
    /// <see cref="CacheSwitch{T}"/> options
    /// </summary>
    /// <typeparam name="T">Cache item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class CacheSwitchOptions<T>()
    {
        /// <summary>
        /// Cache
        /// </summary>
        public required ICache<T> Cache { get; init; }

        /// <summary>
        /// Maximum allowed item size
        /// </summary>
        public required int MaxItemSize { get; init; }

        /// <summary>
        /// Any cache type ID (positive value, free to choose; can be used to address a special cache type)
        /// </summary>
        public int CacheType { get; init; }
    }
}
