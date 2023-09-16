namespace wan24.Core
{
    /// <summary>
    /// Stored object
    /// </summary>
    public sealed class StoredObject<tKey, tObj> : DisposableBase
        where tKey : notnull
        where tObj : class, IStoredObject<tKey>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage">Storage</param>
        /// <param name="obj">Object</param>
        public StoredObject(in IObjectStorage<tKey, tObj> storage, in tObj obj) : base(asyncDisposing: false)
        {
            Storage = storage;
            Object = obj;
        }

        /// <summary>
        /// Storage
        /// </summary>
        public IObjectStorage<tKey, tObj> Storage { get; }

        /// <summary>
        /// Object
        /// </summary>
        public tObj Object { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Storage.Release(Object);

        /// <summary>
        /// Cast as object
        /// </summary>
        /// <param name="so"><see cref="StoredObject{tKey, tObj}"/></param>
        public static implicit operator tObj(StoredObject<tKey, tObj> so) => so.Object;
    }
}
