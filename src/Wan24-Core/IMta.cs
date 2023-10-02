namespace wan24.Core
{
    /// <summary>
    /// Interface for a MTA
    /// </summary>
    public interface IMta : IMtaConnection
    {
        /// <summary>
        /// Get a connection
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Connection (if no <c>settings</c> were given, the instance should be returned using <see cref="ReturnConnection(IMtaConnection)"/>)</returns>
        IMtaConnection GetConnection(IMtaSettings? settings = null);
        /// <summary>
        /// Get a connection
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Connection (if no <c>settings</c> were given, the instance should be returned using <see cref="ReturnConnectionAsync(IMtaConnection, CancellationToken)"/>)</returns>
        Task<IMtaConnection> GetConnectionAsync(IMtaSettings? settings = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Return a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        void ReturnConnection(IMtaConnection connection);
        /// <summary>
        /// Return a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReturnConnectionAsync(IMtaConnection connection, CancellationToken cancellationToken = default);
    }
}
