namespace wan24.Core
{
    /// <summary>
    /// Interface for a status provider
    /// </summary>
    public interface IStatusProvider
    {
        /// <summary>
        /// Status informations
        /// </summary>
        IEnumerable<Status> State { get; }
    }
}
