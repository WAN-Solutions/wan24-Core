namespace wan24.Core
{
    /// <summary>
    /// Interface for a status provider
    /// </summary>
    public interface IStatusProvider
    {
        /// <summary>
        /// Status information
        /// </summary>
        IEnumerable<Status> State { get; }
    }
}
