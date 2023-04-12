using Microsoft.Extensions.Hosting;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a timed hosted service
    /// </summary>
    public abstract class TimedHostedServiceBase : DisposableBase, IHostedService
    {
        /// <summary>
        /// Stopping event (raised when stopped)
        /// </summary>
        protected readonly ManualResetEventSlim Stop = new(initialState: true);
        /// <summary>
        /// Timer
        /// </summary>
        protected readonly System.Timers.Timer Timer = new()
        {
            AutoReset = false
        };
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
        protected TimedHostedServiceBase(double interval, HostedServiceTimers timer = HostedServiceTimers.Default) : base()
        {
            Timer.Elapsed += (s, e) => ServiceAsync();
            SetTimerAsync(interval, timer).Wait();
        }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Is running?
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Is stopping?
        /// </summary>
        public bool IsStopping => !Stop.IsSet;

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
        /// <param name="nextRun">Fixed next run time (may interrupt a running worker, service will be stopped/(re)started!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetTimerAsync(double interval, HostedServiceTimers timer, DateTime? nextRun = null, CancellationToken cancellationToken = default)
        {
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));
            // Handle fixed next run time
            if (nextRun != null)
                while (true)
                    try
                    {
                        // Stop the timer and set a one second interval to start the timer temporary
                        await StopAsync(cancellationToken).DynamicContext();
                        lock (SyncObject)
                        {
                            if (IsRunning) continue;
                            Interval = 1000;
                            TimerType = timer;
                        }
                        await StartAsync(cancellationToken).DynamicContext();
                        // Reset the timer to elapse on the desired time
                        lock (SyncObject)
                        {
                            if (!Timer.Enabled) continue;
                            Timer.Stop();
                            Interval = interval;
                            TimerType = timer;
                            if (nextRun.HasValue && nextRun <= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(nextRun));
                            NextRun = nextRun.Value;
                            Timer.Interval = (nextRun.Value - DateTime.Now).TotalMilliseconds;
                            Timer.Start();
                            return;
                        }
                    }
                    catch
                    {
                        await StopAsync(default).DynamicContext();
                        throw;
                    }
            // Setup the timer
            lock (SyncObject)
            {
                Interval = interval;
                TimerType = timer;
                if (!Timer.Enabled) return;
                // Update the running timer
                Timer.Stop();
                nextRun = (LastRun + LastDuration).AddMilliseconds(interval);
                if (nextRun >= DateTime.Now)
                {
                    ServiceAsync();
                    return;
                }
                NextRun = nextRun.Value;
                Timer.Interval = (nextRun - DateTime.Now).Value.TotalMilliseconds;
                Timer.Start();
            }
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            lock (SyncObject)
            {
                if (IsRunning) return Task.CompletedTask;
                IsRunning = true;
                Started = DateTime.Now;
                Cancellation = new();
                LastRun = DateTime.MinValue;
                LastDuration = TimeSpan.Zero;
                EnableTimer();
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            bool waitStop = false;
            lock (SyncObject)
            {
                if (!IsRunning) return;
                if (!Stop.IsSet)
                {
                    // Another thread is stopping the service - wait until that tread did stop the service
                    waitStop = true;
                }
                else
                {
                    // We're going to stop the service
                    Stop.Reset();
                }
            }
            if (waitStop)
            {
                // Another thread is stopping the service - wait until that tread did stop the service
                Stop.Wait();
                return;
            }
            // Stop the service
            Timer.Stop();
            Task? serviceTask = ServiceTask;
            Cancellation!.Cancel();
            if (serviceTask != null) await serviceTask.DynamicContext();
            NextRun = DateTime.MinValue;
            Stop.Set();
        }

        /// <summary>
        /// Enable the timer
        /// </summary>
        protected void EnableTimer()
        {
            lock (SyncObject)
            {
                if (LastRun == DateTime.MinValue)
                {
                    // Fresh started service
                    NextRun = DateTime.Now.AddMilliseconds(Interval);
                }
                else
                {
                    // Find the interval for restarting the timer
                    switch (TimerType)
                    {
                        case HostedServiceTimers.Default:
                            NextRun = DateTime.Now.AddMilliseconds(Interval);
                            break;
                        case HostedServiceTimers.Exact:
                            NextRun = (LastRun + LastDuration).AddMilliseconds(Interval - LastDuration.TotalMilliseconds);
                            while (NextRun <= DateTime.Now) NextRun = NextRun.AddMilliseconds(Interval);
                            break;
                        case HostedServiceTimers.ExactCatchingUp:
                            NextRun = (NextRun + LastDuration).AddMilliseconds(Interval - LastDuration.TotalMilliseconds);
                            if (NextRun <= DateTime.Now)
                            {
                                // Catch up a missing processing run
                                ServiceAsync();
                                return;
                            }
                            break;
                        default:
                            throw new NotImplementedException($"Timer type {TimerType} isn't implemented");
                    }
                }
                // Start the timer
                Timer.Interval = (NextRun - DateTime.Now).TotalMilliseconds;
                Timer.Start();
            }
        }

        /// <summary>
        /// Service handler
        /// </summary>
        protected async void ServiceAsync()
        {
            await Task.Yield();
            bool hadException = false;
            try
            {
                // Wait for the worker to finish
                LastRun = DateTime.Now;
                ServiceTask = WorkerAsync();
                await ServiceTask.DynamicContext();
            }
            catch (Exception ex)
            {
                // Handle a worker exception
                hadException = true;
                LastException = ex;
                OnException?.Invoke(this, new());
            }
            finally
            {
                ServiceTask = null;
                LastDuration = DateTime.Now - LastRun;
                if (!Stop.IsSet || hadException || RunOnce)
                {
                    // Process requested service stop (also stop on exception)
                    Cancellation!.Dispose();
                    Cancellation = null;
                    IsRunning = false;
                    if (RunOnce) RaiseOnRan();
                }
                else
                {
                    // Restart the timer
                    EnableTimer();
                }
            }
        }

        /// <summary>
        /// Service worker
        /// </summary>
        protected abstract Task WorkerAsync();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            StopAsync(default).Wait();
            Stop.Dispose();
            Timer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await StopAsync(default).DynamicContext();
            Stop.Dispose();
            Timer.Dispose();
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
        protected async void RaiseOnRan()
        {
            if (OnRan == null) return;
            await Task.Yield();
            OnRan?.Invoke(this, new());
        }
    }
}
