namespace wan24.Core.Caching
{
    /// <summary>
    /// Cache timeouts
    /// </summary>
    public enum CacheTimeouts
    {
        /// <summary>
        /// None (never expires)
        /// </summary>
        None = 0,
        /// <summary>
        /// Fixed time
        /// </summary>
        Fixed = 1,
        /// <summary>
        /// Sliding time (expiring time will be updated when updating the cached value or accessing it)
        /// </summary>
        Sliding = 2
    }
}
