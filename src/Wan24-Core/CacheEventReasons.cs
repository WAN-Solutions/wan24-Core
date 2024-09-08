namespace wan24.Core
{
    /// <summary>
    /// Cache event reasons enumeration
    /// </summary>
    public enum CacheEventReasons : byte
    {
        /// <summary>
        /// User action
        /// </summary>
        [DisplayText("User action")]
        UserAction = 0,
        /// <summary>
        /// Automatic action
        /// </summary>
        [DisplayText("Automatic action")]
        Automatic = 1,
        /// <summary>
        /// Tidy action
        /// </summary>
        [DisplayText("Tidy action")]
        Tidy = 2
    }
}
