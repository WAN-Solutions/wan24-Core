namespace wan24.Core
{
    /// <summary>
    /// Interface for a throttle
    /// </summary>
    public interface IThrottle : IBasicDisposableObject, IDisposable, IAsyncDisposable, IStatusProvider
    {
        /// <summary>
        /// GUID
        /// </summary>
        string GUID { get; }
        /// <summary>
        /// Name
        /// </summary>
        string? Name { get; }
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
        /// <summary>
        /// Update settings (will restart the throttling timer)
        /// </summary>
        /// <param name="limit">Limit within <c>timeout</c> (zero to disable throttling)</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void UpdateSettings(int limit, TimeSpan timeout, CancellationToken cancellationToken = default);
        /// <summary>
        /// Update settings (will restart the throttling timer)
        /// </summary>
        /// <param name="limit">Limit within <c>timeout</c> (zero to disable throttling)</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateSettingsAsync(int limit, TimeSpan timeout, CancellationToken cancellationToken = default);
    }
}
