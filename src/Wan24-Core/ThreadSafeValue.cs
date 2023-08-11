namespace wan24.Core
{
    /// <summary>
    /// Thread-safe value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public class ThreadSafeValue<T> : DisposableBase
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Value
        /// </summary>
        protected T? _Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialValue">Initial value</param>
        public ThreadSafeValue(T? initialValue = default) : base()
        {
            _Value = initialValue;
        }

        /// <summary>
        /// Value
        /// </summary>
        public virtual T? Value
        {
            get => _Value;
            set
            {
                using SemaphoreSyncContext ssc = Sync.Sync();
                _Value = value;
            }
        }

        /// <summary>
        /// Set the value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Old value</returns>
        public virtual async Task<T?> SetValueAsync(T? value, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncAsync(cancellationToken).DynamicContext();
            T? res = _Value;
            _Value = value;
            return res;
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>Current value</returns>
        public virtual T? Execute(Action_Delegate action)
        {
            using SemaphoreSyncContext ssc = Sync.Sync();
            return _Value = action(_Value);
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Current value</returns>
        public virtual async Task<T?> ExecuteAsync(AsyncAction_Delegate action, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncAsync(cancellationToken).DynamicContext();
            return await action(_Value).DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Sync.Semaphore.Wait();
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Sync.Semaphore.WaitAsync().DynamicContext();
            Sync.Dispose();
        }

        /// <summary>
        /// Delegate for an action
        /// </summary>
        /// <param name="value">Current value</param>
        /// <returns>Value to set</returns>
        public delegate T? Action_Delegate(T? value);

        /// <summary>
        /// Delegate for an action
        /// </summary>
        /// <param name="value">Current value</param>
        /// <returns>Value to set</returns>
        public delegate Task<T?> AsyncAction_Delegate(T? value);

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator T?(ThreadSafeValue<T?> value) => value.Value;

        /// <summary>
        /// Cast as new instance
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator ThreadSafeValue<T>(T? value) => new(value);
    }
}
