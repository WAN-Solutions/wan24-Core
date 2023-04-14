namespace wan24.Core
{
    /// <summary>
    /// Timeout
    /// </summary>
    public class Timeout : DisposableBase
    {
        /// <summary>
        /// Timer
        /// </summary>
        protected readonly System.Timers.Timer Timer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time">Timeout time</param>
        /// <param name="autoReset">Auto-reset?</param>
        /// <param name="start">Start?</param>
        public Timeout(TimeSpan time, bool autoReset = false, bool start = false) : base()
        {
            Timer = new()
            {
                AutoReset = autoReset
            };
            Timer.Elapsed += (s, e) => RaiseOnTimeout();
            Time = time;
            if (start) Start();
        }

        /// <summary>
        /// Timeout time (setting will reset the timeout)
        /// </summary>
        public TimeSpan Time
        {
            get => TimeSpan.FromMilliseconds(Timer.Interval);
            set
            {
                bool restart = Timer.Enabled;
                if (restart) Timer.Stop();
                Timer.Interval = value.TotalMilliseconds;
                if (restart) Timer.Start();
            }
        }

        /// <summary>
        /// Auto-reset?
        /// </summary>
        public bool AutoReset
        {
            get => Timer.AutoReset;
            set => Timer.AutoReset = value;
        }

        /// <summary>
        /// Is running?
        /// </summary>
        public bool IsRunning => Timer.Enabled;

        /// <summary>
        /// Start
        /// </summary>
        public virtual void Start() => Timer.Start();

        /// <summary>
        /// Stop
        /// </summary>
        public virtual void Stop() => Timer.Stop();

        /// <summary>
        /// Reset
        /// </summary>
        public virtual void Reset()
        {
            Stop();
            Start();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Timer.Dispose();

        /// <summary>
        /// Delegate for timeout events
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="e">Event arguments</param>
        public delegate void Timeout_Delegate(Timeout timeout, EventArgs e);
        /// <summary>
        /// Raised on timeout
        /// </summary>
        public event Timeout_Delegate? OnTimeout;
        /// <summary>
        /// Raise the <see cref="OnTimeout"/> event
        /// </summary>
        protected virtual void RaiseOnTimeout() => OnTimeout?.Invoke(this, new());
    }
}
