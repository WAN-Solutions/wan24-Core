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
        /// Default stack allocation border
        /// </summary>
        public const int DEFAULT_STACK_ALLOC_BORDER = 1024;

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public static int BufferSize { get; set; } = DEFAULT_BUFFER_SIZE;

        /// <summary>
        /// Temporary folder (may be the customized value or the system users temporary folder)
        /// </summary>
        public static string TempFolder => CustomTempFolder ?? Path.GetTempPath();

        /// <summary>
        /// Custom temporary folder
        /// </summary>
        public static string? CustomTempFolder { get; set; }

        /// <summary>
        /// Stack allocation border in bytes
        /// </summary>
        public static int StackAllocBorder { get; set; } = DEFAULT_STACK_ALLOC_BORDER;
    }
}
