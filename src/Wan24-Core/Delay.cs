namespace wan24.Core
{
    /// <summary>
    /// Delay
    /// </summary>
    public sealed class Delay : DisposableBase
    {
        /// <summary>
        /// Completion
        /// </summary>
        private readonly TaskCompletionSource Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        /// <summary>
        /// Synchronization
        /// </summary>
        private readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="delay">Delay</param>
        public Delay(TimeSpan delay) : this(DateTime.Now + delay) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="runTime">Runtime</param>
        /// <param name="guid">GUID to use</param>
        public Delay(DateTime runTime, string? guid = null) : base()
        {
            GUID = guid ?? Guid.NewGuid().ToString();
            RunTime = runTime;
            DelayTable.Delays[GUID] = this;
            DelayService.Instance.AddDelay(this);
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Runtime
        /// </summary>
        public DateTime RunTime { get; }

        /// <summary>
        /// Task
        /// </summary>
        public Task Task => Completion.Task;

        /// <summary>
        /// Any tagged object
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Cancel (and dispose)
        /// </summary>
        public void Cancel()
        {
            if (IsDisposing) return;
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                if (IsDisposing) return;
                Completion.TrySetException(new OperationCanceledException());
            }
            Dispose();
        }

        /// <summary>
        /// Cancel (and dispose)
        /// </summary>
        public async Task CancelAsync()
        {
            if (IsDisposing) return;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext())
            {
                if (IsDisposing) return;
                Completion.TrySetException(new OperationCanceledException());
            }
            await DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Complete (and dispose)
        /// </summary>
        public void Complete()
        {
            if (IsDisposing) return;
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                if (IsDisposing) return;
                Completion.TrySetResult();
            }
            Dispose();
        }

        /// <summary>
        /// Complete (and dispose)
        /// </summary>
        public async Task CompleteAsync()
        {
            if (IsDisposing) return;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext())
            {
                if (IsDisposing) return;
                Completion.TrySetResult();
            }
            await DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Fail (and dispose)
        /// </summary>
        /// <param name="ex">Exception</param>
        public void Fail(Exception? ex = null)
        {
            if (IsDisposing) return;
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                if (IsDisposing) return;
                Completion.TrySetException(ex ?? new OperationCanceledException("Delay failed"));
            }
            Dispose();
        }

        /// <summary>
        /// Fail (and dispose)
        /// </summary>
        /// <param name="ex">Exception</param>
        public async Task FailAsync(Exception? ex = null)
        {
            if (IsDisposing) return;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext())
            {
                if (IsDisposing) return;
                Completion.TrySetException(ex ?? new OperationCanceledException("Delay failed"));
            }
            await DisposeAsync().DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
                Completion.TrySetException(new ObjectDisposedException(GetType().Name));
            DelayTable.Delays.TryRemove(GUID, out _);
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext())
                Completion.TrySetException(new ObjectDisposedException(GetType().Name));
            DelayTable.Delays.TryRemove(GUID, out _);
            await Sync.DisposeAsync().DynamicContext();
        }
    }
}
