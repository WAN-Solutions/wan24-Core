namespace wan24.Core
{
    /// <summary>
    /// Interface for an in-memory cache management strategy
    /// </summary>
    /// <typeparam name="T">Cached item type</typeparam>
    public interface IInMemoryCacheStrategy<T> : IComparer<InMemoryCacheEntry<T>>
    {
        /// <summary>
        /// Pre-filter cache entries
        /// </summary>
        /// <param name="entries">Cache entries</param>
        /// <returns>Filtered cache entries</returns>
        IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries);
    }
}
