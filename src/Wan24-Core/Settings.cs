using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace wan24.Core
{
    /// <summary>
    /// Settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Default buffer size in bytes
        /// </summary>
        public const int DEFAULT_BUFFER_SIZE = 81_920;
        /// <summary>
        /// Default stack allocation border
        /// </summary>
        public const int DEFAULT_STACK_ALLOC_BORDER = 1_024;
        /// <summary>
        /// Default browser app ID
        /// </summary>
        public const string DEFAULT_BROWSER_APP_ID = "browserApp";
        /// <summary>
        /// Default process ID
        /// </summary>
        public const string DEFAULT_PROCESS_ID = "main";

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        private static int _BufferSize = DEFAULT_BUFFER_SIZE;
        /// <summary>
        /// Custom temporary folder
        /// </summary>
        private static string? _CustomTempFolder = null;
        /// <summary>
        /// Stack allocation border in bytes
        /// </summary>
        private static int _StackAllocBorder = DEFAULT_STACK_ALLOC_BORDER;

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        [CliConfig]
        [Range(1, int.MaxValue)]
        public static int BufferSize
        {
            get => _BufferSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                _BufferSize = value;
            }
        }

        /// <summary>
        /// Temporary folder (may be the customized value or the system users temporary folder)
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Not supported in a browser app</exception>
        public static string TempFolder => ENV.IsBrowserApp ? throw new PlatformNotSupportedException("Browser app") : CustomTempFolder ?? Path.GetTempPath();

        /// <summary>
        /// Custom temporary folder
        /// </summary>
        [CliConfig]
        public static string? CustomTempFolder
        {
            get => _CustomTempFolder;
            set
            {
                if (value is not null && !Directory.Exists(value)) throw new DirectoryNotFoundException(value);
                _CustomTempFolder = value;
            }
        }

        /// <summary>
        /// Stack allocation border in bytes
        /// </summary>
        public static int StackAllocBorder
        {
            get => _StackAllocBorder;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                _StackAllocBorder = value;
            }
        }

        /// <summary>
        /// An unique app ID ("myapp" f.e.; will be used in filenames!)
        /// </summary>
        public static string AppId { get; set; } = ENV.IsBrowserApp ? DEFAULT_BROWSER_APP_ID : Path.GetFileNameWithoutExtension(ENV.App);

        /// <summary>
        /// An unique process ID ("service" f.e.; only one process with this ID should run at once and have a specific order; will be used in filenames!)
        /// </summary>
        [CliConfig]
        [Required]
        public static string ProcessId { get; set; } = DEFAULT_PROCESS_ID;

        /// <summary>
        /// Default file create mode
        /// </summary>
        [CliConfig]
        public static UnixFileMode CreateFileMode { get; set; }
            = UnixFileMode.UserRead | UnixFileMode.UserWrite | 
                UnixFileMode.GroupRead | UnixFileMode.OtherRead;

        /// <summary>
        /// Default folder create mode
        /// </summary>
        [CliConfig]
        public static UnixFileMode CreateFolderMode { get; set; }
            = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | 
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute | 
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

        /// <summary>
        /// Default log level
        /// </summary>
        [CliConfig]
        public static LogLevel LogLevel { get; set; } = Logging.DEFAULT_LOGLEVEL;
    }
}
