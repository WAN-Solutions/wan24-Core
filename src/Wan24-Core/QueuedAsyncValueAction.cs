namespace wan24.Core
{
    /// <summary>
    /// Queued asynchronous action
    /// </summary>
    /// <typeparam name="T">Action return value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Action">Action</param>
    /// <param name="CancellationToken">Cancellation token</param>
    public sealed record class QueuedAsyncValueAction<T>(
        in Func<QueuedAsyncValueAction<T>, CancellationToken, Task<T>> Action, 
        in CancellationToken CancellationToken = default
        )
        : SimpleDisposableRecordBase()
    {
        /// <summary>
        /// Action
        /// </summary>
        private readonly Func<QueuedAsyncValueAction<T>, CancellationToken, Task<T>> Action = Action;
        /// <summary>
        /// Cancellation token
        /// </summary>
        private readonly CancellationToken CancelToken = CancellationToken;
        /// <summary>
        /// Completion
        /// </summary>
        private readonly TaskCompletionSource<T> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Tage
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Task
        /// </summary>
        public Task<T> Task => Completion.Task;

        /// <summary>
        /// Creation time
        /// </summary>
        public DateTime Created { get; } = DateTime.Now;

        /// <summary>
        /// Execution time
        /// </summary>
        public DateTime Executed { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Done time
        /// </summary>
        public DateTime Done { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Runtime
        /// </summary>
        public TimeSpan Runtime
            => Executed == DateTime.MinValue || Done == DateTime.MinValue
                ? TimeSpan.Zero
                : Done - Executed;

        /// <summary>
        /// Execute the action (needs to be called from the queue worker)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        public async Task<T> ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                EnsureUndisposed();
                Executed = DateTime.Now;
                CancelToken.ThrowIfCancellationRequested();
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                    [..new CancellationToken[]
                    {
                        CancelToken,
                        cancellationToken
                    }.RemoveNoneAndDefault()
                        .RemoveDoubles()]
                    );
                T ret = await Action(this, cts.Token).DynamicContext();
                Completion.TrySetResult(ret);
                return ret;
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
                throw;
            }
            finally
            {
                Done = DateTime.Now;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!Completion.Task.IsCompleted) Completion.TrySetException(new ObjectDisposedException(GetType().ToString()));
        }
    }
}
