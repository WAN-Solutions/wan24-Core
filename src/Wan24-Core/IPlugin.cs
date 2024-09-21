namespace wan24.Core
{
    /// <summary>
    /// Interface for a plugin
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// GUID
        /// </summary>
        string GUID { get; }
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Description
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Version
        /// </summary>
        Version Version { get; }
    }
}
