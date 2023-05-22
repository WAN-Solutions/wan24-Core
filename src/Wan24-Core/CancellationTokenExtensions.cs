namespace wan24.Core
{
    /// <summary>
    /// <see cref="CancellationToken"/> extensions
    /// </summary>
    public static class CancellationTokenExtensions
    {
        /// <summary>
        /// Get a cancellation awaiter for a cancellation token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cancellation awaiter</returns>
        public static CancellationAwaiter GetAwaiter(this CancellationToken cancellationToken) => new(cancellationToken);
    }
}
