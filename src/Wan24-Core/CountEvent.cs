namespace wan24.Core
{
    /// <summary>
    /// Count event (used to wait for a number of events until cancellation (or timeout))
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class CountEvent() : SimpleDisposableBase()
    {
        /// <summary>
        /// Cancellation
        /// </summary>
        protected readonly CancellationTokenSource Cancellation = new();
        /// <summary>
        /// Total event count
        /// </summary>
        protected volatile float _TotalCount = 0;

        /// <summary>
        /// If to count <see cref="TotalCount"/> when <see cref="Raise(in int)"/> was called
        /// </summary>
        public bool CountTotal { get; init; } = true;

        /// <summary>
        /// Total event count (increased when <see cref="Raise(in int)"/> was called)
        /// </summary>
        public float TotalCount => _TotalCount;

        /// <summary>
        /// Raise an event
        /// </summary>
        public virtual void Raise() => Raise(count: 1);

        /// <summary>
        /// Raise a number of events
        /// </summary>
        /// <param name="count">Count</param>
        public virtual void Raise(in int count)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            if (CountTotal) _TotalCount += (uint)count;
            RaiseOnCount(count);
        }

        /// <summary>
        /// Wait for a number of events
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task WaitAsync(int count, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            cancellationToken.ThrowIfCancellationRequested();
            await WaitIntAsync(count, new(Methods.Wait, default, cancellationToken), cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Wait for a number of events and don't throw, if <c>cancellationToken</c> was canceled
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of events (possibly until cancellation)</returns>
        public virtual async Task<int> TryWaitAsync(int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            Counter counter = new(Methods.TryWait, default, cancellationToken);
            try
            {
                await WaitIntAsync(count, counter, cancellationToken).DynamicContext();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            lock (counter) return counter.Count;
        }

        /// <summary>
        /// Wait for a number of events and don't throw on timeout
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of events within the timeout</returns>
        public virtual async Task<int> WaitAsync(int count, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            cancellationToken.ThrowIfCancellationRequested();
            using CancellationTokenSource cts = cancellationToken.CombineWith(timeout);
            Counter counter = new(Methods.WaitTimeout, timeout, cancellationToken);
            try
            {
                await WaitIntAsync(count, counter, cts.Token).DynamicContext();
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
            }
            lock (counter) return counter.Count;
        }

        /// <summary>
        /// Wait for a number of events and don't throw on timeout or cancellation
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of events within the timeout (or possibly until cancellation)</returns>
        public virtual async Task<int> TryWaitAsync(int count, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            using CancellationTokenSource cts = cancellationToken.CombineWith(timeout);
            Counter counter = new(Methods.TryWaitTimeout, timeout, cancellationToken);
            try
            {
                await WaitIntAsync(count, counter, cts.Token).DynamicContext();
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
            }
            lock (counter) return counter.Count;
        }

        /// <summary>
        /// Wait for a total number of events (<see cref="TotalCount"/>)
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task WaitTotalCountAsync(float count, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            if (!CountTotal) throw new InvalidOperationException();
            cancellationToken.ThrowIfCancellationRequested();
            await WaitTotalCountIntAsync(count, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Wait for a total number of events (<see cref="TotalCount"/>) and don't throw, if <c>cancellationToken</c> was canceled
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task TryWaitTotalCountAsync(float count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            if (!CountTotal) throw new InvalidOperationException();
            try
            {
                await WaitTotalCountIntAsync(count, cancellationToken).DynamicContext();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        /// <summary>
        /// Wait for a total number of events (<see cref="TotalCount"/>) and don't throw on timeout
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task WaitTotalCountAsync(float count, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            if (!CountTotal) throw new InvalidOperationException();
            cancellationToken.ThrowIfCancellationRequested();
            using CancellationTokenSource cts = cancellationToken.CombineWith(timeout);
            try
            {
                await WaitTotalCountIntAsync(count, cts.Token).DynamicContext();
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
            }
        }

        /// <summary>
        /// Wait for a total number of events (<see cref="TotalCount"/>) and don't throw on timeout or cancellation
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task TryWaitTotalCountAsync(float count, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 1, nameof(count));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            if (!CountTotal) throw new InvalidOperationException();
            using CancellationTokenSource cts = cancellationToken.CombineWith(timeout);
            try
            {
                await WaitTotalCountIntAsync(count, cts.Token).DynamicContext();
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
            }
        }

        /// <summary>
        /// Reset the <see cref="TotalCount"/>
        /// </summary>
        public virtual void ResetTotalCount()
        {
            EnsureUndisposed();
            if (!CountTotal) throw new InvalidOperationException();
            _TotalCount = 0;
        }

        /// <summary>
        /// Wait for a number of events
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="counter">Counter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task WaitIntAsync(int count, Counter counter, CancellationToken cancellationToken)
        {
            TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenSource cts = cancellationToken.CombineWith(Cancellation.Token);
            void HandleOnCountInt(CountEvent sender, CountEventArgs e) => HandleOnCount(count, counter, tcs, e, cancellationToken);
            OnCount += HandleOnCountInt;
            try
            {
                await tcs.Task.WaitAsync(cts.Token).DynamicContext();
            }
            finally
            {
                OnCount -= HandleOnCountInt;
            }
        }

        /// <summary>
        /// Wait for a number of events counted in <see cref="TotalCount"/>
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task WaitTotalCountIntAsync(float count, CancellationToken cancellationToken)
        {
            TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenSource cts = cancellationToken.CombineWith(Cancellation.Token);
            void HandleOnCountInt(CountEvent sender, CountEventArgs e) => HandleOnCountTotal(count, tcs, e, cancellationToken);
            OnCount += HandleOnCountInt;
            try
            {
                await tcs.Task.WaitAsync(cts.Token).DynamicContext();
            }
            finally
            {
                OnCount -= HandleOnCountInt;
            }
        }

        /// <summary>
        /// Handle an event count
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="counter">Counter</param>
        /// <param name="taskCompletion">Waiting method task completion</param>
        /// <param name="e">Count event arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual void HandleOnCount(
            in int count,
            in Counter counter,
            in TaskCompletionSource taskCompletion,
            in CountEventArgs e,
            in CancellationToken cancellationToken
            )
        {
            lock (counter) counter.Count += e.Count;
            if (counter.Count >= count) taskCompletion.TrySetResult();
        }

        /// <summary>
        /// Handle an event count
        /// </summary>
        /// <param name="count">Number of events to wait for</param>
        /// <param name="taskCompletion">Waiting method task completion</param>
        /// <param name="e">Count event arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual void HandleOnCountTotal(
            in float count,
            in TaskCompletionSource taskCompletion,
            in CountEventArgs e,
            in CancellationToken cancellationToken
            )
        {
            if (_TotalCount >= count) taskCompletion.TrySetResult();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Cancellation.Cancel();
            Cancellation.Dispose();
        }

        /// <summary>
        /// Delegate for an <see cref="OnCount"/> event handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        public delegate void Count_Delegate(CountEvent sender, CountEventArgs e);
        /// <summary>
        /// Raised when <see cref="Raise(in int)"/> was called
        /// </summary>
        public event Count_Delegate? OnCount;
        /// <summary>
        /// Raise the <see cref="OnCount"/> event
        /// </summary>
        /// <param name="count">Count</param>
        protected virtual void RaiseOnCount(in int count) => OnCount?.Invoke(this, new(count));

        /// <summary>
        /// <see cref="OnCount"/> event arguments
        /// </summary>
        /// <param name="count">Count</param>
        public class CountEventArgs(in int count) : EventArgs()
        {
            /// <summary>
            /// Count
            /// </summary>
            public int Count { get; } = count;
        }

        /// <summary>
        /// Counter
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="method">Waiting method</param>
        /// <param name="timeout">Caller timeout</param>
        /// <param name="cancellationToken">Caller cancellation token</param>
        protected class Counter(in Methods method, in TimeSpan timeout, in CancellationToken cancellationToken)
        {
            /// <summary>
            /// Waiting method
            /// </summary>
            public readonly Methods Method = method;
            /// <summary>
            /// Caller timeout
            /// </summary>
            public readonly TimeSpan Timeout = timeout;
            /// <summary>
            /// Caller cancellation token
            /// </summary>
            public readonly CancellationToken Cancellation = cancellationToken;
            /// <summary>
            /// Count
            /// </summary>
            public int Count = 0;
        }

        /// <summary>
        /// Wait methods enumeration
        /// </summary>
        protected enum Methods
        {
            /// <summary>
            /// <see cref="WaitAsync(int, CancellationToken)"/>
            /// </summary>
            Wait,
            /// <summary>
            /// <see cref="TryWaitAsync(int, CancellationToken)"/>
            /// </summary>
            TryWait,
            /// <summary>
            /// <see cref="WaitAsync(int, TimeSpan, CancellationToken)"/>
            /// </summary>
            WaitTimeout,
            /// <summary>
            /// <see cref="TryWaitAsync(int, TimeSpan, CancellationToken)"/>
            /// </summary>
            TryWaitTimeout
        }
    }
}
