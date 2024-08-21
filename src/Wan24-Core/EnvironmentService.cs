using static wan24.Core.TranslationHelper;

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
                yield return new(__("User"), Environment.UserName, __("Current user name"));
                yield return new(__("Domain"), Environment.UserDomainName, __("Current user domain"));
                yield return new(__("Interactive"), Environment.UserInteractive, __("Is the current user interactive?"));
                yield return new(__("Machine"), Environment.MachineName, __("Machine name"));
                yield return new(__("OS"), Environment.OSVersion, __("Operating system identifier"));
                yield return new(__("64bit OS"), Environment.Is64BitOperatingSystem, __("Is a 64bit operating system?"));
                yield return new(__("Browser"), ENV.IsBrowserApp, __("Is an app running in a browser?"));
                yield return new(__("Browser environment"), ENV.IsBrowserEnvironment, __("Is a browser app environment (WASM)?"));
                yield return new(__("Uptime"), TimeSpan.FromMilliseconds(Environment.TickCount64), __("Operating system uptime since the last restart"));
                yield return new(__(".NET CLR version"), Environment.Version, __(".NET CLR version"));
                yield return new(__("CPU cores"), Environment.ProcessorCount, __("Number of CPU cores"));
                yield return new(__("Process ID"), Environment.ProcessId, __("Current process ID"));
                yield return new(__("64bit process"), Environment.Is64BitProcess, __("Is a 64bit process?"));
                yield return new(__("Privileged"), Environment.IsPrivilegedProcess, __("Is the process privileged to perform security sensitive operations?"));
                yield return new(__("Process path"), Environment.ProcessPath, __("Current process path"));
                yield return new(__("System folder"), Environment.SystemDirectory, __("Operating system folder"));
                yield return new(__("Temp folder"), Settings.TempFolder, __("Temporary folder"));
                yield return new(__("Current folder"), Environment.CurrentDirectory, __("Current folder"));
                yield return new(__("Page size"), Environment.SystemPageSize, __("Operating system memory page size in bytes"));
                yield return new(__("Working set"), Environment.WorkingSet, __("Physical memory mapped to the current process in bytes"));
                yield return new(__("Buffer size"), Settings.BufferSize, __("Default buffer size in bytes"));
                yield return new(__("stackalloc border"), Settings.StackAllocBorder, __("Stack allocation limitation border in bytes"));
                yield return new(__("App ID"), Settings.AppId, __("App ID"));
                yield return new(__("App process"), Settings.ProcessId, __("App process ID"));
                yield return new(__("Create file"), Settings.CreateFileMode, __("Linux create file mode"));
                yield return new(__("Create folder"), Settings.CreateFolderMode, __("Linux create folder mode"));
                yield return new(__("Log level"), Settings.LogLevel, __("Default log level"));
                yield return new(__("Services"), ServiceWorkerTable.ServiceWorkers.Count, __("Number of registered service worker instances"));
                yield return new(__("Timers"), TimerTable.Timers.Count, __("Number of registered timer instances"));
                yield return new(__("Pools"), PoolTable.Pools.Count, __("Number of pools"));
                yield return new(__("Object lock managers"), ObjectLockTable.ObjectLocks.Count, __("Number of registered object lock manager instances"));
                yield return new(__("Active processes"), ProcessTable.Processing.Count, __("Number of active registered processes"));
                yield return new(__("Delayed processes"), DelayTable.Delays.Count, __("Number of delayed processes"));
                yield return new(__("Throttles"), ThrottleTable.Throttles.Count, __("Number of throttles"));
                yield return new(__("Exceptions"), ErrorHandling.ExceptionCount, __("Total number of exceptions"));
                yield return new(__("Problems"), Problems.Count, __("Total number of collected problems"));
                if (!IncludeSummaries) yield break;
                yield return new(__("Running services"), ServiceWorkerTable.ServiceWorkers.Values.Count(s => s.IsRunning), __("Number of running service workers"));
                yield return new(__("Queued items"), ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IQueueWorker qw ? qw.Queued : 0)), __("Number of enqueued items to be processed by service workers"));
                yield return new(__("Pre-forked"), ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IInstancePool ip ? ip.Available : 0)), __("Number of pre-forked object instances"));
                yield return new(__("Objects"), ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IObjectStorage os ? os.Stored : 0)), __("Number of stored object instances"));
                yield return new(__("References"), ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (double)(s is IObjectStorage os ? os.ObjectReferences : 0)), __("Number of active stored object references"));
                yield return new(__("Running timers"), TimerTable.Timers.Values.Count(t => t.IsRunning), __("Number of running timers"));
                yield return new(__("Active object locks"), ObjectLockTable.ObjectLocks.Values.Sum(l => (long)l.ActiveLocks), __("Number of locked objects"));
            }
        }

        /// <summary>
        /// Include summaries?
        /// </summary>
        public bool IncludeSummaries { get; set; } = true;

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
        /// Average number of enqueued items to be processed by service workers
        /// </summary>
        public long QueuedItems { get; protected set; } = ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IQueueWorker qw ? qw.Queued : 0));

        /// <summary>
        /// Average number of pre-forked object instances
        /// </summary>
        public long PreForked { get; protected set; } = ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IInstancePool ip ? ip.Available : 0));

        /// <summary>
        /// Average number of stored object instances
        /// </summary>
        public long StoredObjects { get; protected set; } = ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IObjectStorage os ? os.Stored : 0));

        /// <summary>
        /// Average number of references to stored object instances
        /// </summary>
        public double ObjectReferences { get; protected set; } = ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (double)(s is IObjectStorage os ? os.ObjectReferences : 0));

        /// <summary>
        /// Running timers count average
        /// </summary>
        public long LockedObjects { get; protected set; } = ObjectLockTable.ObjectLocks.Values.Sum(l => (long)l.ActiveLocks);

        /// <inheritdoc/>
        protected override Task TimedWorkerAsync()
        {
            WorkingSet = (long)Math.Ceiling((double)(WorkingSet + Environment.WorkingSet) / 2);
            Services = (long)Math.Ceiling((double)(Services + ServiceWorkerTable.ServiceWorkers.Count) / 2);
            Timers = (long)Math.Ceiling((double)(Timers + TimerTable.Timers.Count) / 2);
            if (!IncludeSummaries) return Task.CompletedTask;
            RunningServices = (long)Math.Ceiling((double)(RunningServices + ServiceWorkerTable.ServiceWorkers.Values.Count(s => s.IsRunning)) / 2);
            RunningTimers = (long)Math.Ceiling((double)(RunningTimers + TimerTable.Timers.Values.Count(t => t.IsRunning)) / 2);
            QueuedItems = (long)Math.Ceiling((double)(QueuedItems + ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IQueueWorker qw ? qw.Queued : 0))) / 2);
            PreForked = (long)Math.Ceiling((double)ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IInstancePool ip ? ip.Available : 0)) / 2);
            StoredObjects = (long)Math.Ceiling((double)ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (long)(s is IObjectStorage os ? os.Stored : 0)) / 2);
            ObjectReferences = Math.Ceiling(ServiceWorkerTable.ServiceWorkers.Values.Sum(s => (double)(s is IObjectStorage os ? os.ObjectReferences : 0)) / 2);
            LockedObjects = (long)Math.Ceiling((double)(LockedObjects + ObjectLockTable.ObjectLocks.Values.Sum(l => l.ActiveLocks)) / 2);
            return Task.CompletedTask;
        }
    }
}
