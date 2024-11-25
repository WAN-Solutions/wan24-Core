using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Queued asynchronous action
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="action">Action</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QueuedAsyncVoidAction(in Func<QueuedAsyncVoidAction, CancellationToken, Task> action, in CancellationToken cancellationToken = default)
        : IDisposable
    {
        /// <summary>
        /// Action
        /// </summary>
        public readonly Func<QueuedAsyncVoidAction, CancellationToken, Task> Action = action;
        /// <summary>
        /// Cancellation token
        /// </summary>
        public readonly CancellationToken CancelToken = cancellationToken;
        /// <summary>
        /// Completion
        /// </summary>
        public readonly TaskCompletionSource Completion = new();

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
                    await Action(this, cts.Token).DynamicContext();
                    Completion.SetResult();
                }
            }
            catch(Exception ex)
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
