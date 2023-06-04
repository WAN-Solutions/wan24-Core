using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Cancellation awaiter
    /// </summary>
    public readonly record struct CancellationAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public CancellationAwaiter(CancellationToken cancellationToken) => CancellationToken = cancellationToken;

        /// <summary>
        /// Cancellation token
        /// </summary>
        internal CancellationToken CancellationToken { get; }

        /// <summary>
        /// Is completed (canceled)?
        /// </summary>
        public bool IsCompleted => CancellationToken.IsCancellationRequested;

        /// <summary>
        /// Get the awaiter
        /// </summary>
        /// <returns>This</returns>
        public CancellationAwaiter GetAwaiter() => this;

        /// <summary>
        /// Get the result (will throw an exception!)
        /// </summary>
        /// <exception cref="OperationCanceledException">Canceled</exception>
        /// <exception cref="InvalidOperationException">Not canceled</exception>
        public void GetResult()
        {
            CancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException();
        }

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => CancellationToken.Register(continuation);

        /// <inheritdoc/>
        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
    }
}
