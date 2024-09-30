using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Reset event
    /// </summary>
    public sealed class ResetEvent : BasicAllDisposableBase
    {
        /// <summary>
        /// Thread synchronization object
        /// </summary>
        private readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Thread synchronization for the synchronous set event
        /// </summary>
        private readonly ManualResetEventSlim? SyncSet;
        /// <summary>
        /// Task completion
        /// </summary>
        private volatile TaskCompletionSource TaskCompletion;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialState">Initial state</param>
        /// <param name="enableSyncWaiting">Enable synchronous waiting?</param>
        public ResetEvent(in bool initialState = false, in bool enableSyncWaiting = true) : base()
        {
            TaskCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            SyncSet = enableSyncWaiting ? new(initialState) : null;
            if (initialState) TaskCompletion.SetResult();
        }

        /// <summary>
        /// Is set?
        /// </summary>
        public bool IsSet => TaskCompletion.Task.IsCompleted;

        /// <summary>
        /// Is synchronous waiting enabled?
        /// </summary>
        public bool SynchronousWaiting => SyncSet is not null;

        /// <summary>
        /// Set the state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Did set?</returns>
        public bool Set(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            EnsureUndisposed();
            if (IsSet) return false;
            TaskCompletion.SetResult();
            SyncSet?.Set();
            return true;
        }

        /// <summary>
        /// Set the state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Did set?</returns>
        public async Task<bool> SetAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (IsSet) return false;
            TaskCompletion.SetResult();
            SyncSet?.Set();
            return true;
        }

        /// <summary>
        /// Reset the state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Did reset?</returns>
        public bool Reset(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            EnsureUndisposed();
            if (!IsSet) return false;
            TaskCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            SyncSet?.Reset();
            return true;
        }

        /// <summary>
        /// Reset the state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Did reset?</returns>
        public async Task<bool> ResetAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (!IsSet) return false;
            TaskCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            SyncSet?.Reset();
            return true;
        }

        /// <summary>
        /// Wait for the state to be set
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException">Canceled</exception>
        public void Wait(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (!SynchronousWaiting) throw new InvalidOperationException("Synchronous waiting disabled");
            SyncSet!.Wait(cancellationToken);
        }

        /// <summary>
        /// Wait for the state to be set
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException">Canceled</exception>
        public async Task WaitAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await TaskCompletion.Task.WaitAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Wait for the state to be set
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <exception cref="TimeoutException">Timeout</exception>
        public void Wait(in TimeSpan timeout)
        {
            EnsureUndisposed();
            if (!SynchronousWaiting) throw new InvalidOperationException("Synchronous waiting disabled");
            SyncSet!.Wait(timeout);
        }

        /// <summary>
        /// Wait for the state to be set
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <exception cref="TimeoutException">Timeout</exception>
        public async Task WaitAsync(TimeSpan timeout)
        {
            EnsureUndisposed();
            await TaskCompletion.Task.WaitAsync(timeout).DynamicContext();
        }

        /// <summary>
        /// Wait for set and reset
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void WaitAndReset(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            while (true)
            {
                Wait(cancellationToken);
                if (Reset(cancellationToken)) return;
            }
        }

        /// <summary>
        /// Wait for set and reset
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task WaitAndResetAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            while (true)
            {
                await WaitAsync(cancellationToken).DynamicContext();
                if (await ResetAsync(cancellationToken).DynamicContext()) return;
            }
        }

        /// <summary>
        /// Wait for set and reset
        /// </summary>
        /// <param name="timeout">Timeout</param>
        public void WaitAndReset(TimeSpan timeout)
        {
            EnsureUndisposed();
            DateTime start = DateTime.Now;
            while (TimeSpanHelper.UpdateTimeout(ref start, ref timeout))
            {
                Wait(timeout);
                if (Reset()) return;
            }
            throw new TimeoutException();
        }

        /// <summary>
        /// Wait for set and reset
        /// </summary>
        /// <param name="timeout">Timeout</param>
        public async Task WaitAndResetAsync(TimeSpan timeout)
        {
            EnsureUndisposed();
            DateTime start = DateTime.Now;
            while (TimeSpanHelper.UpdateTimeout(ref start, ref timeout))
            {
                await WaitAsync(timeout).DynamicContext();
                if (await ResetAsync().DynamicContext()) return;
            }
            throw new TimeoutException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            using ManualResetEventSlim? syncSet = SyncSet;
            if (IsSet) TaskCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletion.SetException(new ObjectDisposedException(GetType().ToString()));
            syncSet?.Set();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext();
            using ManualResetEventSlim? syncSet = SyncSet;
            if (IsSet) TaskCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletion.SetException(new ObjectDisposedException(GetType().ToString()));
            syncSet?.Set();
        }

        /// <summary>
        /// Cast as set-flag
        /// </summary>
        /// <param name="e">Event</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in ResetEvent e) => e.IsSet;
    }
}
