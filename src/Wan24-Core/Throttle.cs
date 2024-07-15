namespace wan24.Core
{
    /// <summary>
    /// Generic throttling helper (if it's important to throttle more exact, and performance counts less, use <see cref="ExactThrottle"/> instead)
    /// </summary>
    public class Throttle : DisposableBase, IThrottle
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Throttle event (raised when not throttling)
        /// </summary>
        protected readonly ResetEvent ThrottleEvent = new(initialState: true);
        /// <summary>
        /// Throttle timer (started as soon as <see cref="CurrentCount"/> is greater than zero)
        /// </summary>
        protected readonly System.Timers.Timer ThrottleTimer;
        /// <summary>
        /// Limit within <see cref="_Timeout"/> (zero to disable throttling)
        /// </summary>
        protected int _Limit;
        /// <summary>
        /// Timeout
        /// </summary>
        protected TimeSpan _Timeout;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limit">Limit within <c>timeout</c> (zero to disable throttling)</param>
        /// <param name="timeout">Timeout</param>
        public Throttle(in int limit, in TimeSpan timeout) : base()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 0, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            _Limit = limit;
            _Timeout = timeout;
            ThrottleTimer = new(timeout)
            {
                AutoReset = false
            };
            ThrottleTimer.Elapsed += (s, e) =>
            {
                try
                {
                    using SemaphoreSyncContext ssc = Sync;
                    if (_Limit < 1 || _Limit > CurrentCount)
                    {
                        CurrentCount = 0;
                    }
                    else
                    {
                        CurrentCount -= _Limit;
                    }
                    if (_Limit < 1 || CurrentCount < _Limit)
                    {
                        ThrottleEvent.Set();
                    }
                    else if (!ThrottleTimer.Enabled && _Limit > 0 && CurrentCount >= _Limit)
                    {
                        ThrottleTimer.Start();
                    }
                }
                catch (ObjectDisposedException) when (IsDisposing)
                {
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new("Throttle timer update failed", ex));
                }
            };
        }

        /// <inheritdoc/>
        public virtual int Limit
        {
            get => _Limit;
            set
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 0, nameof(value));
                using SemaphoreSyncContext ssc = Sync;
                _Limit = value;
                if (value < 1)
                {
                    ThrottleEvent.Set();
                }
                else if (CurrentCount >= value)
                {
                    ThrottleEvent.Reset();
                }
            }
        }

        /// <inheritdoc/>
        public virtual TimeSpan Timeout
        {
            get => _Timeout;
            set
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero, nameof(value));
                using SemaphoreSyncContext ssc = Sync;
                _Timeout = value;
                bool isTimerRunning = ThrottleTimer.Enabled;
                if (isTimerRunning)
                    ThrottleTimer.Stop();
                ThrottleTimer.Interval = value.TotalMilliseconds;
                if (isTimerRunning)
                    ThrottleTimer.Start();
            }
        }

        /// <inheritdoc/>
        public int CurrentCount { get; protected set; }

        /// <inheritdoc/>
        public bool IsThrottling => !IsDisposing && !ThrottleEvent.IsSet;

        /// <summary>
        /// If throttling is applied when counting one more
        /// </summary>
        public bool WillThrottle => !IsDisposing && _Limit > 0 && CurrentCount >= _Limit;

        /// <inheritdoc/>
        public void CountOne(CancellationToken cancellationToken = default) => Count(count: 1, cancellationToken);

        /// <inheritdoc/>
        public Task CountOneAsync(CancellationToken cancellationToken = default) => CountAsync(count: 1, cancellationToken);

        /// <inheritdoc/>
        public virtual void Count(int count, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (count < 1)
                return;
            while (_Limit > 0)
            {
                ThrottleEvent.Wait(cancellationToken);
                using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
                if (_Limit < 1)
                    return;
                if (!ThrottleEvent.IsSet)
                    continue;
                CurrentCount += count;
                if (CurrentCount >= _Limit)
                    ThrottleEvent.Reset(CancellationToken.None);
                if (!ThrottleTimer.Enabled)
                    ThrottleTimer.Start();
                return;
            }
        }

        /// <inheritdoc/>
        public virtual async Task CountAsync(int count, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (count < 1)
                return;
            while (_Limit > 0)
            {
                await ThrottleEvent.WaitAsync(cancellationToken).DynamicContext();
                using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                if (_Limit < 1)
                    return;
                if (!ThrottleEvent.IsSet)
                    continue;
                CurrentCount += count;
                if (CurrentCount >= _Limit)
                    await ThrottleEvent.ResetAsync(CancellationToken.None).DynamicContext();
                if (!ThrottleTimer.Enabled)
                    ThrottleTimer.Start();
                return;
            }
        }

        /// <inheritdoc/>
        public virtual void Release(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_Limit < 1 || CurrentCount < 1)
                return;
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            if (_Limit < 1 || CurrentCount < 1)
                return;
            ThrottleTimer.Stop();
            CurrentCount = 0;
            ThrottleEvent.Set(CancellationToken.None);
        }

        /// <inheritdoc/>
        public virtual async Task ReleaseAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_Limit < 1 || CurrentCount < 1)
                return;
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (_Limit < 1 || CurrentCount < 1)
                return;
            ThrottleTimer.Stop();
            CurrentCount = 0;
            await ThrottleEvent.SetAsync(CancellationToken.None);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = sync;
            ThrottleEvent.Dispose();
            ThrottleTimer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = await sync.SyncContextAsync().DynamicContext();
            await ThrottleEvent.DisposeAsync().DynamicContext();
            ThrottleTimer.Dispose();
        }
    }
}
