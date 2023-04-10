namespace wan24.Core.Threading
{
    /// <summary>
    /// Worker
    /// </summary>
    public abstract class Worker : DisposableBase
    {
        /// <summary>
        /// Working state (set when working)
        /// </summary>
        private readonly State Working = new()
        {
            StateWhenDisposing = false
        };
        /// <summary>
        /// Running state (set when started)
        /// </summary>
        protected readonly State Running = new()
        {
            StateWhenDisposing = false
        };
        /// <summary>
        /// Work state (set when having work)
        /// </summary>
        protected readonly State Work = new(initialState: true)
        {
            StateWhenDisposing = false
        };
        /// <summary>
        /// Run synchronous worker method?
        /// </summary>
        protected readonly bool SyncWork;
        /// <summary>
        /// Run synchronous wait logic method?
        /// </summary>
        protected readonly bool SyncWait;
        /// <summary>
        /// Worker task
        /// </summary>
        protected Task? WorkerTask = null;
        /// <summary>
        /// Cancellation
        /// </summary>
        protected CancellationTokenSource? Cancellation = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="syncWork">Run synchronous worker method?</param>
        /// <param name="syncWait">Run synchronous wait logic method?</param>
        protected Worker(bool syncWork, bool syncWait = true) : base()
        {
            SyncWork = syncWork;
            SyncWait = syncWait;
            // Start the work processor and raise the OnStart event
            Running.OnSetLocked += (s, e) => WorkerTask = WorkProcessor();
            Running.OnSet += (s, e) => OnStart?.Invoke(this, new());
            // Cancel the work processor and raise the OnStop event
            Running.OnResetLocked += (s, e) =>
            {
                try
                {
                    Cancellation?.Cancel();
                }
                catch
                {
                }
                WorkerTask?.Wait();
            };
            Running.OnReset += (s, e) => OnStop?.Invoke(this, new());
            // Raise the work state events
            Working.OnSet += (s, e) => OnWork?.Invoke(this, new());
            Working.OnReset += (s, e) => OnWorkDone?.Invoke(this, new());
        }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Is running?
        /// </summary>
        public bool IsRunning => Running.IsSet;

        /// <summary>
        /// Is working?
        /// </summary>
        public bool IsWorking => Working.IsSet;

        /// <summary>
        /// Is cancelled?
        /// </summary>
        public bool IsCancelled => (Cancellation?.IsCancellationRequested ?? true) || IsDisposing;

        /// <summary>
        /// Last start time (or <see cref="DateTime.MinValue"/>, if never started)
        /// </summary>
        public DateTime LastStarted { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Last work time (or <see cref="DateTime.MinValue"/>, if never worked)
        /// </summary>
        public DateTime LastWork { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Last work duration (or <see cref="TimeSpan.Zero"/>, if never worked)
        /// </summary>
        public TimeSpan LastWorkDuration { get; protected set; } = TimeSpan.MinValue;

        /// <summary>
        /// Last stopped time (or <see cref="DateTime.MinValue"/>, if never stopped)
        /// </summary>
        public DateTime LastStopped { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Last (current) runtime (or <see cref="TimeSpan.Zero"/>, if never started)
        /// </summary>
        public TimeSpan LastRunTime => LastStarted == DateTime.MinValue
            ? TimeSpan.Zero
            : LastStopped < LastStarted
                ? DateTime.Now - LastStarted
                : LastStopped - LastStarted;

        /// <summary>
        /// Total work count
        /// </summary>
        public long TotalWorkCount { get; protected set; }

        /// <summary>
        /// Last (current) work count
        /// </summary>
        public long LastWorkCount { get; protected set; }

        /// <summary>
        /// Worker exception
        /// </summary>
        public Exception? WorkerException { get; private set; }

        /// <summary>
        /// Start
        /// </summary>
        public void Start() => IfUndisposed(() => Running.Set(state: true));

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop() => IfUndisposed(() => Running.Set(state: false), allowDisposing: true);

        /// <summary>
        /// Wait started
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Started?</returns>
        public bool WaitStart(TimeSpan? timeout = null) => IfUndisposed(() => Running.WaitSet(timeout));

        /// <summary>
        /// Wait started
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Started?</returns>
        public bool WaitStart(CancellationToken cancellationToken) => IfUndisposed(() => Running.WaitSet(cancellationToken));

        /// <summary>
        /// Wait stopped
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Stopped?</returns>
        public bool WaitStop(TimeSpan? timeout = null) => Running.WaitReset(timeout);

        /// <summary>
        /// Wait stopped
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stopped?</returns>
        public bool WaitStop(CancellationToken cancellationToken) => Running.WaitReset(cancellationToken);

        /// <summary>
        /// Wait for busy state
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Is busy?</returns>
        public bool WaitBusy(TimeSpan? timeout = null) => IfUndisposed(() => Working.WaitSet(timeout));

        /// <summary>
        /// Wait for busy state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Is busy?</returns>
        public bool WaitBusy(CancellationToken cancellationToken) => IfUndisposed(() => Working.WaitSet(cancellationToken));

        /// <summary>
        /// Wait for boring state
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <returns>Is bored?</returns>
        public bool WaitBoring(TimeSpan? timeout = null) => IfUndisposed(() => Working.WaitReset(timeout));

        /// <summary>
        /// Wait for boring state
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Is bored?</returns>
        public bool WaitBoring(CancellationToken cancellationToken) => IfUndisposed(() => Working.WaitReset(cancellationToken));

        /// <summary>
        /// Do work synchronous
        /// </summary>
        protected virtual void DoWork() => throw new NotImplementedException();

        /// <summary>
        /// Do work asynchronous
        /// </summary>
        protected virtual Task DoWorkAsync() => throw new NotImplementedException();

        /// <summary>
        /// Synchronous wait work logic
        /// </summary>
        protected virtual void WaitWorkLogic() => Work.WaitSet(Cancellation!.Token);

        /// <summary>
        /// Asynchronous wait work logic
        /// </summary>
        /// <returns></returns>
        protected virtual Task WaitWorkLogicAsync()
        {
            Work.WaitSet(Cancellation!.Token);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => DisposingActions();

        /// <inheritdoc/>
        protected override Task DisposeCore()
        {
            DisposingActions();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Perform disposing actions
        /// </summary>
        private void DisposingActions()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            Stop();
            Running.Dispose();
            Working.Dispose();
            Work.Dispose();
        }

        /// <summary>
        /// Worker thread
        /// </summary>
        private async Task WorkProcessor()
        {
            Cancellation = new();
            LastWorkCount = 0;
            await Task.Yield();
            try
            {
                while (!IsCancelled)
                {
                    if (SyncWait)
                    {
                        WaitWorkLogic();
                    }
                    else
                    {
                        await WaitWorkLogicAsync().DynamicContext();
                    }
                    if (IsCancelled) break;
                    LastWorkCount++;
                    TotalWorkCount++;
                    Working.Set(state: true);
                    try
                    {
                        if (IsCancelled) break;
                        LastWork = DateTime.Now;
                        if (SyncWork)
                        {
                            DoWork();
                        }
                        else
                        {
                            await DoWorkAsync().DynamicContext();
                        }
                    }
                    finally
                    {
                        LastWorkDuration = DateTime.Now - LastWork;
                        Working.Set(state: false);
                    }
                }
            }
            catch (Exception ex)
            {
                WorkerException = ex;
                _ = Task.Run(() => OnException?.Invoke(this, new()));
            }
            finally
            {
                Working.Set(state: false);
                Cancellation!.Cancel();
                using CancellationTokenSource cts = Cancellation;
                Cancellation = null;
                WorkerTask = null;
            }
        }

        /// <summary>
        /// Delegate for worker events
        /// </summary>
        /// <param name="worker">Worker</param>
        /// <param name="e">Event arguments</param>
        public delegate void Worker_Delegate(Worker worker, EventArgs e);

        /// <summary>
        /// Raised on start
        /// </summary>
        public event Worker_Delegate? OnStart;

        /// <summary>
        /// Raised on stop
        /// </summary>
        public event Worker_Delegate? OnStop;

        /// <summary>
        /// Raised when working
        /// </summary>
        public event Worker_Delegate? OnWork;

        /// <summary>
        /// Raised when waiting for work
        /// </summary>
        public event Worker_Delegate? OnWorkDone;

        /// <summary>
        /// Raised on exception (the worker will stop working, and <see cref="WorkerException"/> will contain the last exception)
        /// </summary>
        public event Worker_Delegate? OnException;

        /// <summary>
        /// Cast as boolean (is running?)
        /// </summary>
        /// <param name="worker">Worker</param>
        public static implicit operator bool(Worker worker) => worker.IsRunning;
    }
}
