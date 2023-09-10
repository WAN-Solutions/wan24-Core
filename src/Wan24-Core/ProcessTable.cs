using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Process table
    /// </summary>
    public static class ProcessTable
    {
        /// <summary>
        /// Processes (key is a GUID)
        /// </summary>
        public static readonly ConcurrentDictionary<string, IProcessingInfo> Processing = new();
    }
}
