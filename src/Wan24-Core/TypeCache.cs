using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Type cache
    /// </summary>
    public static class TypeCache
    {
        /// <summary>
        /// Type cache (key is the types hash code)
        /// </summary>
        public static readonly ConcurrentDictionary<int, Type> Types = new();
    }
}
