namespace wan24.Core
{
    /// <summary>
    /// Timed idle state
    /// </summary>
    public sealed class IdleState : DisposableBase
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        private readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Idle timeout
        /// </summary>
        private readonly Timeout IdleTimeout;
        /// <summary>
        /// If busy
        /// </summary>
        private volatile bool _IsBusy = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout</param>
        public IdleState(in TimeSpan idleTimeout) : base()
        {
            IdleTimeout = new(idleTimeout, start: true);
            IdleTimeout.OnTimeout += (s, e) =>
            {
                using SemaphoreSyncContext ssc = Sync;
                if (_IsBusy)
                {
                    IdleTimeout.Start();
                    return;
                }
                if (Logging.Debug)
                    Logging.WriteDebug($"{Name ?? GetType().ToString()} is idle after being busy for {LastBusyDuration}");
                LastIdleStartTime = DateTime.Now;
                _IsBusy = false;
                RaiseOnIdle();
            };
        }

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// If busy (can only be set to <see langword="true"/>!)
        /// </summary>
        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                EnsureUndisposed();
                if (!value) throw new ArgumentException("Value must be TRUE", nameof(value));
                using SemaphoreSyncContext ssc = Sync;
                SetIsBusy();
            }
        }

        /// <summary>
        /// Last (or current) idle state start time (or <see cref="DateTime.MinValue"/>)
        /// </summary>
        public DateTime LastIdleStartTime { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// If having ever been idle
        /// </summary>
        public bool WasIdle => LastIdleStartTime != DateTime.MinValue;

        /// <summary>
        /// Last (or current) idle state duration
        /// </summary>
        public TimeSpan LastIdleDuration => _IsBusy
            ? WasIdle
                ? LastBusyStartTime - LastIdleStartTime
                : TimeSpan.Zero
            : DateTime.Now - LastIdleStartTime;

        /// <summary>
        /// Time when the last busy state was issued
        /// </summary>
        public DateTime LastBusyStateIssued { get; private set; } = DateTime.Now;

        /// <summary>
        /// Last (or current) busy state start
        /// </summary>
        public DateTime LastBusyStartTime { get; private set; } = DateTime.Now;

        /// <summary>
        /// Last (or current) busy state duration
        /// </summary>
        public TimeSpan LastBusyDuration => _IsBusy ? DateTime.Now - LastBusyStartTime : LastIdleStartTime - LastBusyStartTime;

        /// <summary>
        /// Set <see cref="IsBusy"/> to <see langword="true"/>
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetIsBusyAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken);
            SetIsBusy();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            IdleTimeout.Dispose();
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await IdleTimeout.DisposeAsync().DynamicContext();
            await Sync.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Set <see cref="IsBusy"/> to <see langword="true"/>
        /// </summary>
        private void SetIsBusy()
        {
            DateTime now = DateTime.Now;
            IdleTimeout.Reset();
            LastBusyStateIssued = now;
            if (_IsBusy) return;
            if (Logging.Debug)
                Logging.WriteDebug($"{Name ?? GetType().ToString()} is busy after being idle for {LastIdleDuration}");
            LastBusyStartTime = now;
            _IsBusy = true;
            RaiseOnBusy();
        }

        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="idleState">Idle state</param>
        /// <param name="e">Arguments</param>
        public delegate void Idle_Delegate(IdleState idleState, EventArgs e);

        /// <summary>
        /// Raised when idle (while thread locked!)
        /// </summary>
        public event Idle_Delegate? OnIdle;
        /// <summary>
        /// Raise the <see cref="OnIdle"/> event
        /// </summary>
        private void RaiseOnIdle() => OnIdle?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised when busy (while thread locked!)
        /// </summary>
        public event Idle_Delegate? OnBusy;
        /// <summary>
        /// Raise the <see cref="OnBusy"/> event
        /// </summary>
        private void RaiseOnBusy() => OnBusy?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Cast as idle state
        /// </summary>
        /// <param name="state"><see cref="IdleState"/></param>
        public static implicit operator bool(in IdleState state) => !state._IsBusy;
    }
}
