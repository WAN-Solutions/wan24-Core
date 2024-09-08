namespace wan24.Core
{
    /// <summary>
    /// Interface for a cache entry
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface ICacheEntry<T>
    {
        /// <summary>
        /// Hosting cache
        /// </summary>
        ICache<T> Cache { get; }
        /// <summary>
        /// If the item can be used
        /// </summary>
        bool CanUse { get; }
        /// <summary>
        /// Created time
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Unique cache entry key
        /// </summary>
        string Key { get; }
        /// <summary>
        /// Item
        /// </summary>
        T Item { get; }
        /// <summary>
        /// Item size
        /// </summary>
        int Size { get; }
        /// <summary>
        /// Options
        /// </summary>
        ICacheEntryOptions? Options { get; }
    }
}
