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
    /// <param name="group">Group name</param>
    public sealed class Status(in string name, in object? state, in string? description = null, in string? group = null)
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Group
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
    }
}
