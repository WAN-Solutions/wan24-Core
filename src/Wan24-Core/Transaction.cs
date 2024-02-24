using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Transaction
    /// </summary>
    public sealed class Transaction : DisposableBase
    {
        /// <summary>
        /// Synchronous rollback methods
        /// </summary>
        private readonly Dictionary<int, SyncRollback_Delegate> SyncRollback = [];
        /// <summary>
        /// Asynchronous rollback methods
        /// </summary>
        private readonly Dictionary<int, AsyncRollback_Delegate> AsyncRollback = [];
        /// <summary>
        /// Action return values
        /// </summary>
        private readonly List<object?> ReturnValues = [];
        /// <summary>
        /// Thread synchronization helper
        /// </summary>
        private readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public Transaction() : base() { }

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
        public int ActionCount => ReturnValues.Count;

        /// <summary>
        /// Is committed?
        /// </summary>
        public bool IsCommitted => ActionCount < 1;

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback action</param>
        public void Execute(Action action, SyncRollback_Delegate rollback)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            SyncRollback[ActionCount] = rollback;
            ReturnValues.Add(null);
            action();
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback action</param>
        /// <returns>Return value</returns>
        public T Execute<T>(Func<T> action, SyncRollback_Delegate rollback)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            SyncRollback[ActionCount] = rollback;
            ReturnValues.Add(null);
            T res = action();
            ReturnValues[^1] = res;
            return res;
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ExecuteAsync(Func<Task> action, AsyncRollback_Delegate rollback, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            AsyncRollback[ActionCount] = rollback;
            ReturnValues.Add(null);
            await action().DynamicContext();
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="rollback">Rollback action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, AsyncRollback_Delegate rollback, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            AsyncRollback[ActionCount] = rollback;
            ReturnValues.Add(null);
            T res = await action().DynamicContext();
            ReturnValues[^1] = res;
            return res;
        }

        /// <summary>
        /// Append a transaction
        /// </summary>
        /// <param name="transaction">Transaction (won't be disposed)</param>
        public void Append(Transaction transaction) => Execute(() => { }, (ta, ret) => transaction.Rollback());

        /// <summary>
        /// Append a transaction
        /// </summary>
        /// <param name="transaction">Transaction (won't be disposed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task AppendAsync(Transaction transaction, CancellationToken cancellationToken = default)
            => ExecuteAsync(() => Task.CompletedTask, (ta, ret, ct) => transaction.RollbackAsync(ct), cancellationToken);

        /// <summary>
        /// Append a transaction
        /// </summary>
        /// <param name="transaction">Transaction (won't be disposed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task AppendAsync(ParallelTransaction transaction, CancellationToken cancellationToken = default)
            => ExecuteAsync(() => Task.CompletedTask, (ta, ret, ct) => transaction.RollbackAsync(ct), cancellationToken);

        /// <summary>
        /// Commit
        /// </summary>
        public void Commit()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            SyncRollback.Clear();
            AsyncRollback.Clear();
            ReturnValues.Clear();
        }

        /// <summary>
        /// Rollback
        /// </summary>
        public void Rollback()
        {
            EnsureUndisposed(allowDisposing: true);
            if (IsCommitted) return;
            if (Debug) Logging.WriteDebug($"{this} rolling back {ActionCount} actions");
            using SemaphoreSyncContext ssc = Sync;
            for (int i = ActionCount - 1; i >= 0; i--)
                try
                {
                    if (SyncRollback.TryGetValue(i, out SyncRollback_Delegate? syncRollback))
                    {
                        syncRollback(this, ReturnValues.Count <= i ? null : ReturnValues[i]);
                    }
                    else if (AsyncRollback.TryGetValue(i, out AsyncRollback_Delegate? asyncRollback))
                    {
                        asyncRollback(this, ReturnValues.Count <= i ? null : ReturnValues[i], CancellationToken.None).Wait();
                    }
                    else
                    {
                        throw new InvalidProgramException($"Rollback action #{i} not found");
                    }
                }
                catch (Exception ex)
                {
                    if (Error) Logging.WriteError($"{this} rollback error at #{i}: {ex.Message}");
                    throw;
                }
            SyncRollback.Clear();
            AsyncRollback.Clear();
            ReturnValues.Clear();
        }

        /// <summary>
        /// Rollback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed(allowDisposing: true);
            if (IsCommitted) return;
            if (Debug) Logging.WriteDebug($"{this} rolling back {ActionCount} actions");
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            for (int i = ActionCount - 1; i >= 0; i--)
                try
                {
                    if (AsyncRollback.TryGetValue(i, out AsyncRollback_Delegate? asyncRollback))
                    {
                        await asyncRollback(this, ReturnValues.Count <= i ? null : ReturnValues[i], cancellationToken).DynamicContext();
                    }
                    else if (SyncRollback.TryGetValue(i, out SyncRollback_Delegate? syncRollback))
                    {
                        syncRollback(this, ReturnValues.Count <= i ? null : ReturnValues[i]);
                    }
                    else
                    {
                        throw new InvalidProgramException($"Rollback action #{i} not found");
                    }
                }
                catch (Exception ex)
                {
                    if (Error) Logging.WriteError($"{this} rollback error at #{i}: {ex.Message}");
                    throw;
                }
            SyncRollback.Clear();
            AsyncRollback.Clear();
            ReturnValues.Clear();
        }

        /// <inheritdoc/>
        public override string? ToString() => Name ?? base.ToString();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!IsCommitted)
            {
                if (Warning) Logging.WriteWarning($"{this} rolling back during disposing");
                try
                {
                    Rollback();
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new($"{this} failed to rollback during disposing", ex));
                }
            }
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (!IsCommitted)
            {
                if (Warning) Logging.WriteWarning($"{this} rolling back during disposing");
                try
                {
                    await RollbackAsync().DynamicContext();
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new($"{this} failed to rollback during disposing", ex));
                }
            }
            await Sync.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for a rollback action
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <param name="returnValue">Action return value</param>
        public delegate void SyncRollback_Delegate(Transaction transaction, object? returnValue);

        /// <summary>
        /// Delegate for a rollback action
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <param name="returnValue">Action return value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task AsyncRollback_Delegate(Transaction transaction, object? returnValue, CancellationToken cancellationToken);
    }
}
