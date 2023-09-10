namespace wan24.Core
{
    /// <summary>
    /// Interface for a processing information
    /// </summary>
    public interface IProcessingInfo
    {
        /// <summary>
        /// GUID
        /// </summary>
        string GUID { get; }
        /// <summary>
        /// Processing start time
        /// </summary>
        DateTime Started { get; }
        /// <summary>
        /// Description
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Any tagged object
        /// </summary>
        object? Tag { get; }
    }
}
