namespace wan24.Core
{
    /// <summary>
    /// Statistics value modes
    /// </summary>
    public enum StatisticsValueModes
    {
        /// <summary>
        /// Period counter (reset after each period)
        /// </summary>
        [DisplayText("Period counter")]
        PeriodCounter,
        /// <summary>
        /// Total counter (never reset)
        /// </summary>
        [DisplayText("Total counter")]
        TotalCounter
    }
}
