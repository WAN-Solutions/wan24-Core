namespace wan24.Core
{
    /// <summary>
    /// Interface for a throttle
    /// </summary>
    public interface IThrottle : IWillDispose
    {
        /// <summary>
        /// Limit within <see cref="Timeout"/> (zero to disable throttling)
        /// </summary>
        int Limit { get; set; }
        /// <summary>
        /// Timeout (setting will restart the throttling timer)
        /// </summary>
        TimeSpan Timeout { get; set; }
        /// <summary>
        /// Current count
        /// </summary>
        int CurrentCount { get; }
        /// <summary>
        /// If throttling
        /// </summary>
        bool IsThrottling { get; }
        /// <summary>
        /// Count one (will throttle, if required)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        void CountOne(CancellationToken cancellationToken = default);
        /// <summary>
        /// Count one (will throttle, if required)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CountOneAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Count (will throttle, if required)
        /// </summary>
        /// <param name="count">Number to count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void Count(int count, CancellationToken cancellationToken = default);
        /// <summary>
        /// Count (will throttle, if required)
        /// </summary>
        /// <param name="count">Number to count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CountAsync(int count, CancellationToken cancellationToken = default);
        /// <summary>
        /// Clear the counter and release the throttle
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        void Release(CancellationToken cancellationToken = default);
        /// <summary>
        /// Clear the counter and release the throttle
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReleaseAsync(CancellationToken cancellationToken = default);
    }
}
