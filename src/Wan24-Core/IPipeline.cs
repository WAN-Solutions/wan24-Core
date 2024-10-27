namespace wan24.Core
{
    /// <summary>
    /// Interface for a pipeline
    /// </summary>
    public interface IPipeline : IServiceWorkerStatus
    {
        /// <summary>
        /// GUID
        /// </summary>
        string GUID { get; }
        /// <summary>
        /// Number of currently processing contexts
        /// </summary>
        int ContextCount { get; }
    }
}
