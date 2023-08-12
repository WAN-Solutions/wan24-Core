namespace wan24.Core
{
    /// <summary>
    /// Object lock
    /// </summary>
    public sealed class ObjectLock : DisposableBase
    {
        /// <summary>
        /// Task completion
        /// </summary>
        private readonly TaskCompletionSource TaskCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        /// <summary>
        /// Thread synchronization
        /// </summary>
        private readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Has a task?
        /// </summary>
        private bool HasTask = false;
        /// <summary>
        /// Is constructed?
        /// </summary>
        internal bool IsConstructed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="tag">Tagged object</param>
        internal ObjectLock(object key, object? tag) : base()
        {
            Key = key;
            Tag = tag;
        }

        /// <summary>
        /// Tagged object
        /// </summary>
        public object? Tag { get; }

        /// <summary>
        /// Object key
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// Task for awaiting the lock being released
        /// </summary>
        public Task Task => TaskCompletion.Task;

        /// <summary>
        /// Run a task asynchronous (the lock will be released when the task is done)
        /// </summary>
        /// <param name="task">Task</param>
        public async Task RunTaskAsync(Task task)
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext())
            {
                if (HasTask) throw new InvalidOperationException("Has a task already");
                HasTask = true;
            }
            try
            {
                EnsureUndisposed();
                await task.DynamicContext();
                await SetCompletedAsync().DynamicContext();
            }
            catch (Exception ex)
            {
                await SetCompletedAsync(ex).DynamicContext();
            }
            finally
            {
                await DisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Set completed
        /// </summary>
        /// <param name="ex">Exception</param>
        private void SetCompleted(Exception? ex = null)
        {
            using SemaphoreSyncContext ssc = Sync.SyncContext();
            if (!IsConstructed || TaskCompletion.Task.IsCompleted) return;
            if (ex == null)
            {
                TaskCompletion.SetResult();
            }
            else
            {
                TaskCompletion.SetException(ex);
            }
        }

        /// <summary>
        /// Set completed
        /// </summary>
        /// <param name="ex">Exception</param>
        private async Task SetCompletedAsync(Exception? ex = null)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext();
            if (!IsConstructed || TaskCompletion.Task.IsCompleted) return;
            if (ex == null)
            {
                TaskCompletion.SetResult();
            }
            else
            {
                TaskCompletion.SetException(ex);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            SetCompleted(new ObjectDisposedException(GetType().ToString()));
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await SetCompletedAsync(new ObjectDisposedException(GetType().ToString())).DynamicContext();
            Sync.Dispose();
        }

        /// <summary>
        /// Cast as disposing-flag
        /// </summary>
        /// <param name="ol">Lock</param>
        public static implicit operator bool(ObjectLock ol) => !ol.IsDisposing;
    }
}
