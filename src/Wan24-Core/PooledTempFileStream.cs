using System.Runtime;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Pooled temporary file stream
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="folder">Temporary folder</param>
    /// <param name="mode">Unix create file mode</param>
    public sealed class PooledTempFileStream(in string? folder, in UnixFileMode? mode = null) : FileStream(
          Path.Combine(
                      folder ?? Settings.TempFolder,
                      $"{Guid.NewGuid()}{$"-{Settings.AppId}"}{$"-{Settings.ProcessId}"}.pooled"
                      ),
          ENV.IsLinux
            ? new FileStreamOptions()
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
                BufferSize = BufferSize,
                Options = FileOptions.DeleteOnClose | FileOptions.SequentialScan | FileOptions.Asynchronous,
#pragma warning disable CA1416 // Platform specific
                UnixCreateMode = ENV.IsLinux ? mode ?? Settings.CreateFileMode : null
#pragma warning restore CA1416 // Platform specific
            }
            : new FileStreamOptions()
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
                BufferSize = BufferSize,
                Options = FileOptions.DeleteOnClose | FileOptions.SequentialScan | FileOptions.Asynchronous,
            }
            ), IObjectPoolItem
    {
        /// <summary>
        /// Default buffer size in bytes
        /// </summary>
        public const int DEFAULT_BUFFER_SIZE = 4096;

        /// <summary>
        /// Static constructor
        /// </summary>
        static PooledTempFileStream()
        {
            foreach (string file in Directory.GetFiles(
                Settings.TempFolder,
                $"*{$"-{Settings.AppId}"}{$"-{Settings.ProcessId}"}.pooled",
                SearchOption.TopDirectoryOnly
                ))
                try
                {
                    if (Debug) Logging.WriteDebug($"Deleting previously pooled temporary file \"{file}\"");
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    if (Warning) Logging.WriteWarning($"Failed to delete \"{file}\": {ex.Message}");
                }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mode">Unix create file mode</param>
        public PooledTempFileStream(in UnixFileMode? mode) : this(folder: null, mode) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public PooledTempFileStream() : this(folder: null) { }

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public static int BufferSize { get; set; } = DEFAULT_BUFFER_SIZE;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public void Reset()
        {
            SetLength(0);
            Position = 0;
        }
    }
}
