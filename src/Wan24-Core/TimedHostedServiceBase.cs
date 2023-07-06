using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a timed hosted service
    /// </summary>
    public abstract class TimedHostedServiceBase : DisposableBase, IHostedService, ITimer, IServiceWorkerStatus
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
        protected readonly ResetEvent RunEvent = new(initialState: false);
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSlim Sync = new(1, 1);
        /// <summary>
        /// Thread synchronization for the start/stop control
        /// </summary>
        protected readonly SemaphoreSlim SyncControl = new(1, 1);
        /// <summary>
        /// Stop task
        /// </summary>
        protected volatile TaskCompletionSource? StopTask = null;
        /// <summary>
        /// Cancellation token
        /// </summary>
        protected CancellationTokenSource? Cancellation = null;
        /// <summary>
        /// Service task
        /// </summary>
        protected Task? ServiceTask = null;
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
        protected TimedHostedServiceBase(double interval, HostedServiceTimers timer = HostedServiceTimers.Default, DateTime? nextRun = null) : base()
        {
            TimerTable.Timers[GUID] = this;
            ServiceWorkerTable.ServiceWorkers[GUID] = this;
            if (nextRun != null && nextRun <= DateTime.Now) throw new ArgumentException("Next run is in the past", nameof(nextRun));
            Timer.Elapsed += (s, e) => RunEvent.Set();
            Interval = interval;
            TimerType = timer;
            NextRun = (nextRun ?? DateTime.Now) - TimeSpan.FromMilliseconds(interval);
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public virtual string? Name { get; set; }

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
        /// Is running?
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Is stopping?
        /// </summary>
        public bool IsStopping => StopTask != null;

        /// <summary>
        /// Last exception
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <summary>
        /// Interval in ms
        /// </summary>
        public double Interval { get; protected set; }

        /// <summary>
        /// Timer type
        /// </summary>
        public HostedServiceTimers TimerType { get; protected set; }

        /// <summary>
        /// Started time (or <see cref="DateTime.MinValue"/>, if never started)
        /// </summary>
        public DateTime Started { get; protected set; } = DateTime.MinValue;

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
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));
            timer ??= TimerType;
            // Handle fixed next run time
            if (nextRun != null)
            {
                await SyncControl.WaitAsync(cancellationToken).DynamicContext();
                try
                {
                    // Stop the timer and set a one second interval to start the timer temporary
                    await StopAsyncInt(cancellationToken).DynamicContext();
                    await Sync.WaitAsync(cancellationToken).DynamicContext();
                    try
                    {
                        Interval = 30000;
                        TimerType = timer.Value;
                    }
                    finally
                    {
                        Sync.Release();
                    }
                    await StartAsyncInt(cancellationToken).DynamicContext();
                    // Reset the timer to elapse on the desired time
                    await Sync.WaitAsync(cancellationToken).DynamicContext();
                    try
                    {
                        Timer.Stop();
                        Interval = interval;
                        TimerType = timer.Value;
                        DateTime now = DateTime.Now;
                        if (nextRun <= now) throw new ArgumentOutOfRangeException(nameof(nextRun));
                        NextRun = nextRun.Value;
                        Timer.Interval = (nextRun.Value - now).TotalMilliseconds;
                        Timer.Start();
                        return;
                    }
                    finally
                    {
                        Sync.Release();
                    }
                }
                catch
                {
                    await StopAsyncInt(default).DynamicContext();
                    throw;
                }
                finally
                {
                    SyncControl.Release();
                }
            }
            // Setup the timer
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                Interval = interval;
                TimerType = timer.Value;
                if (!Timer.Enabled) return;
                // Update the running timer
                Timer.Stop();
                nextRun = (LastRun + LastDuration).AddMilliseconds(interval);
                if (nextRun <= DateTime.Now)
                {
                    await RunEvent.SetAsync().DynamicContext();
                    return;
                }
                NextRun = nextRun.Value;
                Timer.Interval = (nextRun - DateTime.Now).Value.TotalMilliseconds;
                Timer.Start();
            }
            finally
            {
                Sync.Release();
            }
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await SyncControl.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                await StartAsyncInt(cancellationToken).DynamicContext();
            }
            finally
            {
                SyncControl.Release();
            }
        }

        /// <inheritdoc/>
        Task ITimer.StartAsync() => StartAsync(default);

        /// <inheritdoc/>
        Task IServiceWorker.StartAsync() => StartAsync(default);

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await SyncControl.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                await StopAsyncInt(cancellationToken).DynamicContext();
            }
            finally
            {
                SyncControl.Release();
            }
        }

        /// <inheritdoc/>
        Task ITimer.StopAsync() => StopAsync(default);

        /// <inheritdoc/>
        Task IServiceWorker.StopAsync() => StopAsync(default);

        /// <inheritdoc/>
        async Task ITimer.RestartAsync()
        {
            await StopAsync(default).DynamicContext();
            await StartAsync(default).DynamicContext();
        }

        /// <inheritdoc/>
        async Task IServiceWorker.RestartAsync()
        {
            await StopAsync(default).DynamicContext();
            await StartAsync(default).DynamicContext();
        }

        /// <inheritdoc/>
        public async Task StartAsyncInt(CancellationToken cancellationToken)
        {
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                if (IsRunning) return;
                Started = DateTime.Now;
                LastRun = DateTime.MinValue;
                LastDuration = TimeSpan.Zero;
                IsRunning = true;
                Cancellation = new();
                await RunEvent.ResetAsync().DynamicContext();
                _ = RunServiceAsync();
            }
            finally
            {
                Sync.Release();
            }
            await EnableTimerAsync().DynamicContext();
        }

        /// <inheritdoc/>
        public async Task StopAsyncInt(CancellationToken cancellationToken)
        {
            Task? stopTask = null;
            await Sync.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                if (!IsRunning) return;
                if (StopTask == null)
                {
                    stopTask = (StopTask = new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
                    Cancellation?.Cancel();
                    Timer.Stop();
                    NextRun = DateTime.MinValue;
                    await RunEvent.SetAsync().DynamicContext();
                }
                else
                {
                    stopTask = StopTask?.Task;
                }
            }
            finally
            {
                Sync.Release();
            }
            if (stopTask != null) await stopTask.DynamicContext();
        }

        /// <summary>
        /// Run the service
        /// </summary>
        protected async Task RunServiceAsync()
        {
            bool hadException = false;
            try
            {
                while (!Cancellation!.IsCancellationRequested)
                    try
                    {
                        await RunEvent.WaitAsync(Cancellation.Token).DynamicContext();
                        await RunEvent.ResetAsync().DynamicContext();
                        if (Cancellation.IsCancellationRequested) break;
                        LastRun = DateTime.Now;
                        ServiceTask = WorkerAsync();
                        await ServiceTask.DynamicContext();
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (!Cancellation.IsCancellationRequested)
                        {
                            hadException = true;
                            LastException = ex;
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        hadException = true;
                        LastException = ex;
                        throw;
                    }
                    finally
                    {
                        LastDuration = DateTime.Now - LastRun;
                        ServiceTask = null;
                        if (StopTask != null || hadException || RunOnce || Cancellation.IsCancellationRequested)
                        {
                            Cancellation.Cancel();
                        }
                        else
                        {
                            await EnableTimerAsync().DynamicContext();
                        }
                    }
            }
            catch (OperationCanceledException ex)
            {
                if (!hadException && !Cancellation!.IsCancellationRequested)
                {
                    hadException = true;
                    LastException = ex;
                    throw;
                }
                else if (hadException)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (!hadException)
                {
                    hadException = true;
                    LastException = ex;
                }
                throw;
            }
            finally
            {
                if (hadException) OnException?.Invoke(this, new());
                await Sync.WaitAsync().DynamicContext();
                try
                {
                    Cancellation!.Cancel();
                    Cancellation.Dispose();
                    Cancellation = null;
                    IsRunning = false;
                    if (StopTask != null)
                    {
                        StopTask.SetResult();
                        StopTask = null;
                    }
                }
                finally
                {
                    Sync.Release();
                }
                if (RunOnce) _ = RaiseOnRan();
            }
        }

        /// <summary>
        /// Service worker
        /// </summary>
        protected abstract Task WorkerAsync();

        /// <summary>
        /// Enable the timer
        /// </summary>
        /// <returns>Enabled?</returns>
        protected async Task<bool> EnableTimerAsync()
        {
            await Sync.WaitAsync().DynamicContext();
            try
            {
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
                            await RunEvent.SetAsync().DynamicContext();
                            return false;
                        }
                        else
                        {
                            NextRun = (LastRun + LastDuration).AddMilliseconds(Interval - LastDuration.TotalMilliseconds);
                            while (NextRun < now) NextRun = NextRun.AddMilliseconds(Interval);
                            if (NextRun == now)
                            {
                                await RunEvent.SetAsync().DynamicContext();
                                return false;
                            }
                        }
                        break;
                    case HostedServiceTimers.ExactCatchingUp:
                        if (NextRun == DateTime.MinValue)
                        {
                            NextRun = DateTime.Now;
                            await RunEvent.SetAsync().DynamicContext();
                            return false;
                        }
                        else
                        {
                            NextRun = (NextRun + LastDuration).AddMilliseconds(Interval - LastDuration.TotalMilliseconds);
                            if (NextRun <= now)
                            {
                                // Catch up on a missing processing run
                                await RunEvent.SetAsync().DynamicContext();
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
            finally
            {
                Sync.Release();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            StopAsync().Wait();
            TimerTable.Timers.Remove(GUID, out _);
            ServiceWorkerTable.ServiceWorkers.Remove(GUID, out _);
            Timer.Dispose();
            RunEvent.Dispose();
            Sync.Dispose();
            SyncControl.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await StopAsync().DynamicContext();
            TimerTable.Timers.Remove(GUID, out _);
            ServiceWorkerTable.ServiceWorkers.Remove(GUID, out _);
            Timer.Dispose();
            RunEvent.Dispose();
            Sync.Dispose();
            SyncControl.Dispose();
        }

        /// <summary>
        /// Delegate for a hosted service event
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="e">Event arguments</param>
        public delegate void TimedHostedService_Delegate(TimedHostedServiceBase service, EventArgs e);

        /// <summary>
        /// Raised on exception
        /// </summary>
        public event TimedHostedService_Delegate? OnException;

        /// <summary>
        /// Raised after ran once
        /// </summary>
        public event TimedHostedService_Delegate? OnRan;
        /// <summary>
        /// Raise the <see cref="OnRan"/> event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async Task RaiseOnRan()
        {
            if (OnRan == null) return;
            await Task.Yield();
            OnRan?.Invoke(this, new());
        }

        /// <summary>
        /// Cast as running-flag
        /// </summary>
        /// <param name="service">Service</param>
        public static implicit operator bool(TimedHostedServiceBase service) => service.IsRunning;

        /// <summary>
        /// Cast as time until next run
        /// </summary>
        /// <param name="service">Service</param>
        public static implicit operator TimeSpan(TimedHostedServiceBase service) => service.IsRunning ? DateTime.Now - service.NextRun : TimeSpan.Zero;
    }
}
