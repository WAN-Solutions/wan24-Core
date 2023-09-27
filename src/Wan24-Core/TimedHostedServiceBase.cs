using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a timed hosted service
    /// </summary>
    public abstract class TimedHostedServiceBase : HostedServiceBase, ITimer, IServiceWorkerStatus
    {
        /// <summary>
        /// Timer
        /// </summary>
        protected readonly System.Timers.Timer Timer = new()
        {
            AutoReset = false
        };
        /// <summary>
        /// Run event (raised when should run)
        /// </summary>
        protected readonly ResetEvent RunningEvent = new(initialState: false);
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync WorkerSync = new();
        /// <summary>
        /// Service control thread synchronization
        /// </summary>
        protected readonly SemaphoreSync SyncControl = new();
        /// <summary>
        /// Interval
        /// </summary>
        protected double _Interval = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interval">Interval in ms</param>
        /// <param name="timer">Timer type</param>
        /// <param name="nextRun">Fixed next run time</param>
        protected TimedHostedServiceBase(in double interval, in HostedServiceTimers timer = HostedServiceTimers.Default, in DateTime? nextRun = null) : base()
        {
            TimerTable.Timers[GUID] = this;
            if (nextRun is not null && nextRun <= DateTime.Now) throw new ArgumentException("Next run is in the past", nameof(nextRun));
            Timer.Elapsed += (s, e) => RunningEvent.Set();
            Interval = interval;
            TimerType = timer;
            NextRun = (nextRun ?? DateTime.Now) - TimeSpan.FromMilliseconds(interval);
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        TimeSpan ITimer.Interval => TimeSpan.FromMilliseconds(Interval);

        /// <inheritdoc/>
        DateTime ITimer.LastElapsed => LastRun;

        /// <inheritdoc/>
        DateTime ITimer.Sheduled => NextRun;

        /// <inheritdoc/>
        bool ITimer.AutoReset => true;

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new("GUID", GUID, "Unique ID of the service object");
                yield return new("Last exception", LastException?.Message, "Last exception message");
                yield return new("Timer type", TimerType, "Type of the timer");
                yield return new("Interval", TimeSpan.FromMilliseconds(Interval), "Timer interval");
                yield return new("Last duration", LastDuration, "Last run duration");
                yield return new("Sheduled next run", NextRun, "Next sheduled run time");
                yield return new("Run once", RunOnce, "Run once, then stop and wait for the next start?");
            }
        }

        /// <summary>
        /// Interval in ms
        /// </summary>
        public double Interval { get; protected set; }

        /// <summary>
        /// Timer type
        /// </summary>
        public HostedServiceTimers TimerType { get; protected set; }

        /// <summary>
        /// Last run time (or <see cref="DateTime.MinValue"/>, if never run)
        /// </summary>
        public DateTime LastRun { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Last run duration (or <see cref="TimeSpan.Zero"/>, if never finished a run)
        /// </summary>
        public TimeSpan LastDuration { get; protected set; } = TimeSpan.Zero;

        /// <summary>
        /// Next run time (or <see cref="DateTime.MinValue"/>, if unknown)
        /// </summary>
        public DateTime NextRun { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Run once?
        /// </summary>
        public bool RunOnce { get; set; }

        /// <summary>
        /// Set the timer
        /// </summary>
        /// <param name="interval">Interval</param>
        /// <param name="timer">Timer</param>
        /// <param name="nextRun">Fixed next run time (may interrupt a running worker, service will be (re)started!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetTimerAsync(double interval, HostedServiceTimers? timer = null, DateTime? nextRun = null, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));
            timer ??= TimerType;
            // Handle fixed next run time
            if (nextRun is not null)
            {
                using SemaphoreSyncContext sscControl = await SyncControl.SyncContextAsync(cancellationToken).DynamicContext();
                try
                {
                    // Stop the timer and set a one second interval to start the timer temporary
                    await base.StopAsync(cancellationToken).DynamicContext();
                    using (SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(cancellationToken).DynamicContext())
                    {
                        Interval = 30000;
                        TimerType = timer.Value;
                    }
                    await base.StartAsync(cancellationToken).DynamicContext();
                    // Reset the timer to elapse on the desired time
                    using (SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(cancellationToken).DynamicContext())
                    {
                        Timer.Stop();
                        Interval = interval;
                        TimerType = timer.Value;
                        DateTime now = DateTime.Now;
                        if (nextRun <= now) nextRun = now;
                        NextRun = nextRun.Value;
                        Timer.Interval = nextRun == now ? 1 : (nextRun.Value - now).TotalMilliseconds;
                        Timer.Start();
                        return;
                    }
                }
                catch
                {
                    await base.StopAsync(CancellationToken.None).DynamicContext();
                    throw;
                }
            }
            // Setup the timer
            using (SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                Interval = interval;
                TimerType = timer.Value;
                if (!Timer.Enabled) return;
                // Update the running timer
                Timer.Stop();
                nextRun = (LastRun + LastDuration).AddMilliseconds(interval);
                if (nextRun <= DateTime.Now)
                {
                    await RunningEvent.SetAsync().DynamicContext();
                    return;
                }
                NextRun = nextRun.Value;
                Timer.Interval = (nextRun - DateTime.Now).Value.TotalMilliseconds;
                Timer.Start();
            }
        }

        /// <inheritdoc/>
        public sealed override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncControl.SyncContextAsync(cancellationToken).DynamicContext();
            await StartTimerAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override async Task StopAsync(CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await SyncControl.SyncContextAsync(cancellationToken).DynamicContext();
            await StopTimerAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected Task StartTimerAsync(CancellationToken cancellationToken) => base.StartAsync(cancellationToken);

        /// <summary>
        /// Stop the timer
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected Task StopTimerAsync(CancellationToken cancellationToken) => base.StopAsync(cancellationToken);

        /// <inheritdoc/>
        protected override async Task AfterStartAsync(CancellationToken cancellationToken)
        {
            using (SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                LastRun = DateTime.MinValue;
                LastDuration = TimeSpan.Zero;
                await RunningEvent.ResetAsync().DynamicContext();
            }
            await EnableTimerAsync().DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterStopAsync(CancellationToken cancellationToken)
        {
            using (SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync(cancellationToken).DynamicContext())
                Timer.Stop();
            NextRun = DateTime.MinValue;
            await RunningEvent.SetAsync().DynamicContext();
        }

        /// <inheritdoc/>
        protected sealed override async Task WorkerAsync()
        {
            bool hadException = false;
            try
            {
                while (!CancelToken.IsCancellationRequested)
                    try
                    {
                        await RunningEvent.WaitAndResetAsync(CancelToken).DynamicContext();
                        if (CancelToken.IsCancellationRequested) break;
                        LastRun = DateTime.Now;
                        await TimedWorkerAsync().DynamicContext();
                    }
                    catch (OperationCanceledException)
                    {
                        if (!CancelToken.IsCancellationRequested)
                        {
                            hadException = true;
                            throw;
                        }
                    }
                    catch
                    {
                        hadException = true;
                        throw;
                    }
                    finally
                    {
                        LastDuration = DateTime.Now - LastRun;
                        if (StopTask is not null || hadException || RunOnce || CancelToken.IsCancellationRequested)
                        {
                            if (!CancelToken.IsCancellationRequested) Cancellation!.Cancel();
                        }
                        else
                        {
                            await EnableTimerAsync().DynamicContext();
                        }
                    }
            }
            catch (OperationCanceledException)
            {
                if (hadException || !CancelToken.IsCancellationRequested) throw;
            }
            finally
            {
                if (RunOnce) _ = RaiseOnRan().DynamicContext();
            }
        }

        /// <summary>
        /// Timed service worker
        /// </summary>
        protected abstract Task TimedWorkerAsync();

        /// <summary>
        /// Enable the timer
        /// </summary>
        /// <returns>Enabled?</returns>
        protected async Task<bool> EnableTimerAsync()
        {
            using SemaphoreSyncContext ssc = await WorkerSync.SyncContextAsync().DynamicContext();
            // Find the interval for restarting the timer
            DateTime now = DateTime.Now;
            switch (TimerType)
            {
                case HostedServiceTimers.Default:
                    NextRun = now.AddMilliseconds(Interval);
                    break;
                case HostedServiceTimers.Exact:
                    if (LastRun == DateTime.MinValue)
                    {
                        NextRun = DateTime.Now;
                        await RunningEvent.SetAsync().DynamicContext();
                        return false;
                    }
                    else
                    {
                        NextRun = (LastRun + LastDuration).AddMilliseconds(Interval - LastDuration.TotalMilliseconds);
                        while (NextRun < now) NextRun = NextRun.AddMilliseconds(Interval);
                        if (NextRun == now)
                        {
                            await RunningEvent.SetAsync().DynamicContext();
                            return false;
                        }
                    }
                    break;
                case HostedServiceTimers.ExactCatchingUp:
                    if (NextRun == DateTime.MinValue)
                    {
                        NextRun = DateTime.Now;
                        await RunningEvent.SetAsync().DynamicContext();
                        return false;
                    }
                    else
                    {
                        NextRun = (NextRun + LastDuration).AddMilliseconds(Interval - LastDuration.TotalMilliseconds);
                        if (NextRun <= now)
                        {
                            // Catch up on a missing processing run
                            await RunningEvent.SetAsync().DynamicContext();
                            return false;
                        }
                    }
                    break;
                default:
                    throw new InvalidProgramException($"Timer type {TimerType} isn't implemented");
            }
            // Start the timer
            Timer.Interval = (NextRun - now).TotalMilliseconds;
            Timer.Start();
            return true;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            StopAsync().Wait();
            TimerTable.Timers.Remove(GUID, out _);
            Timer.Dispose();
            RunningEvent.Dispose();
            WorkerSync.Dispose();
            SyncControl.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await StopAsync().DynamicContext();
            TimerTable.Timers.Remove(GUID, out _);
            Timer.Dispose();
            RunningEvent.Dispose();
            WorkerSync.Dispose();
            SyncControl.Dispose();
        }

        /// <summary>
        /// Delegate for a hosted service event
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="e">Event arguments</param>
        public delegate void TimedHostedService_Delegate(TimedHostedServiceBase service, EventArgs e);

        /// <summary>
        /// Raised after ran once
        /// </summary>
        public event TimedHostedService_Delegate? OnRan;
        /// <summary>
        /// Raise the <see cref="OnRan"/> event
        /// </summary>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected async Task RaiseOnRan()
        {
            if (OnRan is null) return;
            await Task.Yield();
            OnRan?.Invoke(this, new());
        }

        /// <summary>
        /// Cast as running-flag
        /// </summary>
        /// <param name="service">Service</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in TimedHostedServiceBase service) => service.IsRunning;

        /// <summary>
        /// Cast as time until next run
        /// </summary>
        /// <param name="service">Service</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator TimeSpan(in TimedHostedServiceBase service) => service.IsRunning ? DateTime.Now - service.NextRun : TimeSpan.Zero;
    }
}
