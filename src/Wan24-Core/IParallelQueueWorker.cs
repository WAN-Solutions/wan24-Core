namespace wan24.Core
{
    /// <summary>
    /// Interface for a parallel queue worker
    /// </summary>
    public interface IParallelQueueWorker : IQueueWorker
    {
        /// <summary>
        /// Number of threads
        /// </summary>
        int Threads { get; }
        /// <summary>
        /// Wait until all queued work was done
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>All done?</returns>
        bool WaitBoring(TimeSpan timeout);
        /// <summary>
        /// Wait until all queued work was done
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>All done?</returns>
        Task<bool> WaitBoringAsync(TimeSpan timeout);
        /// <summary>
        /// Wait until all queued work was done
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>All done?</returns>
        bool WaitBoring(CancellationToken cancellationToken = default);
        /// <summary>
        /// Wait until all queued work was done
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>All done?</returns>
        Task<bool> WaitBoringAsync(CancellationToken cancellationToken = default);
    }
}
