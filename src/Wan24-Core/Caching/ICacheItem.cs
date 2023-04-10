namespace wan24.Core.Caching
{
    /// <summary>
    /// Interface for a cache item
    /// </summary>
    public interface ICacheItem : IDisposableObject
    {
        /// <summary>
        /// Cache
        /// </summary>
        ICache Cache { get; }
        /// <summary>
        /// Key
        /// </summary>
        string Key { get; }
        /// <summary>
        /// Added time
        /// </summary>
        DateTime Added { get; }
        /// <summary>
        /// Updated time
        /// </summary>
        DateTime Updated { get; }
        /// <summary>
        /// Cache timeout
        /// </summary>
        CacheTimeouts CacheTimeout { get; }
        /// <summary>
        /// Expires time
        /// </summary>
        DateTime Expires { get; }
        /// <summary>
        /// Timeout
        /// </summary>
        TimeSpan? Timeout { get; }
    }
}
