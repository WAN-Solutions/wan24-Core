namespace wan24.Core
{
    /// <summary>
    /// Event throttle (allow to raise once within N ms)
    /// </summary>
    public abstract class EventThrottle : DisposableBase, IEventThrottle
    {
        /// <summary>
        /// Timer
        /// </summary>
        protected readonly System.Timers.Timer Timer;
        /// <summary>
        /// Timeout in ms
        /// </summary>
        protected int _Timeout = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeout">Timeout in ms</param>
        protected EventThrottle(int timeout) : base()
        {
            if (timeout < 1) throw new ArgumentOutOfRangeException(nameof(timeout));
            _Timeout = timeout;
            Timer = new()
            {
                AutoReset = false,
                Interval = timeout
            };
            Timer.Elapsed += (s, e) =>
            {
                DateTime raised;
                int raisedCount;
                lock (SyncObject)
                {
                    if (RaisedCount < 1) return;
                    raised = RaisedTime;
                    raisedCount = RaisedCount;
                    RaisedTime = DateTime.MinValue;
                    RaisedCount = 0;
                    Timer.Start();
                    TotalThrottledRaisedCount++;
                }
                LastRaised = DateTime.Now;
                HandleEvent(raised, raisedCount);
            };
        }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Timeout in ms
        /// </summary>
        public int Timeout
        {
            get => _Timeout;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));
                _Timeout = value;
                bool restart = Timer.Enabled;
                if (restart) Timer.Stop();
                Timer.Interval = value;
                if (restart) Timer.Start();
            }
        }

        /// <summary>
        /// Last raised (or <see cref="DateTime.MinValue"/> when never raised)
        /// </summary>
        public DateTime LastRaised { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// First raised time during throttling (or <see cref="DateTime.MinValue"/> when not raised during throttline)
        /// </summary>
        public DateTime RaisedTime { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Raised count during throttling
        /// </summary>
        public int RaisedCount { get; protected set; }

        /// <summary>
        /// Total raised count
        /// </summary>
        public long TotalRaisedCount { get; protected set; }

        /// <summary>
        /// Total throttled raised count
        /// </summary>
        public long TotalThrottledRaisedCount { get; protected set; }

        /// <summary>
        /// First raised time (or <see cref="DateTime.MinValue"/> when never raised)
        /// </summary>
        public DateTime FirstRaised { get; protected set; } = DateTime.MinValue;

        /// <inheritdoc/>
        public bool Raise()
        {
            lock (DisposeSyncObject)
            {
                EnsureUndisposed();
                lock (SyncObject)
                {
                    if (FirstRaised == DateTime.MinValue) FirstRaised = DateTime.Now;
                    TotalRaisedCount++;
                    if (Timer.Enabled)
                    {
                        if (RaisedTime == DateTime.MinValue) RaisedTime = DateTime.Now;
                        RaisedCount++;
                        return false;
                    }
                    RaisedTime = DateTime.MinValue;
                    RaisedCount = 0;
                    Timer.Start();
                    TotalThrottledRaisedCount++;
                }
            }
            LastRaised = DateTime.Now;
            HandleEvent(DateTime.Now, raisedCount: 1);
            return true;
        }

        /// <summary>
        /// Handle the event
        /// </summary>
        /// <param name="raised">First raised time</param>
        /// <param name="raisedCount">Raised count</param>
        protected abstract void HandleEvent(DateTime raised, int raisedCount);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Timer.Dispose();
    }
}
