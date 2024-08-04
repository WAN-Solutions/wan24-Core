using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="IThrottle"/> table
    /// </summary>
    public static class ThrottleTable
    {
        /// <summary>
        /// Throttles (key is the GUID)
        /// </summary>
        [InstanceTable]
        public static readonly ConcurrentDictionary<string, IThrottle> Throttles = [];
    }
}
