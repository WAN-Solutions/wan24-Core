namespace wan24.Core
{
    /// <summary>
    /// Hosted service timer types
    /// </summary>
    public enum HostedServiceTimers : byte
    {
        /// <summary>
        /// Default (restart after processed)
        /// </summary>
        Default = 0,
        /// <summary>
        /// Exact (restart to match the exact interval without processing time)
        /// </summary>
        Exact = 1,
        /// <summary>
        /// Exact and catching up (restart to match the exact interval without processing time and catch up missing processing runs without a delay)
        /// </summary>
        ExactCatchingUp = 2
    }
}
