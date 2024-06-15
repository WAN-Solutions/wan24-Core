namespace wan24.Core
{
    /// <summary>
    /// Interface for an in-memory cacheable item
    /// </summary>
    public interface IInMemoryCacheItem
    {
        /// <summary>
        /// Cache entry key
        /// </summary>
        string Key { get; }
        /// <summary>
        /// Item size in the cache
        /// </summary>
        int Size { get; }
        /// <summary>
        /// Cache entry options
        /// </summary>
        InMemoryCacheEntryOptions? Options { get; }
    }
}
