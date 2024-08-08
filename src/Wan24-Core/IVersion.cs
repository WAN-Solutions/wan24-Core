namespace wan24.Core
{
    /// <summary>
    /// Interface for an object which exports its version
    /// </summary>
    public interface IVersion
    {
        /// <summary>
        /// Version
        /// </summary>
        int Version { get; }
    }
}
