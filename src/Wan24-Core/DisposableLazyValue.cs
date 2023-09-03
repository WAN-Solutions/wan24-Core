namespace wan24.Core
{
    /// <summary>
    /// Disposable lazy value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public sealed class DisposableLazyValue<T> : DisposableBase where T : IDisposable
    {
        /// <summary>
        /// Factory
        /// </summary>
        private readonly Func<T> Factory;
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Value
        /// </summary>
        private T? _Value = default;
        /// <summary>
        /// Has a value?
        /// </summary>
        private bool HasValue = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="factory">Factory</param>
        public DisposableLazyValue(in Func<T> factory) : base() => Factory = factory;

        /// <summary>
        /// Value
        /// </summary>
        public T Value
        {
            get
            {
                lock (SyncObject)
                {
                    EnsureUndisposed();
                    if (HasValue) return _Value!;
                    HasValue = true;
                    return _Value = Factory();
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (SyncObject)
                if (HasValue)
                    if (_Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else if (_Value is IAsyncDisposable asyncDisposable)
                    {
                        asyncDisposable.DisposeAsync().AsTask().Wait();
                    }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            IAsyncDisposable? value = null;
            bool dispose = false;
            lock (SyncObject)
                if (HasValue)
                    if (_Value is IAsyncDisposable asyncDisposable)
                    {
                        value = asyncDisposable;
                        dispose = true;
                    }
                    else
                    {
                        ((IDisposable)_Value!).Dispose();
                    }
            if (dispose) await value!.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="lazyValue">Lazy value</param>
        public static implicit operator T(in DisposableLazyValue<T> lazyValue) => lazyValue.Value;

        /// <summary>
        /// Cast as has-value-flag
        /// </summary>
        /// <param name="lazyValue">Lazy value</param>
        public static implicit operator bool(in DisposableLazyValue<T> lazyValue) => lazyValue.HasValue;
    }
}
