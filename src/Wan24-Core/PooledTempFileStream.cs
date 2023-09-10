using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Pooled temporary file stream
    /// </summary>
    public sealed class PooledTempFileStream : FileStream, IObjectPoolItem
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
                $"*{(Settings.AppId is null ? string.Empty : $"-{Settings.AppId}")}{(Settings.ProcessId is null ? string.Empty : $"-{Settings.ProcessId}")}.pooled",
                SearchOption.TopDirectoryOnly
                ))
                try
                {
                    Logging.WriteDebug($"Deleting previously pooled temporary file \"{file}\"");
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Logging.WriteWarning($"Failed to delete \"{file}\": {ex.Message}");
                }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PooledTempFileStream() : this(folder: null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folder">Temporary folder</param>
        public PooledTempFileStream(in string? folder)
            : base(
                  Path.Combine(
                      folder ?? Settings.TempFolder,
                      $"{Guid.NewGuid()}{(Settings.AppId is null ? string.Empty : $"-{Settings.AppId}")}{(Settings.ProcessId is null ? string.Empty : $"-{Settings.ProcessId}")}.pooled"
                      ),
                  FileMode.CreateNew,
                  FileAccess.ReadWrite,
                  FileShare.None,
                  BufferSize,
                  FileOptions.DeleteOnClose | FileOptions.SequentialScan | FileOptions.Asynchronous
            )
        { }

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
