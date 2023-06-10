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
        private readonly SemaphoreSlim Sync = new(1, 1);
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
            await Sync.WaitAsync().DynamicContext();
            try
            {
                if (HasTask) throw new InvalidOperationException("Has a task already");
                HasTask = true;
            }
            finally
            {
                Sync.Release();
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
            Sync.Wait();
            try
            {
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
            finally
            {
                Sync.Release();
            }
        }

        /// <summary>
        /// Set completed
        /// </summary>
        /// <param name="ex">Exception</param>
        private async Task SetCompletedAsync(Exception? ex = null)
        {
            await Sync.WaitAsync().DynamicContext();
            try
            {
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
            finally
            {
                Sync.Release();
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
