namespace wan24.Core
{
    /// <summary>
    /// User action information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed record class UserActionInfo()
    {
        /// <summary>
        /// Method name
        /// </summary>
        public required string Method { get; init; }

        /// <summary>
        /// Action name
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Object provider type name
        /// </summary>
        public string? ProviderType { get; init; }

        /// <summary>
        /// Object provider concurrent dictionary property name
        /// </summary>
        public string? ProviderProperty { get; init; }

        /// <summary>
        /// Object provider dictionary key
        /// </summary>
        public string? ProviderKey { get; init; }
    }
}
