using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Internal
    public sealed partial class CounterEvent
    {
        /// <summary>
        /// Thread synchronization object
        /// </summary>
        private readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Counter
        /// </summary>
        private volatile int _Counter = initialValue;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Sync.Dispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await Sync.DisposeAsync().DynamicContext();

        /// <summary>
        /// Set a new counter value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="throwOnError">Throw an exception, if the new value is invalid?</param>
        /// <param name="fitNewValue">Fit the new value into the range?</param>
        /// <param name="raiseEvent">Raise the <see cref="OnCount"/> event?</param>
        /// <returns>Old counter value or <see langword="null"/>, if the new value is out of range</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int? SetCounterInt(int value, in bool throwOnError, in bool fitNewValue, in bool raiseEvent = true)
        {
            if (value < MinCounter || value > MaxCounter)
                if (FitNewValue || fitNewValue)
                {
                    value = value < MinCounter ? MinCounter : MaxCounter;
                }
                else
                {
                    if (!throwOnError) return null;
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            if (!ForceRaiseEvent && value == _Counter) return value;
            int res = _Counter;
            _Counter = value;
            if (raiseEvent) RaiseOnCount(res);
            return res;
        }

        /// <summary>
        /// Wait for a counter condition
        /// </summary>
        /// <param name="condition">Condition (<see cref="CounterEvent"/> instance is locked during the condition is evaluated)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int WaitCounterCondition(Condition_Delegate condition, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            int? res = condition();
            if (res.HasValue) return res.Value;
            using ResetEvent resultEvent = new(initialState: false);
            void HandleCount(CounterEvent counter, CountEventArgs e)
            {
                if (!resultEvent.IsSet && condition() is int ret && resultEvent.Set(CancellationToken.None))
                    res = ret;
            }
            void HandleDisposing(IDisposableObject sender, EventArgs e) => resultEvent.Set(CancellationToken.None);
            OnCount += HandleCount;
            OnDisposing += HandleDisposing;
            try
            {
                resultEvent.Wait(cancellationToken);
                return res ?? throw new ObjectDisposedException(GetType().ToString());
            }
            finally
            {
                OnCount -= HandleCount;
                OnDisposing -= HandleDisposing;
            }
        }

        /// <summary>
        /// Wait for a counter condition
        /// </summary>
        /// <param name="condition">Condition (<see cref="CounterEvent"/> instance is locked during the condition is evaluated)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private async Task<int> WaitCounterConditionAsync(Condition_Delegate condition, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (condition() is int res) return res;
            TaskCompletionSource<int> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            void HandleCount(CounterEvent counter, CountEventArgs e)
            {
                if (condition() is int res) tcs.TrySetResult(res);
            }
            void HandleDisposing(IDisposableObject sender, EventArgs e) => tcs.TrySetException(new ObjectDisposedException(GetType().ToString()));
            OnCount += HandleCount;
            OnDisposing += HandleDisposing;
            try
            {
                return await tcs.Task.WaitAsync(cancellationToken).DynamicContext();
            }
            finally
            {
                OnCount -= HandleCount;
                OnDisposing -= HandleDisposing;
            }
        }
    }
}
