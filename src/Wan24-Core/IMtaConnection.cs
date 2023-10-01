namespace wan24.Core
{
    /// <summary>
    /// Interface for a MTA connection
    /// </summary>
    public interface IMtaConnection : IDisposableObject
    {
        /// <summary>
        /// MTA
        /// </summary>
        IMta MTA { get; }
        /// <summary>
        /// Settings
        /// </summary>
        IMtaSettings Settings { get; }
        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Succeed?</returns>
        bool Send(IEmail email);
        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Succeed?</returns>
        Task<bool> SendAsync(IEmail email, CancellationToken cancellationToken = default);
    }
}
