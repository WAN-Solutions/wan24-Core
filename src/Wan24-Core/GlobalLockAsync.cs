using System.Collections.Concurrent;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Global lock using <see cref="Mutex"/> for use with an asynchronous context (does have to reserve a thread during the mutex exists :( but it's possible to inject actions)
    /// </summary>
    public sealed class GlobalLockAsync : DisposableBase, IGlobalLock
    {
        /// <summary>
        /// Dispose event
        /// </summary>
        private readonly ResetEvent ActionEvent = new();
        /// <summary>
        /// Action queue
        /// </summary>
        private readonly ConcurrentQueue<(Func<object?> Action, TaskCompletionSource<object?> Task)> ActionQueue = new();
        /// <summary>
        /// Global lock thread
        /// </summary>
        private readonly Task Task;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="task">Global lock thread</param>
        /// <param name="createdNew">Created a new mutex?</param>
        /// <exception cref="TimeoutException">Couldn't lock within the timeout</exception>
        private GlobalLockAsync(in Guid guid, in Task task, in bool createdNew) : base()
        {
            Task = task;
            GUID = guid;
            CreatedNew = createdNew;
        }

        /// <inheritdoc/>
        public Guid GUID { get; }

        /// <inheritdoc/>
        public string ID => $"Global\\{GUID}";

        /// <inheritdoc/>
        public bool CreatedNew { get; }

        /// <summary>
        /// Execute an action asynchronous in the global lock thread
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>Action return value</returns>
        public Task<object?> ExecuteAsync(in Func<object?> action)
        {
            EnsureUndisposed();
            TaskCompletionSource<object?> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            ActionQueue.Enqueue((action, tcs));
            ActionEvent.Set();
            return tcs.Task;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            // Run the global lock thread and wait for exit
            ActionEvent.Set();
            try
            {
                Task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (Error) Logging.WriteError($"Error in global lock {ID} thread: {ex}");
            }
            ActionEvent.Dispose();
            // Cancel all eventually pending actions
            if (ActionQueue.IsEmpty) return;
            ObjectDisposedException exception = new(GetType().ToString());
            foreach (var info in ActionQueue)
                info.Task.TrySetException(exception);
            ActionQueue.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            // Run the global lock thread and wait for exit
            await ActionEvent.SetAsync().DynamicContext();
            try
            {
                await Task.DynamicContext();
            }
            catch (Exception ex)
            {
                if (Error) Logging.WriteError($"Error in global lock {ID} thread: {ex}");
            }
            await ActionEvent.DisposeAsync().DynamicContext();
            // Cancel all eventually pending actions
            if (ActionQueue.IsEmpty) return;
            ObjectDisposedException exception = new(GetType().ToString());
            foreach (var info in ActionQueue)
                info.Task.TrySetException(exception);
            ActionQueue.Clear();
        }

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="timeout">Timeout in ms (<c>-n</c> to wait for <see cref="int.MaxValue"/><c>+timeout</c>ms)</param>
        /// <param name="longRunning">Create a long running task?</param>
        /// <returns>Global lock instance</returns>
        /// <exception cref="TimeoutException">Couldn't lock within the timeout</exception>
        public static async Task<GlobalLockAsync> CreateAsync(Guid guid, int timeout = -1, bool longRunning = true)
        {
            await Task.Yield();
            // Construct the inner thread (global lock thread) which manages the mutex and injected action executions
            TaskCompletionSource<GlobalLockAsync> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using SemaphoreSlim taskEvent = new(1, 1);
            taskEvent.Wait();
            Task task = null!;
            task = Task.Factory.StartNew(
                () =>
                {
                    // Create a mutex
                    Mutex mutex = new(initiallyOwned: false, $"Global\\{guid}", out bool createdNew);
                    try
                    {
                        if (!mutex.WaitOne(TimeSpan.FromMilliseconds(timeout < 0 ? int.MaxValue + timeout : timeout), exitContext: false))
                            throw new TimeoutException();
                    }
                    catch (AbandonedMutexException)
                    {
                        // Still good!
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                        mutex.Dispose();
                        return;
                    }
                    // Wait for our task to become available in the scope from the outer thread
                    try
                    {
                        taskEvent.Wait();
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                        try
                        {
                            mutex.ReleaseMutex();
                        }
                        catch (Exception ex2)
                        {
                            if (Error)
                                Logging.WriteError($"Failed to release the mutex Global\\{guid} after an error during construction: {ex2.Message}");
                        }
                        finally
                        {
                            mutex.Dispose();
                        }
                        return;
                    }
                    // Create the global lock object and send it to the waiting outer thread
                    GlobalLockAsync res = new(guid, task, createdNew);
                    try
                    {
                        tcs.SetResult(res);
                        // Wait for the global lock being disposed from anywhere and execute injected actions
                        while (!res.IsDisposing || !res.ActionQueue.IsEmpty)
                        {
                            // Wait for an action event, if not disposing
                            if (!res.IsDisposing) res.ActionEvent.WaitAndReset();
                            // Execute injected actions, if any
                            if (!res.ActionQueue.IsEmpty)
                                while (res.ActionQueue.TryDequeue(out var info))
                                    try
                                    {
                                        info.Task.TrySetResult(info.Action());
                                    }
                                    catch (Exception ex)
                                    {
                                        info.Task.TrySetException(ex);
                                    }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Error) Logging.WriteError($"Failed to process injected actions and waiting mutex {res.ID} to be disposed: {ex}");
                    }
                    finally
                    {
                        // Release the mutex
                        try
                        {
                            mutex.ReleaseMutex();
                        }
                        catch (Exception ex)
                        {
                            if (Error) Logging.WriteError($"Failed to release the mutex {res.ID}, finally: {ex.Message}");
                        }
                        finally
                        {
                            mutex.Dispose();
                        }
                    }
                },
                CancellationToken.None,// Will be canceled by disposing the global lock
                longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.PreferFairness,
                TaskScheduler.Current
                );
            // Continue the inner thread, which is waiting for the task to be in the scope
            taskEvent.Release();
            // Return the early result of the inner thread, while it continues managing the mutex
            return await tcs.Task.DynamicContext();
        }
    }
}
