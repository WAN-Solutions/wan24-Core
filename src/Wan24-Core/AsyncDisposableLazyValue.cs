using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Disposable lazy value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="factory">Factory</param>
    public sealed class AsyncDisposableLazyValue<T>(in Func<T> factory) : SimpleDisposableBase() where T : IAsyncDisposable
    {
        /// <summary>
        /// Factory
        /// </summary>
        private readonly Func<T> Factory = factory;
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
        /// Value
        /// </summary>
        public T Value
        {
            get
            {
                EnsureUndisposed();
                if (HasValue) return _Value!;
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
            lock (SyncObject) if (HasValue) _Value.TryDispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            T? value = default;
            bool dispose = false;
            lock (SyncObject)
                if (HasValue) 
                {
                    value = _Value!;
                    dispose = true;
                }
            if (dispose) await value!.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="lazyValue">Lazy value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(in AsyncDisposableLazyValue<T> lazyValue) => lazyValue.Value;

        /// <summary>
        /// Cast as has-value-flag
        /// </summary>
        /// <param name="lazyValue">Lazy value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in AsyncDisposableLazyValue<T> lazyValue) => lazyValue.HasValue;
    }
}
