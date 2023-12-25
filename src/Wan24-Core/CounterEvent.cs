using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Counter event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="initialValue">Initial counter value</param>
    public sealed partial class CounterEvent(in int initialValue = 0) : DisposableBase()
    {
        /// <summary>
        /// Current counter value
        /// </summary>
        public int Counter => _Counter;

        /// <summary>
        /// Min. counter value
        /// </summary>
        public int MinCounter { get; init; } = int.MinValue;

        /// <summary>
        /// Max. counter value
        /// </summary>
        public int MaxCounter { get; init; } = int.MaxValue;

        /// <summary>
        /// Fit a new counter value into the range (to avoid an exception when setting an out of range value)?
        /// </summary>
        public bool FitNewValue { get; init; }

        /// <summary>
        /// Force raising the <see cref="OnCount"/> event (even if the new counter value is equal to the old counter value)?
        /// </summary>
        public bool ForceRaiseEvent { get; init; }

        /// <summary>
        /// Set the counter to a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public void SetCounter(in int value, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            SetCounterInt(value, throwOnError: true, fitNewValue: false);
        }

        /// <summary>
        /// Set the counter to a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetCounterAsync(int value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            SetCounterInt(value, throwOnError: true, fitNewValue: false);
        }

        /// <summary>
        /// Count a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="fitNewValue">Fit the new value into the range (won't override <see cref="FitNewValue"/>)?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New counter value or <see langword="null"/>, if failed (new value is out of range)</returns>
        public int? Count(in int value = 1, in bool fitNewValue = false, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            return SetCounterInt(value, throwOnError: false, fitNewValue) is int res ? res + value : null;
        }

        /// <summary>
        /// Count a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="fitNewValue">Fit the new value into the range (won't override <see cref="FitNewValue"/>)?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New counter value or <see langword="null"/>, if failed (new value is out of range)</returns>
        public async Task<int?> CountAsync(int value = 1, bool fitNewValue = false, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            return SetCounterInt(value, throwOnError: false, fitNewValue) is int res ? res + value : null;
        }

        /// <summary>
        /// Count a value (won't throw on overflow, won't fit the new value into the range)
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New counter value or <see langword="null"/>, if failed (new value is out of range)</returns>
        public int? TryCount(in int value = 1, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            int newValue = _Counter + value;
            if (newValue < MinCounter || newValue > MaxCounter) return null;
            return SetCounterInt(value, throwOnError: false, fitNewValue: false) is int res ? res + value : null;
        }

        /// <summary>
        /// Count a value (won't throw on overflow, won't fit the new value into the range)
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New counter value or <see langword="null"/>, if failed (new value is out of range)</returns>
        public async Task<int?> TryCountAsync(int value = 1, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            int newValue = _Counter + value;
            if (newValue < MinCounter || newValue > MaxCounter) return null;
            return SetCounterInt(value, throwOnError: false, fitNewValue: false) is int res ? res + value : null;
        }

        /// <summary>
        /// Wait for the counter to equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public void WaitCounterEquals(int value, CancellationToken cancellationToken = default)
            => WaitCounter(() =>
            {
                int counter = _Counter;
                return counter == value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task WaitCounterEqualsAsync(int value, CancellationToken cancellationToken = default)
            => WaitCounterAsync(() =>
            {
                int counter = _Counter;
                return counter == value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public int WaitCounterNotEquals(int value, CancellationToken cancellationToken = default)
            => WaitCounter(() =>
            {
                int counter = _Counter;
                return counter != value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public Task<int> WaitCounterNotEqualsAsync(int value, CancellationToken cancellationToken = default)
            => WaitCounterAsync(() =>
            {
                int counter = _Counter;
                return counter != value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be greater or equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public int WaitCounterGreaterOrEquals(int value, CancellationToken cancellationToken = default)
            => WaitCounter(() =>
            {
                int counter = _Counter;
                return counter >= value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be greater or equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public Task<int> WaitCounterGreaterOrEqualsAsync(int value, CancellationToken cancellationToken = default)
            => WaitCounterAsync(() =>
            {
                int counter = _Counter;
                return counter >= value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be lower or equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public int WaitCounterLowerOrEquals(int value, CancellationToken cancellationToken = default)
            => WaitCounter(() =>
            {
                int counter = _Counter;
                return counter <= value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be lower or equal a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public Task<int> WaitCounterLowerOrEqualsAsync(int value, CancellationToken cancellationToken = default)
            => WaitCounterAsync(() =>
            {
                int counter = _Counter;
                return counter <= value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be greater than a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public int WaitCounterGreater(int value, CancellationToken cancellationToken = default)
            => WaitCounter(() =>
            {
                int counter = _Counter;
                return counter > value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be greater than a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public Task<int> WaitCounterGreaterAsync(int value, CancellationToken cancellationToken = default)
            => WaitCounterAsync(() =>
            {
                int counter = _Counter;
                return counter > value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be lower than a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public int WaitCounterLower(int value, CancellationToken cancellationToken = default)
            => WaitCounter(() =>
            {
                int counter = _Counter;
                return counter < value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to lower than a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public Task<int> WaitCounterLowerAsync(int value, CancellationToken cancellationToken = default)
            => WaitCounterAsync(() =>
            {
                int counter = _Counter;
                return counter < value ? counter : null;
            }, cancellationToken);

        /// <summary>
        /// Wait for the counter to be counted
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public int WaitCount(CancellationToken cancellationToken = default) => WaitCounterNotEquals(_Counter, cancellationToken);

        /// <summary>
        /// Wait for the counter to be counted
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public Task<int> WaitCountAsync(CancellationToken cancellationToken = default) => WaitCounterNotEqualsAsync(_Counter, cancellationToken);

        /// <summary>
        /// Wait for a condition
        /// </summary>
        /// <param name="condition">Condition (<see cref="CounterEvent"/> instance is locked during the condition is evaluated)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public int WaitCondition(in Condition_Delegate condition, CancellationToken cancellationToken = default) => WaitCounter(condition, cancellationToken);

        /// <summary>
        /// Wait for a condition
        /// </summary>
        /// <param name="condition">Condition (<see cref="CounterEvent"/> instance is locked during the condition is evaluated)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Counter value</returns>
        public Task<int> WaitConditionAsync(Condition_Delegate condition, CancellationToken cancellationToken = default) => WaitCounterAsync(condition, cancellationToken);

        /// <summary>
        /// Delegate for a condition (<see cref="CounterEvent"/> instance is locked during the condition is evaluated)
        /// </summary>
        /// <returns>Counter value to return or <see langword="null"/>, if the condition wasn't met</returns>
        public delegate int? Condition_Delegate();

        /// <summary>
        /// Delegate for the <see cref="OnCount"/> event
        /// </summary>
        /// <param name="counter">Counter</param>
        /// <param name="e">Arguments</param>
        public delegate void Count_Delegate(CounterEvent counter, CountEventArgs e);
        /// <summary>
        /// Raised when counted (<see cref="CounterEvent"/> instance is locked during event handling)
        /// </summary>
        public event Count_Delegate? OnCount;
        /// <summary>
        /// Raise the <see cref="OnCount"/> event
        /// </summary>
        /// <param name="oldValue">Old value</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void RaiseOnCount(int oldValue)
        {
            for (CountEventArgs e = new(oldValue, _Counter); ; e = new(oldValue, _Counter))
            {
                OnCount?.Invoke(this, e);
                if (!e.SetValue.HasValue) return;
                oldValue = _Counter;
                SetCounterInt(e.SetValue.Value, throwOnError: true, fitNewValue: false, raiseEvent: false);
            }
        }

        /// <summary>
        /// Cast as counter value
        /// </summary>
        /// <param name="counter">Counter</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(CounterEvent counter) => counter.Counter;

        /// <summary>
        /// Arguments for the <see cref="OnCount"/> event
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        public sealed class CountEventArgs(in int oldValue, in int newValue) : EventArgs()
        {
            /// <summary>
            /// Old value
            /// </summary>
            public int OldValue { get; } = oldValue;

            /// <summary>
            /// New value
            /// </summary>
            public int NewValue { get; } = newValue;

            /// <summary>
            /// New value to set after the event was handled, and during the <see cref="CounterEvent"/> instance is still locked
            /// </summary>
            public int? SetValue { get; set; }
        }
    }
}
