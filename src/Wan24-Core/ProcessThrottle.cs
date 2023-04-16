namespace wan24.Core
{
    /// <summary>
    /// Process throttle
    /// </summary>
    public abstract class ProcessThrottle : DisposableBase
    {
        /// <summary>
        /// Throttle timer
        /// </summary>
        protected readonly System.Timers.Timer Timer;
        /// <summary>
        /// Throttle (raised when not throttling)
        /// </summary>
        protected readonly ManualResetEventSlim Throttle = new(initialState: true);
        /// <summary>
        /// Processing event (raised when not processing)
        /// </summary>
        protected readonly ManualResetEventSlim Processing = new(initialState: true);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limit">Processing count limit</param>
        /// <param name="timeout">Processing count limit timeout in ms</param>
        protected ProcessThrottle(int limit, double timeout) : base()
        {
            Timer = new()
            {
                AutoReset = false
            };
            Timer.Elapsed += (s, e) =>
            {
                lock (SyncObject)
                {
                    ThrottleStart = DateTime.MinValue;
                    CurrentCount = 0;
                }
                Throttle.Set();
                OnThrottleEnd?.Invoke(this, new());
            };
            SetLimit(limit, timeout);
        }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Processing count limit
        /// </summary>
        public int Limit { get; protected set; }

        /// <summary>
        /// Processing count limit timeout in ms
        /// </summary>
        public double Timeout { get; protected set; }

        /// <summary>
        /// Throttle start time
        /// </summary>
        public DateTime ThrottleStart { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Current count
        /// </summary>
        public int CurrentCount { get; protected set; }

        /// <summary>
        /// Is throttling?
        /// </summary>
        public bool IsThrottling => !Throttle.IsSet;

        /// <summary>
        /// Set a new limit
        /// </summary>
        /// <param name="limit">Processing count limit</param>
        /// <param name="timeout">Processing count limit timeout in ms</param>
        public void SetLimit(int limit, double timeout)
        {
            EnsureUndisposed();
            if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit));
            if (timeout <= 0) throw new ArgumentOutOfRangeException(nameof(timeout));
            Limit = limit;
            Timeout = timeout;
        }

        /// <summary>
        /// Get a chunk size to process now
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Number that can be processed now</returns>
        public int GetProcessChunkSize(int count) => GetProcessChunkSize(count, process: false);

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="processor">Processor</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Processed count</returns>
        protected async Task<int> ProcessAsync(int count, Processor_Delegate processor, TimeSpan timeout)
        {
            await Task.Yield();
            EnsureUndisposed();
            int res = 0;
            DateTime started = DateTime.Now,
                now;
            bool UpdateTimeout()
            {
                now = DateTime.Now;
                if (now - started > timeout) return false;
                timeout -= now - started;
                started = DateTime.Now;
                return true;
            }
            while (EnsureUndisposed())
            {
                Processing.Wait(timeout);
                lock (SyncObject)
                {
                    if (!UpdateTimeout()) return res;
                    if (!Processing.IsSet) continue;
                    Processing.Reset();
                    break;
                }
            }
            try
            {
                while (count > 0)
                {
                    int chunk = GetProcessChunkSize(count, process: true);
                    if (chunk < 1)
                    {
                        Throttle.Wait(timeout);
                        if (!UpdateTimeout()) return res;
                        EnsureUndisposed();
                        continue;
                    }
                    count -= chunk;
                    res += chunk;
                    await processor(chunk).DynamicContext();
                    if (!UpdateTimeout()) return res;
                }
                return res;
            }
            finally
            {
                Processing.Set();
            }
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="processor">Processor</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Processed count</returns>
        protected async Task<int> ProcessAsync(int count, Processor_Delegate processor, CancellationToken cancellationToken)
        {
            await Task.Yield();
            EnsureUndisposed();
            int res = 0;
            while (EnsureUndisposed())
            {
                try
                {
                    Processing.Wait(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return res;
                }
                lock (SyncObject)
                {
                    if (!Processing.IsSet) continue;
                    Processing.Reset();
                    break;
                }
            }
            try
            {
                while (count > 0)
                {
                    int chunk = GetProcessChunkSize(count, process: true);
                    if (chunk < 1)
                    {
                        try
                        {
                            Throttle.Wait(cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            return res;
                        }
                        EnsureUndisposed();
                        continue;
                    }
                    count -= chunk;
                    res += chunk;
                    await processor(chunk).DynamicContext();
                    if (cancellationToken.IsCancellationRequested) return res;
                }
                return res;
            }
            finally
            {
                Processing.Set();
            }
        }

        /// <summary>
        /// Get a chunk size to process now
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="process">Process the chunk size?</param>
        /// <returns>Number to process now</returns>
        protected int GetProcessChunkSize(int count, bool process)
        {
            bool throttling = false;
            try
            {
                lock (DisposeSyncObject)
                {
                    EnsureUndisposed();
                    lock (SyncObject)
                    {
                        if (!Throttle.IsSet) return 0;
                        if (ThrottleStart == DateTime.MinValue || (DateTime.Now - ThrottleStart).TotalMilliseconds >= Timeout)
                        {
                            ThrottleStart = DateTime.Now;
                            CurrentCount = 0;
                        }
                        int canProcess = Limit - CurrentCount;
                        if (canProcess <= 0) return 0;
                        if (count >= canProcess)
                        {
                            if (process)
                            {
                                CurrentCount += canProcess;
                                Throttle.Reset();
                                Timer.Interval = Timeout - (DateTime.Now - ThrottleStart).TotalMilliseconds;
                                Timer.Start();
                                throttling = true;
                            }
                            return canProcess;
                        }
                        else
                        {
                            if (process) CurrentCount += count;
                            return count;
                        }
                    }
                }
            }
            finally
            {
                if (throttling) OnThrottleStart?.Invoke(this, new());
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Timer.Dispose();
            Throttle.Dispose();
            Processing.Dispose();
        }

        /// <summary>
        /// Delegate for a processor
        /// </summary>
        /// <param name="count">Count</param>
        public delegate Task Processor_Delegate(int count);

        /// <summary>
        /// Delegate for a process throttle event
        /// </summary>
        /// <param name="throttle">Throttle</param>
        /// <param name="e">Event arguments</param>
        public delegate void ProcessThrottleEvent_Delegate(ProcessThrottle throttle, EventArgs e);

        /// <summary>
        /// Raised when starting to throttle
        /// </summary>
        public event ProcessThrottleEvent_Delegate? OnThrottleStart;

        /// <summary>
        /// Raised when stopped throttling
        /// </summary>
        public event ProcessThrottleEvent_Delegate? OnThrottleEnd;
    }
}
