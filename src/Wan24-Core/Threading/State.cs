using System.Collections.Concurrent;

namespace wan24.Core.Threading
{
    /// <summary>
    /// State
    /// </summary>
    public class State : DisposableBase
    {
        /// <summary>
        /// Set event (raised when set)
        /// </summary>
        protected readonly ManualResetEventSlim SetEvent = new(initialState: false);
        /// <summary>
        /// Reset event (raised when reset)
        /// </summary>
        protected readonly ManualResetEventSlim ResetEvent = new(initialState: true);
        /// <summary>
        /// Set queue (will reset the state)
        /// </summary>
        protected readonly ConcurrentQueue<State> SetQueue = new();
        /// <summary>
        /// Reset queue (will set the state)
        /// </summary>
        protected readonly ConcurrentQueue<State> ResetQueue = new();
        /// <summary>
        /// Is set?
        /// </summary>
        protected volatile bool _IsSet = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialState">Initial state</param>
        public State(bool initialState = false) : base() => Set(initialState);

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// State to ensure when disposing
        /// </summary>
        public bool? StateWhenDisposing { get; set; }

        /// <summary>
        /// Is set?
        /// </summary>
        public bool IsSet => _IsSet;

        /// <summary>
        /// Set a state
        /// </summary>
        /// <param name="state">State</param>
        /// <returns>Changed to the desired state?</returns>
        public bool Set(bool state)
        {
            bool setState = state,// The original desired state to set
                res = true;// The result
            lock (SyncObject)
            {
                // Set the desired state
                EnsureUndisposed();
                if (state == _IsSet) return false;
                _IsSet = state;
                RaiseLockedEvents();
                // Process the set/reset queue
                for (
                    State? e;
                    EnsureUndisposed() && ((state && SetQueue.TryDequeue(out e)) || (!state && ResetQueue.TryDequeue(out e))) && !e.IsSet;
                    e.Dispose()
                    )
                    try
                    {
                        state = _IsSet = !state;
                        RaiseLockedEvents();
                        e.Set(state: true);
                    }
                    catch
                    {
                        e.Dispose();
                        throw;
                    }
                // The result is TRUE, if the final state is the desired state
                res = _IsSet == setState;
            }
            // Raise the non-locked events
            RaiseNonLockedEvents(state);
            return res;
        }

        /// <summary>
        /// Set a state, if it would change the current state (don't lock, if the state does match already)
        /// </summary>
        /// <param name="state">State</param>
        /// <returns>Changed to the desired state?</returns>
        public bool SetIfEffects(bool state) => _IsSet != state && Set(state);

        /// <summary>
        /// Wait set
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Is set?</returns>
        public bool WaitSet(TimeSpan? timeout = null)
        {
            if (IsDisposing) return _IsSet;
            if (timeout == null)
            {
                SetEvent.Wait();
                return _IsSet;
            }
            else
            {
                return SetEvent.Wait(timeout.Value) && _IsSet;
            }
        }

        /// <summary>
        /// Wait set
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Is set?</returns>
        /// <exception cref="OperationCanceledException">Cancelled</exception>
        public bool WaitSet(CancellationToken cancellationToken)
        {
            if (IsDisposing) return _IsSet;
            SetEvent.Wait(cancellationToken);
            return _IsSet;
        }

        /// <summary>
        /// Wait set and reset
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Is reset?</returns>
        public bool WaitSetAndReset(TimeSpan? timeout = null)
        {
            // Get a queue
            State e;
            lock (DisposeSyncObject)
            {
                EnsureUndisposed();
                e = new(initialState: false);
                SetQueue.Enqueue(e);
            }
            // Wait until the queue was processed
            if (!WaitSet(timeout)) return false;
            if (timeout != null) return e.WaitSet(timeout.Value);
            e.WaitSet();
            return true;
        }

        /// <summary>
        /// Wait set and reset
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Is reset?</returns>
        public bool WaitSetAndReset(CancellationToken cancellationToken)
        {
            // Get a queue
            State e;
            lock (DisposeSyncObject)
            {
                EnsureUndisposed();
                e = new(initialState: false);
                SetQueue.Enqueue(e);
            }
            // Wait until the queue was processed
            return WaitSet(cancellationToken) && e.WaitSet(cancellationToken);
        }

        /// <summary>
        /// Wait reset
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Is reset?</returns>
        public bool WaitReset(TimeSpan? timeout = null)
        {
            if (IsDisposing) return !_IsSet;
            if (timeout == null)
            {
                ResetEvent.Wait();
                return !_IsSet;
            }
            else
            {
                return ResetEvent.Wait(timeout.Value) && !_IsSet;
            }
        }

        /// <summary>
        /// Wait reset
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Is reset?</returns>
        /// <exception cref="OperationCanceledException">Cancelled</exception>
        public bool WaitReset(CancellationToken cancellationToken)
        {
            if (IsDisposing) return !_IsSet;
            ResetEvent.Wait(cancellationToken);
            return !_IsSet;
        }

        /// <summary>
        /// Wait reset and set
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Is set?</returns>
        public bool WaitResetAndSet(TimeSpan? timeout = null)
        {
            // Get a queue
            State e;
            lock (DisposeSyncObject)
            {
                EnsureUndisposed();
                e = new(initialState: false);
                ResetQueue.Enqueue(e);
            }
            // Wait until the queue was processed
            if (!WaitSet(timeout)) return false;
            if (timeout != null) return e.WaitSet(timeout.Value);
            e.WaitSet();
            return true;
        }

        /// <summary>
        /// Wait reset and set
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Is set?</returns>
        public bool WaitResetAndSet(CancellationToken cancellationToken)
        {
            // Get a queue
            State e;
            lock (DisposeSyncObject)
            {
                EnsureUndisposed();
                e = new(initialState: false);
                ResetQueue.Enqueue(e);
            }
            // Wait until the queue was processed
            return WaitReset(cancellationToken) && e.WaitSet(cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            // Set the ensured disposing state (don't process the queues)
            if (StateWhenDisposing != null)
            {
                bool changed = false;// Has the state been changed?
                // Ensure the desired disposing state
                lock (SyncObject)
                    if (changed = StateWhenDisposing.Value != _IsSet)
                    {
                        // Change the state
                        _IsSet = StateWhenDisposing.Value;
                        // Raise events during locked
                        RaiseLockedEvents();
                    }
                // Raise non-locked events, if the state was changed
                if (changed) RaiseNonLockedEvents(StateWhenDisposing.Value);
            }
            // Dispose the infrastructure
            SetEvent.Dispose();
            ResetEvent.Dispose();
            SetQueue.DisposeAll();
            ResetQueue.DisposeAll();
        }

        /// <summary>
        /// Raise the state events during being locked
        /// </summary>
        private void RaiseLockedEvents()
        {
            if (_IsSet)
            {
                ResetEvent.Reset();
                SetEvent.Set();
                OnSetLocked?.Invoke(this, new());
            }
            else
            {
                SetEvent.Reset();
                ResetEvent.Set();
                OnResetLocked?.Invoke(this, new());
            }
            OnStateChangedLocked?.Invoke(this, new());
        }

        /// <summary>
        /// Raise the state events without being locked
        /// </summary>
        /// <param name="state">State</param>
        private void RaiseNonLockedEvents(bool state)
        {
            if (state)
            {
                OnSet?.Invoke(this, new());
            }
            else
            {
                OnReset?.Invoke(this, new());
            }
            OnStateChanged?.Invoke(this, new());
        }

        /// <summary>
        /// Delegate for state events
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="e">Event arguments</param>
        public delegate void State_Delegate(State state, EventArgs e);

        /// <summary>
        /// Raised on state change during thread locked
        /// </summary>
        public event State_Delegate? OnStateChangedLocked;

        /// <summary>
        /// Raised on state change
        /// </summary>
        public event State_Delegate? OnStateChanged;

        /// <summary>
        /// Raised on set during thread locked
        /// </summary>
        public event State_Delegate? OnSetLocked;

        /// <summary>
        /// Raised on set
        /// </summary>
        public event State_Delegate? OnSet;

        /// <summary>
        /// Raised on reset during thread locked
        /// </summary>
        public event State_Delegate? OnResetLocked;

        /// <summary>
        /// Raised on reset
        /// </summary>
        public event State_Delegate? OnReset;

        /// <summary>
        /// Cast as boolean (is set?)
        /// </summary>
        /// <param name="state">State</param>
        public static implicit operator bool(State state) => state.IsSet;
    }
}
