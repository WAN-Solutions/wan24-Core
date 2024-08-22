namespace wan24.Core
{
    /// <summary>
    /// In-memory cache entry types
    /// </summary>
    public enum InMemoryCacheEntryTypes
    {
        /// <summary>
        /// Persistent entry (won't be removed)
        /// </summary>
        [DisplayText("Persistent entry (won't be removed)")]
        Persistent,
        /// <summary>
        /// Timeout entry (removed after timeout)
        /// </summary>
        [DisplayText("Timeout entry (removed after timeout)")]
        Timeout,
        /// <summary>
        /// Variable entry (removed on demand)
        /// </summary>
        [DisplayText("Variable entry (removed on demand)")]
        Variable
    }
}
