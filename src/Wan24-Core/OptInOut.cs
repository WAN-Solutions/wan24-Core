namespace wan24.Core
{
    /// <summary>
    /// Opt in/out enumeration
    /// </summary>
    public enum OptInOut : byte
    {
        /// <summary>
        /// Opt in
        /// </summary>
        [DisplayText("Opt in")]
        OptIn = 0,
        /// <summary>
        /// Opt out
        /// </summary>
        [DisplayText("Opt out")]
        OptOut = 1
    }
}
