using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Timeout (when comparing instances, and not the timeout time, you should use the <see cref="Timeout.Equals(object?)"/> method!)
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
            Timer.Elapsed += (s, e) =>
            {
                LastTimeout = DateTime.Now;
                RaiseOnTimeout();
            };
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
        /// Last timeout
        /// </summary>
        public DateTime LastTimeout { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Remaining time until the next timeout
        /// </summary>
        public TimeSpan RemainingTime => Timer.Enabled ? Time - (DateTime.Now - LastTimeout) : TimeSpan.Zero;

        /// <summary>
        /// Start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Start()
        {
            if (Timer.Enabled) return;
            LastTimeout = DateTime.Now;
            Timer.Start();
        }

        /// <summary>
        /// Stop
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals(object? obj) => base.Equals(obj);

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

        /// <summary>
        /// Remaining time until the next timeout
        /// </summary>
        /// <param name="timeout">Timeout</param>
        public static implicit operator TimeSpan(Timeout timeout) => timeout.RemainingTime;

        /// <summary>
        /// If running
        /// </summary>
        /// <param name="timeout">Timeout</param>
        public static implicit operator bool(Timeout timeout) => timeout.Timer.Enabled;

        /// <summary>
        /// Last timeout
        /// </summary>
        /// <param name="timeout">Timeout</param>
        public static implicit operator DateTime(Timeout timeout) => timeout.LastTimeout;

        /// <summary>
        /// Cast a <see cref="TimeSpan"/> (the timeout) as <see cref="Timeout"/>
        /// </summary>
        /// <param name="timeout">Timeout</param>
        public static explicit operator Timeout(TimeSpan timeout) => new(timeout);

        /// <summary>
        /// Add more time (and restart, if running)
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="time">Time to add</param>
        /// <returns>Timeout</returns>
        public static Timeout operator +(Timeout timeout, TimeSpan time)
        {
            timeout.Time += time;
            return timeout;
        }

        /// <summary>
        /// Remove time (and restart, if running)
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="time">Time to remove</param>
        /// <returns>Timeout</returns>
        public static Timeout operator -(Timeout timeout, TimeSpan time)
        {
            timeout.Time -= time;
            return timeout;
        }

        /// <summary>
        /// Time equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is equal?</returns>
        public static bool operator ==(Timeout a, Timeout b) => a.Time == b.Time;

        /// <summary>
        /// Time equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is equal?</returns>
        public static bool operator ==(Timeout a, TimeSpan b) => a.Time == b;

        /// <summary>
        /// Time not equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is not equal?</returns>
        public static bool operator !=(Timeout a, Timeout b) => a.Time != b.Time;

        /// <summary>
        /// Time not equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is not equal?</returns>
        public static bool operator !=(Timeout a, TimeSpan b) => a.Time != b;

        /// <summary>
        /// Lower
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower?</returns>
        public static bool operator <(Timeout a, Timeout b) => a.Time < b.Time;

        /// <summary>
        /// Lower
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower?</returns>
        public static bool operator <(Timeout a, TimeSpan b) => a.Time < b;

        /// <summary>
        /// Greater
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater?</returns>
        public static bool operator >(Timeout a, Timeout b) => a.Time > b.Time;

        /// <summary>
        /// Greater
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater?</returns>
        public static bool operator >(Timeout a, TimeSpan b) => a.Time > b;

        /// <summary>
        /// Lower or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower or equal?</returns>
        public static bool operator <=(Timeout a, Timeout b) => a.Time <= b.Time;

        /// <summary>
        /// Lower or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower or equal?</returns>
        public static bool operator <=(Timeout a, TimeSpan b) => a.Time <= b;

        /// <summary>
        /// Greater or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater or equal?</returns>
        public static bool operator >=(Timeout a, Timeout b) => a.Time >= b.Time;

        /// <summary>
        /// Greater or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater or equal?</returns>
        public static bool operator >=(Timeout a, TimeSpan b) => a.Time >= b;

        /// <summary>
        /// Run an acion
        /// </summary>
        /// <param name="delay">Delay</param>
        /// <param name="action">Action</param>
        /// <returns>Timeout (will be disposed automatic when the action is being executed)</returns>
        public static Timeout RunAction(TimeSpan delay, Action action)
        {
            Timeout res = new(delay);
            res.OnTimeout += (s, e) =>
            {
                res.Dispose();
                _ = Task.Run(action);
            };
            res.Start();
            return res;
        }

        /// <summary>
        /// Run an asynchronous acion
        /// </summary>
        /// <param name="delay">Delay</param>
        /// <param name="action">Action</param>
        /// <returns>Timeout (will be disposed automatic when the action is being executed)</returns>
        public static Timeout RunAction(TimeSpan delay, Func<Task> action)
        {
            Timeout res = new(delay);
            res.OnTimeout += async (s, e) =>
            {
                await Task.Yield();
                res.Dispose();
                await action().DynamicContext();
            };
            res.Start();
            return res;
        }
    }
}
