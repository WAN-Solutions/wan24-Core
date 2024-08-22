namespace wan24.Core
{
    /// <summary>
    /// In-memory cache management strategies for automatic cache entry cleanup selection
    /// </summary>
    public enum InMemoryCacheStrategy
    {
        /// <summary>
        /// Remove oldest first
        /// </summary>
        [DisplayText("Remove oldest first")]
        Age,
        /// <summary>
        /// Remove least accessed first
        /// </summary>
        [DisplayText("Remove least accessed first")]
        AccessTime,
        /// <summary>
        /// Remove largest first
        /// </summary>
        [DisplayText("Remove largest first")]
        Largest,
        /// <summary>
        /// Remove smallest first
        /// </summary>
        [DisplayText("Remove smallest first")]
        Smallest,
        /// <summary>
        /// Custom strategy 1
        /// </summary>
        [DisplayText("Custom strategy 1")]
        Custom1,
        /// <summary>
        /// Custom strategy 2
        /// </summary>
        [DisplayText("Custom strategy 2")]
        Custom2,
        /// <summary>
        /// Custom strategy 3
        /// </summary>
        [DisplayText("Custom strategy 3")]
        Custom3
    }
}
