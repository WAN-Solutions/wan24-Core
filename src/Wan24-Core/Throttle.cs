using System.ComponentModel;
using static wan24.Core.TranslationHelper;

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
            ThrottleTable.Throttles[GUID] = this;
        }

        /// <inheritdoc/>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public string? Name { get; set; }

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
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("Type"), GetType(), __("Throttle CLR type"));
                yield return new(__("GUID"), GUID, __("Throttle instance GUID"));
                yield return new(__("Name"), Name, __("Throttle name"));
                yield return new(__("Limit"), Limit, __("Limit within the timeout or zero, if disabled"));
                yield return new(__("Timeout"), Timeout, __("Throttline timeout"));
                yield return new(__("Count"), CurrentCount, __("Current count"));
                yield return new(__("Throttling"), IsThrottling, __("If throttling at present"));
            }
        }

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
        [UserAction(MultiAction = true), DisplayText("Release"), Description("Clear the counter and release the throttle")]
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
        public virtual void UpdateSettings(int limit, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 0, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            bool isTimerRunning = ThrottleTimer.Enabled;
            if (isTimerRunning)
                ThrottleTimer.Stop();
            _Limit = limit;
            if (limit < 1)
            {
                ThrottleEvent.Set(CancellationToken.None);
            }
            else if (CurrentCount >= limit)
            {
                ThrottleEvent.Reset(CancellationToken.None);
            }
            _Timeout = timeout;
            ThrottleTimer.Interval = timeout.TotalMilliseconds;
            if (isTimerRunning)
                ThrottleTimer.Start();
        }

        /// <inheritdoc/>
        [UserAction(MultiAction = true), DisplayText("Settings"), Description("Update the settings and restart the throttling timer")]
        public virtual async Task UpdateSettingsAsync(int limit, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 0, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            bool isTimerRunning = ThrottleTimer.Enabled;
            if (isTimerRunning)
                ThrottleTimer.Stop();
            _Limit = limit;
            if (limit < 1)
            {
                await ThrottleEvent.SetAsync(CancellationToken.None).DynamicContext();
            }
            else if (CurrentCount >= limit)
            {
                await ThrottleEvent.ResetAsync(CancellationToken.None).DynamicContext();
            }
            _Timeout = timeout;
            ThrottleTimer.Interval = timeout.TotalMilliseconds;
            if (isTimerRunning)
                ThrottleTimer.Start();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ThrottleTable.Throttles.Remove(GUID, out _);
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = sync;
            ThrottleEvent.Dispose();
            ThrottleTimer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            ThrottleTable.Throttles.Remove(GUID, out _);
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = await sync.SyncContextAsync().DynamicContext();
            await ThrottleEvent.DisposeAsync().DynamicContext();
            ThrottleTimer.Dispose();
        }
    }
}
