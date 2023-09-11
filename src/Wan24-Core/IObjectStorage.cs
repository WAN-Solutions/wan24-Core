namespace wan24.Core
{
    /// <summary>
    /// Interface for an object storage
    /// </summary>
    public interface IObjectStorage : IServiceWorkerStatus
    {
        /// <summary>
        /// Max. number of objects to hold in memory
        /// </summary>
        int InMemoryLimit { get; }
        /// <summary>
        /// Number of objects in memory
        /// </summary>
        int Stored { get; }
        /// <summary>
        /// Max. counted number of objects in memory
        /// </summary>
        int StoredPeak { get; }
        /// <summary>
        /// Number of currently active object references
        /// </summary>
        long ObjectReferences { get; }
    }

    /// <summary>
    /// Interface for an object storage
    /// </summary>
    /// <typeparam name="tKey">Object key type</typeparam>
    /// <typeparam name="tObj">Object type</typeparam>
    public interface IObjectStorage<tKey, tObj> : IObjectStorage
        where tKey : notnull
        where tObj : class, IStoredObject<tKey>
    {
        /// <summary>
        /// Get an object
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Object</returns>
        StoredObject<tKey, tObj>? GetObject(in tKey key);
        /// <summary>
        /// Get an object
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        Task<StoredObject<tKey, tObj>?> GetObjectAsnc(tKey key, CancellationToken cancellationToken = default);
        /// <summary>
        /// Release an object
        /// </summary>
        /// <param name="obj">Object</param>
        void Release(in tObj obj);
        /// <summary>
        /// Remove an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Object</returns>
        tObj Remove(in tObj obj);
    }
}
