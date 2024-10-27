using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="IStatusProvider"/> table
    /// </summary>
    public static class StatusProviderTable
    {
        /// <summary>
        /// Status providers (key is the status group name (use a backslash to define sub-groups), value is the state enumeration)
        /// </summary>
        public static readonly ConcurrentChangeTokenDictionary<string, IEnumerable<Status>> Providers = [];

        /// <summary>
        /// App state
        /// </summary>
        public static IEnumerable<Status> State
        {
            get
            {
                ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
                ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
                yield return new(__("App path"), ENV.App, __("App assembly path"), "Core");
                yield return new(__("App folder"), ENV.AppFolder, __("App folder"), "Core");
                yield return new(__("App command"), ENV.AppCommand, __("App command"), "Core");
                yield return new(__("CLI arguments"), ENV.CliArguments.Length, __("Number of CLI arguments"), "Core");
                yield return new(__("Browser"), ENV.IsBrowserApp, __("Is an app running in a browser?"));
                yield return new(__("Browser environment"), ENV.IsBrowserEnvironment, __("Is a browser app environment (WASM)?"));
                yield return new(__("Windows"), ENV.IsWindows, __("If the executing platform is Windows"), "Core");
                yield return new(__("Linux"), ENV.IsLinux, __("If the executing platform is Linux"), "Core");
                yield return new(__("OS"), Environment.OSVersion, __("Operating system identifier"), "Core");
                yield return new(__("64bit OS"), Environment.Is64BitOperatingSystem, __("Is a 64bit operating system?"), "Core");
                yield return new(__("Debug"), ENV.IsDebug, __("If this is a debug build"), "Core");
                yield return new(__("User"), Environment.UserName, __("Current user name"), "Core");
                yield return new(__("Domain"), Environment.UserDomainName, __("Current user domain"), "Core");
                yield return new(__("Interactive"), Environment.UserInteractive, __("Is the current user interactive?"), "Core");
                yield return new(__("Machine"), Environment.MachineName, __("Machine name"), "Core");
                yield return new(__("Uptime"), TimeSpan.FromMilliseconds(Environment.TickCount64), __("Operating system uptime since the last restart"), "Core");
                yield return new(__(".NET CLR version"), Environment.Version, __(".NET CLR version"), "Core");
                yield return new(__("CLR platform"), ENV.ClrPlatformTarget, __(".NET CLR platform target"), "Core");
                yield return new(__("CPU cores"), Environment.ProcessorCount, __("Number of CPU cores"), "Core");
                yield return new(__("Process ID"), Environment.ProcessId, __("Current process ID"), "Core");
                yield return new(__("64bit process"), Environment.Is64BitProcess, __("Is a 64bit process?"), "Core");
                yield return new(__("Privileged"), Environment.IsPrivilegedProcess, __("Is the process privileged to perform security sensitive operations?"), "Core");
                yield return new(__("Process path"), Environment.ProcessPath, __("Current process path"), "Core");
                yield return new(__("System folder"), Environment.SystemDirectory, __("Operating system folder"), "Core");
                yield return new(__("Temp folder"), Settings.TempFolder, __("Temporary folder"), "Core");
                yield return new(__("Current folder"), Environment.CurrentDirectory, __("Current folder"), "Core");
                yield return new(__("Page size"), Environment.SystemPageSize, __("Operating system memory page size in bytes"), "Core");
                yield return new(__("Working set"), Environment.WorkingSet, __("Physical memory mapped to the current process in bytes"), "Core");
                yield return new(__("Threads"), ThreadPool.ThreadCount, __("Number of currently existing threads from the thread pool"), "Core");
                yield return new(__("Cmp. threads"), ThreadPool.CompletedWorkItemCount, __("Number of work items that have been processed so far using the thread pool"), "Core");
                yield return new(__("Avail. threads"), availableWorkerThreads, __("Number of available worker threads from the thread pool"), "Core");
                yield return new(__("Avail. cmp. threads"), availableCompletionPortThreads, __("Number of available completion port threads from the thread pool"), "Core");
                yield return new(__("Min. threads"), minWorkerThreads, __("Number of minimum worker threads which are created on demand from the thread pool"), "Core");
                yield return new(__("Min. cmp. threads"), minCompletionPortThreads, __("Number of minimum completion port threads which are created on demand from the thread pool"), "Core");
                yield return new(__("Max. threads"), maxWorkerThreads, __("Maximum number of concurrent thread pool worker thread requests"), "Core");
                yield return new(__("Max. cmp. threads"), maxCompletionPortThreads, __("Maximum number of concurrent thread pool completion port thread requests"), "Core");
                yield return new(__("Buffer size"), Settings.BufferSize, __("Default buffer size in bytes"), "Core");
                yield return new(__("Allocation"), Settings.StackAllocBorder, __("Stack allocation limitation in bytes"), "Core");
                yield return new(__("App ID"), Settings.AppId, __("App ID"), "Core");
                yield return new(__("App process"), Settings.ProcessId, __("App process ID"), "Core");
                yield return new(__("Create file"), Settings.CreateFileMode, __("Linux create file mode"), "Core");
                yield return new(__("Create folder"), Settings.CreateFolderMode, __("Linux create folder mode"), "Core");
                yield return new(__("Log level"), Settings.LogLevel, __("Default log level"), "Core");
                yield return new(__("Delayed processes"), DelayTable.Delays.Count, __("Number of delayed processes"), "Core");
                yield return new(__("Exceptions"), ErrorHandling.ExceptionCount, __("Total number of exceptions"), "Core");
                yield return new(__("Problems"), Problems.Count, __("Total number of collected problems"));
                // Services
                foreach (var kvp in ServiceWorkerTable.ServiceWorkers)
                    if (kvp.Value is IStatusProvider sp)
                    {
                        foreach (Status status in sp.State)
                            yield return new(status.Name, status.State, status.Description, $"Core\\{__("Services")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                    }
                    else
                    {
                        string group = $"Core\\{__("Services")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName()}";
                        yield return new(__("Type"), kvp.Value.GetType(), __("Service worker CLR type"), group);
                        yield return new(__("Name"), kvp.Value.Name, __("Service worker name"), group);
                        yield return new(__("GUID"), kvp.Key, __("Service worker GUID"), group);
                        yield return new(__("Pause"), kvp.Value.CanPause, __("If the service worker can pause"), group);
                        yield return new(__("Paused"), kvp.Value.IsPaused, __("If the service worker is paused"), group);
                        yield return new(__("Running"), kvp.Value.IsRunning, __("If the service worker is running"), group);
                        yield return new(__("Started"), kvp.Value.Started == DateTime.MinValue ? __("never") : kvp.Value.Started.ToString(), __("Last service start time"), group);
                        yield return new(__("Stopped"), kvp.Value.Stopped == DateTime.MinValue ? __("never") : kvp.Value.Stopped.ToString(), __("Last service stop time"), group);
                    }
                // Timers
                foreach (var kvp in TimerTable.Timers)
                    if (kvp.Value is IStatusProvider sp)
                    {
                        foreach (Status status in sp.State)
                            yield return new(status.Name, status.State, status.Description, $"Core\\{__("Timers")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                    }
                    else
                    {
                        string group = $"Core\\{__("Timers")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName()}";
                        yield return new(__("Type"), kvp.Value.GetType(), __("Timer CLR type"), group);
                        yield return new(__("Name"), kvp.Value.Name, __("Timer name"), group);
                        yield return new(__("GUID"), kvp.Key, __("Timer GUID"), group);
                        yield return new(__("Pause"), kvp.Value.CanPause, __("If the timer can pause"), group);
                        yield return new(__("Paused"), kvp.Value.IsPaused, __("If the timer is paused"), group);
                        yield return new(__("Running"), kvp.Value.IsRunning, __("If the timer is running"), group);
                        yield return new(__("Started"), kvp.Value.Started == DateTime.MinValue ? __("never") : kvp.Value.Started.ToString(), __("Last timer start time"), group);
                        yield return new(__("Stopped"), kvp.Value.Stopped == DateTime.MinValue ? __("never") : kvp.Value.Stopped.ToString(), __("Last timer stop time"), group);
                        yield return new(__("Reset"), kvp.Value.AutoReset, __("If the timer does auto-reset"), group);
                        yield return new(__("Interval"), kvp.Value.Interval, __("Timer interval"), group);
                        yield return new(__("Elapsed"), kvp.Value.LastElapsed == DateTime.MinValue ? "never" : kvp.Value.LastElapsed.ToString(), __("Last timer elapsed time"), group);
                        yield return new(__("Scheduled"), kvp.Value.Scheduled == DateTime.MinValue ? "not scheduled" : kvp.Value.Scheduled.ToString(), __("Next timer scheduled time"), group);
                    }
                // Pools
                foreach (var kvp in PoolTable.Pools)
                    if (kvp.Value is IStatusProvider sp)
                    {
                        foreach (Status status in sp.State)
                            yield return new(status.Name, status.State, status.Description, $"Core\\{__("Pools")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                    }
                    else
                    {
                        string group = $"Core\\{__("Pools")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName()}";
                        yield return new(__("Type"), kvp.Value.GetType(), __("Pool CLR type"), group);
                        yield return new(__("Name"), kvp.Value.Name, __("Pool name"), group);
                        yield return new(__("GUID"), kvp.Key, __("Pool GUID"), group);
                        yield return new(__("Items"), kvp.Value.ItemType, __("Pool item CLR type"), group);
                        yield return new(__("Capacity"), kvp.Value.Capacity, __("Pool item capacity"), group);
                        yield return new(__("Available"), kvp.Value.Available, __("Number of currently available items"), group);
                    }
                // Pooled temp stream memory limits
                foreach(var kvp in PooledTempStreamMemoryLimitTable.Limits)
                    foreach(Status status in kvp.Value.State)
                        yield return new(status.Name, status.State, status.Description, $"Core\\{__("Temp stream memory limits")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                // In-memory caches
                foreach (var kvp in InMemoryCacheTable.Caches)
                    foreach (Status status in kvp.Value.State)
                        yield return new(status.Name, status.State, status.Description, $"Core\\{__("In-memory caches")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                // Object lock managers
                foreach (var kvp in ObjectLockTable.ObjectLocks)
                    if (kvp.Value is IStatusProvider sp)
                    {
                        foreach (Status status in sp.State)
                            yield return new(status.Name, status.State, status.Description, $"Core\\{__("Object lock managers")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                    }
                    else
                    {
                        string group = $"Core\\{__("Object lock managers")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName()}";
                        yield return new(__("Type"), kvp.Value.GetType(), __("Object lock manager CLR type"), group);
                        yield return new(__("Name"), kvp.Value.Name, __("Object lock manager name"), group);
                        yield return new(__("GUID"), kvp.Key, __("Object lock manager GUID"), group);
                        yield return new(__("Object"), kvp.Value.ObjectType, __("Managed object CLR type"), group);
                        yield return new(__("Locks"), kvp.Value.ActiveLocks, __("Number of active locks"), group);
                    }
                // Active processes
                foreach (var kvp in ProcessTable.Processing)
                    if (kvp.Value is IStatusProvider sp)
                    {
                        foreach (Status status in sp.State)
                            yield return new(status.Name, status.State, status.Description, $"Core\\{__("Processes")}\\{kvp.Value.Description.NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                    }
                    else
                    {
                        string group = $"Core\\{__("Processes")}\\{kvp.Value.Description.NormalizeStatusGroupName()}";
                        yield return new(__("Type"), kvp.Value.GetType(), __("Process CLR type"), group);
                        yield return new(__("Description"), kvp.Value.Description, __("Process description"), group);
                        yield return new(__("GUID"), kvp.Key, __("Process GUID"), group);
                        yield return new(__("Started"), kvp.Value.Started, __("Processing start time"), group);
                    }
                // Named object serializers
                foreach (string name in ObjectSerializer.NamedSerializers.Keys)
                    yield return new(__("Serializer"), name, __("Object serializer name"), __("Object serializers"));
                // Regular expressions
                foreach (var kvp in RegularExpressions.NamedExpressions)
                    yield return new(kvp.Key, kvp.Value, __("Named regular expression"), __("Regular expressions"));
                // Throttles
                foreach (var kvp in ThrottleTable.Throttles)
                    foreach (Status status in kvp.Value.State)
                        yield return new(status.Name, status.State, status.Description, $"Core\\{__("Throttles")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                // Pipelines
                foreach (var kvp in PipelineTable.Pipelines)
                    foreach (Status status in kvp.Value.State)
                        yield return new(status.Name, status.State, status.Description, $"Core\\{__("Pipelines")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                // Queue event workers
                foreach (var kvp in QueueEventWorkerTable.Workers)
                    foreach (Status status in kvp.Value.State)
                        yield return new(status.Name, status.State, status.Description, $"Core\\{__("Queue event workers")}\\{(kvp.Value.Name ?? kvp.Key).NormalizeStatusGroupName().CombineStatusGroupNames(status.Group)}");
                // Other states
                foreach (KeyValuePair<string, IEnumerable<Status>> kvp in Providers)
                    foreach (Status status in kvp.Value)
                        yield return new(status.Name, status.State, status.Description, kvp.Key.NormalizeStatusGroupName().CombineStatusGroupNames(status.Group));
            }
        }

        /// <summary>
        /// Normalize a status group name
        /// </summary>
        /// <param name="name">Status group name</param>
        /// <returns>Normalized name</returns>
        public static string NormalizeStatusGroupName(this string name) => name.Replace('\\', '/');

        /// <summary>
        /// Combine status group names
        /// </summary>
        /// <param name="name">Status group name (should be normalized!)</param>
        /// <param name="names">Additional status group names (should be normalized!)</param>
        /// <returns>Combined name</returns>
        public static string CombineStatusGroupNames(this string name, params string?[] names)
            => string.Join('\\', [name, .. names.Where(n => !string.IsNullOrWhiteSpace(n))]);
    }
}
