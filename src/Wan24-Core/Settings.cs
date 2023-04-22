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
        public const int DEFAULT_BUFFER_SIZE = 81920;

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public static int BufferSize { get; set; } = DEFAULT_BUFFER_SIZE;
    }
}
