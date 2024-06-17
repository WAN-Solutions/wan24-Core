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
        [InstanceTable]
        public static readonly ConcurrentChangeTokenDictionary<string, IObjectLockManager> ObjectLocks = [];
    }
}
