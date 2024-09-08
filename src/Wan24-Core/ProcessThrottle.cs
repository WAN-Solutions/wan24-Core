using System.Runtime;

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
        protected readonly ResetEvent Throttle = new(initialState: true);
        /// <summary>
        /// Processing event (raised when not processing)
        /// </summary>
        protected readonly ResetEvent Processing = new(initialState: true);
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limit">Processing count limit</param>
        /// <param name="timeout">Processing count limit timeout in ms</param>
        protected ProcessThrottle(in int limit, in double timeout) : base()
        {
            Timer = new()
            {
                AutoReset = false
            };
            Timer.Elapsed += (s, e) =>
            {
                using (SemaphoreSyncContext ssc = Sync.SyncContext())
                {
                    ThrottleStart = DateTime.MinValue;
                    CurrentCount = 0;
                }
                Throttle.Set();
                OnThrottleEnd?.Invoke(this, EventArgs.Empty);
            };
            SetLimit(limit, timeout);
        }

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
        public void SetLimit(in int limit, in double timeout)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeout);
            Limit = limit;
            Timeout = timeout;
        }

        /// <summary>
        /// Get a chunk size to process now
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Number that can be processed now</returns>
        public int GetProcessChunkSize(in int count) => GetProcessChunkSize(count, process: false);

        /// <summary>
        /// Get a chunk size to process now
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Number that can be processed now</returns>
        public Task<int> GetProcessChunkSizeAsync(in int count) => GetProcessChunkSizeAsync(count, process: false);

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="processor">Processor</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Processed count</returns>
        protected async Task<int> ProcessAsync(int count, Processor_Delegate processor, TimeSpan timeout)
        {
            EnsureUndisposed();
            int res = 0;
            DateTime started = DateTime.Now;
            try
            {
                await Processing.WaitAndResetAsync(timeout).DynamicContext();
                while (EnsureUndisposed() && TimeSpanHelper.UpdateTimeout(ref started, ref timeout) && count > 0)
                {
                    int chunk = await GetProcessChunkSizeAsync(count, process: true).DynamicContext();
                    if (chunk < 1)
                    {
                        await Throttle.WaitAsync(timeout).DynamicContext();
                        continue;
                    }
                    EnsureUndisposed();
                    count -= chunk;
                    res += chunk;
                    await processor(chunk).DynamicContext();
                }
                return res;
            }
            catch (TimeoutException)
            {
                return res;
            }
            finally
            {
                await Processing.SetAsync().DynamicContext();
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
            EnsureUndisposed();
            int res = 0;
            try
            {
                await Processing.WaitAndResetAsync(cancellationToken).DynamicContext();
                while (EnsureUndisposed() && !cancellationToken.IsCancellationRequested && count > 0)
                {
                    int chunk = await GetProcessChunkSizeAsync(count, process: true).DynamicContext();
                    if (chunk < 1)
                    {
                        await Throttle.WaitAsync(cancellationToken).DynamicContext();
                        continue;
                    }
                    EnsureUndisposed();
                    count -= chunk;
                    res += chunk;
                    await processor(chunk).DynamicContext();
                }
                return res;
            }
            catch (OperationCanceledException)
            {
                return res;
            }
            finally
            {
                await Processing.SetAsync(CancellationToken.None).DynamicContext();
            }
        }

        /// <summary>
        /// Get a chunk size to process now
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="process">Process the chunk size?</param>
        /// <returns>Number to process now</returns>
        protected int GetProcessChunkSize(in int count, in bool process)
        {
            bool throttling = false;
            int canProcess;
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                (canProcess, throttling) = GetProcessChunkSizeLocked(count, process);
                if (throttling)
                {
                    Throttle.Reset();
                    Timer.Interval = Timeout - (DateTime.Now - ThrottleStart).TotalMilliseconds;
                    Timer.Start();
                }
            }
            if (throttling) OnThrottleStart?.Invoke(this, EventArgs.Empty);
            return canProcess;
        }

        /// <summary>
        /// Get a chunk size to process now
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="process">Process the chunk size?</param>
        /// <returns>Number to process now</returns>
        protected async Task<int> GetProcessChunkSizeAsync(int count, bool process)
        {
            bool throttling = false;
            int canProcess;
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync())
            {
                (canProcess, throttling) = GetProcessChunkSizeLocked(count, process);
                if (throttling)
                {
                    await Throttle.ResetAsync().DynamicContext();
                    Timer.Interval = Timeout - (DateTime.Now - ThrottleStart).TotalMilliseconds;
                    Timer.Start();
                }
            }
            if (throttling) OnThrottleStart?.Invoke(this, EventArgs.Empty);
            return canProcess;
        }

        /// <summary>
        /// Get a chunk size to process now
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="process">Process the chunk size?</param>
        /// <returns>Number to process now, and if throttling</returns>
        protected (int Process, bool Throttling) GetProcessChunkSizeLocked(in int count, in bool process)
        {
            bool throttling = false;
            if (!Throttle.IsSet) return (0, throttling);
            if (ThrottleStart == DateTime.MinValue || (DateTime.Now - ThrottleStart).TotalMilliseconds >= Timeout)
            {
                ThrottleStart = DateTime.Now;
                CurrentCount = 0;
            }
            int canProcess = Limit - CurrentCount;
            if (canProcess <= 0) return (0, throttling);
            if (count >= canProcess)
            {
                if (process)
                {
                    CurrentCount += canProcess;
                    throttling = true;
                }
                return (canProcess, throttling);
            }
            if (process) CurrentCount += count;
            return (count, throttling);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Timer.Dispose();
            Throttle.Dispose();
            Processing.Dispose();
            Sync.Dispose();
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

        /// <summary>
        /// Cast as throttling-flag
        /// </summary>
        /// <param name="throttle">Throttle</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in ProcessThrottle throttle) => throttle.IsThrottling;
    }
}
