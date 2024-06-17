namespace wan24.Core
{
    /// <summary>
    /// Delay table
    /// </summary>
    public static class DelayTable
    {
        /// <summary>
        /// Delays (key is the GUID)
        /// </summary>
        [InstanceTable]
        public static readonly ConcurrentChangeTokenDictionary<string, Delay> Delays = [];
    }
}
