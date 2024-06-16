using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Timeout (when comparing instances, and not the timeout time, you should use the <see cref="Equals(object?)"/> method!)
    /// </summary>
    public class Timeout : DisposableBase, ITimer
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
        public Timeout(in TimeSpan time, in bool autoReset = false, in bool start = false) : base()
        {
            TimerTable.Timers[GUID] = this;
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
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        TimeSpan ITimer.Interval => Time;

        /// <inheritdoc/>
        DateTime ITimer.LastElapsed => LastTimeout;

        /// <inheritdoc/>
        DateTime ITimer.Scheduled => DateTime.Now + RemainingTime;

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

        /// <inheritdoc/>
        public bool AutoReset
        {
            get => Timer.AutoReset;
            set => Timer.AutoReset = value;
        }

        /// <inheritdoc/>
        public bool IsRunning => Timer.Enabled;

        /// <summary>
        /// Last timeout
        /// </summary>
        public DateTime LastTimeout { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Remaining time until the next timeout
        /// </summary>
        public TimeSpan RemainingTime => Timer.Enabled ? Time - (DateTime.Now - LastTimeout) : TimeSpan.Zero;

        /// <inheritdoc/>
        public DateTime Started { get; protected set; } = DateTime.MinValue;

        /// <inheritdoc/>
        public DateTime Stopped { get; protected set; } = DateTime.MinValue;

        /// <inheritdoc/>
        bool IServiceWorker.IsPaused => false;

        /// <inheritdoc/>
        bool IServiceWorker.CanPause => false;

        /// <inheritdoc/>
        DateTime IServiceWorker.Paused => DateTime.MinValue;

        /// <summary>
        /// Start
        /// </summary>
        [UserAction(), DisplayText("Start"), Description("Start the timer")]
        public virtual void Start()
        {
            if (Timer.Enabled) return;
            LastTimeout = DateTime.Now;
            Started = DateTime.Now;
            Timer.Start();
        }

        /// <inheritdoc/>
        Task IServiceWorker.StartAsync()
        {
            Start();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        Task IServiceWorker.PauseAsync() => throw new NotSupportedException();

        /// <inheritdoc/>
        Task IServiceWorker.ResumeAsync() => throw new NotSupportedException();

        /// <summary>
        /// Stop
        /// </summary>
        [UserAction(), DisplayText("Stop"), Description("Stop the timer")]
        public virtual void Stop()
        {
            if (!Timer.Enabled) return;
            Stopped = DateTime.Now;
            Timer.Stop();
        }

        /// <inheritdoc/>
        Task IServiceWorker.StopAsync()
        {
            Stop();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reset
        /// </summary>
        [UserAction(), DisplayText("Reset"), Description("Stop and start the timer")]
        public virtual void Reset()
        {
            Stop();
            Start();
        }

        /// <inheritdoc/>
        Task IServiceWorker.RestartAsync()
        {
            Reset();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            TimerTable.Timers.Remove(GUID, out _);
            Timer.Dispose();
        }

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
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator TimeSpan(in Timeout timeout) => timeout.RemainingTime;

        /// <summary>
        /// If running
        /// </summary>
        /// <param name="timeout">Timeout</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in Timeout timeout) => timeout.Timer.Enabled;

        /// <summary>
        /// Last timeout
        /// </summary>
        /// <param name="timeout">Timeout</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator DateTime(in Timeout timeout) => timeout.LastTimeout;

        /// <summary>
        /// Cast a <see cref="TimeSpan"/> (the timeout) as <see cref="Timeout"/>
        /// </summary>
        /// <param name="timeout">Timeout</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static explicit operator Timeout(in TimeSpan timeout) => new(timeout);

        /// <summary>
        /// Add more time (and restart, if running)
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="time">Time to add</param>
        /// <returns>Timeout</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Timeout operator +(in Timeout timeout, in TimeSpan time)
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
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Timeout operator -(in Timeout timeout, in TimeSpan time)
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
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in Timeout a, in Timeout b) => a.Time == b.Time;

        /// <summary>
        /// Time equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in Timeout a, in TimeSpan b) => a.Time == b;

        /// <summary>
        /// Time not equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in Timeout a, in Timeout b) => a.Time != b.Time;

        /// <summary>
        /// Time not equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in Timeout a, in TimeSpan b) => a.Time != b;

        /// <summary>
        /// Lower
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator <(in Timeout a, in Timeout b) => a.Time < b.Time;

        /// <summary>
        /// Lower
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator <(in Timeout a, in TimeSpan b) => a.Time < b;

        /// <summary>
        /// Greater
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator >(in Timeout a, in Timeout b) => a.Time > b.Time;

        /// <summary>
        /// Greater
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator >(in Timeout a, in TimeSpan b) => a.Time > b;

        /// <summary>
        /// Lower or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower or equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator <=(in Timeout a, in Timeout b) => a.Time <= b.Time;

        /// <summary>
        /// Lower or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is lower or equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator <=(in Timeout a, in TimeSpan b) => a.Time <= b;

        /// <summary>
        /// Greater or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater or equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator >=(in Timeout a, in Timeout b) => a.Time >= b.Time;

        /// <summary>
        /// Greater or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is greater or equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator >=(in Timeout a, in TimeSpan b) => a.Time >= b;

        /// <summary>
        /// Run an action
        /// </summary>
        /// <param name="delay">Delay</param>
        /// <param name="action">Action</param>
        /// <returns>Timeout (will be disposed automatic when the action is being executed)</returns>
        public static Timeout RunAction(in TimeSpan delay, Action action)
        {
            Timeout res = new(delay)
            {
                Name = "Delayed action execution"
            };
            res.OnTimeout += (s, e) =>
            {
                res.Dispose();
                _ = Task.Run(action);
            };
            res.Start();
            return res;
        }

        /// <summary>
        /// Run an asynchronous action
        /// </summary>
        /// <param name="delay">Delay</param>
        /// <param name="action">Action</param>
        /// <returns>Timeout (will be disposed automatic when the action is being executed)</returns>
        public static Timeout RunAction(in TimeSpan delay, Func<Task> action)
        {
            Timeout res = new(delay)
            {
                Name = "Delayed action execution"
            };
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
