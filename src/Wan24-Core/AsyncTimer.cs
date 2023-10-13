using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Asynchronous timer
    /// </summary>
    public sealed class AsyncTimer : HostedServiceBase, ITimer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interval">Interval</param>
        /// <param name="handler">Elapsed handler</param>
        /// <param name="autoReset">Auto-reset?</param>
        public AsyncTimer(in TimeSpan interval, in Elapsed_Delegate handler, in bool autoReset = true) : base()
        {
            Interval = interval;
            Handler = handler;
            AutoReset = autoReset;
            TimerTable.Timers[GUID] = this;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Interval
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Elapsed handler
        /// </summary>
        public Elapsed_Delegate Handler { get; }

        /// <summary>
        /// Remaining time
        /// </summary>
        public TimeSpan RemainingTime => IsRunning && DateTime.Now > Sheduled ? DateTime.Now - Sheduled : TimeSpan.Zero;

        /// <summary>
        /// Auto-reset?
        /// </summary>
        public bool AutoReset { get; set; }

        /// <inheritdoc/>
        public DateTime LastElapsed { get; private set; } = DateTime.MinValue;

        /// <inheritdoc/>
        public DateTime Sheduled { get; private set; } = DateTime.MinValue;

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            Sheduled = DateTime.Now + Interval;
            for (; !Cancellation!.IsCancellationRequested;)
            {
                await Task.Delay(Interval, Cancellation!.Token).DynamicContext();
                if (Cancellation!.IsCancellationRequested) return;
                LastElapsed = DateTime.Now;
                await Handler(this).DynamicContext();
                if (!AutoReset) return;
                Sheduled = DateTime.Now + Interval;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            TimerTable.Timers.Remove(GUID, out _);
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override Task DisposeCore()
        {
            TimerTable.Timers.Remove(GUID, out _);
            return base.DisposeCore();
        }

        /// <summary>
        /// Delegate for an elapsed handler
        /// </summary>
        /// <param name="timer">Timer</param>
        public delegate Task Elapsed_Delegate(AsyncTimer timer);

        /// <summary>
        /// Remaining time until the next timeout
        /// </summary>
        /// <param name="timer">Timer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator TimeSpan(in AsyncTimer timer) => timer.RemainingTime;

        /// <summary>
        /// If running
        /// </summary>
        /// <param name="timer">Timer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in AsyncTimer timer) => timer.IsRunning;

        /// <summary>
        /// Last timer
        /// </summary>
        /// <param name="timer">Timer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator DateTime(in AsyncTimer timer) => timer.LastElapsed;
    }
}
