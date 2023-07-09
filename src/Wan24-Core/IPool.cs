namespace wan24.Core
{
    /// <summary>
    /// Interface for a pool
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// Display name
        /// </summary>
        string? Name { get; }
        /// <summary>
        /// Capacity
        /// </summary>
        int Capacity { get; }
        /// <summary>
        /// Number of available items
        /// </summary>
        int Available { get; }
        /// <summary>
        /// Item type
        /// </summary>
        Type ItemType { get; }
    }
}
