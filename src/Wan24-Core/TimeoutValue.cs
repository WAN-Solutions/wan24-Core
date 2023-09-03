using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Timeout value (disposable value will be disposed on timeout or when disposing)
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public sealed class TimeoutValue<T> : DisposableBase
    {
        /// <summary>
        /// Factory
        /// </summary>
        private readonly Func<T> Factory;
        /// <summary>
        /// Timer
        /// </summary>
        private readonly System.Timers.Timer Timer;
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
        /// <param name="timeout">Timeout</param>
        public TimeoutValue(in Func<T> factory, in TimeSpan timeout) : base()
        {
            Factory = factory;
            Timer = new()
            {
                Interval = timeout.TotalMilliseconds,
                AutoReset = false
            };
            Timer.Elapsed += (s, e) =>
            {
                lock (SyncObject)
                {
                    if (HasValue)
                        if (_Value is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                        else if (_Value is IAsyncDisposable asyncDisposable)
                        {
                            asyncDisposable.DisposeAsync().AsTask().Wait();
                        }
                    HasValue = false;
                    _Value = default;
                }
            };
        }

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
                    if (HasValue)
                    {
                        Timer.Stop();
                        Timer.Start();
                        return _Value!;
                    }
                    HasValue = true;
                    _Value = Factory();
                    Timer.Start();
                    return _Value;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Timer.Dispose();
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
            Timer.Dispose();
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
        /// <param name="timeoutValue">Timeout value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(in TimeoutValue<T> timeoutValue) => timeoutValue.Value;

        /// <summary>
        /// Cast as has-value-flag
        /// </summary>
        /// <param name="timeoutValue">Timeout value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in TimeoutValue<T> timeoutValue) => timeoutValue.HasValue;
    }
}
