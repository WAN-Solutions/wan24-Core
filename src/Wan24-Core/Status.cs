namespace wan24.Core
{
    /// <summary>
    /// Status
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Name</param>
    /// <param name="state">State</param>
    /// <param name="description">Description</param>
    /// <param name="group">Group name (use a backslash to define sub-groups)</param>
    public sealed class Status(in string name, in object? state, in string? description = null, in string? group = null)
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Group (a backslash defines sub-groups)
        /// </summary>
        public string? Group { get; } = group;

        /// <summary>
        /// State
        /// </summary>
        public object? State { get; } = state;

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; } = description;

        /// <inheritdoc/>
        public override string ToString() => $"{(Group is null ? string.Empty : $"{Group}\\")}{Name}: {State}";
    }
}
