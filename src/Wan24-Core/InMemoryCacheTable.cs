using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// In-memory cache table
    /// </summary>
    public static class InMemoryCacheTable
    {
        /// <summary>
        /// In-memory caches (key is the GUID)
        /// </summary>
        public static readonly ConcurrentDictionary<string, IInMemoryCache> Caches = [];
    }
}
