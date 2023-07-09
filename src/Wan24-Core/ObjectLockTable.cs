using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Object locks table
    /// </summary>
    public static class ObjectLockTable
    {
        /// <summary>
        /// Object locks (key ia a GUID)
        /// </summary>
        public static readonly ConcurrentDictionary<string, IObjectLockManager> ObjectLocks = new();
    }
}
