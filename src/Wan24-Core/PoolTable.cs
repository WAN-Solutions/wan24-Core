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
        [InstanceTable]
        public static readonly ConcurrentChangeTokenDictionary<string, IPool> Pools = [];
    }
}
