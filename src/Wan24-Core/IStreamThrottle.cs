namespace wan24.Core
{
    /// <summary>
    /// Interface for a stream throttle
    /// </summary>
    public interface IStreamThrottle : IDisposableObject
    {
        /// <summary>
        /// Quota value for the read count (used to normalize the stream collection read count)
        /// </summary>
        int ReadCountQuota { get; }
        /// <summary>
        /// Read count (zero to disable read throttling)
        /// </summary>
        int ReadCount { get; set; }
        /// <summary>
        /// Quota value for the write count (used to normalize the stream collection write count)
        /// </summary>
        int WriteCountQuota { get; }
        /// <summary>
        /// Write count (zero to disable write throttling)
        /// </summary>
        int WriteCount { get; set; }
    }
}
