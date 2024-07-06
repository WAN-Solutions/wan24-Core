namespace wan24.Core
{
    /// <summary>
    /// <see cref="RentedThread"/> options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class RentedThreadOptions()
    {
        /// <summary>
        /// If this is a background thread
        /// </summary>
        public bool IsBackground { get; init; } = true;

        /// <summary>
        /// Priority
        /// </summary>
        public ThreadPriority Priority { get; init; } = ThreadPriority.Normal;

        /// <summary>
        /// Error source ID to use if the worker thread crashed (see <see cref="ErrorHandling"/>)
        /// </summary>
        public int ErrorSource { get; init; } = ErrorHandling.SERVICE_ERROR;
    }
}
