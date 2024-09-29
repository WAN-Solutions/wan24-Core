using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="PipelineStream"/> table
    /// </summary>
    public static class PipelineStreamTable
    {
        /// <summary>
        /// Streams (key is the GUID)
        /// </summary>
        [InstanceTable]
        public static readonly ConcurrentDictionary<string, PipelineStream> Streams = [];
    }
}
