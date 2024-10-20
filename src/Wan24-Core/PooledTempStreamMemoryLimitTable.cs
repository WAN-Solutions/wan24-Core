using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="PooledTempStreamMemoryLimit"/> table
    /// </summary>
    public static class PooledTempStreamMemoryLimitTable
    {
        /// <summary>
        /// Memory limits (key is the GUID)
        /// </summary>
        [InstanceTable]
        public static readonly ConcurrentDictionary<string, PooledTempStreamMemoryLimit> Limits = [];
    }
}
