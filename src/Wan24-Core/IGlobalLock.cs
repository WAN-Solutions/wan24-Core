namespace wan24.Core
{
    /// <summary>
    /// Interface for a global lock
    /// </summary>
    public interface IGlobalLock : IDisposableObject
    {
        /// <summary>
        /// GUID
        /// </summary>
        Guid GUID { get; }
        /// <summary>
        /// ID
        /// </summary>
        string ID { get; }
        /// <summary>
        /// Created a new mutex?
        /// </summary>
        bool CreatedNew { get; }
    }
}
