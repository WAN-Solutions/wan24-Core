namespace wan24.Core
{
    /// <summary>
    /// Plugin information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class PluginInfo() : IPlugin
    {
        /// <inheritdoc/>
        public required string GUID { get; init; }

        /// <inheritdoc/>
        public required string Name { get; init; }

        /// <inheritdoc/>
        public required string Description { get; init; }

        /// <inheritdoc/>
        public required Version Version { get; init; }
    }
}
