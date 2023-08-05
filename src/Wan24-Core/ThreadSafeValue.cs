using System.Threading;

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
        protected readonly SemaphoreSlim Sync = new(1, 1);
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
                Sync.Wait();
                try
                {
                    _Value = value;
                }
                finally
                {
                    Sync.Release();
                }
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
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                T? res = _Value;
                _Value = value;
                return res;
            }
            finally
            {
                Sync.Release();
            }
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>Current value</returns>
        public virtual T? Execute(Action_Delegate action)
        {
            Sync.Wait();
            try
            {
                return _Value = action(_Value);
            }
            finally
            {
                Sync.Release();
            }
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Current value</returns>
        public virtual async Task<T?> ExecuteAsync(AsyncAction_Delegate action, CancellationToken cancellationToken = default)
        {
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                return await action(_Value).DynamicContext();
            }
            finally
            {
                Sync.Release();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Sync.Wait();
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Sync.WaitAsync().DynamicContext();
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
