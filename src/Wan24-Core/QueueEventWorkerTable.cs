using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Queue event worker table
    /// </summary>
    public static class QueueEventWorkerTable
    {
        /// <summary>
        /// Registered workers (key is the GUID)
        /// </summary>
        [InstanceTable]
        public static readonly ConcurrentDictionary<string, IQueueEventWorker> Workers = [];
    }
}
