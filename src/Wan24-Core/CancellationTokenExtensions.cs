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

        /// <summary>
        /// Get if cancellation is requested and throw an exception, if canceled.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException">Cancellation is requested</exception>
        /// <returns><see langword="false"/></returns>
        public static bool GetIsCancellationRequested(this CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }

        /// <summary>
        /// Ensure a non-default cancellation token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="alternate">Alternative cancellation token</param>
        /// <returns>Non-default cancellation token</returns>
        public static CancellationToken EnsureNotDefault(this CancellationToken cancellationToken, in CancellationToken alternate)
            => cancellationToken.IsEqualTo(default) ? alternate : cancellationToken;

        /// <summary>
        /// Determine if a cancellation token is equal to another cancellation token
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="other">Other cancellation token</param>
        /// <returns>If the cancellation tokens are equal</returns>
        public static bool IsEqualTo(this CancellationToken cancellationToken, CancellationToken other)
            => CancellationToken.Equals(cancellationToken, other);

        /// <summary>
        /// Remove the default and <see cref="CancellationToken.None"/> cancellation tokens
        /// </summary>
        /// <param name="tokens">Cancellation tokens</param>
        /// <returns>Resulting cancellation tokens</returns>
        public static IEnumerable<CancellationToken> RemoveNoneAndDefault(this IEnumerable<CancellationToken> tokens)
            => Remove(tokens, default, CancellationToken.None);

        /// <summary>
        /// Remove the default and <see cref="CancellationToken.None"/> cancellation tokens
        /// </summary>
        /// <param name="tokens">Cancellation tokens</param>
        /// <param name="removeTokens">Cancellation tokens to remove</param>
        /// <returns>Resulting cancellation tokens</returns>
        public static IEnumerable<CancellationToken> RemoveNoneAndDefaultAnd(this IEnumerable<CancellationToken> tokens, params CancellationToken[] removeTokens)
            => Remove(tokens, [default, CancellationToken.None, ..removeTokens]);

        /// <summary>
        /// Remove the default and <see cref="CancellationToken.None"/> cancellation tokens
        /// </summary>
        /// <param name="tokens">Cancellation tokens</param>
        /// <param name="removeTokens">Cancellation tokens to remove</param>
        /// <returns>Resulting cancellation tokens</returns>
        public static IEnumerable<CancellationToken> Remove(this IEnumerable<CancellationToken> tokens, params CancellationToken[] removeTokens)
            => tokens.Where(t => !removeTokens.Any(rt => rt.IsEqualTo(t)));

        /// <summary>
        /// Remove double cancellation tokens (ensure having a distinct list)
        /// </summary>
        /// <param name="tokens">Cancellation tokens</param>
        /// <returns>Resulting cancellation tokens</returns>
        public static IEnumerable<CancellationToken> RemoveDoubles(this IEnumerable<CancellationToken> tokens)
            => new HashSet<CancellationToken>().AddRange(tokens);
    }
}
