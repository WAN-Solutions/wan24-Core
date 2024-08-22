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
            IsBrowserApp = IsBrowserEnvironment = RuntimeInformation.OSDescription.IsLike("web") ||
                RuntimeInformation.OSDescription.IsLike("Browser");
            IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IsLinux = Environment.OSVersion.Platform == PlatformID.Unix;
#if !RELEASE
            IsDebug = true;
#else
            IsDebug = false;
#endif
            IsPrivileged = Environment.IsPrivilegedProcess;
            if (!IsBrowserEnvironment)
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
                            if (string.IsNullOrWhiteSpace(app)) throw new InvalidProgramException("Failed to determine app path and filename");
                        }
                    }
                }
                App = app;
                AppFolder = Path.GetDirectoryName(App) ?? throw new InvalidProgramException("Failed to get the app path");
                AppCommand = app.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    ? IsWindows ? $"dotnet {app}" : $"/usr/bin/dotnet {app}"
                    : app;
                _CliArguments = Environment.GetCommandLineArgs();
            }
            else
            {
                App = AppFolder = AppCommand = string.Empty;
                _CliArguments = [];
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
        public static bool IsBrowserApp { get; set; }

        /// <summary>
        /// Is a browser environment?
        /// </summary>
        public static bool IsBrowserEnvironment { get; }

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

        /// <summary>
        /// Current call stack
        /// </summary>
        public static string Stack => new StackTrace(fNeedFileInfo: true).ToString();
    }
}
