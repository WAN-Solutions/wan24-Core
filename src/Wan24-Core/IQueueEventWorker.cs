namespace wan24.Core
{
    /// <summary>
    /// Interface for a queue event worker
    /// </summary>
    public interface IQueueEventWorker : IServiceWorkerStatus
    {
        /// <summary>
        /// Maximum number of threads for parallel processing
        /// </summary>
        int MaxThreads { get; }
        /// <summary>
        /// Number of processing items
        /// </summary>
        int ProcessingItems { get; }
    }
}
