namespace wan24.Core
{
    /// <summary>
    /// Base class for a volatile value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract class VolatileValueBase<T>() : DisposableBase()
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Cancellation
        /// </summary>
        protected readonly CancellationTokenSource Cancellation = new();
        /// <summary>
        /// Current values
        /// </summary>
        protected volatile TaskCompletionSource<T> _CurrentValue = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Current value
        /// </summary>
        public virtual Task<T> CurrentValue => _CurrentValue.Task;

        /// <summary>
        /// Last exception
        /// </summary>
        public virtual Exception? LastException { get; protected set; }

        /// <summary>
        /// Value created time
        /// </summary>
        public DateTime ValueCreated { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Value reset time
        /// </summary>
        public DateTime ValueReset { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Set the current value to <see cref="_CurrentValue"/> (should use <see cref="Cancellation"/>, now throw and set <see cref="ValueCreated"/>)
        /// </summary>
        protected abstract void SetCurrentValue();

        /// <summary>
        /// Reset the current value
        /// </summary>
        protected virtual void Reset()
        {
            EnsureUndisposed();
            TaskCompletionSource<T> oldValue;
            using (SemaphoreSyncContext ssc = Sync)
            {
                EnsureUndisposed();
                oldValue = _CurrentValue;
                _CurrentValue = new(TaskCreationOptions.RunContinuationsAsynchronously);
                ValueReset = DateTime.Now;
            }
            if (!oldValue.Task.IsCompleted)
                oldValue.TrySetException(new InvalidOperationException("No current value"));
        }

        /// <summary>
        /// Reset the current value
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            TaskCompletionSource<T> oldValue;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                EnsureUndisposed();
                oldValue = _CurrentValue;
                _CurrentValue = new(TaskCreationOptions.RunContinuationsAsynchronously);
                ValueReset = DateTime.Now;
            }
            if (!oldValue.Task.IsCompleted)
                oldValue.TrySetException(new InvalidOperationException("No current value"));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Cancellation.Cancel();
            Sync.Dispose();
            if (_CurrentValue.Task.IsCompleted)
                _CurrentValue = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _CurrentValue.TrySetException(new ObjectDisposedException(GetType().ToString()));
            Cancellation.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Cancellation.CancelAsync().DynamicContext();
            await Sync.DisposeAsync().DynamicContext();
            if (_CurrentValue.Task.IsCompleted)
                _CurrentValue = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _CurrentValue.TrySetException(new ObjectDisposedException(GetType().ToString()));
            Cancellation.Dispose();
        }
    }
}
