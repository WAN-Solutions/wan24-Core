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
        /// Constructor
        /// </summary>
        public PooledTempFileStream():this(folder: null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folder">Temporary folder</param>
        public PooledTempFileStream(string? folder)
            : base(
                  Path.Combine(folder ?? Settings.TempFolder, Guid.NewGuid().ToString()),
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
        public void Reset()
        {
            SetLength(0);
            Position = 0;
        }
    }
}
