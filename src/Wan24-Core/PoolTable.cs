using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Pool table
    /// </summary>
    public static class PoolTable
    {
        /// <summary>
        /// Pool table (key is a GUID)
        /// </summary>
        public static readonly ConcurrentDictionary<string, IPool> Pools = new();
    }
}
