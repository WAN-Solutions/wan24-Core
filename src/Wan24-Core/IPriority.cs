namespace wan24.Core
{
    /// <summary>
    /// Interface for an object which exports its priority (higher priority comes first)
    /// </summary>
    public interface IPriority
    {
        /// <summary>
        /// Priority
        /// </summary>
        int Priority { get; }
    }
}
