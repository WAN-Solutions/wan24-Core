﻿using System.Collections.Concurrent;
using static wan24.Core.Logging;

namespace wan24.Core
{
    // Internal
    public sealed partial class ParallelTransaction
    {
        /// <summary>
        /// Pending actions
        /// </summary>
        private readonly Dictionary<int, RunningAction> _RunningActions = [];
        /// <summary>
        /// Rollback methods
        /// </summary>
        private readonly Dictionary<int, Rollback_Delegate> Rollback = [];
        /// <summary>
        /// Exception
        /// </summary>
        private readonly ConcurrentDictionary<int, Exception?> _Exceptions = new();
        /// <summary>
        /// Action return values
        /// </summary>
        private readonly ConcurrentDictionary<int, object?> _ReturnValues = new();
        /// <summary>
        /// Thread synchronization helper
        /// </summary>
        private readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Cancellation
        /// </summary>
        private CancellationTokenSource Cancellation = null!;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using CancellationTokenSource cts = Cancellation;
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = sync;
            CancelIntAsync(CancellationToken.None).Wait();
            if (ActionCount != 0)
            {
                if (Warning) Logging.WriteWarning($"{this} rolling back during disposing");
                try
                {
                    RollbackIntAsync(CancellationToken.None).Wait();
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new($"{this} failed to rollback during disposing", ex));
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            using CancellationTokenSource cts = Cancellation;
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = await sync.SyncContextAsync().DynamicContext();
            await CancelIntAsync(CancellationToken.None).DynamicContext();
            if (ActionCount != 0)
            {
                if (Warning) Logging.WriteWarning($"{this} rolling back during disposing");
                try
                {
                    await RollbackIntAsync(CancellationToken.None).DynamicContext();
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new($"{this} failed to rollback during disposing", ex));
                }
            }
        }

        /// <summary>
        /// Cancel all running actions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task CancelIntAsync(CancellationToken cancellationToken)
        {
            if (Cancellation.IsCancellationRequested) return;
            if (Debug) Logging.WriteDebug($"{this} canceling pending actions");
            Cancellation.Cancel();
            if (Debug) Logging.WriteDebug($"{this} waiting for actions after canceled");
            await WaitDoneIntAsync(throwOnError: false, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Wait all running actions to be done
        /// </summary>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task WaitDoneIntAsync(bool throwOnError, CancellationToken cancellationToken)
        {
            if (!throwOnError && IsCommitted) return;
            List<Exception> exceptions = [];
            foreach (Task task in _RunningActions.Values.Select(a => a.Completion.Task))
                try
                {
                    if (!task.IsCompleted)
                    {
                        await task.WaitAsync(cancellationToken).DynamicContext();
                    }
                    else if (!task.IsCompletedSuccessfully)
                    {
                        exceptions.Add(task.Exception ?? throw new InvalidProgramException("Failed task without exception"));
                    }
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cancellationToken) throw;
                    if (throwOnError) exceptions.Add(ex);
                }
                catch (Exception ex)
                {
                    if (throwOnError) exceptions.Add(ex);
                }
            if (exceptions.Count != 0) throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Cancel all running actions and perform a rollback
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task RollbackIntAsync(CancellationToken cancellationToken)
        {
            if (!IsCommitted)
            {
                if (Debug) Logging.WriteDebug($"{this} rolling back {ActionCount} actions");
                await CancelIntAsync(cancellationToken).DynamicContext();
                foreach (KeyValuePair<int, Rollback_Delegate> kvp in Rollback.OrderBy(kvp => kvp.Key))
                {
                    if (Trace) Logging.WriteTrace($"{this} rolling back action #{kvp.Key}");
                    _ReturnValues.TryGetValue(kvp.Key, out object? returnValue);
                    await kvp.Value(this, returnValue, cancellationToken).DynamicContext();
                }
            }
            Initialize();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize()
        {
            if ((Cancellation?.IsCancellationRequested ?? true) && !(Cancellation?.TryReset() ?? false))
            {
                Cancellation?.Dispose();
                Cancellation = new();
                CancelToken = Cancellation.Token;
            }
            _RunningActions.Clear();
            Rollback.Clear();
            _Exceptions.Clear();
            _ReturnValues.Clear();
            ActionCount = 0;
        }

        /// <summary>
        /// Running action
        /// </summary>
        private sealed record class RunningAction
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="transaction">Transaction</param>
            /// <param name="index">Action index</param>
            public RunningAction(in ParallelTransaction transaction, in int index)
            {
                Transaction = transaction;
                Index = index;
            }

            /// <summary>
            /// Transaction
            /// </summary>
            public ParallelTransaction Transaction { get; }

            /// <summary>
            /// Action index
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// Inner action task
            /// </summary>
            public Task? InnerTask { get; private set; } = null;

            /// <summary>
            /// Outer action task
            /// </summary>
            public TaskCompletionSource<object?> Completion { get; } = new();

            /// <summary>
            /// Run the action
            /// </summary>
            /// <param name="action">Action</param>
            /// <param name="cancellationToken">Cancellation token</param>
            public async void RunActionAsync(Action_Delegate action, CancellationToken cancellationToken)
            {
                try
                {
                    if (Trace) Logging.WriteTrace($"{Transaction} running action #{Index}");
                    InnerTask = action(cancellationToken);
                    await InnerTask.DynamicContext();
                    Transaction._ReturnValues[Index] = null;
                    Completion.TrySetResult(null);
                    if (Trace) Logging.WriteTrace($"{Transaction} action #{Index} done");
                }
                catch (Exception ex)
                {
                    if (Warning) Logging.WriteWarning($"{Transaction} action #{Index} failed with an exception: ({ex.GetType()}) {ex.Message}");
                    InnerTask ??= Task.FromException(ex);
                    using (SemaphoreSyncContext ssc = await Transaction.Sync.SyncContextAsync(CancellationToken.None).DynamicContext())
                    {
                        Transaction._Exceptions[Index] = ex;
                        Completion.TrySetException(ex);
                    }
                    Transaction.RaiseOnError(Index, ex);
                }
                if (Transaction.RunningActions == 0) Transaction.RaiseOnDone();
            }

            /// <summary>
            /// Run the action
            /// </summary>
            /// <param name="action">Action</param>
            /// <param name="cancellationToken">Cancellation token</param>
            public async void RunActionAsync(ReturningAction_Delegate action, CancellationToken cancellationToken)
            {
                try
                {
                    if (Trace) Logging.WriteTrace($"{Transaction} running action #{Index}");
                    Task<object?> task = action(cancellationToken);
                    InnerTask = task;
                    object? res = await task.DynamicContext();
                    Transaction._ReturnValues[Index] = res;
                    Completion.TrySetResult(res);
                    if (Trace) Logging.WriteTrace($"{Transaction} action #{Index} done");
                }
                catch (Exception ex)
                {
                    if (Warning) Logging.WriteWarning($"{Transaction} action #{Index} failed with an exception: ({ex.GetType()}) {ex.Message}");
                    InnerTask ??= Task.FromException(ex);
                    using (SemaphoreSyncContext ssc = await Transaction.Sync.SyncContextAsync(CancellationToken.None).DynamicContext())
                    {
                        Transaction._Exceptions[Index] = ex;
                        Completion.TrySetException(ex);
                    }
                    Transaction.RaiseOnError(Index, ex);
                }
                if (Transaction.RunningActions == 0) Transaction.RaiseOnDone();
            }
        }
    }
}
