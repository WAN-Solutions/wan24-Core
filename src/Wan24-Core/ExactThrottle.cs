using System.ComponentModel;
using System.Runtime.InteropServices;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// (Almost) Exact throttle (with an overhead; use <see cref="Throttle"/> for a low overhead instead, if performance is more important than being exact)
    /// </summary>
    public class ExactThrottle : DisposableBase, IThrottle
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Counter
        /// </summary>
        protected readonly Queue<Counted> _Counter = [];
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
        /// <param name="accurence">Timer accurence in ms</param>
        public ExactThrottle(in int limit, in TimeSpan timeout, in int accurence = 5) : base()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 0, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            ArgumentOutOfRangeException.ThrowIfLessThan(accurence, 1, nameof(accurence));
            _Limit = limit;
            _Timeout = timeout;
            Accurence = accurence;
            ThrottleTimer = new(timeout)
            {
                AutoReset = false
            };
            ThrottleTimer.Elapsed += (s, e) =>
            {
                try
                {
                    using SemaphoreSyncContext ssc = Sync;
                    UpdateState();
                }
                catch (ObjectDisposedException) when (IsDisposing)
                {
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new("Throttle timer update failed", ex, tag: this));
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
                UpdateState();
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
                UpdateState();
            }
        }

        /// <summary>
        /// Timer accurence in ms
        /// </summary>
        public int Accurence { get; }

        /// <summary>
        /// Current number of stored counting events
        /// </summary>
        public int Counter => IfUndisposed(() => _Counter.Count);

        /// <inheritdoc/>
        public int CurrentCount { get; protected set; }

        /// <summary>
        /// If to count more exact (if <see langword="true"/>, cancellation support when counting is disabled)
        /// </summary>
        public bool CountExact { get; init; }

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
                if (StackInfo is not null)
                {
                    yield return new(__("Stack"), StackInfo.Stack, __("Instance creation stack"));
                    yield return new(__("Created"), StackInfo.Created, __("Instance creation time"));
                }
                yield return new(__("Name"), Name, __("Throttle name"));
                yield return new(__("Limit"), Limit, __("Limit within the timeout or zero, if disabled"));
                yield return new(__("Timeout"), Timeout, __("Throttling timeout"));
                yield return new(__("Count"), CurrentCount, __("Current count"));
                yield return new(__("Counter"), _Counter.Count, __("Current number of counting events"));
                yield return new(__("Accurence"), Accurence, __("Throttle timer accurence in ms"));
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
            if (CountExact)
                cancellationToken = CancellationToken.None;
            while (count > 0 && _Limit > 0)
            {
                ThrottleEvent.Wait(cancellationToken);
                using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
                if (_Limit < 1)
                    return;
                if (!ThrottleEvent.IsSet)
                    continue;
                if (CountExact)
                {
                    int cnt = Math.Min(count, CurrentCount - _Limit);
                    CurrentCount += cnt;
                    _Counter.Enqueue(cnt);
                    count -= cnt;
                }
                else
                {
                    CurrentCount += count;
                    _Counter.Enqueue(count);
                    count = 0;
                }
                UpdateState();
            }
        }

        /// <inheritdoc/>
        public virtual async Task CountAsync(int count, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_Limit < 1)
                return;
            if (CountExact)
                cancellationToken = CancellationToken.None;
            while (count > 0 && _Limit > 0)
            {
                await ThrottleEvent.WaitAsync(cancellationToken).DynamicContext();
                using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                if (_Limit < 1)
                    return;
                if (!ThrottleEvent.IsSet)
                    continue;
                if (CountExact)
                {
                    int cnt = Math.Min(count, CurrentCount - _Limit);
                    CurrentCount += cnt;
                    _Counter.Enqueue(cnt);
                    count -= cnt;
                }
                else
                {
                    CurrentCount += count;
                    _Counter.Enqueue(count);
                    count = 0;
                }
                await UpdateStateAsync().DynamicContext();
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
            _Counter.Clear();
            CurrentCount = 0;
            UpdateState();
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
            _Counter.Clear();
            CurrentCount = 0;
            await UpdateStateAsync().DynamicContext();
        }

        /// <inheritdoc/>
        public virtual void UpdateSettings(int limit, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 0, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            _Limit = limit;
            _Timeout = timeout;
            UpdateState();
        }

        /// <inheritdoc/>
        [UserAction(MultiAction = true), DisplayText("Settings"), Description("Update the settings and restart the throttling timer")]
        public virtual async Task UpdateSettingsAsync(int limit, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 0, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero, nameof(timeout));
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            _Limit = limit;
            _Timeout = timeout;
            await UpdateStateAsync().DynamicContext();
        }

        /// <summary>
        /// Update the state
        /// </summary>
        protected virtual void UpdateState()
        {
            ThrottleTimer.Stop();
            if (_Limit < 1)
            {
                _Counter.Clear();
                CurrentCount = 0;
            }
            else
            {
                Counted counted;
                TimeSpan timeout;
                int unCount = 0;
                while (true)
                {
                    while (_Counter.TryPeek(out counted) && counted >= _Timeout && _Counter.TryDequeue(out counted))
                        unCount += counted;
                    if (_Counter.TryPeek(out counted))
                    {
                        timeout = _Timeout - counted;
                        if (timeout.TotalMilliseconds < Accurence)
                            continue;
                        ThrottleTimer.Interval = timeout.TotalMilliseconds;
                        ThrottleTimer.Start();
                    }
                    break;
                }
                if (unCount > 0)
                    CurrentCount -= unCount;
            }
            if (_Limit < 1 || CurrentCount < _Limit)
            {
                ThrottleEvent.Set();
            }
            else
            {
                ThrottleEvent.Reset();
            }
        }

        /// <summary>
        /// Update the state
        /// </summary>
        protected virtual async Task UpdateStateAsync()
        {
            ThrottleTimer.Stop();
            if (_Limit < 1)
            {
                _Counter.Clear();
                CurrentCount = 0;
            }
            else
            {
                Counted counted;
                TimeSpan timeout;
                int unCount = 0;
                while (true)
                {
                    while (_Counter.TryPeek(out counted) && counted >= _Timeout && _Counter.TryDequeue(out counted))
                        unCount += counted;
                    if (_Counter.TryPeek(out counted))
                    {
                        timeout = _Timeout - counted;
                        if (timeout.TotalMilliseconds < Accurence)
                            continue;
                        ThrottleTimer.Interval = timeout.TotalMilliseconds;
                        ThrottleTimer.Start();
                    }
                    break;
                }
                if (unCount > 0)
                    CurrentCount -= unCount;
            }
            if (_Limit < 1 || CurrentCount < _Limit)
            {
                await ThrottleEvent.SetAsync().DynamicContext();
            }
            else
            {
                await ThrottleEvent.ResetAsync().DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ThrottleTable.Throttles.Remove(GUID, out _);
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = sync;
            ThrottleEvent.Dispose();
            ThrottleTimer.Dispose();
            _Counter.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            ThrottleTable.Throttles.Remove(GUID, out _);
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = await sync.SyncContextAsync().DynamicContext();
            await ThrottleEvent.DisposeAsync().DynamicContext();
            ThrottleTimer.Dispose();
            _Counter.Clear();
        }

        /// <summary>
        /// Counted
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="count">Count</param>
        [StructLayout(LayoutKind.Explicit)]
        protected readonly struct Counted(in int count)
        {
            /// <summary>
            /// Count
            /// </summary>
            [FieldOffset(0)]
            public readonly int Count = count;
            /// <summary>
            /// Time
            /// </summary>
            [FieldOffset(sizeof(int))]
            public readonly DateTime Time = DateTime.Now;

            /// <summary>
            /// Age
            /// </summary>
            public TimeSpan Age => DateTime.Now - Time;

            /// <summary>
            /// Cast from <see cref="int"/>
            /// </summary>
            /// <param name="count">Count</param>
            public static implicit operator Counted(in int count) => new(count);

            /// <summary>
            /// Cast as <see cref="Count"/>
            /// </summary>
            /// <param name="counted"><see cref="Counted"/></param>
            public static implicit operator int(in Counted counted) => counted.Count;

            /// <summary>
            /// Cast as <see cref="Age"/>
            /// </summary>
            /// <param name="counted"><see cref="Counted"/></param>
            public static implicit operator TimeSpan(in Counted counted) => counted.Age;
        }
    }
}
