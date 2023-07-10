namespace wan24.Core
{
    /// <summary>
    /// Global lock using <see cref="Mutex"/> for use with an asynchronous context (does have to reserve a thread during the mutex exists :( )
    /// </summary>
    public sealed class GlobalLockAsync : DisposableBase, IGlobalLock
    {
        /// <summary>
        /// Dispose event
        /// </summary>
        private readonly SemaphoreSlim DisposeEvent = new(1, 1);
        /// <summary>
        /// Outher task
        /// </summary>
        private readonly Task Task;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="task">Outher task</param>
        /// <param name="createdNew">Created a new mutex?</param>
        /// <exception cref="TimeoutException">Couldn't lock within the timeout</exception>
        private GlobalLockAsync(Guid guid, Task task, bool createdNew) : base()
        {
            Task = task;
            GUID = guid;
            CreatedNew = createdNew;
            DisposeEvent.Wait();
        }

        /// <inheritdoc/>
        public Guid GUID { get; }

        /// <inheritdoc/>
        public string ID => $"Global\\{GUID}";

        /// <inheritdoc/>
        public bool CreatedNew { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            DisposeEvent.Release();
            try
            {
                Task.Wait();
            }
            finally
            {
                DisposeEvent.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            DisposeEvent.Release();
            try
            {
                await Task.DynamicContext();
            }
            finally
            {
                DisposeEvent.Dispose();
            }
        }

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="timeout">Timeout in ms (<c>-1</c> to wait for <see cref="int.MaxValue"/><c>-1</c>ms)</param>
        /// <returns>Global lock instance</returns>
        /// <exception cref="TimeoutException">Couldn't lock within the timeout</exception>
        public static async Task<GlobalLockAsync> CreateAsync(Guid guid, int timeout = -1)
        {
            await Task.Yield();
            TaskCompletionSource<GlobalLockAsync> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using SemaphoreSlim taskEvent = new(1, 1);
            taskEvent.Wait();
            try
            {
                Task task = null!;
                task = Task.Factory.StartNew(
                    () =>
                    {
                        // Create a mutex
                        Mutex mutex = new(initiallyOwned: false, $"Global\\{guid}", out bool createdNew);
                        try
                        {
                            if (!mutex.WaitOne(TimeSpan.FromMilliseconds(timeout < 0 ? int.MaxValue - 1 : timeout), exitContext: false))
                                throw new TimeoutException();
                        }
                        catch (AbandonedMutexException)
                        {
                            // Still good
                        }
                        catch (Exception ex)
                        {
                            mutex.Dispose();
                            tcs.SetException(ex);
                            return;
                        }
                        // Wait for out task to become available from the outher thread
                        taskEvent.Wait();
                        // Create the global lock object and send it to the waiting outher thread
                        GlobalLockAsync res = new(guid, task, createdNew);
                        try
                        {
                            tcs.SetResult(res);
                            // Wait for the global lock being disposed from anywhere
                            res.DisposeEvent.Wait();
                        }
                        finally
                        {
                            // Release the mutex
                            mutex.ReleaseMutex();
                            mutex.Dispose();
                        }
                    },
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Current
                    );
            }
            finally
            {
                taskEvent.Release();
            }
            return await tcs.Task.DynamicContext();
        }
    }
}
