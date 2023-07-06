namespace wan24.Core
{
    /// <summary>
    /// Status
    /// </summary>
    public sealed class Status
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="state">State</param>
        /// <param name="description">Description</param>
        /// <param name="group">Group name</param>
        public Status(string name, object? state, string? description = null, string? group = null)
        {
            Name = name;
            Group = group;
            State = state;
            Description = description;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Group
        /// </summary>
        public string? Group { get; }

        /// <summary>
        /// State
        /// </summary>
        public object? State { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }
    }
}
