using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Queued asynchronous action
    /// </summary>
    /// <typeparam name="T">Action return value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="action">Action</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QueuedAsyncValueAction<T>(in Func<QueuedAsyncValueAction<T>, CancellationToken, Task<T>> action, in CancellationToken cancellationToken = default)
        : IDisposable
    {
        /// <summary>
        /// Action
        /// </summary>
        public readonly Func<QueuedAsyncValueAction<T>, CancellationToken, Task<T>> Action = action;
        /// <summary>
        /// Cancellation token
        /// </summary>
        public readonly CancellationToken CancelToken = cancellationToken;
        /// <summary>
        /// Completion
        /// </summary>
        public readonly TaskCompletionSource<T> Completion = new();

        /// <summary>
        /// Execute the action (needs to be called from the queue worker)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                CancelToken.ThrowIfCancellationRequested();
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                    [..new CancellationToken[]
                    {
                        CancelToken,
                        cancellationToken
                    }.RemoveNoneAndDefault()
                        .RemoveDoubles()]
                    );
                if (Completion.Task.IsCompleted)
                {
                    await Completion.Task.DynamicContext();
                }
                else
                {
                    Completion.SetResult(await Action(this, cts.Token).DynamicContext());
                }
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!Completion.Task.IsCompleted) Completion.TrySetException(new ObjectDisposedException(GetType().ToString()));
        }
    }
}
