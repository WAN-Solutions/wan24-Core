using System.Collections.Concurrent;

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
        public static readonly ConcurrentDictionary<string, Delay> Delays = new();
    }
}
