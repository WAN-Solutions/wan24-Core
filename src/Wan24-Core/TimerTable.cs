namespace wan24.Core
{
    /// <summary>
    /// Timer table
    /// </summary>
    public static class TimerTable
    {
        /// <summary>
        /// Timer table (key is a GUID)
        /// </summary>
        public static readonly ConcurrentChangeTokenDictionary<string, ITimer> Timers = new();
    }
}
