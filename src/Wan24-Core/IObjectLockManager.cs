namespace wan24.Core
{
    /// <summary>
    /// Interface for an object lock manager
    /// </summary>
    public interface IObjectLockManager
    {
        /// <summary>
        /// Display name
        /// </summary>
        string? Name { get; }
        /// <summary>
        /// Managing object type
        /// </summary>
        Type ObjectType { get; }
        /// <summary>
        /// Number of active locks
        /// </summary>
        int ActiveLocks { get; }
    }
}
