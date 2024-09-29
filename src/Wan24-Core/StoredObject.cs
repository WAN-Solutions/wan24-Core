namespace wan24.Core
{
    /// <summary>
    /// Stored object
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="storage">Storage</param>
    /// <param name="obj">Object</param>
    public sealed class StoredObject<tKey, tObj>(in IObjectStorage<tKey, tObj> storage, in tObj obj) : SimpleDisposableBase()
        where tKey : notnull
        where tObj : class, IStoredObject<tKey>
    {
        /// <summary>
        /// Storage
        /// </summary>
        public IObjectStorage<tKey, tObj> Storage { get; } = storage;

        /// <summary>
        /// Object
        /// </summary>
        public tObj Object { get; } = obj;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Storage.Release(Object);

        /// <summary>
        /// Cast as object
        /// </summary>
        /// <param name="so"><see cref="StoredObject{tKey, tObj}"/></param>
        public static implicit operator tObj(StoredObject<tKey, tObj> so) => so.Object;
    }
}
