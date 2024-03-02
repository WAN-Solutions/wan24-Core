namespace wan24.Core
{
    /// <summary>
    /// Interface for an app configuration
    /// </summary>
    public interface IAppConfig
    {
        /// <summary>
        /// Apply this app configuration
        /// </summary>
        void Apply();
        /// <summary>
        /// Apply this app configuration
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ApplyAsync(CancellationToken cancellationToken = default);
    }
}
