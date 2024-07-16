namespace wan24.Core
{
    /// <summary>
    /// Interface for problem informations
    /// </summary>
    public interface IProblemInfo
    {
        /// <summary>
        /// Created time (UTC)
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Title
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Details
        /// </summary>
        string? Details { get; }
        /// <summary>
        /// Call stack
        /// </summary>
        string Stack { get; }
        /// <summary>
        /// Meta data
        /// </summary>
        IReadOnlyDictionary<string, object?>? Meta { get; }
    }
}
