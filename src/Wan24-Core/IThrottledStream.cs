namespace wan24.Core
{
    /// <summary>
    /// Interface for a throttled stream
    /// </summary>
    public interface IThrottledStream : IStreamWrapper
    {
        /// <summary>
        /// Read count (zero to disable read throttling)
        /// </summary>
        int ReadCount { get; set; }
        /// <summary>
        /// Read time
        /// </summary>
        TimeSpan ReadTime { get; set; }
        /// <summary>
        /// Last read time start
        /// </summary>
        DateTime LastReadTimeStart { get; }
        /// <summary>
        /// Red count since the last read time start
        /// </summary>
        int RedCount { get; }
        /// <summary>
        /// Write count (zero to disable write throttling)
        /// </summary>
        int WriteCount { get; set; }
        /// <summary>
        /// Write time
        /// </summary>
        TimeSpan WriteTime { get; set; }
        /// <summary>
        /// Last write time start
        /// </summary>
        DateTime LastWriteTimeStart { get; }
        /// <summary>
        /// Wrote count since last write time start
        /// </summary>
        int WroteCount { get; }
        /// <summary>
        /// Reset the write throttle
        /// </summary>
        void ResetWriteThrottle();
        /// <summary>
        /// Reset the read throttle
        /// </summary>
        void ResetReadThrottle();
    }
}
