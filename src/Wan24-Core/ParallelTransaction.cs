using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Parallel transaction
    /// </summary>
    public sealed partial class ParallelTransaction : DisposableBase, ITransaction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ParallelTransaction() : base() => Initialize();

        /// <summary>
        /// Get an action task
        /// </summary>
        /// <param name="index">Action index</param>
        /// <returns>Task</returns>
        public Task<object?> this[int index]
        {
            get
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
                if (!_RunningActions.TryGetValue(index, out RunningAction? action)) throw new ArgumentOutOfRangeException(nameof(index));
                return action.Completion.Task;
            }
        }

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Any tagged object
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Number of executed actions since the last commit
        /// </summary>
        public int ActionCount { get; private set; }

        /// <summary>
        /// Is committed?
        /// </summary>
        public bool IsCommitted => ActionCount == 0 && (IsDisposing || !Cancellation.IsCancellationRequested);

        /// <summary>
        /// Cancel running actions on error?
        /// </summary>
        public bool CancelOnError { get; set; } = true;

        /// <summary>
        /// Number of running actions
        /// </summary>
        public int RunningActions => _RunningActions.Values.Count(a => a.InnerTask is null || !a.InnerTask.IsCompleted);

        /// <summary>
        /// Cancellation token (canceled on error)
        /// </summary>
        public CancellationToken CancelToken { get; private set; }

        /// <summary>
        /// Running action tasks
        /// </summary>
        public IReadOnlyDictionary<int, Task<object?>> Tasks
            => new Dictionary<int, Task<object?>>(_RunningActions.Select(a => new KeyValuePair<int, Task<object?>>(a.Key, a.Value.Completion.Task)));

        /// <summary>
        /// Exceptions
        /// </summary>
        public IReadOnlyDictionary<int, Exception?> Exceptions => new Dictionary<int, Exception?>(_Exceptions);

        /// <summary>
        /// Is exceptional?
        /// </summary>
        public bool IsExceptional => !_Exceptions.IsEmpty;

        /// <summary>
        /// Return values
        /// </summary>
        public IReadOnlyDictionary<int, object?> ReturnValues => new Dictionary<int, object?>(_ReturnValues);

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback</param>
        /// <param name="commit">Commit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Running action result</returns>
        /// <exception cref="InvalidOperationException">Previous actions had exceptions</exception>
        public RunningActionResult Execute(in Action_Delegate action, in Rollback_Delegate rollback, in Commit_Delegate? commit = null, in CancellationToken cancellationToken = default)
        {
            RunningAction runningAction;
            RunningActionResult result;
            int index;
            using (SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken))
            {
                EnsureUndisposed();
                Cancellation.Token.ThrowIfCancellationRequested();
                if (IsExceptional) throw new InvalidOperationException("Previous actions had exceptions");
                index = ActionCount;
                result = new(index);
                runningAction = new(this, result);
                result.Task = runningAction.Completion.Task;
                _RunningActions[index] = runningAction;
                RollbackActions[index] = rollback;
                CommitActions[index] = commit;
                ActionCount++;
            }
            runningAction.RunActionAsync(action, cancellationToken);
            return result;
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback</param>
        /// <param name="commit">Commit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Running action result</returns>
        /// <exception cref="InvalidOperationException">Previous actions had exceptions</exception>
        public RunningActionResult Execute(
            in ReturningAction_Delegate action, 
            in Rollback_Delegate rollback, 
            in Commit_Delegate? commit = null, 
            in CancellationToken cancellationToken = default
            )
        {
            RunningAction runningAction;
            RunningActionResult result;
            int index;
            using (SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken))
            {
                EnsureUndisposed();
                Cancellation.Token.ThrowIfCancellationRequested();
                if (IsExceptional) throw new InvalidOperationException("Previous actions had exceptions");
                index = ActionCount;
                result = new(index);
                runningAction = new(this, result);
                result.Task = runningAction.Completion.Task;
                _RunningActions[index] = runningAction;
                RollbackActions[index] = rollback;
                CommitActions[index] = commit;
                ActionCount++;
            }
            runningAction.RunActionAsync(action, cancellationToken);
            return result;
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback</param>
        /// <param name="commit">Commit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Running action result</returns>
        /// <exception cref="InvalidOperationException">Previous actions had exceptions</exception>
        public async Task<RunningActionResult> ExecuteAsync(Action_Delegate action, Rollback_Delegate rollback, Commit_Delegate? commit = null, CancellationToken cancellationToken = default)
        {
            RunningAction runningAction;
            RunningActionResult result;
            int index;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                EnsureUndisposed();
                Cancellation.Token.ThrowIfCancellationRequested();
                if (IsExceptional) throw new InvalidOperationException("Previous actions had exceptions");
                index = ActionCount;
                result = new(index);
                runningAction = new(this, result);
                result.Task = runningAction.Completion.Task;
                _RunningActions[index] = runningAction;
                RollbackActions[index] = rollback;
                CommitActions[index] = commit;
                ActionCount++;
            }
            runningAction.RunActionAsync(action, cancellationToken);
            return result;
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback</param>
        /// <param name="commit">Commit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Running action result</returns>
        /// <exception cref="InvalidOperationException">Previous actions had exceptions</exception>
        public async Task<RunningActionResult> ExecuteAsync(
            ReturningAction_Delegate action, 
            Rollback_Delegate rollback, 
            Commit_Delegate? commit = null, 
            CancellationToken cancellationToken = default
            )
        {
            RunningAction runningAction;
            RunningActionResult result;
            int index;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                EnsureUndisposed();
                Cancellation.Token.ThrowIfCancellationRequested();
                if (IsExceptional) throw new InvalidOperationException("Previous actions had exceptions");
                index = ActionCount;
                result = new(index);
                runningAction = new(this, result);
                result.Task = runningAction.Completion.Task;
                _RunningActions[index] = runningAction;
                RollbackActions[index] = rollback;
                CommitActions[index] = commit;
                ActionCount++;
            }
            runningAction.RunActionAsync(action, cancellationToken);
            return result;
        }

        /// <summary>
        /// Append a transaction
        /// </summary>
        /// <param name="transaction">Transaction (won't be disposed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Running action result</returns>
        public RunningActionResult Append(ITransaction transaction, in CancellationToken cancellationToken = default)
            => Execute(
                (ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    return Task.CompletedTask;
                },
                async (ta, ret, ct) => await transaction.RollbackAsync(ct).DynamicContext(),
                async (ta, ret, ct) => await transaction.CommitAsync(ct).DynamicContext(),
                cancellationToken);

        /// <summary>
        /// Append a transaction
        /// </summary>
        /// <param name="transaction">Transaction (won't be disposed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Running action result</returns>
        public Task<RunningActionResult> AppendAsync(ITransaction transaction, CancellationToken cancellationToken = default)
            => ExecuteAsync(
                (ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    return Task.CompletedTask;
                },
                async (ta, ret, ct) => await transaction.RollbackAsync(ct).DynamicContext(),
                async (ta, ret, ct) => await transaction.CommitAsync(ct).DynamicContext(),
                cancellationToken);

        /// <summary>
        /// Wait all running actions to be done
        /// </summary>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task WaitDoneAsync(bool throwOnError = true, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return WaitDoneIntAsync(throwOnError, cancellationToken);
        }

        /// <inheritdoc/>
        void ITransaction.Commit() => Commit();

        /// <summary>
        /// Commit all actions since the last commit
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="InvalidOperationException">Actions are running, or have been canceled, or had exceptions</exception>
        public void Commit(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            EnsureUndisposed();
            if (Debug) Logging.WriteDebug($"{this} commit called");
            if (RunningActions != 0) throw new InvalidOperationException("Some actions are still running");
            if (Cancellation.IsCancellationRequested) throw new InvalidOperationException("Transaction has been canceled");
            if (IsExceptional) throw new InvalidOperationException("Some actions had exceptions");
            if (Debug) Logging.WriteDebug($"{this} committing {ActionCount} actions");
            CommitIntAsync(cancellationToken).GetAwaiter().GetResult();
            Initialize();
        }

        /// <inheritdoc/>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (Debug) Logging.WriteDebug($"{this} commit called");
            if (RunningActions != 0) throw new InvalidOperationException("Some actions are still running");
            if (Cancellation.IsCancellationRequested) throw new InvalidOperationException("Transaction has been canceled");
            if (IsExceptional) throw new InvalidOperationException("Some actions had exceptions");
            if (Debug) Logging.WriteDebug($"{this} committing {ActionCount} actions");
            await CommitIntAsync(cancellationToken).DynamicContext();
            Initialize();
        }

        /// <summary>
        /// Cancel all running actions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task CancelAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (Debug) Logging.WriteDebug($"{this} cancellation called");
            await CancelIntAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        void ITransaction.Rollback() => Rollback();

        /// <summary>
        /// Cancel all running actions and perform a rollback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void Rollback(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            EnsureUndisposed();
            if (Debug) Logging.WriteDebug($"{this} rollback called");
            RollbackIntAsync(cancellationToken).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (Debug) Logging.WriteDebug($"{this} rollback called");
            await RollbackIntAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override string? ToString() => Name ?? base.ToString();

        /// <summary>
        /// Delegate for an action
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task Action_Delegate(CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for an action which returns a value
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        public delegate Task<object?> ReturningAction_Delegate(CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a rollback action
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <param name="returnValue">Action return value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task Rollback_Delegate(ParallelTransaction transaction, object? returnValue, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a commit action
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <param name="returnValue">Action return value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task Commit_Delegate(ParallelTransaction transaction, object? returnValue, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for an <see cref="OnError"/> handler
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <param name="e">Arguments</param>
        public delegate void Error_Delegate(ParallelTransaction transaction, ErrorEventArgs e);
        /// <summary>
        /// Raised on error
        /// </summary>
        public event Error_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        /// <param name="action">Action index</param>
        /// <param name="ex">Exception</param>
        private void RaiseOnError(in int action, in Exception ex)
        {
            if (CancelOnError)
            {
                if (Warning) Logging.WriteWarning($"{this} canceling asynchronous on error from action #{action}: ({ex.GetType()}) {ex.Message}");
                _ = CancelAsync().DynamicContext();
            }
            OnError?.Invoke(this, new(action, ex));
        }

        /// <summary>
        /// Delegate for an <see cref="OnDone"/> handler
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <param name="e">Arguments</param>
        public delegate void Done_Delegate(ParallelTransaction transaction, EventArgs e);
        /// <summary>
        /// Raised when done
        /// </summary>
        public event Done_Delegate? OnDone;
        /// <summary>
        /// Raise the <see cref="OnDone"/> event
        /// </summary>
        private void RaiseOnDone() => OnDone?.Invoke(this, new());

        /// <summary>
        /// Error event arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="action">Action index</param>
        /// <param name="ex">Exception</param>
        public sealed class ErrorEventArgs(in int action, in Exception ex) : EventArgs()
        {
            /// <summary>
            /// Action index
            /// </summary>
            public int Action { get; } = action;

            /// <summary>
            /// Exception
            /// </summary>
            public Exception Exception { get; } = ex;
        }
    }
}
