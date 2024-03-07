namespace wan24.Core
{
    /// <summary>
    /// <see cref="IStatusProvider"/> table
    /// </summary>
    public static class StatusProviderTable
    {
        /// <summary>
        /// Status providers (key is the status group name)
        /// </summary>
        public static readonly ConcurrentChangeTokenDictionary<string, IEnumerable<Status>> Providers = [];
    }
}
