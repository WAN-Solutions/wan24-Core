using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Pipeline table
    /// </summary>
    public static class PipelineTable
    {
        /// <summary>
        /// Registered pipelines (key is the GUID)
        /// </summary>
        [InstanceTable]
        public static readonly ConcurrentDictionary<string, IPipeline> Pipelines = [];
    }
}
