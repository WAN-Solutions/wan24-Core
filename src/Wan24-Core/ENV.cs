using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Environment information
    /// </summary>
    public static class ENV
    {
        /// <summary>
        /// CLI Arguments
        /// </summary>
        internal static string[] _CliArguments;

        /// <summary>
        /// Constructor
        /// </summary>
        static ENV()
        {
            IsBrowserApp = RuntimeInformation.OSDescription == "web";
            IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IsLinux = Environment.OSVersion.Platform == PlatformID.Unix;
#if !RELEASE
            IsDebug = true;
#else
            IsDebug = false;
#endif
            IsPrivileged = Environment.IsPrivilegedProcess;
            if (!IsBrowserApp)
            {
                string? app = Assembly.GetEntryAssembly()?.Location;
                if (string.IsNullOrWhiteSpace(app))
                {
                    app = Environment.ProcessPath;
                    if (string.IsNullOrWhiteSpace(app))
                    {
                        try
                        {
                            app = Process.GetCurrentProcess().MainModule?.FileName;
                        }
                        catch
                        {
                            app = null;
                        }
                        if (string.IsNullOrWhiteSpace(app))
                        {
                            try
                            {
                                app = Environment.GetCommandLineArgs().FirstOrDefault();
                                if (string.IsNullOrWhiteSpace(app) || !File.Exists(app)) app = null;
                            }
                            catch
                            {
                                app = null;
                            }
                            if (string.IsNullOrWhiteSpace(app)) throw new InvalidProgramException("Faied to determine app path and filename");
                        }
                    }
                }
                App = app;
                AppFolder = Path.GetDirectoryName(App) ?? throw new InvalidProgramException("Failed to get the app path");
                if (app.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    AppCommand = IsWindows ? $"dotnet {app}" : $"/usr/bin/dotnet {app}";
                }
                else
                {
                    AppCommand = app;
                }
                _CliArguments = Environment.GetCommandLineArgs();
            }
            else
            {
                App = AppFolder = AppCommand = string.Empty;
                _CliArguments = [];
            }
        }

        /// <summary>
        /// App state
        /// </summary>
        public static IEnumerable<Status> State
        {
            get
            {
                yield return new("App path", App, "App assemly path");
                yield return new("App folder", AppFolder, "App folder");
                yield return new("App command", AppCommand, "App command");
                yield return new("CLI arguments", CliArguments.Length, "Number of CLI arguments");
                yield return new("Browser app", IsBrowserApp, "If this is a browser app");
                yield return new("Windows", IsWindows, "If the executing platform is Windows");
                yield return new("Linux", IsLinux, "If the executing platform is Linux");
                yield return new("Debug", IsDebug, "If this is a debug build");
                yield return new("User", Environment.UserName, "Current user name");
                yield return new("Domain", Environment.UserDomainName, "Current user domain");
                yield return new("Interactive", Environment.UserInteractive, "Is the current user interactive?");
                yield return new("Machine", Environment.MachineName, "Machine name");
                yield return new("OS", Environment.OSVersion, "Operating system identifier");
                yield return new("64bit OS", Environment.Is64BitOperatingSystem, "Is a 64bit operating system?");
                yield return new("Uptime", TimeSpan.FromMilliseconds(Environment.TickCount64), "Operating system uptime since the last restart");
                yield return new(".NET CLR version", Environment.Version, ".NET CLR version");
                yield return new("CPU cores", Environment.ProcessorCount, "Number of CPU cores");
                yield return new("Process ID", Environment.ProcessId, "Current process ID");
                yield return new("64bit process", Environment.Is64BitProcess, "Is a 64bit process?");
                yield return new("Privileged", Environment.IsPrivilegedProcess, "Is the process privileged to perform security sensitive operations?");
                yield return new("Process path", Environment.ProcessPath, "Current process path");
                yield return new("System folder", Environment.SystemDirectory, "Operating system folder");
                yield return new("Temp folder", Settings.TempFolder, "Temporary folder");
                yield return new("Current folder", Environment.CurrentDirectory, "Current folder");
                yield return new("Page size", Environment.SystemPageSize, "Operating system memory page size in bytes");
                yield return new("Working set", Environment.WorkingSet, "Physical memory mapped to the current process in bytes");
                yield return new("Buffer size", Settings.BufferSize, "Default bufer size in bytes");
                yield return new("stackalloc border", Settings.StackAllocBorder, "Stack allocation limitation border in bytes");
                yield return new("App ID", Settings.AppId, "App ID");
                yield return new("App process", Settings.ProcessId, "App process ID");
                yield return new("Create file", Settings.CreateFileMode, "Linux create file mode");
                yield return new("Create folder", Settings.CreateFolderMode, "Linux create folder mode");
                yield return new("Log level", Settings.LogLevel, "Default log level");
                yield return new("Services", ServiceWorkerTable.ServiceWorkers.Count, "Number of registered service worker instances");
                yield return new("Timers", TimerTable.Timers.Count, "Number of registered timer instances");
                yield return new("Pools", PoolTable.Pools.Count, "Number of pools");
                yield return new("Object lock managers", ObjectLockTable.ObjectLocks.Count, "Number of registered object lock manager instances");
                yield return new("Active processes", ProcessTable.Processing.Count, "Number of active registered processes");
                yield return new("Delayed processes", DelayTable.Delays.Count, "Number of delayed processes");
                foreach (KeyValuePair<string, IEnumerable<Status>> kvp in StatusProviderTable.Provider)
                    foreach (Status status in kvp.Value)
                        yield return new(status.Name, status.State, status.Description, kvp.Key);
            }
        }

        /// <summary>
        /// Absolute app path including entry assembly filename (empty string, if runnng in a browser)
        /// </summary>
        [CliConfig]
        [Required]
        public static string App { get; set; }

        /// <summary>
        /// Absolute app folder (empty string, if runnng in a browser)
        /// </summary>
        [CliConfig]
        [Required]
        public static string AppFolder { get; set; }

        /// <summary>
        /// App start command (empty string, if runnng in a browser)
        /// </summary>
        [CliConfig]
        [Required]
        public static string AppCommand { get; set; }

        /// <summary>
        /// (A copy of) CLI arguments
        /// </summary>
        public static string[] CliArguments => [.. _CliArguments];

        /// <summary>
        /// If there are any CLI arguments (excluding the first argument, which should be the assembly filename)
        /// </summary>
        public static bool HasCliArguments => _CliArguments.Length > 1;

        /// <summary>
        /// Is a browser app?
        /// </summary>
        public static bool IsBrowserApp { get; }

        /// <summary>
        /// Is a Windows OS?
        /// </summary>
        public static bool IsWindows { get; }

        /// <summary>
        /// Is a Linux OS?
        /// </summary>
        public static bool IsLinux { get; }

        /// <summary>
        /// Is a <c>DEBUG</c> build?
        /// </summary>
        public static bool IsDebug { get; }

        /// <summary>
        /// Is this process privileged to perform security sensitive actions?
        /// </summary>
        public static bool IsPrivileged { get; }
    }
}
