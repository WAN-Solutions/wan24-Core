namespace wan24.Core
{
    /// <summary>
    /// Environment metrics measuring service
    /// </summary>
    public class EnvironmentService : TimedHostedServiceBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interval">Interval in ms</param>
        /// <param name="timer">Timer type</param>
        /// <param name="nextRun">Fixed next run time</param>
        public EnvironmentService(in double interval, in HostedServiceTimers timer = HostedServiceTimers.Default, in DateTime? nextRun = null) : base(interval, timer, nextRun)
            => Name = "Environment metrics";

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status state in base.State) yield return state;
                yield return new("User", Environment.UserName, "Current user name");
                yield return new("Domain", Environment.UserDomainName, "Current user dmain");
                yield return new("Interactive", Environment.UserInteractive, "Is the current user interactive?");
                yield return new("Machine", Environment.MachineName, "Machine name");
                yield return new("OS", Environment.OSVersion, "Operating system identifier");
                yield return new("64bit OS", Environment.Is64BitOperatingSystem, "Is a 64bit operating system?");
                yield return new("Uptime", TimeSpan.FromMilliseconds(Environment.TickCount64), "Operating system uptime since the last restart");
                yield return new(".NET CLR version", Environment.Version, ".NET CLR version");
                yield return new("CPU cores", Environment.ProcessorCount, "Number of CPU cores");
                yield return new("Process ID", Environment.ProcessId, "Current process ID");
                yield return new("64bit process", Environment.Is64BitProcess, "Is a 64bit process?");
                yield return new("Process path", Environment.ProcessPath, "Current process path");
                yield return new("System folder", Environment.SystemDirectory, "Operating system folder");
                yield return new("Temp folder", Settings.TempFolder, "Temporary folder");
                yield return new("Current folder", Environment.CurrentDirectory, "Current folder");
                yield return new("Page size", Environment.SystemPageSize, "Operating system memory page size in bytes");
                yield return new("Working set", Environment.WorkingSet, "Physical memory mapped to the current process in bytes");
                yield return new("Services", ServiceWorkerTable.ServiceWorkers.Count, "Number of registered service worker instances");
                yield return new("Running services", ServiceWorkerTable.ServiceWorkers.Values.Count(s => s.IsRunning), "Number of running service workers");
                yield return new("Queued items", ServiceWorkerTable.ServiceWorkers.Values.Sum(s => s is IQueueWorker qw ? qw.Queued : 0), "Number of enqueued items to be processed by service workers");
                yield return new("Timers", TimerTable.Timers.Count, "Number of registered timer instances");
                yield return new("Running timers", TimerTable.Timers.Values.Count(t => t.IsRunning), "Number of running timers");
                yield return new("Pools", PoolTable.Pools.Count, "Number of pools");
                yield return new("Object lock managers", ObjectLockTable.ObjectLocks.Count, "Number of registered object lock manager instances");
                yield return new("Active object locks", ObjectLockTable.ObjectLocks.Values.Sum(l => l.ActiveLocks), "Number of locked objects");
                yield return new("Active processes", ProcessTable.Processing.Count, "Number of active registered processings");
                yield return new("Delayed processes", DelayTable.Delays.Count, "Number of delayed processings");
            }
        }

        /// <summary>
        /// Working set average in bytes
        /// </summary>
        public long WorkingSet { get; protected set; } = Environment.WorkingSet;

        /// <summary>
        /// Service count average
        /// </summary>
        public long Services { get; protected set; } = ServiceWorkerTable.ServiceWorkers.Count;

        /// <summary>
        /// Running service count average
        /// </summary>
        public long RunningServices { get; protected set; } = ServiceWorkerTable.ServiceWorkers.Values.Count(s => s.IsRunning);

        /// <summary>
        /// Timers count average
        /// </summary>
        public long Timers { get; protected set; } = TimerTable.Timers.Count;

        /// <summary>
        /// Running timers count average
        /// </summary>
        public long RunningTimers { get; protected set; } = TimerTable.Timers.Values.Count(t => t.IsRunning);

        /// <summary>
        /// Number of enqueued items to be processed by service workers
        /// </summary>
        public long QueuedItems { get; protected set; } = ServiceWorkerTable.ServiceWorkers.Values.Sum(s => s is IQueueWorker qw ? qw.Queued : 0);

        /// <summary>
        /// Running timers count average
        /// </summary>
        public long LockedObjects { get; protected set; } = ObjectLockTable.ObjectLocks.Values.Sum(l => l.ActiveLocks);

        /// <inheritdoc/>
        protected override Task TimedWorkerAsync()
        {
            WorkingSet = (long)Math.Ceiling((double)(WorkingSet + Environment.WorkingSet) / 2);
            Services = (long)Math.Ceiling((double)(Services + ServiceWorkerTable.ServiceWorkers.Count) / 2);
            RunningServices = (long)Math.Ceiling((double)(RunningServices + ServiceWorkerTable.ServiceWorkers.Values.Count(s => s.IsRunning)) / 2);
            Timers = (long)Math.Ceiling((double)(Timers + TimerTable.Timers.Count) / 2);
            RunningTimers = (long)Math.Ceiling((double)(RunningTimers + TimerTable.Timers.Values.Count(t => t.IsRunning)) / 2);
            QueuedItems = (long)Math.Ceiling((double)(QueuedItems + ServiceWorkerTable.ServiceWorkers.Values.Sum(s => s is IQueueWorker qw ? qw.Queued : 0)) / 2);
            LockedObjects = (long)Math.Ceiling((double)(LockedObjects + ObjectLockTable.ObjectLocks.Values.Sum(l => l.ActiveLocks)) / 2);
            return Task.CompletedTask;
        }
    }
}
